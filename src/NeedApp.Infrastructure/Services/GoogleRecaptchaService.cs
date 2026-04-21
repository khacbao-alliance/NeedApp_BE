using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NeedApp.Application.Interfaces;

namespace NeedApp.Infrastructure.Services;

public class GoogleRecaptchaService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<GoogleRecaptchaService> logger) : IRecaptchaService
{
    private readonly string _secretKey = configuration["Recaptcha:SecretKey"] ?? "";

    public async Task<bool> VerifyTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            logger.LogWarning("Recaptcha SecretKey is not configured. Failing open (returning true) for development.");
            return true;
        }

        try
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _secretKey),
                new KeyValuePair<string, string>("response", token)
            });

            var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>(cancellationToken: cancellationToken);
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying Recaptcha token.");
            return false;
        }
    }

    private class RecaptchaResponse
    {
        public bool Success { get; set; }
    }
}
