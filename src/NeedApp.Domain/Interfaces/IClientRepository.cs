using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<IEnumerable<Client>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
