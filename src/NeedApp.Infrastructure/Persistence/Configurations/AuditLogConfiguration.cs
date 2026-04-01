using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Infrastructure.Persistence.Converters;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TableName).HasColumnName("table_name");
        builder.Property(x => x.RecordId).HasColumnName("record_id");
        builder.Property(x => x.Action).HasColumnName("action").HasColumnType("audit_action")
            .HasConversion(PostgresEnumConverter.ForNullableEnum<AuditAction>());
        builder.Property(x => x.OldData).HasColumnName("old_data").HasColumnType("jsonb");
        builder.Property(x => x.NewData).HasColumnName("new_data").HasColumnType("jsonb");
        builder.Property(x => x.ChangedBy).HasColumnName("changed_by");
        builder.Property(x => x.ChangedAt).HasColumnName("changed_at");

        builder.HasIndex(x => x.TableName).HasDatabaseName("idx_audit_table");
        builder.HasIndex(x => x.RecordId).HasDatabaseName("idx_audit_record");
    }
}
