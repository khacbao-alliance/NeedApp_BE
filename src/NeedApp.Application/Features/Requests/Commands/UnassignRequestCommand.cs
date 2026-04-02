using MediatR;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.DTOs.Request;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Requests.Commands;

public record UnassignRequestCommand(Guid RequestId) : IRequest<RequestDto>;

public class UnassignRequestCommandHandler(
    IRequestRepository requestRepository,
    IUserRepository userRepository,
    IRequestParticipantRepository participantRepository,
    IMessageRepository messageRepository,
    ICurrentUserService currentUserService,
    IChatHubService chatHubService,
    IUnitOfWork unitOfWork) : IRequestHandler<UnassignRequestCommand, RequestDto>
{
    public async Task<RequestDto> Handle(UnassignRequestCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");

        var request = await requestRepository.GetWithDetailsAsync(command.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), command.RequestId);

        if (!request.AssignedTo.HasValue)
            throw new DomainException("This request is not assigned to anyone.");

        var oldStaff = await userRepository.GetByIdAsync(request.AssignedTo.Value, cancellationToken);

        // Remove old assignee from participants
        var participants = await participantRepository.GetByRequestIdAsync(command.RequestId, cancellationToken);
        var assigneeParticipant = participants.FirstOrDefault(p =>
            p.UserId == request.AssignedTo.Value && p.Role == ParticipantRole.Assignee);
        if (assigneeParticipant != null)
        {
            participantRepository.Remove(assigneeParticipant);
        }

        // Clear assignment
        request.AssignedTo = null;
        request.UpdatedAt = DateTime.UtcNow;
        request.UpdatedBy = userId;
        requestRepository.Update(request);

        // System message
        var staffName = oldStaff?.Name ?? "Unknown";
        var systemMsg = new Message
        {
            RequestId = request.Id,
            Type = MessageType.System,
            Content = $"\"{staffName}\" has been unassigned from this request."
        };
        await messageRepository.AddAsync(systemMsg, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Push SignalR
        var systemMsgDto = new MessageDto(
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
            null, // No assigned user
            creator != null
                ? new RequestUserDto(creator.UserId, creator.User?.Name, creator.User?.AvatarUrl)
                : null,
            request.Messages.Count(m => !m.IsDeleted),
            request.CreatedAt,
            request.UpdatedAt
        );
    }
}
