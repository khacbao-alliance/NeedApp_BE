using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class FileAttachmentRepository(AppDbContext context)
    : BaseRepository<FileAttachment>(context), IFileAttachmentRepository
{

    public async Task<IEnumerable<FileAttachment>> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default)
        => await Context.FileAttachments.Where(f => f.MessageId == messageId).ToListAsync(cancellationToken);
}
