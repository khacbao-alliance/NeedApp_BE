using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class Client : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public ICollection<ClientUser> ClientUsers { get; set; } = [];
    public ICollection<Request> Requests { get; set; } = [];
}
