namespace NeedApp.Domain.Entities;

public class MessageEditHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public string PreviousContent { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; } = DateTime.UtcNow;
    public Guid? EditedBy { get; set; }

    // Navigation
    public Message Message { get; set; } = default!;
    public User? Editor { get; set; }
}
