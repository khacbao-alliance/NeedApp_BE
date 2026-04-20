using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    Task<(IEnumerable<Message> Items, bool HasMore)> GetByRequestIdAsync(
        Guid requestId,
        DateTime? cursorDate,
        Guid? cursorId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<int> GetCountByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, int>> GetCountsByRequestIdsAsync(
        IEnumerable<Guid> requestIds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Message>> GetByTypeAsync(
        Guid requestId,
        MessageType type,
        CancellationToken cancellationToken = default);

    Task<List<Message>> GetAllByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<List<Message>> SearchAsync(
        Guid requestId,
        string query,
        int limit = 50,
        CancellationToken cancellationToken = default);
}
