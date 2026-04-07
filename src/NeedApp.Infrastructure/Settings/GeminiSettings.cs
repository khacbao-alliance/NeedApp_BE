namespace NeedApp.Infrastructure.Settings;

public class GeminiSettings
{
    public const string SectionName = "GeminiSettings";
    public string ApiKey { get; set; } = default!;
    public string Model { get; set; } = "gemini-2.5-flash-lite";
}
