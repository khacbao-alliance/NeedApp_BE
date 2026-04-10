using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class RequestParticipantRepository(AppDbContext context)
    : BaseRepository<RequestParticipant>(context), IRequestParticipantRepository
{
    private readonly AppDbContext _context = context;

    public async Task<bool> IsParticipantAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default)
        => await _context.RequestParticipants.AnyAsync(p => p.RequestId == requestId && p.UserId == userId, cancellationToken);

    public async Task<IEnumerable<RequestParticipant>> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        => await _context.RequestParticipants
            .Include(p => p.User)
            .Where(p => p.RequestId == requestId)
            .ToListAsync(cancellationToken);
}
