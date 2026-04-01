using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Exceptions;
using NeedApp.Infrastructure.Settings;

namespace NeedApp.Infrastructure.Services;

public class GoogleAuthService(IOptions<GoogleSettings> options) : IGoogleAuthService
{
    private readonly GoogleSettings _settings = options.Value;

    public async Task<GoogleUserPayload> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_settings.ClientId]
            });

            return new GoogleUserPayload(
                Subject: payload.Subject,
                Email: payload.Email,
                Name: payload.Name
            );
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException("Invalid Google token.");
        }
    }
}
