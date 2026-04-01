using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class RequestParticipantConfiguration : IEntityTypeConfiguration<RequestParticipant>
{
    public void Configure(EntityTypeBuilder<RequestParticipant> builder)
    {
        builder.ToTable("request_participants");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RequestId).HasColumnName("request_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => new { x.RequestId, x.UserId }).IsUnique().HasDatabaseName("uq_request_user");

        builder.HasOne(x => x.Request)
            .WithMany(r => r.Participants)
            .HasForeignKey(x => x.RequestId)
            .HasConstraintName("fk_rp_request")
            .IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_rp_user")
            .IsRequired(false);
    }
}
