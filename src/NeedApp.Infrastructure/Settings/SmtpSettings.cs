namespace NeedApp.Infrastructure.Settings;

public class SmtpSettings
{
    public const string SectionName = "SmtpSettings";
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string SenderEmail { get; set; } = default!;
    public string SenderName { get; set; } = "NeedApp";
    public string AppPassword { get; set; } = default!;
    public bool EnableSsl { get; set; } = true;
    public string LogoUrl { get; set; } = "";
    public string AppUrl { get; set; } = "";
}
