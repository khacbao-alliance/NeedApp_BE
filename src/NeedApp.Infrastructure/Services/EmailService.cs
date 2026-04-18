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
        var subject = "Chào mừng bạn đến với NeedApp!";
        var body = BuildWelcomeEmailHtml(displayName);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Welcome email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string? userName, string otpCode, CancellationToken cancellationToken = default)
    {
        var displayName = userName ?? "bạn";
        var subject = "Mã OTP đặt lại mật khẩu - NeedApp";
        var body = BuildPasswordResetEmailHtml(displayName, otpCode);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Password reset OTP email sent to {Email}", toEmail);
    }

    public async Task SendNotificationEmailAsync(string toEmail, string? userName, string title, string content, CancellationToken cancellationToken = default)
    {
        var displayName = userName ?? "bạn";
        var subject = $"{title} - NeedApp";
        var body = BuildNotificationEmailHtml(displayName, title, content);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Notification email sent to {Email}: {Title}", toEmail, title);
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

    private string BuildEmailWrapper(string headerSubtitle, string bodyContent)
    {
        var logoUrl = _smtp.LogoUrl;
        var appUrl = _smtp.AppUrl;
        var year = DateTime.UtcNow.Year;

        return $"""
        <!DOCTYPE html>
        <html lang="vi">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>NeedApp</title>
        </head>
        <body style="margin:0;padding:0;background-color:#f0f4f8;-webkit-font-smoothing:antialiased;">
            <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="background-color:#f0f4f8;">
                <tr>
                    <td align="center" style="padding:32px 16px;">
                        <table role="presentation" cellpadding="0" cellspacing="0" width="600" style="max-width:600px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 16px rgba(0,0,0,0.06);">

                            <!-- Header with Logo -->
                            <tr>
                                <td style="background-color:#0d3b82;padding:28px 40px;text-align:center;">
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                        <tr>
                                            <td align="center">
                                                <img src="{logoUrl}" alt="NeedApp" width="180" style="display:block;max-width:180px;height:auto;" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="center" style="padding-top:12px;">
                                                <span style="font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:13px;color:rgba(255,255,255,0.75);letter-spacing:0.5px;">{headerSubtitle}</span>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                            <!-- Accent line -->
                            <tr>
                                <td style="height:3px;background:linear-gradient(90deg,#1a56db,#3b82f6,#60a5fa);font-size:0;line-height:0;">&nbsp;</td>
                            </tr>

                            <!-- Body Content -->
                            <tr>
                                <td style="padding:36px 40px;font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                    {bodyContent}
                                </td>
                            </tr>

                            <!-- Divider -->
                            <tr>
                                <td style="padding:0 40px;">
                                    <div style="height:1px;background-color:#e5e7eb;"></div>
                                </td>
                            </tr>

                            <!-- Footer -->
                            <tr>
                                <td style="padding:24px 40px 32px;text-align:center;font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                    <p style="margin:0 0 8px;font-size:13px;color:#6b7280;">
                                        Đây là email tự động từ hệ thống NeedApp.
                                    </p>
                                    <p style="margin:0 0 12px;font-size:13px;color:#6b7280;">
                                        Nếu cần hỗ trợ, vui lòng liên hệ qua ứng dụng hoặc trả lời email này.
                                    </p>
                                    <a href="{appUrl}" style="display:inline-block;font-size:13px;color:#1a56db;text-decoration:none;font-weight:600;">needapp.netlify.app</a>
                                    <p style="margin:16px 0 0;font-size:11px;color:#9ca3af;">
                                        &copy; {year} NeedApp &mdash; Client Requirement Management. All rights reserved.
                                    </p>
                                </td>
                            </tr>

                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }

    private string BuildWelcomeEmailHtml(string displayName)
    {
        var appUrl = _smtp.AppUrl;
        var bodyContent = $"""
                                    <h2 style="margin:0 0 8px;font-size:22px;color:#111827;font-weight:700;">Chào mừng đến với NeedApp!</h2>
                                    <p style="margin:0 0 24px;font-size:15px;color:#6b7280;line-height:1.5;">Xin chào <strong style="color:#111827;">{displayName}</strong>,</p>

                                    <p style="margin:0 0 16px;font-size:15px;color:#374151;line-height:1.7;">
                                        Tài khoản của bạn đã được tạo thành công. Bạn hiện có thể sử dụng đầy đủ các tính năng
                                        quản lý yêu cầu trên NeedApp.
                                    </p>

                                    <!-- Feature cards -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:24px 0;">
                                        <tr>
                                            <td style="background-color:#eff6ff;border-radius:8px;padding:20px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td width="36" valign="top" style="padding-right:12px;">
                                                            <div style="width:32px;height:32px;background-color:#1a56db;border-radius:8px;text-align:center;line-height:32px;font-size:16px;color:#ffffff;">&#9998;</div>
                                                        </td>
                                                        <td style="font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                                            <p style="margin:0;font-size:14px;font-weight:600;color:#1e40af;">Tạo yêu cầu</p>
                                                            <p style="margin:4px 0 0;font-size:13px;color:#6b7280;line-height:1.5;">Gửi yêu cầu mới từ dashboard một cách nhanh chóng.</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr><td style="height:12px;"></td></tr>
                                        <tr>
                                            <td style="background-color:#eff6ff;border-radius:8px;padding:20px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td width="36" valign="top" style="padding-right:12px;">
                                                            <div style="width:32px;height:32px;background-color:#1a56db;border-radius:8px;text-align:center;line-height:32px;font-size:16px;color:#ffffff;">&#128172;</div>
                                                        </td>
                                                        <td style="font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                                            <p style="margin:0;font-size:14px;font-weight:600;color:#1e40af;">Chat trực tiếp</p>
                                                            <p style="margin:4px 0 0;font-size:13px;color:#6b7280;line-height:1.5;">Trao đổi trực tiếp với đội ngũ xử lý yêu cầu.</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr><td style="height:12px;"></td></tr>
                                        <tr>
                                            <td style="background-color:#eff6ff;border-radius:8px;padding:20px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td width="36" valign="top" style="padding-right:12px;">
                                                            <div style="width:32px;height:32px;background-color:#1a56db;border-radius:8px;text-align:center;line-height:32px;font-size:16px;color:#ffffff;">&#128276;</div>
                                                        </td>
                                                        <td style="font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                                            <p style="margin:0;font-size:14px;font-weight:600;color:#1e40af;">Theo dõi & thông báo</p>
                                                            <p style="margin:4px 0 0;font-size:13px;color:#6b7280;line-height:1.5;">Nhận cập nhật tức thì về trạng thái xử lý.</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- CTA Button -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:28px 0 0;">
                                        <tr>
                                            <td align="center">
                                                <a href="{appUrl}" style="display:inline-block;background-color:#1a56db;color:#ffffff;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:15px;font-weight:600;padding:14px 36px;border-radius:8px;text-decoration:none;letter-spacing:0.3px;">
                                                    Truy cập NeedApp
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
        """;

        return BuildEmailWrapper("Client Requirement Management", bodyContent);
    }

    private string BuildPasswordResetEmailHtml(string displayName, string otpCode)
    {
        var bodyContent = $"""
                                    <h2 style="margin:0 0 8px;font-size:22px;color:#111827;font-weight:700;">Đặt lại mật khẩu</h2>
                                    <p style="margin:0 0 24px;font-size:15px;color:#6b7280;line-height:1.5;">Xin chào <strong style="color:#111827;">{displayName}</strong>,</p>

                                    <p style="margin:0 0 16px;font-size:15px;color:#374151;line-height:1.7;">
                                        Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.
                                        Vui lòng sử dụng mã OTP bên dưới để xác nhận:
                                    </p>

                                    <!-- OTP Code -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:28px 0;">
                                        <tr>
                                            <td align="center">
                                                <table role="presentation" cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td style="background-color:#0d3b82;border-radius:12px;padding:20px 48px;">
                                                            <span style="font-family:'Courier New',Consolas,monospace;font-size:36px;font-weight:800;color:#ffffff;letter-spacing:10px;">{otpCode}</span>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- Warning box -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:0 0 20px;">
                                        <tr>
                                            <td style="background-color:#fef9ec;border:1px solid #f5d78e;border-radius:8px;padding:16px 20px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td width="24" valign="top" style="padding-right:10px;font-size:16px;">&#9200;</td>
                                                        <td style="font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                                            <p style="margin:0;font-size:13px;color:#92400e;line-height:1.6;">
                                                                <strong>Mã OTP có hiệu lực trong 15 phút.</strong><br/>
                                                                Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
                                                            </p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- Security note -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:0;">
                                        <tr>
                                            <td style="background-color:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:16px 20px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td width="24" valign="top" style="padding-right:10px;font-size:16px;">&#128274;</td>
                                                        <td style="font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                                            <p style="margin:0;font-size:13px;color:#991b1b;line-height:1.6;">
                                                                <strong>Lưu ý bảo mật:</strong> Không chia sẻ mã OTP này với bất kỳ ai. NeedApp sẽ không bao giờ yêu cầu bạn cung cấp mã qua điện thoại hoặc tin nhắn.
                                                            </p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
        """;

        return BuildEmailWrapper("Xác nhận đặt lại mật khẩu", bodyContent);
    }

    private string BuildNotificationEmailHtml(string displayName, string title, string content)
    {
        var appUrl = _smtp.AppUrl;
        var bodyContent = $"""
                                    <h2 style="margin:0 0 8px;font-size:22px;color:#111827;font-weight:700;">Thông báo mới</h2>
                                    <p style="margin:0 0 24px;font-size:15px;color:#6b7280;line-height:1.5;">Xin chào <strong style="color:#111827;">{displayName}</strong>,</p>

                                    <p style="margin:0 0 20px;font-size:15px;color:#374151;line-height:1.7;">
                                        Bạn có một thông báo mới từ hệ thống NeedApp:
                                    </p>

                                    <!-- Notification card -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:0 0 24px;">
                                        <tr>
                                            <td style="border-left:4px solid #1a56db;background-color:#f8fafc;border-radius:0 8px 8px 0;padding:20px 24px;">
                                                <p style="margin:0 0 8px;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:16px;font-weight:700;color:#0d3b82;">{title}</p>
                                                <p style="margin:0;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:14px;color:#4b5563;line-height:1.7;">{content}</p>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- CTA Button -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:4px 0 0;">
                                        <tr>
                                            <td align="center">
                                                <a href="{appUrl}" style="display:inline-block;background-color:#1a56db;color:#ffffff;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:15px;font-weight:600;padding:14px 36px;border-radius:8px;text-decoration:none;letter-spacing:0.3px;">
                                                    Xem chi tiết
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
        """;

        return BuildEmailWrapper("Thông báo", bodyContent);
    }

    // ═══════════════════════════════════════════════════════════════
    // NEW: Request Assigned Email
    // ═══════════════════════════════════════════════════════════════

    public async Task SendRequestAssignedEmailAsync(
        string toEmail, string? userName, string requestTitle, Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var displayName = userName ?? "bạn";
        var subject = $"Yêu cầu mới được gán cho bạn — {requestTitle}";
        var body = BuildRequestAssignedHtml(displayName, requestTitle, requestId);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Request assigned email sent to {Email} for request {RequestId}", toEmail, requestId);
    }

    private string BuildRequestAssignedHtml(string displayName, string requestTitle, Guid requestId)
    {
        var appUrl = _smtp.AppUrl;
        var requestUrl = $"{appUrl}/requests/{requestId}";
        var bodyContent = $"""
                                    <h2 style="margin:0 0 8px;font-size:22px;color:#111827;font-weight:700;">Yêu cầu mới được gán</h2>
                                    <p style="margin:0 0 24px;font-size:15px;color:#6b7280;line-height:1.5;">Xin chào <strong style="color:#111827;">{displayName}</strong>,</p>

                                    <p style="margin:0 0 20px;font-size:15px;color:#374151;line-height:1.7;">
                                        Bạn vừa được gán xử lý yêu cầu sau:
                                    </p>

                                    <!-- Request card -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:0 0 24px;">
                                        <tr>
                                            <td style="background-color:#eff6ff;border-radius:10px;padding:24px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td width="40" valign="top" style="padding-right:14px;">
                                                            <div style="width:36px;height:36px;background-color:#1a56db;border-radius:10px;text-align:center;line-height:36px;font-size:18px;color:#ffffff;">&#128203;</div>
                                                        </td>
                                                        <td style="font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                                            <p style="margin:0;font-size:10px;font-weight:600;color:#6b7280;text-transform:uppercase;letter-spacing:1px;">YÊU CẦU</p>
                                                            <p style="margin:4px 0 0;font-size:17px;font-weight:700;color:#1e40af;line-height:1.4;">{requestTitle}</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- CTA -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:4px 0 0;">
                                        <tr>
                                            <td align="center">
                                                <a href="{requestUrl}" style="display:inline-block;background-color:#1a56db;color:#ffffff;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:15px;font-weight:600;padding:14px 36px;border-radius:8px;text-decoration:none;letter-spacing:0.3px;">
                                                    Xem yêu cầu
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
        """;

        return BuildEmailWrapper("Yêu cầu được gán", bodyContent);
    }

    // ═══════════════════════════════════════════════════════════════
    // NEW: Overdue Alert Email
    // ═══════════════════════════════════════════════════════════════

    public async Task SendOverdueAlertEmailAsync(
        string toEmail, string? userName, string requestTitle, DateTime dueDate, Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var displayName = userName ?? "bạn";
        var subject = $"⚠️ Yêu cầu quá hạn — {requestTitle}";
        var body = BuildOverdueAlertHtml(displayName, requestTitle, dueDate, requestId);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Overdue alert email sent to {Email} for request {RequestId}", toEmail, requestId);
    }

    private string BuildOverdueAlertHtml(string displayName, string requestTitle, DateTime dueDate, Guid requestId)
    {
        var appUrl = _smtp.AppUrl;
        var requestUrl = $"{appUrl}/requests/{requestId}";
        var overdueSince = (DateTime.UtcNow - dueDate).TotalHours;
        var overdueLabel = overdueSince < 24
            ? $"{Math.Round(overdueSince)} giờ"
            : $"{Math.Round(overdueSince / 24)} ngày";

        var bodyContent = $"""
                                    <h2 style="margin:0 0 8px;font-size:22px;color:#dc2626;font-weight:700;">⚠️ Yêu cầu quá hạn</h2>
                                    <p style="margin:0 0 24px;font-size:15px;color:#6b7280;line-height:1.5;">Xin chào <strong style="color:#111827;">{displayName}</strong>,</p>

                                    <p style="margin:0 0 20px;font-size:15px;color:#374151;line-height:1.7;">
                                        Yêu cầu dưới đây đã <strong style="color:#dc2626;">quá hạn {overdueLabel}</strong> và cần được xử lý ngay:
                                    </p>

                                    <!-- Warning card -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:0 0 24px;">
                                        <tr>
                                            <td style="background-color:#fef2f2;border:1px solid #fecaca;border-radius:10px;padding:24px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td width="40" valign="top" style="padding-right:14px;">
                                                            <div style="width:36px;height:36px;background-color:#dc2626;border-radius:10px;text-align:center;line-height:36px;font-size:18px;color:#ffffff;">&#128293;</div>
                                                        </td>
                                                        <td style="font-family:'Segoe UI',Roboto,Arial,sans-serif;">
                                                            <p style="margin:0;font-size:10px;font-weight:600;color:#991b1b;text-transform:uppercase;letter-spacing:1px;">QUÁ HẠN</p>
                                                            <p style="margin:4px 0 0;font-size:17px;font-weight:700;color:#991b1b;line-height:1.4;">{requestTitle}</p>
                                                            <p style="margin:6px 0 0;font-size:13px;color:#b91c1c;">Deadline: {dueDate:dd/MM/yyyy HH:mm} UTC</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- CTA -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:4px 0 0;">
                                        <tr>
                                            <td align="center">
                                                <a href="{requestUrl}" style="display:inline-block;background-color:#dc2626;color:#ffffff;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:15px;font-weight:600;padding:14px 36px;border-radius:8px;text-decoration:none;letter-spacing:0.3px;">
                                                    Xử lý ngay
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
        """;

        return BuildEmailWrapper("Cảnh báo quá hạn", bodyContent);
    }

    // ═══════════════════════════════════════════════════════════════
    // NEW: Digest Email
    // ═══════════════════════════════════════════════════════════════

    public async Task SendDigestEmailAsync(
        string toEmail, string? userName, string period, List<DigestItem> items,
        CancellationToken cancellationToken = default)
    {
        var displayName = userName ?? "bạn";
        var subject = $"📊 Tóm tắt {period} — NeedApp";
        var body = BuildDigestHtml(displayName, period, items);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
        logger.LogInformation("Digest email ({Period}) sent to {Email} with {Count} items", period, toEmail, items.Count);
    }

    private string BuildDigestHtml(string displayName, string period, List<DigestItem> items)
    {
        var appUrl = _smtp.AppUrl;
        var overdueCount = items.Count(i => i.IsOverdue);
        var openCount = items.Count(i => !i.IsOverdue);

        // Build table rows
        var rows = string.Join("", items.Select(item =>
        {
            var statusColor = item.IsOverdue ? "#dc2626" : item.Status switch
            {
                "InProgress" => "#2563eb",
                "MissingInfo" => "#d97706",
                "Pending" => "#6b7280",
                _ => "#374151"
            };
            var statusLabel = item.IsOverdue ? "Quá hạn" : item.Status switch
            {
                "InProgress" => "Đang xử lý",
                "MissingInfo" => "Cần bổ sung",
                "Pending" => "Chờ xử lý",
                "Done" => "Hoàn tất",
                _ => item.Status
            };
            var priorityEmoji = item.Priority switch
            {
                "Urgent" => "🔴",
                "High" => "🟠",
                "Medium" => "🔵",
                "Low" => "🟢",
                _ => "⚪"
            };

            return $"""
                <tr>
                    <td style="padding:10px 12px;border-bottom:1px solid #f3f4f6;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:13px;color:#374151;">
                        <a href="{appUrl}/requests/{item.RequestId}" style="color:#1a56db;text-decoration:none;font-weight:600;">{item.Title}</a>
                    </td>
                    <td style="padding:10px 12px;border-bottom:1px solid #f3f4f6;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:12px;text-align:center;">
                        <span style="display:inline-block;background-color:{statusColor}15;color:{statusColor};padding:2px 8px;border-radius:20px;font-weight:600;font-size:11px;">{statusLabel}</span>
                    </td>
                    <td style="padding:10px 12px;border-bottom:1px solid #f3f4f6;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:13px;text-align:center;">
                        {priorityEmoji}
                    </td>
                </tr>
            """;
        }));

        var bodyContent = $"""
                                    <h2 style="margin:0 0 8px;font-size:22px;color:#111827;font-weight:700;">📊 Tóm tắt {period}</h2>
                                    <p style="margin:0 0 24px;font-size:15px;color:#6b7280;line-height:1.5;">Xin chào <strong style="color:#111827;">{displayName}</strong>,</p>

                                    <p style="margin:0 0 20px;font-size:15px;color:#374151;line-height:1.7;">
                                        Đây là bản tóm tắt các yêu cầu bạn đang phụ trách:
                                    </p>

                                    <!-- Stats -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:0 0 24px;">
                                        <tr>
                                            <td width="50%" style="padding-right:6px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td style="background-color:#eff6ff;border-radius:8px;padding:16px;text-align:center;">
                                                            <p style="margin:0;font-size:28px;font-weight:800;color:#1a56db;">{openCount}</p>
                                                            <p style="margin:4px 0 0;font-size:12px;color:#6b7280;font-weight:600;">Đang mở</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td width="50%" style="padding-left:6px;">
                                                <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                                                    <tr>
                                                        <td style="background-color:#fef2f2;border-radius:8px;padding:16px;text-align:center;">
                                                            <p style="margin:0;font-size:28px;font-weight:800;color:#dc2626;">{overdueCount}</p>
                                                            <p style="margin:4px 0 0;font-size:12px;color:#6b7280;font-weight:600;">Quá hạn</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- Requests table -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:0 0 24px;border:1px solid #e5e7eb;border-radius:8px;overflow:hidden;">
                                        <tr style="background-color:#f9fafb;">
                                            <th style="padding:10px 12px;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:11px;font-weight:700;color:#6b7280;text-align:left;text-transform:uppercase;letter-spacing:0.5px;">Yêu cầu</th>
                                            <th style="padding:10px 12px;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:11px;font-weight:700;color:#6b7280;text-align:center;text-transform:uppercase;letter-spacing:0.5px;">Trạng thái</th>
                                            <th style="padding:10px 12px;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:11px;font-weight:700;color:#6b7280;text-align:center;text-transform:uppercase;letter-spacing:0.5px;">Ưu tiên</th>
                                        </tr>
                                        {rows}
                                    </table>

                                    <!-- CTA -->
                                    <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:4px 0 0;">
                                        <tr>
                                            <td align="center">
                                                <a href="{appUrl}/requests" style="display:inline-block;background-color:#1a56db;color:#ffffff;font-family:'Segoe UI',Roboto,Arial,sans-serif;font-size:15px;font-weight:600;padding:14px 36px;border-radius:8px;text-decoration:none;letter-spacing:0.3px;">
                                                    Xem tất cả yêu cầu
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
        """;

        return BuildEmailWrapper($"Tóm tắt {period}", bodyContent);
    }
}
