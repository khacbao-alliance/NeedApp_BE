using MediatR;
using NeedApp.Application.DTOs.Message;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Exceptions;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Messages.Queries;

public record GetMessagesQuery(Guid RequestId, string? Cursor, int Limit = 20) : IRequest<MessageListResponse>;

public class GetMessagesQueryHandler(
    IMessageRepository messageRepository,
    IRequestRepository requestRepository,
    IClientUserRepository clientUserRepository,
    IRequestParticipantRepository participantRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetMessagesQuery, MessageListResponse>
{
    public async Task<MessageListResponse> Handle(GetMessagesQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User not authenticated.");
        var userRole = currentUserService.UserRole;

        var request = await requestRepository.GetByIdAsync(query.RequestId, cancellationToken)
            ?? throw new NotFoundException("Request", query.RequestId);

        // Authorization check:
        // - Admin: can view all requests
        // - Staff: must be a participant (assigned) of the request
        // - Client: must belong to the same Client company that owns the request
        if (userRole == UserRole.Client)
        {
            var clientUser = await clientUserRepository.GetByUserIdAsync(userId, cancellationToken);
            if (clientUser == null || request.ClientId != clientUser.ClientId)
                throw new NotFoundException("Request", query.RequestId);
        }
        else if (userRole == UserRole.Staff)
        {
            var isParticipant = await participantRepository.IsParticipantAsync(query.RequestId, userId, cancellationToken);
            if (!isParticipant)
                throw new NotFoundException("Request", query.RequestId);
        }
        // Admin: no restriction

        DateTime? cursorDate = null;
        Guid? cursorId = null;

        if (!string.IsNullOrEmpty(query.Cursor))
        {
            var parts = query.Cursor.Split('_', 2);
            if (parts.Length == 2 && DateTime.TryParse(parts[0], out var d) && Guid.TryParse(parts[1], out var id))
            {
                cursorDate = d;
                cursorId = id;
            }
        }

        var limit = Math.Clamp(query.Limit, 1, 50);
        var (items, hasMore) = await messageRepository.GetByRequestIdAsync(
            query.RequestId, cursorDate, cursorId, limit, cancellationToken);

        var messages = items.Select(m => new MessageDto(
            m.Id, m.Type, m.Content,
            m.Sender != null ? new MessageSenderDto(m.Sender.Id, m.Sender.Name, m.Sender.Role, m.Sender.AvatarUrl) : null,
            m.Metadata?.RootElement,
            m.ReplyToId,
            m.Files.Select(f => new FileAttachmentDto(f.Id, f.FileName, f.Url, f.ContentType, f.FileSize)).ToList(),
            m.CreatedAt,
            m.Reactions.Count > 0
                ? m.Reactions.GroupBy(r => r.Emoji).Select(g => new ReactionSummaryDto(g.Key, g.Count(), g.Select(r => r.UserId).ToList())).ToList()
                : null
        )).ToList();

        string? nextCursor = null;
        if (hasMore && messages.Count > 0)
        {
            var last = messages.Last();
            nextCursor = $"{last.CreatedAt:O}_{last.Id}";
        }

        return new MessageListResponse(messages, nextCursor, hasMore);
    }
}

