using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class EmailPreference : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>Email when a request is assigned to you.</summary>
    public bool OnAssignment { get; set; } = true;

    /// <summary>Email when a request status changes.</summary>
    public bool OnStatusChange { get; set; } = true;

    /// <summary>Email when a request becomes overdue.</summary>
    public bool OnOverdue { get; set; } = true;

    /// <summary>Email when a new request is created (Staff/Admin).</summary>
    public bool OnNewRequest { get; set; } = true;

    /// <summary>Digest frequency: None / Daily / Weekly.</summary>
    public DigestFrequency DigestFrequency { get; set; } = DigestFrequency.None;

    /// <summary>Track last digest sent time to avoid duplicates.</summary>
    public DateTime? LastDigestSentAt { get; set; }

    public User User { get; set; } = default!;
}
