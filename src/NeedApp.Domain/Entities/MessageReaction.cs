using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

/// <summary>
/// Tracks emoji reactions on messages (e.g. 👍, ❤️, ✅)
/// </summary>
public class MessageReaction : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = default!; // e.g. "👍", "❤️", "✅"

    public Message Message { get; set; } = default!;
    public User User { get; set; } = default!;
}
