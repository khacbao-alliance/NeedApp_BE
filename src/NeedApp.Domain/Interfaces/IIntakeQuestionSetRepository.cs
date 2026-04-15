using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IIntakeQuestionSetRepository : IRepository<IntakeQuestionSet>
{
    Task<IntakeQuestionSet?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<IntakeQuestionSet?> GetFirstActiveAsync(CancellationToken cancellationToken = default);
    Task<IntakeQuestionSet?> GetWithQuestionsAsync(Guid id, CancellationToken cancellationToken = default);
}
