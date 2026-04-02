using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class IntakeQuestion : BaseEntity
{
    public Guid QuestionSetId { get; set; }
    public string Content { get; set; } = default!;
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; } = true;
    public string? Placeholder { get; set; }

    public IntakeQuestionSet QuestionSet { get; set; } = default!;
}
