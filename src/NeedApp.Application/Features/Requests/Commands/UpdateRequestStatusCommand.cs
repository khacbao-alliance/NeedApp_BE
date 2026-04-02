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

        var request = await requestRepository.GetWithDetailsAsync(command.RequestId, cancellationToken)
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

        // Build response
        var creator = request.Participants.FirstOrDefault(p => p.Role == ParticipantRole.Creator);

        return new RequestDto(
            request.Id,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            new RequestClientDto(request.Client.Id, request.Client.Name),
            request.AssignedUser != null
                ? new RequestUserDto(request.AssignedUser.Id, request.AssignedUser.Name, request.AssignedUser.AvatarUrl)
                : null,
            creator != null
                ? new RequestUserDto(creator.UserId, creator.User?.Name, creator.User?.AvatarUrl)
                : null,
            request.Messages.Count(m => !m.IsDeleted),
            request.CreatedAt,
            request.UpdatedAt
        );
    }
}
