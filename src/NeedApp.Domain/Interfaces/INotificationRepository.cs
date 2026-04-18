using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadByReferenceAsync(Guid userId, Guid referenceId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetUnreadCountsByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<Notification?> GetByUserAndReferenceAsync(Guid userId, Guid referenceId, string referenceType, CancellationToken cancellationToken = default);
}
