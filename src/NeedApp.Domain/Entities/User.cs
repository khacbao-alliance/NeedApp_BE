using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; set; } = default!;
    public string? Name { get; set; }
    public UserRole? Role { get; set; }
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarPublicId { get; set; }
    public bool HasClient { get; set; } = false;

    public ICollection<ClientUser> ClientUsers { get; set; } = [];
    public ICollection<Request> AssignedRequests { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
