namespace NeedApp.Application.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string? userName, CancellationToken cancellationToken = default);
    Task SendPasswordResetOtpAsync(string toEmail, string? userName, string otpCode, CancellationToken cancellationToken = default);
    Task SendNotificationEmailAsync(string toEmail, string? userName, string title, string content, CancellationToken cancellationToken = default);
}
