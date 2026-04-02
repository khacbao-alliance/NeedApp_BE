using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class RequestParticipant : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantRole Role { get; set; } = ParticipantRole.Observer;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Request Request { get; set; } = default!;
    public User User { get; set; } = default!;
}
