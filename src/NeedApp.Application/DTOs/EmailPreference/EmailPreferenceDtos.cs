using NeedApp.Domain.Enums;

namespace NeedApp.Application.DTOs.EmailPreference;

public class EmailPreferenceDto
{
    public bool OnAssignment { get; set; } = true;
    public bool OnStatusChange { get; set; } = true;
    public bool OnOverdue { get; set; } = true;
    public bool OnNewRequest { get; set; } = true;
    public DigestFrequency DigestFrequency { get; set; } = DigestFrequency.None;
}

public class UpdateEmailPreferenceRequest
{
    public bool OnAssignment { get; set; }
    public bool OnStatusChange { get; set; }
    public bool OnOverdue { get; set; }
    public bool OnNewRequest { get; set; }
    public DigestFrequency DigestFrequency { get; set; }
}
