using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

/// <summary>
/// Stores the SLA deadline hours for each priority level.
/// Only one row per priority — acts as a configuration table.
/// </summary>
public class SlaConfig : BaseEntity
{
    public RequestPriority Priority { get; set; }
    public double DeadlineHours { get; set; }
    public string? Description { get; set; }
}
