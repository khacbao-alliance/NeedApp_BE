using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public string? Description { get; set; }
    public Guid? RequestId { get; set; }

    public User? User { get; set; }
    public Request? Request { get; set; }
}
