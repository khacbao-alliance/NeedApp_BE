using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class ClientUserRepository(AppDbContext context)
    : BaseRepository<ClientUser>(context), IClientUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<ClientUser?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.ClientUsers.FirstOrDefaultAsync(cu => cu.UserId == userId, cancellationToken);

    public async Task<IEnumerable<ClientUser>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        => await _context.ClientUsers
            .Include(cu => cu.User)
            .Where(cu => cu.ClientId == clientId && !cu.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<ClientUser?> GetByUserAndClientIdAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default)
        => await _context.ClientUsers
            .Include(cu => cu.Client)
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ClientId == clientId && !cu.IsDeleted, cancellationToken);

    public async Task<ClientUser?> GetByUserAndClientIdIncludeDeletedAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default)
        => await _context.ClientUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.ClientId == clientId, cancellationToken);
}
