using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class Need : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public decimal? Budget { get; private set; }
    public NeedStatus Status { get; private set; } = NeedStatus.Open;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }

    private Need() { }

    public static Need Create(string title, string description, Guid userId, Guid? categoryId = null, string? location = null, decimal? budget = null)
    {
        return new Need
        {
            Title = title,
            Description = description,
            UserId = userId,
            CategoryId = categoryId,
            Location = location,
            Budget = budget
        };
    }

    public void Update(string title, string description, string? location, decimal? budget)
    {
        Title = title;
        Description = description;
        Location = location;
        Budget = budget;
        SetUpdatedAt();
    }

    public void ChangeStatus(NeedStatus status)
    {
        Status = status;
        SetUpdatedAt();
    }
}
