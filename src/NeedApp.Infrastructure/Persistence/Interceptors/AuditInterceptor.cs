using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NeedApp.Application.Interfaces;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;

namespace NeedApp.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var auditLogs = CreateAuditLogs(eventData.Context);
            if (auditLogs.Count > 0)
                eventData.Context.Set<AuditLog>().AddRange(auditLogs);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> CreateAuditLogs(DbContext context)
    {
        var auditLogs = new List<AuditLog>();

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog
                     && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Insert,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => (AuditAction?)null
            };

            if (action is null) continue;

            var recordId = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue as Guid?;

            var auditLog = new AuditLog
            {
                TableName = entry.Metadata.GetTableName(),
                RecordId = recordId,
                Action = action,
                ChangedBy = currentUserService.UserId,
                ChangedAt = DateTime.UtcNow
            };

            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                var oldValues = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                auditLog.OldData = JsonSerializer.SerializeToDocument(oldValues);
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var newValues = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                auditLog.NewData = JsonSerializer.SerializeToDocument(newValues);
            }

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }
}
