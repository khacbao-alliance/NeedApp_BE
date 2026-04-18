using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Interfaces;

public interface IEmailPreferenceRepository : IRepository<EmailPreference>
{
    Task<EmailPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<EmailPreference>> GetByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<List<EmailPreference>> GetByDigestFrequencyAsync(DigestFrequency frequency, CancellationToken cancellationToken = default);
}
