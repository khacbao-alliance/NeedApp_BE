using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClientId).HasColumnName("client_id");
        builder.Property(x => x.InvitedUserId).HasColumnName("invited_user_id");
        builder.Property(x => x.InvitedByUserId).HasColumnName("invited_by_user_id");
        builder.Property(x => x.Role).HasColumnName("role");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.RespondedAt).HasColumnName("responded_at");

        // Only one pending invitation per user per client
        builder.HasIndex(x => new { x.ClientId, x.InvitedUserId, x.Status })
            .HasDatabaseName("idx_invitations_unique_pending")
            .HasFilter("status = 'pending'")
            .IsUnique();

        builder.HasIndex(x => x.InvitedUserId).HasDatabaseName("idx_invitations_user");

        builder.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.InvitedUser).WithMany().HasForeignKey(x => x.InvitedUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.InvitedByUser).WithMany().HasForeignKey(x => x.InvitedByUserId).OnDelete(DeleteBehavior.Cascade);
    }
}
