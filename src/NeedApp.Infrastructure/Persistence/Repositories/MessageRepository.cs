using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class MessageRepository(AppDbContext context)
    : BaseRepository<Message>(context), IMessageRepository
{

    public async Task<(IEnumerable<Message> Items, bool HasMore)> GetByRequestIdAsync(
        Guid requestId, DateTime? cursorDate, Guid? cursorId, int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Messages.AsNoTracking()
            .Where(m => m.RequestId == requestId)
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .Include(m => m.Reactions)
            .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender)
            .AsQueryable();

        // Apply cursor filter BEFORE ordering for better query plan
        if (cursorDate.HasValue && cursorId.HasValue)
        {
            query = query.Where(m =>
                m.CreatedAt > cursorDate.Value ||
                (m.CreatedAt == cursorDate.Value && m.Id.CompareTo(cursorId.Value) > 0));
        }

        var items = await query
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return (items, hasMore);
    }

    public async Task<int> GetCountByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        => await Context.Messages.CountAsync(
            m => m.RequestId == requestId
              && (m.Type == MessageType.Text || m.Type == MessageType.File),
            cancellationToken);

    public async Task<Dictionary<Guid, int>> GetCountsByRequestIdsAsync(
        IEnumerable<Guid> requestIds,
        CancellationToken cancellationToken = default)
    {
        var ids = requestIds.ToList();
        return await Context.Messages
            .Where(m => ids.Contains(m.RequestId)
                     && (m.Type == MessageType.Text || m.Type == MessageType.File))
            .GroupBy(m => m.RequestId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
    }

    public async Task<IEnumerable<Message>> GetByTypeAsync(Guid requestId, MessageType type, CancellationToken cancellationToken = default)
        => await Context.Messages.AsNoTracking()
            .Where(m => m.RequestId == requestId && m.Type == type)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<Message>> GetAllByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        => await Context.Messages.AsNoTracking()
            .Where(m => m.RequestId == requestId && !m.IsDeleted)
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<Message>> SearchAsync(
        Guid requestId, string query, int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = "%" + SearchHelper.RemoveDiacritics(query).ToLowerInvariant() + "%";
        var vnFrom = SearchHelper.SqlTranslateFrom;
        var vnTo = SearchHelper.SqlTranslateTo;

        return await Context.Messages.FromSqlInterpolated(
            $@"SELECT * FROM messages
               WHERE request_id = {requestId}
                 AND is_deleted = false
                 AND translate(lower(COALESCE(content, '')), {vnFrom}, {vnTo}) LIKE {normalizedSearch}")
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(m => m.Sender)
            .Include(m => m.Files)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
