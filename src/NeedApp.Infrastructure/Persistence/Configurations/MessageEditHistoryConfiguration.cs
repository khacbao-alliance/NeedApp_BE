using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class MessageEditHistoryConfiguration : IEntityTypeConfiguration<MessageEditHistory>
{
    public void Configure(EntityTypeBuilder<MessageEditHistory> builder)
    {
        builder.ToTable("message_edit_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.MessageId).HasColumnName("message_id");
        builder.Property(x => x.PreviousContent).HasColumnName("previous_content").IsRequired();
        builder.Property(x => x.EditedAt).HasColumnName("edited_at");
        builder.Property(x => x.EditedBy).HasColumnName("edited_by");

        // Index for fast lookup by message, newest first
        builder.HasIndex(x => new { x.MessageId, x.EditedAt })
            .IsDescending(false, true)
            .HasDatabaseName("idx_message_edit_history_message_edited");

        builder.HasOne(x => x.Message)
            .WithMany(m => m.EditHistory)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Editor)
            .WithMany()
            .HasForeignKey(x => x.EditedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
