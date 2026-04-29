using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IMessageEditHistoryRepository
{
    Task AddAsync(MessageEditHistory entry, CancellationToken cancellationToken = default);
    Task<List<MessageEditHistory>> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default);
}
