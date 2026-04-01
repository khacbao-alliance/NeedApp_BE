using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class MissingInformation : BaseEntity
{
    public Guid RequestId { get; set; }
    public string Question { get; set; } = default!;
    public string? Answer { get; set; }
    public MissingInfoStatus Status { get; set; } = MissingInfoStatus.Pending;
    public Guid? CreatedBy { get; set; }
    public Guid? AssignedTo { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Request Request { get; set; } = default!;
}
