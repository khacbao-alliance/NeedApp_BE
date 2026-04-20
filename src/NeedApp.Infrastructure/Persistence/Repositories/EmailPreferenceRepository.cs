using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class EmailPreferenceRepository(AppDbContext context)
    : BaseRepository<EmailPreference>(context), IEmailPreferenceRepository
{
    public async Task<EmailPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(ep => ep.UserId == userId, cancellationToken);

    public async Task<List<EmailPreference>> GetByUserIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToList();
        return await DbSet.Where(ep => ids.Contains(ep.UserId)).ToListAsync(cancellationToken);
    }

    public async Task<List<EmailPreference>> GetByDigestFrequencyAsync(
        DigestFrequency frequency, CancellationToken cancellationToken = default)
        => await DbSet.Include(ep => ep.User)
            .Where(ep => ep.DigestFrequency == frequency)
            .ToListAsync(cancellationToken);
}
