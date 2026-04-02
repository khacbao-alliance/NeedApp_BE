using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeedApp.Application.Interfaces;
using NeedApp.Infrastructure.Settings;

namespace NeedApp.Infrastructure.Services;

public class EmailService(IOptions<SmtpSettings> smtpOptions, ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public async Task SendWelcomeEmailAsync(string toEmail, string? userName, CancellationToken cancellationToken = default)
    {
        var displayName = userName ?? "bạn";
        var subject = "🎉 Chào mừng bạn đến với NeedApp!";
        var body = BuildWelcomeEmailHtml(displayName);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Welcome email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string? userName, string otpCode, CancellationToken cancellationToken = default)
    {
        var displayName = userName ?? "bạn";
        var subject = "🔐 Mã OTP đặt lại mật khẩu - NeedApp";
        var body = BuildPasswordResetEmailHtml(displayName, otpCode);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Password reset OTP email sent to {Email}", toEmail);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        using var smtpClient = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            Credentials = new NetworkCredential(_smtp.SenderEmail, _smtp.AppPassword),
            EnableSsl = _smtp.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.SenderEmail, _smtp.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private static string BuildWelcomeEmailHtml(string displayName)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;font-family:'Segoe UI',Roboto,Arial,sans-serif;background-color:#f4f7fa;">
            <div style="max-width:600px;margin:40px auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">
                <!-- Header -->
                <div style="background:linear-gradient(135deg,#6366f1,#8b5cf6,#a855f7);padding:40px 32px;text-align:center;">
                    <h1 style="color:#ffffff;margin:0;font-size:28px;font-weight:700;">NeedApp</h1>
                    <p style="color:rgba(255,255,255,0.9);margin:8px 0 0;font-size:14px;">Client Requirement Management</p>
                </div>

                <!-- Body -->
                <div style="padding:40px 32px;">
                    <h2 style="color:#1e293b;margin:0 0 16px;font-size:22px;">Xin chào {displayName}! 👋</h2>
                    <p style="color:#475569;font-size:15px;line-height:1.7;margin:0 0 20px;">
                        Chúc mừng bạn đã tạo tài khoản thành công trên <strong>NeedApp</strong>!
                    </p>
                    <p style="color:#475569;font-size:15px;line-height:1.7;margin:0 0 20px;">
                        NeedApp giúp bạn quản lý yêu cầu một cách hiệu quả với hệ thống chat trực tiếp,
                        theo dõi trạng thái và nhận thông báo kịp thời.
                    </p>

                    <div style="background:#f8fafc;border-radius:12px;padding:20px;margin:24px 0;">
                        <p style="color:#334155;font-size:14px;font-weight:600;margin:0 0 12px;">🚀 Bắt đầu ngay:</p>
                        <ul style="color:#475569;font-size:14px;line-height:2;margin:0;padding-left:20px;">
                            <li>Tạo yêu cầu mới từ dashboard</li>
                            <li>Chat trực tiếp với đội ngũ hỗ trợ</li>
                            <li>Theo dõi tiến độ xử lý</li>
                        </ul>
                    </div>

                    <p style="color:#475569;font-size:15px;line-height:1.7;margin:0;">
                        Nếu cần hỗ trợ, hãy liên hệ với chúng tôi bất cứ lúc nào.
                    </p>
                </div>

                <!-- Footer -->
                <div style="background:#f8fafc;padding:24px 32px;text-align:center;border-top:1px solid #e2e8f0;">
                    <p style="color:#94a3b8;font-size:12px;margin:0;">
                        © {DateTime.UtcNow.Year} NeedApp. All rights reserved.
                    </p>
                </div>
            </div>
        </body>
        </html>
        """;
    }

    private static string BuildPasswordResetEmailHtml(string displayName, string otpCode)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;font-family:'Segoe UI',Roboto,Arial,sans-serif;background-color:#f4f7fa;">
            <div style="max-width:600px;margin:40px auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">
                <!-- Header -->
                <div style="background:linear-gradient(135deg,#ef4444,#f97316,#eab308);padding:40px 32px;text-align:center;">
                    <h1 style="color:#ffffff;margin:0;font-size:28px;font-weight:700;">NeedApp</h1>
                    <p style="color:rgba(255,255,255,0.9);margin:8px 0 0;font-size:14px;">Đặt lại mật khẩu</p>
                </div>

                <!-- Body -->
                <div style="padding:40px 32px;">
                    <h2 style="color:#1e293b;margin:0 0 16px;font-size:22px;">Xin chào {displayName}! 🔐</h2>
                    <p style="color:#475569;font-size:15px;line-height:1.7;margin:0 0 20px;">
                        Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
                        Sử dụng mã OTP bên dưới để hoàn tất:
                    </p>

                    <!-- OTP Box -->
                    <div style="text-align:center;margin:32px 0;">
                        <div style="display:inline-block;background:linear-gradient(135deg,#6366f1,#8b5cf6);border-radius:16px;padding:24px 48px;">
                            <span style="font-size:36px;font-weight:800;color:#ffffff;letter-spacing:12px;font-family:'Courier New',monospace;">
                                {otpCode}
                            </span>
                        </div>
                    </div>

                    <div style="background:#fef3c7;border:1px solid #fbbf24;border-radius:12px;padding:16px;margin:24px 0;">
                        <p style="color:#92400e;font-size:14px;margin:0;">
                            ⏰ <strong>Mã OTP có hiệu lực trong 15 phút.</strong><br>
                            Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                        </p>
                    </div>

                    <p style="color:#475569;font-size:15px;line-height:1.7;margin:0;">
                        Vì lý do bảo mật, không chia sẻ mã OTP này với bất kỳ ai.
                    </p>
                </div>

                <!-- Footer -->
                <div style="background:#f8fafc;padding:24px 32px;text-align:center;border-top:1px solid #e2e8f0;">
                    <p style="color:#94a3b8;font-size:12px;margin:0;">
                        © {DateTime.UtcNow.Year} NeedApp. All rights reserved.
                    </p>
                </div>
            </div>
        </body>
        </html>
        """;
    }
}
