using NeedApp.Domain.Common;

namespace NeedApp.Domain.Entities;

public class RequestFile : BaseEntity
{
    public string? Url { get; set; }
    public string? FileName { get; set; }
    public Guid? RequestId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? UploadedBy { get; set; }

    public Request? Request { get; set; }
    public Comment? Comment { get; set; }
}
