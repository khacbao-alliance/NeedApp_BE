using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Interfaces;

public interface IRequestRepository : IRepository<Request>
{
    Task<IEnumerable<Request>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Request?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Request> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        RequestStatus? status,
        RequestPriority? priority,
        Guid? currentUserId,
        UserRole? currentUserRole,
        Guid? currentClientId,
        // ── Advanced filters ──
        Guid? assignedTo = null,
        Guid? clientId = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        bool? isOverdue = null,
        string? sortBy = null,
        CancellationToken cancellationToken = default);
}
