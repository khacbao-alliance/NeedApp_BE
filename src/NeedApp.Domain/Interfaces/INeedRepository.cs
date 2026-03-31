using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Interfaces;

public interface INeedRepository : IRepository<Need>
{
    Task<IEnumerable<Need>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Need>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Need>> GetByStatusAsync(NeedStatus status, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Need> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
