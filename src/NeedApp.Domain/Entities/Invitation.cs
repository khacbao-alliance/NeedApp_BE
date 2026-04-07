using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class Invitation : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid InvitedUserId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public ClientRole Role { get; set; } = ClientRole.Member;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime? RespondedAt { get; set; }

    public Client Client { get; set; } = default!;
    public User InvitedUser { get; set; } = default!;
    public User InvitedByUser { get; set; } = default!;
}
