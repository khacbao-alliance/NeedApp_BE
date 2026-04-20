using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Interfaces;

public interface ISlaConfigRepository : IRepository<SlaConfig>
{
    Task<SlaConfig?> GetByPriorityAsync(RequestPriority priority, CancellationToken cancellationToken = default);
    Task<IEnumerable<SlaConfig>> GetAllConfigsAsync(CancellationToken cancellationToken = default);
}
