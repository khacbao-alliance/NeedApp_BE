using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Interfaces;

public interface IRequestRepository : IRepository<Request>
{
    Task<IEnumerable<Request>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
