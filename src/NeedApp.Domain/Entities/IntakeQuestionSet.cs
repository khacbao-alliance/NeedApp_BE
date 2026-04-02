using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class IntakeQuestionSet : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;

    public ICollection<IntakeQuestion> Questions { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
}
