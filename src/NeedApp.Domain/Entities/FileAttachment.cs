using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class FileAttachment : BaseEntity
{
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = default!;
    public string CloudinaryPublicId { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public Guid? UploadedBy { get; set; }

    public Message Message { get; set; } = default!;
    public User? Uploader { get; set; }
}
