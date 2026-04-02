using Microsoft.EntityFrameworkCore;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Interfaces;

namespace NeedApp.Infrastructure.Persistence.Repositories;

public class FileAttachmentRepository(AppDbContext context)
    : BaseRepository<FileAttachment>(context), IFileAttachmentRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<FileAttachment>> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default)
        => await _context.FileAttachments.Where(f => f.MessageId == messageId).ToListAsync(cancellationToken);
}
