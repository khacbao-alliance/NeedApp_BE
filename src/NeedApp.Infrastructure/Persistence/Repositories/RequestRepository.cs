using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;
using NeedApp.Infrastructure.Persistence;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class RequestRepository(AppDbContext context) : BaseRepository<Request>(context), IRequestRepository
{
    public async Task<IEnumerable<Request>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        => await DbSet.Where(r => r.ClientId == clientId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default)
        => await DbSet.Where(r => r.Status == status).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Request>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.Where(r => r.AssignedTo == userId).ToListAsync(cancellationToken);
}
