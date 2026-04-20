using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RequestId).HasColumnName("request_id");
        builder.Property(x => x.SenderId).HasColumnName("sender_id");
        builder.Property(x => x.Type).HasColumnName("type");
        builder.Property(x => x.Content).HasColumnName("content");
        builder.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(x => x.ReplyToId).HasColumnName("reply_to_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.IsEdited).HasColumnName("is_edited").HasDefaultValue(false);
        builder.Property(x => x.EditedAt).HasColumnName("edited_at");
        builder.Property(x => x.IsPinned).HasColumnName("is_pinned").HasDefaultValue(false);

        builder.HasIndex(x => new { x.RequestId, x.CreatedAt, x.Id })
            .IsDescending(false, true, true)
            .HasDatabaseName("idx_messages_request_cursor")
            .HasFilter("is_deleted = false");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Request).WithMany(r => r.Messages).HasForeignKey(x => x.RequestId);
        builder.HasOne(x => x.Sender).WithMany(u => u.Messages).HasForeignKey(x => x.SenderId).IsRequired(false);
        builder.HasOne(x => x.ReplyTo).WithMany(m => m.Replies).HasForeignKey(x => x.ReplyToId).IsRequired(false);
    }
}
