using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository(AppDbContext context) : BaseRepository<PasswordResetToken>(context), IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetValidTokenAsync(string otpCode, Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(t =>
            t.OtpCode == otpCode &&
            t.UserId == userId &&
            !t.IsUsed &&
            t.ExpiresAt > DateTime.UtcNow,
            cancellationToken);

    public async Task InvalidateAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await DbSet
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
            token.IsUsed = true;
    }
}
