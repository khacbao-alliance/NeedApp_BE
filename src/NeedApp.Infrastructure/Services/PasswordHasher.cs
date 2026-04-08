using NeedApp.Application.Interfaces;

namespace NeedApp.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public Task<string> HashAsync(string password) =>
        Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password));

    public Task<bool> VerifyAsync(string password, string hash) =>
        Task.Run(() => BCrypt.Net.BCrypt.Verify(password, hash));
}
