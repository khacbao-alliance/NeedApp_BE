using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<Need> _needs = [];
    public IReadOnlyCollection<Need> Needs => _needs.AsReadOnly();

    private Category() { }

    public static Category Create(string name, string? description = null, string? iconUrl = null)
    {
        return new Category
        {
            Name = name,
            Description = description,
            IconUrl = iconUrl
        };
    }

    public void Update(string name, string? description, string? iconUrl)
    {
        Name = name;
        Description = description;
        IconUrl = iconUrl;
        SetUpdatedAt();
    }
}
