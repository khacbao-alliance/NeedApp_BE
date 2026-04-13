using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class InvitationRepository(AppDbContext context)
    : BaseRepository<Invitation>(context), IInvitationRepository
{
    private AppDbContext Context => context;

    public async Task<Invitation?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Invitations.AsNoTracking()
            .Include(i => i.Client)
            .Include(i => i.InvitedUser)
            .Include(i => i.InvitedByUser)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    /// <summary>Load with tracking + navigation properties for update operations.</summary>
    public async Task<Invitation?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.Invitations
            .Include(i => i.Client)
            .Include(i => i.InvitedUser)
            .Include(i => i.InvitedByUser)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IEnumerable<Invitation>> GetPendingByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await Context.Invitations.AsNoTracking()
            .Include(i => i.Client)
            .Include(i => i.InvitedByUser)
            .Where(i => i.InvitedUserId == userId && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Invitation?> GetPendingByUserAndClientIdAsync(Guid userId, Guid clientId, CancellationToken cancellationToken = default)
        => await Context.Invitations.AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvitedUserId == userId && i.ClientId == clientId && i.Status == InvitationStatus.Pending, cancellationToken);
}
