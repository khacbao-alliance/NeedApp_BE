using NeedApp.Domain.Common;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string? Content { get; set; }
    public CommentType? Type { get; set; }
    public bool IsDeleted { get; set; } = false;

    public Request Request { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<RequestFile> Files { get; set; } = [];
}
