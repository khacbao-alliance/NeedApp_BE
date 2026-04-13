using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IInvitationRepository : IRepository<Invitation>
{
    Task<Invitation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invitation?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Invitation>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Invitation?> GetPendingByUserAndClientIdAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default);
}
