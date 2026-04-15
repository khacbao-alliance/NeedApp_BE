using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IIntakeQuestionRepository
{
    Task AddRangeAsync(IEnumerable<IntakeQuestion> questions, CancellationToken cancellationToken = default);
    Task DeleteBySetIdAsync(Guid questionSetId, CancellationToken cancellationToken = default);
}
