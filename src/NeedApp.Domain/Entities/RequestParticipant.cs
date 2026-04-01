using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class RequestParticipant : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }

    public Request Request { get; set; } = default!;
    public User User { get; set; } = default!;
}
