namespace NeedApp.Application.Interfaces;

public record GoogleUserPayload(string Subject, string Email, string? Name, string? Picture);

public interface IGoogleAuthService
{
    Task<GoogleUserPayload> VerifyIdTokenAsync(string idToken);
}
