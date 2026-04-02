using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        UserRole? role,
        CancellationToken cancellationToken = default);
}
