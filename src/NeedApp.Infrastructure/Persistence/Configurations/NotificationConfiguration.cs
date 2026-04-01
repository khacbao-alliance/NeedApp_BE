using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Infrastructure.Persistence.Converters;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Title).HasColumnName("title");
        builder.Property(x => x.Content).HasColumnName("content");
        builder.Property(x => x.Type).HasColumnName("type").HasColumnType("notification_type")
            .HasConversion(PostgresEnumConverter.ForNullableEnum<NotificationType>());
        builder.Property(x => x.ReferenceId).HasColumnName("reference_id");
        builder.Property(x => x.IsRead).HasColumnName("is_read");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.UserId).HasDatabaseName("idx_notifications_user");

        builder.HasOne(x => x.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_notifications_user")
            .IsRequired(false);
    }
}
