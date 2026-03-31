using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class NeedRepository(AppDbContext context) : BaseRepository<Need>(context), INeedRepository
{
    public async Task<IEnumerable<Need>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.Include(n => n.User).Include(n => n.Category)
            .Where(n => n.UserId == userId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Need>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await DbSet.Include(n => n.User).Include(n => n.Category)
            .Where(n => n.CategoryId == categoryId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Need>> GetByStatusAsync(NeedStatus status, CancellationToken cancellationToken = default)
        => await DbSet.Include(n => n.User).Include(n => n.Category)
            .Where(n => n.Status == status).ToListAsync(cancellationToken);

    public async Task<(IEnumerable<Need> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(n => n.User).Include(n => n.Category).OrderByDescending(n => n.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
