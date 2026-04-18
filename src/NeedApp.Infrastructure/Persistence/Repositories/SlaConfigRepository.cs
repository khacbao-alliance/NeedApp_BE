using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class SlaConfigRepository(AppDbContext context)
    : BaseRepository<SlaConfig>(context), ISlaConfigRepository
{
    public async Task<SlaConfig?> GetByPriorityAsync(RequestPriority priority, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Priority == priority, cancellationToken);

    public async Task<IEnumerable<SlaConfig>> GetAllConfigsAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().OrderBy(x => x.Priority).ToListAsync(cancellationToken);
}
