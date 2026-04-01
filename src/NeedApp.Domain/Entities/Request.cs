using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class Request : AuditableEntity
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ClientId { get; set; }
    public Guid? AssignedTo { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public string? Priority { get; set; }

    public Client Client { get; set; } = default!;
    public User? AssignedUser { get; set; }
    public ICollection<RequestParticipant> Participants { get; set; } = [];
    public ICollection<MissingInformation> MissingInformations { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<RequestFile> Files { get; set; } = [];
    public ICollection<ActivityLog> ActivityLogs { get; set; } = [];
}
