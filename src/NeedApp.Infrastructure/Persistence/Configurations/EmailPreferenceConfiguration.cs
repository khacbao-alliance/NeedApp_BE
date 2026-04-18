using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class EmailPreferenceConfiguration : IEntityTypeConfiguration<EmailPreference>
{
    public void Configure(EntityTypeBuilder<EmailPreference> builder)
    {
        builder.ToTable("email_preferences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.OnAssignment).HasColumnName("on_assignment");
        builder.Property(e => e.OnStatusChange).HasColumnName("on_status_change");
        builder.Property(e => e.OnOverdue).HasColumnName("on_overdue");
        builder.Property(e => e.OnNewRequest).HasColumnName("on_new_request");
        builder.Property(e => e.DigestFrequency).HasColumnName("digest_frequency");
        builder.Property(e => e.LastDigestSentAt).HasColumnName("last_digest_sent_at");

        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
