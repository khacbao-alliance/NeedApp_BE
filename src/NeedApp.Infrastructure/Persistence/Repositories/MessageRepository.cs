using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class MessageRepository(AppDbContext context)
    : BaseRepository<Message>(context), IMessageRepository
{
    private readonly AppDbContext _context = context;

    public async Task<(IEnumerable<Message> Items, bool HasMore)> GetByRequestIdAsync(
        Guid requestId, DateTime? cursorDate, Guid? cursorId, int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Messages.AsNoTracking()
            .Where(m => m.RequestId == requestId)
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .Include(m => m.Reactions)
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .AsQueryable();

        if (cursorDate.HasValue && cursorId.HasValue)
        {
            query = query.Where(m =>
                m.CreatedAt > cursorDate.Value ||
                (m.CreatedAt == cursorDate.Value && m.Id.CompareTo(cursorId.Value) > 0));
        }

        var items = await query.Take(limit + 1).ToListAsync(cancellationToken);
        var hasMore = items.Count > limit;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return (items, hasMore);
    }

    public async Task<int> GetCountByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        => await _context.Messages.CountAsync(m => m.RequestId == requestId, cancellationToken);

    public async Task<Dictionary<Guid, int>> GetCountsByRequestIdsAsync(
        IEnumerable<Guid> requestIds,
        CancellationToken cancellationToken = default)
    {
        var ids = requestIds.ToList();
        return await _context.Messages
            .Where(m => ids.Contains(m.RequestId))
            .GroupBy(m => m.RequestId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
    }

    public async Task<IEnumerable<Message>> GetByTypeAsync(Guid requestId, MessageType type, CancellationToken cancellationToken = default)
        => await _context.Messages.AsNoTracking()
            .Where(m => m.RequestId == requestId && m.Type == type)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<Message>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        => await _context.Messages.AsNoTracking()
            .Where(m => m.RequestId == requestId && !m.IsDeleted)
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
}
