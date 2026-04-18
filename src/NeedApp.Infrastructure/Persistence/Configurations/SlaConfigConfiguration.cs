using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class SlaConfigConfiguration : IEntityTypeConfiguration<SlaConfig>
{
    public void Configure(EntityTypeBuilder<SlaConfig> builder)
    {
        builder.ToTable("sla_configs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Priority).HasColumnName("priority");
        builder.Property(x => x.DeadlineHours).HasColumnName("deadline_hours");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        // One config per priority
        builder.HasIndex(x => x.Priority).IsUnique().HasDatabaseName("idx_sla_configs_priority");
    }
}
