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
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;
    public Guid? IntakeQuestionSetId { get; set; }
    public int IntakeProgress { get; set; } = 0;

    public Client Client { get; set; } = default!;
    public User? AssignedUser { get; set; }
    public IntakeQuestionSet? IntakeQuestionSet { get; set; }
    public ICollection<RequestParticipant> Participants { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
}
