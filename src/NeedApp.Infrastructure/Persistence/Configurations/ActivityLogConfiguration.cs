using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Action).HasColumnName("action");
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.RequestId).HasColumnName("request_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_activity_user")
            .IsRequired(false);

        builder.HasOne(x => x.Request)
            .WithMany(r => r.ActivityLogs)
            .HasForeignKey(x => x.RequestId)
            .HasConstraintName("fk_activity_request")
            .IsRequired(false);
    }
}
