using System.Text.Json;
using MediatR;
using NeedApp.Application.DTOs.Message;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Application.Features.Messages.Queries;

public record SearchMessagesQuery(
    Guid RequestId,
    string Query,
    int Limit = 50
) : IRequest<IEnumerable<MessageDto>>;

public class SearchMessagesQueryHandler(
    IMessageRepository messageRepository)
    : IRequestHandler<SearchMessagesQuery, IEnumerable<MessageDto>>
{
    public async Task<IEnumerable<MessageDto>> Handle(SearchMessagesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Enumerable.Empty<MessageDto>();

        var messages = await messageRepository.SearchAsync(
            request.RequestId, request.Query, request.Limit, cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.Type,
            m.Content,
            m.Sender != null ? new MessageSenderDto(m.Sender.Id, m.Sender.Name, m.Sender.Role, m.Sender.AvatarUrl) : null,
            m.Metadata != null ? JsonSerializer.Deserialize<object>(m.Metadata) : null,
            m.ReplyToId,
            m.Files.Select(f => new FileAttachmentDto(f.Id, f.FileName, f.Url, f.ContentType, f.FileSize)).ToList(),
            m.CreatedAt
        ));
    }
}
