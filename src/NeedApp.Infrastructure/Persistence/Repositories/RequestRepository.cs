using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

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
            .IgnoreQueryFilters()
            .Where(r => !r.IsDeleted)
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
        Guid? assignedTo = null,
        Guid? clientId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        bool? isOverdue = null,
        string? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Request> query;

        // If search is provided, start from a raw SQL base that applies
        // translate(lower(...)) for Vietnamese diacritics-insensitive matching.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = "%" + SearchHelper.RemoveDiacritics(search).ToLowerInvariant() + "%";
            var vnFrom = SearchHelper.SqlTranslateFrom;
            var vnTo = SearchHelper.SqlTranslateTo;

            query = Context.Requests.FromSqlInterpolated(
                $@"SELECT * FROM requests
                   WHERE is_deleted = false
                     AND (translate(lower(title), {vnFrom}, {vnTo}) LIKE {normalizedSearch}
                      OR translate(lower(COALESCE(description, '')), {vnFrom}, {vnTo}) LIKE {normalizedSearch})")
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(r => r.Client)
                .Include(r => r.AssignedUser)
                .Include(r => r.Participants).ThenInclude(p => p.User);
        }
        else
        {
            query = DbSet.AsNoTracking()
                .IgnoreQueryFilters()
                .Where(r => !r.IsDeleted)
                .Include(r => r.Client)
                .Include(r => r.AssignedUser)
                .Include(r => r.Participants).ThenInclude(p => p.User);
        }

        // Role-based filtering: Client only sees requests belonging to their Client company
        if (currentUserRole == UserRole.Client)
        {
            if (!currentClientId.HasValue)
            {
                return (Enumerable.Empty<Request>(), 0);
            }
            query = query.Where(r => r.ClientId == currentClientId.Value);
        }

        // ── Standard filters ──
        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        // ── Advanced filters ──
        if (assignedTo.HasValue)
            query = query.Where(r => r.AssignedTo == assignedTo.Value);

        if (clientId.HasValue)
            query = query.Where(r => r.ClientId == clientId.Value);

        if (dateFrom.HasValue)
            query = query.Where(r => r.CreatedAt >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(r => r.CreatedAt <= dateTo.Value);

        if (isOverdue == true)
            query = query.Where(r => r.DueDate.HasValue && r.DueDate.Value < DateTime.UtcNow
                && r.Status != RequestStatus.Done && r.Status != RequestStatus.Cancelled);

        var totalCount = await query.CountAsync(cancellationToken);

        // ── Sorting ──
        IOrderedQueryable<Request> orderedQuery = sortBy?.ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(r => r.CreatedAt),
            "priority" => query.OrderByDescending(r => r.Priority).ThenByDescending(r => r.CreatedAt),
            "duedate" => query.OrderBy(r => r.DueDate ?? DateTime.MaxValue).ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt), // "newest" or default
        };

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
