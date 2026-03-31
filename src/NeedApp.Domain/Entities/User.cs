using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public UserRole Role { get; private set; } = UserRole.User;
    public bool IsActive { get; private set; } = true;

    private readonly List<Need> _needs = [];
    public IReadOnlyCollection<Need> Needs => _needs.AsReadOnly();

    private User() { }

    public static User Create(string fullName, string email, string passwordHash, UserRole role = UserRole.User)
    {
        return new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void Update(string fullName, string? phoneNumber, string? avatarUrl)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        AvatarUrl = avatarUrl;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
