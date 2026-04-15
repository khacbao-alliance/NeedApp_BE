using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class IntakeQuestionSetRepository(AppDbContext context)
    : BaseRepository<IntakeQuestionSet>(context), IIntakeQuestionSetRepository
{

    public async Task<IntakeQuestionSet?> GetDefaultAsync(CancellationToken cancellationToken = default)
        => await Context.IntakeQuestionSets.AsNoTracking()
            .Include(s => s.Questions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(s => s.IsDefault && s.IsActive, cancellationToken);

    // Fallback: first active set (default-first, then by creation date)
    public async Task<IntakeQuestionSet?> GetFirstActiveAsync(CancellationToken cancellationToken = default)
        => await Context.IntakeQuestionSets.AsNoTracking()
            .Include(s => s.Questions.OrderBy(q => q.OrderIndex))
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IntakeQuestionSet?> GetWithQuestionsAsync(Guid id, CancellationToken cancellationToken = default)
        => await Context.IntakeQuestionSets.AsNoTracking()
            .Include(s => s.Questions.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
