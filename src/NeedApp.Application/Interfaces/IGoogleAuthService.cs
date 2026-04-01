namespace NeedApp.Application.Interfaces;

public record GoogleUserPayload(string Subject, string Email, string? Name);

public interface IGoogleAuthService
{
    Task<GoogleUserPayload> VerifyIdTokenAsync(string idToken);
}
