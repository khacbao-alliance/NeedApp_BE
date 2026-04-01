using Microsoft.EntityFrameworkCore;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(AppDbContext context) : BaseRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => await DbSet.Include(r => r.User)
                      .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await DbSet
            .Where(r => r.UserId == userId && r.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;
    }
}
