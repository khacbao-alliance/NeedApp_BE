namespace NeedApp.Application.Interfaces;

public interface IRecaptchaService
{
    Task<bool> VerifyTokenAsync(string token, CancellationToken cancellationToken = default);
}
