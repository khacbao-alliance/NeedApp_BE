namespace NeedApp.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string SecretKey { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
