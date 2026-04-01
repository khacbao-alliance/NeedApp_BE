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

    public ICollection<ClientUser> ClientUsers { get; set; } = [];
    public ICollection<Request> AssignedRequests { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
