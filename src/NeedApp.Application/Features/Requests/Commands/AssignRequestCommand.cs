using FluentValidation;
using MediatR;
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
    IUnitOfWork unitOfWork) : IRequestHandler<AssignRequestCommand, RequestDto>
{
    public async Task<RequestDto> Handle(AssignRequestCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");
        var userRole = currentUserService.UserRole;

        var request = await requestRepository.GetWithDetailsAsync(command.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), command.RequestId);

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
        var previousAssignee = request.AssignedUser?.Name;
        request.AssignedTo = command.StaffUserId;
        request.UpdatedAt = DateTime.UtcNow;
        request.UpdatedBy = userId;
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
        var assignMessage = previousAssignee != null
            ? $"Request reassigned from \"{previousAssignee}\" to \"{staffUser.Name}\"."
            : $"Request assigned to \"{staffUser.Name}\".";

        var systemMsg = new Message
        {
            RequestId = request.Id,
            Type = MessageType.System,
            Content = assignMessage
        };
        await messageRepository.AddAsync(systemMsg, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Push SignalR notifications
        var systemMsgDto = new DTOs.Message.MessageDto(
            systemMsg.Id, systemMsg.Type, systemMsg.Content,
            null, null, null, [], systemMsg.CreatedAt);
        await chatHubService.SendMessageToRequest(command.RequestId, systemMsgDto);

        // Build response
        var creator = request.Participants.FirstOrDefault(p => p.Role == ParticipantRole.Creator);

        return new RequestDto(
            request.Id,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            new RequestClientDto(request.Client.Id, request.Client.Name),
            new RequestUserDto(staffUser.Id, staffUser.Name, staffUser.AvatarUrl),
            creator != null
                ? new RequestUserDto(creator.UserId, creator.User?.Name, creator.User?.AvatarUrl)
                : null,
            request.Messages.Count(m => !m.IsDeleted),
            request.CreatedAt,
            request.UpdatedAt
        );
    }
}
