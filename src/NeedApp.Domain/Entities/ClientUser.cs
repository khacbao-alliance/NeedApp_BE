using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class ClientUser : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid UserId { get; set; }
    public ClientRole Role { get; set; } = ClientRole.Member;
    public Guid? CreatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Client Client { get; set; } = default!;
    public User User { get; set; } = default!;
}
