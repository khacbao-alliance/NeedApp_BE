using System.Text.Json;
using NeedApp.Domain.Enums;

namespace NeedApp.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? TableName { get; set; }
    public Guid? RecordId { get; set; }
    public AuditAction? Action { get; set; }
    public JsonDocument? OldData { get; set; }
    public JsonDocument? NewData { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
