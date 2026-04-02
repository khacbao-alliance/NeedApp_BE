using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IRequestParticipantRepository : IRepository<RequestParticipant>
{
    Task<bool> IsParticipantAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RequestParticipant>> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
}
