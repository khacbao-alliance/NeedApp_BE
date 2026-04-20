namespace NeedApp.Application.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string? userName, CancellationToken cancellationToken = default);
    Task SendPasswordResetOtpAsync(string toEmail, string? userName, string otpCode, CancellationToken cancellationToken = default);
    Task SendNotificationEmailAsync(string toEmail, string? userName, string title, string content, CancellationToken cancellationToken = default);

    // ── New email types ──

    /// <summary>Email when a request is assigned to a staff member.</summary>
    Task SendRequestAssignedEmailAsync(string toEmail, string? userName, string requestTitle, Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>Email alert when a request becomes overdue.</summary>
    Task SendOverdueAlertEmailAsync(string toEmail, string? userName, string requestTitle, DateTime dueDate, Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>Digest email summarizing request activity.</summary>
    Task SendDigestEmailAsync(string toEmail, string? userName, string period, List<DigestItem> items, CancellationToken cancellationToken = default);
}

/// <summary>Single item in a digest email.</summary>
public class DigestItem
{
    public string Title { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Priority { get; set; } = default!;
    public bool IsOverdue { get; set; }
    public Guid RequestId { get; set; }
}
