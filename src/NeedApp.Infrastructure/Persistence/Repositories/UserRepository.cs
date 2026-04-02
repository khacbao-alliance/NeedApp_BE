using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;
using NeedApp.Infrastructure.Persistence;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithClientAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(u => u.ClientUsers.Where(cu => !cu.IsDeleted))
                .ThenInclude(cu => cu.Client)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        UserRole? role,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.Email.Contains(search) ||
                (u.Name != null && u.Name.Contains(search)));

        if (role is not null)
            query = query.Where(u => u.Role == role);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
