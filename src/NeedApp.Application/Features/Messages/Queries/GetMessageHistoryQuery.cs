using MediatR;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Messages.Queries;

/// <summary>
/// Returns the edit history for a message (newest first).
/// Accessible by any participant of the request — both Client (own messages) and Staff/Admin.
/// </summary>
public record GetMessageHistoryQuery(Guid RequestId, Guid MessageId) : IRequest<List<MessageEditHistoryDto>>;

public class GetMessageHistoryQueryHandler(
    IMessageEditHistoryRepository historyRepository,
    IRequestRepository requestRepository,
    IRequestParticipantRepository participantRepository,
    IClientUserRepository clientUserRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetMessageHistoryQuery, List<MessageEditHistoryDto>>
{
    public async Task<List<MessageEditHistoryDto>> Handle(
        GetMessageHistoryQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");
        var userRole = currentUserService.UserRole;

        // Verify request exists
        var request = await requestRepository.GetByIdAsync(query.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(Request), query.RequestId);

        // Authorization: participant check
        var isParticipant = await participantRepository.IsParticipantAsync(
            query.RequestId, userId, cancellationToken);

        if (!isParticipant)
        {
            if (userRole == Domain.Enums.UserRole.Client)
            {
                // Client must belong to the organization that owns this request
                var clientUser = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken);
                if (clientUser == null || clientUser.ClientId != request.ClientId)
                    throw new ForbiddenException("You do not have access to this request.");
            }
            else
            {
                throw new ForbiddenException("You do not have access to this request.");
            }
        }

        var history = await historyRepository.GetByMessageIdAsync(query.MessageId, cancellationToken);

        return history.Select(h => new MessageEditHistoryDto(
            h.Id,
            h.PreviousContent,
            h.EditedAt,
            h.Editor?.Name
        )).ToList();
    }
}
