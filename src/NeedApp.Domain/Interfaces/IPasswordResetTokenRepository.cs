using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetValidTokenAsync(string otpCode, Guid userId, CancellationToken cancellationToken = default);
    Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
