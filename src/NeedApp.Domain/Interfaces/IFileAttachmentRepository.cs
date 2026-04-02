using NeedApp.Domain.Entities;

namespace NeedApp.Domain.Interfaces;

public interface IFileAttachmentRepository : IRepository<FileAttachment>
{
    Task<IEnumerable<FileAttachment>> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default);
}
