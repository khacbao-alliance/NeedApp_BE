using System.Text.Json;
using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class Message : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid? SenderId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public string? Content { get; set; }
    public JsonDocument? Metadata { get; set; }
    public Guid? ReplyToId { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Request Request { get; set; } = default!;
    public User? Sender { get; set; }
    public Message? ReplyTo { get; set; }
    public ICollection<Message> Replies { get; set; } = [];
    public ICollection<FileAttachment> Files { get; set; } = [];
    public ICollection<MessageReaction> Reactions { get; set; } = [];
}
