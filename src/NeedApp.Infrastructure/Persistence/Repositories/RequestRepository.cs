using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;
using NeedApp.Infrastructure.Persistence;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class RequestRepository(AppDbContext context) : BaseRepository<Request>(context), IRequestRepository
{
    public async Task<IEnumerable<Request>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(r => r.ClientId == clientId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(r => r.Status == status).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Request>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(r => r.AssignedTo == userId).ToListAsync(cancellationToken);

    public async Task<Request?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(r => r.Client)
            .Include(r => r.AssignedUser)
            .Include(r => r.Participants).ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(IEnumerable<Request> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        RequestStatus? status,
        RequestPriority? priority,
        Guid? currentUserId,
        UserRole? currentUserRole,
        Guid? currentClientId,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(r => r.Client)
            .Include(r => r.AssignedUser)
            .Include(r => r.Participants).ThenInclude(p => p.User)
            .AsQueryable();

        // Role-based filtering: Client only sees requests belonging to their Client company
        if (currentUserRole == UserRole.Client)
        {
            if (!currentClientId.HasValue)
            {
                // Safety guard: Client-role user with no client → return empty (e.g. kicked user with stale token)
                return (Enumerable.Empty<Request>(), 0);
            }
            query = query.Where(r => r.ClientId == currentClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r =>
                r.Title.Contains(search) ||
                (r.Description != null && r.Description.Contains(search)));

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
