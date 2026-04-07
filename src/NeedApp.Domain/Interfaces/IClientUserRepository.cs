using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IClientUserRepository : IRepository<ClientUser>
{
    Task<ClientUser?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientUser>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientUser?> GetByUserAndClientIdAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientUser?> GetByUserAndClientIdIncludeDeletedAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default);
}
