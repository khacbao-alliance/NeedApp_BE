using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

/// <summary>
/// Tracks when a user has read messages in a request conversation.
/// One record per user per request — stores the timestamp of last read.
/// </summary>
public class MessageReadReceipt : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public Request Request { get; set; } = default!;
    public User User { get; set; } = default!;
}
