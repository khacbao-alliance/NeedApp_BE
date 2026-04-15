using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class IntakeQuestionRepository(AppDbContext context) : IIntakeQuestionRepository
{
    public async Task AddRangeAsync(IEnumerable<IntakeQuestion> questions, CancellationToken cancellationToken = default)
        => await context.IntakeQuestions.AddRangeAsync(questions, cancellationToken);

    public async Task DeleteBySetIdAsync(Guid questionSetId, CancellationToken cancellationToken = default)
        => await context.IntakeQuestions
            .Where(q => q.QuestionSetId == questionSetId)
            .ExecuteDeleteAsync(cancellationToken);
}
