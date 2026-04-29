using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class MessageEditHistoryRepository(AppDbContext context) : IMessageEditHistoryRepository
{
    public async Task AddAsync(MessageEditHistory entry, CancellationToken cancellationToken = default)
    {
        await context.MessageEditHistories.AddAsync(entry, cancellationToken);
    }

    public async Task<List<MessageEditHistory>> GetByMessageIdAsync(
        Guid messageId, CancellationToken cancellationToken = default)
    {
        return await context.MessageEditHistories
            .AsNoTracking()
            .Where(h => h.MessageId == messageId)
            .Include(h => h.Editor)
            .OrderByDescending(h => h.EditedAt)
            .ToListAsync(cancellationToken);
    }
}
