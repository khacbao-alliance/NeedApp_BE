using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;
using NeedApp.Infrastructure.Persistence;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class ClientRepository(AppDbContext context) : BaseRepository<Client>(context), IClientRepository
{
    public async Task<IEnumerable<Client>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(c => c.ClientUsers.Any(cu => cu.UserId == userId && !cu.IsDeleted))
            .ToListAsync(cancellationToken);
}
