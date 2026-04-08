using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("message_reactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.MessageId).HasColumnName("message_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Emoji).HasColumnName("emoji").HasMaxLength(8).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        // Each user can only react with a specific emoji once per message
        builder.HasIndex(x => new { x.MessageId, x.UserId, x.Emoji })
            .IsUnique()
            .HasDatabaseName("uq_message_reactions_msg_user_emoji");

        builder.HasOne(x => x.Message).WithMany(m => m.Reactions).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
