using FluentValidation;
using MediatR;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Requests.Commands;

public record UpdateRequestStatusCommand(Guid RequestId, RequestStatus Status) : IRequest<RequestDto>;

public class UpdateRequestStatusCommandValidator : AbstractValidator<UpdateRequestStatusCommand>
{
    public UpdateRequestStatusCommandValidator()
    {
        RuleFor(x => x.RequestId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum()
            .Must(s => s != RequestStatus.Draft && s != RequestStatus.Intake)
            .WithMessage("Cannot manually set status to Draft or Intake.");
    }
}

public class UpdateRequestStatusCommandHandler(
    IRequestRepository requestRepository,
    IMessageRepository messageRepository,
    ICurrentUserService currentUserService,
    INotificationService notificationService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRequestStatusCommand, RequestDto>
{
    // Define valid transitions to prevent invalid state changes
    private static readonly Dictionary<RequestStatus, RequestStatus[]> ValidTransitions = new()
    {
        { RequestStatus.Pending, [RequestStatus.InProgress, RequestStatus.Cancelled] },
        { RequestStatus.MissingInfo, [RequestStatus.InProgress, RequestStatus.Pending, RequestStatus.Cancelled] },
        { RequestStatus.InProgress, [RequestStatus.Done, RequestStatus.MissingInfo, RequestStatus.Pending, RequestStatus.Cancelled] },
        { RequestStatus.Done, [RequestStatus.InProgress] }, // Re-open
    };

    public async Task<RequestDto> Handle(UpdateRequestStatusCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        // Load tracked entity for mutation (no includes → no User tracking conflicts)
        var request = await requestRepository.GetByIdAsync(command.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), command.RequestId);

        // Validate status transition
        if (!ValidTransitions.TryGetValue(request.Status, out var allowedStatuses) ||
            !allowedStatuses.Contains(command.Status))
        {
            throw new DomainException($"Cannot transition from '{request.Status}' to '{command.Status}'.");
        }

        var oldStatus = request.Status;
        request.Status = command.Status;
        request.UpdatedAt = DateTime.UtcNow;
        request.UpdatedBy = userId;
        requestRepository.Update(request);

        // Create system message about the status change
        await messageRepository.AddAsync(new Message
        {
            RequestId = request.Id,
            Type = MessageType.System,
            Content = $"Status changed from \"{oldStatus}\" to \"{command.Status}\"."
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify request creator about status change (critical — sends email)
        if (request.CreatedBy.HasValue && request.CreatedBy.Value != userId)
        {
            await notificationService.NotifyAsync(
                request.CreatedBy.Value,
                Domain.Enums.NotificationType.StatusChange,
                "Trạng thái yêu cầu đã thay đổi",
                $"Request \"{request.Title}\" đã chuyển từ \"{oldStatus}\" sang \"{command.Status}\".",
                request.Id,
                "Request",
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
            detailed.AssignedUser != null
                ? new RequestUserDto(detailed.AssignedUser.Id, detailed.AssignedUser.Name, detailed.AssignedUser.AvatarUrl)
                : null,
            creator != null
                ? new RequestUserDto(creator.UserId, creator.User?.Name, creator.User?.AvatarUrl)
                : null,
            messageCount,
            detailed.CreatedAt,
            detailed.UpdatedAt
        );
    }
}
