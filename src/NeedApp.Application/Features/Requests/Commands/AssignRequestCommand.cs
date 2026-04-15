using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Notification;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Requests.Commands;

public record AssignRequestCommand(Guid RequestId, Guid StaffUserId) : IRequest<RequestDto>;

public class AssignRequestCommandValidator : AbstractValidator<AssignRequestCommand>
{
    public AssignRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.StaffUserId).NotEmpty();
    }
}

public class AssignRequestCommandHandler(
    IRequestRepository requestRepository,
    IUserRepository userRepository,
    IRequestParticipantRepository participantRepository,
    IMessageRepository messageRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignRequestCommand, RequestDto>
{
    public async Task<RequestDto> Handle(AssignRequestCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");
        var userRole = currentUserService.UserRole;

        // Load tracked entity for mutation (no includes → no User tracking conflicts)
        var request = await requestRepository.GetByIdAsync(command.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), command.RequestId);

        // Cannot assign during Draft or Intake phase — must reach Pending first
        if (request.Status is RequestStatus.Draft or RequestStatus.Intake)
            throw new DomainException("Cannot assign staff while request is in Draft or Intake phase. Wait until the intake is complete.");

        // Validate that the target user exists and is Staff or Admin
        var staffUser = await userRepository.GetByIdAsync(command.StaffUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), command.StaffUserId);

        if (staffUser.Role != UserRole.Staff && staffUser.Role != UserRole.Admin)
            throw new DomainException("Can only assign requests to Staff or Admin users.");

        // Self-assign: Staff can only assign themselves
        if (userRole == UserRole.Staff && command.StaffUserId != userId)
            throw new DomainException("Staff can only self-assign. Ask an Admin to assign other staff.");

        // Check if already assigned to someone else (only 1 staff per request)
        if (request.AssignedTo.HasValue && request.AssignedTo.Value != command.StaffUserId)
        {
            if (userRole == UserRole.Staff)
                throw new DomainException("This request is already assigned to another staff member.");

            // Admin can reassign — remove old participant
            var oldParticipants = await participantRepository.GetByRequestIdAsync(command.RequestId, cancellationToken);
            var oldAssignee = oldParticipants.FirstOrDefault(p =>
                p.UserId == request.AssignedTo.Value && p.Role == ParticipantRole.Assignee);
            if (oldAssignee != null)
            {
                participantRepository.Remove(oldAssignee);
            }
        }

        // Update request assignment
        request.AssignedTo = command.StaffUserId;
        request.UpdatedAt = DateTime.UtcNow;
        request.UpdatedBy = userId;

        // Auto-transition to InProgress when staff accepts the request
        if (request.Status is RequestStatus.Pending or RequestStatus.MissingInfo)
        {
            request.Status = RequestStatus.InProgress;
        }

        requestRepository.Update(request);

        // Add staff as participant (Assignee role) if not already
        var isParticipant = await participantRepository.IsParticipantAsync(
            command.RequestId, command.StaffUserId, cancellationToken);

        if (!isParticipant)
        {
            await participantRepository.AddAsync(new RequestParticipant
            {
                RequestId = command.RequestId,
                UserId = command.StaffUserId,
                Role = ParticipantRole.Assignee
            }, cancellationToken);
        }

        // System message
        var systemMsg = new Message
        {
            RequestId = request.Id,
            Type = MessageType.System,
            Content = $"Request assigned to \"{staffUser.Name}\"."
        };
        await messageRepository.AddAsync(systemMsg, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Push SignalR notifications
        var systemMsgDto = new DTOs.Message.MessageDto(
            systemMsg.Id, systemMsg.Type, systemMsg.Content,
            null, null, null, [], systemMsg.CreatedAt);
        await chatHubService.SendMessageToRequest(command.RequestId, systemMsgDto);
        // Broadcast status change (auto-transitioned to InProgress) so chat header updates in real-time
        await chatHubService.SendRequestStatusChanged(command.RequestId, request.Status.ToString());

        // Notify assigned staff (critical — sends email)
        if (command.StaffUserId != userId)
        {
            await notificationService.NotifyAsync(
                command.StaffUserId,
                Domain.Enums.NotificationType.Assignment,
                "Bạn được assign request mới",
                $"Yêu cầu \"{request.Title}\" đã được giao cho bạn.",
                request.Id,
                "Request",
                new AssignmentToMeMetadata(request.Title),
                cancellationToken);
        }
        else
        {
            // Staff self-assigned → notify all Admins
            var admins = await userRepository.FindAsync(
                u => u.Role == UserRole.Admin, cancellationToken);
            var adminIds = admins.Select(u => u.Id);
            await notificationService.NotifyMultipleAsync(
                adminIds,
                Domain.Enums.NotificationType.Assignment,
                "Staff đã nhận xử lí request",
                $"\"{staffUser.Name}\" đã nhận xử lí yêu cầu \"{request.Title}\".",
                request.Id,
                "Request",
                new AssignmentSelfAcceptMetadata(request.Title, staffUser.Name ?? "Unknown"),
                cancellationToken);
        }

        // Reload with full details (untracked) for response DTO
        var detailed = await requestRepository.GetWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), request.Id);

        var creator = detailed.Participants.FirstOrDefault(p => p.Role == ParticipantRole.Creator);
        var messageCount = await messageRepository.GetCountByRequestIdAsync(detailed.Id, cancellationToken);

        return new RequestDto(
            detailed.Id,
            detailed.Title,
            detailed.Description,
            detailed.Status,
            detailed.Priority,
            new RequestClientDto(detailed.Client.Id, detailed.Client.Name),
            new RequestUserDto(staffUser.Id, staffUser.Name, staffUser.AvatarUrl),
            creator != null
                ? new RequestUserDto(creator.UserId, creator.User?.Name, creator.User?.AvatarUrl)
                : null,
            messageCount,
            !detailed.Client.IsDeleted,
            detailed.CreatedAt,
            detailed.UpdatedAt
        );
    }
}
