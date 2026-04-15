using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdWithClientAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
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
        IQueryable<User> query;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = "%" + SearchHelper.RemoveDiacritics(search).ToLowerInvariant() + "%";
            var vnFrom = SearchHelper.SqlTranslateFrom;
            var vnTo = SearchHelper.SqlTranslateTo;

            query = Context.Users.FromSqlInterpolated(
                $@"SELECT * FROM users
                   WHERE translate(lower(email), {vnFrom}, {vnTo}) LIKE {normalizedSearch}
                      OR translate(lower(COALESCE(name, '')), {vnFrom}, {vnTo}) LIKE {normalizedSearch}")
                .AsNoTracking();
        }
        else
        {
            query = DbSet.AsNoTracking();
        }

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
