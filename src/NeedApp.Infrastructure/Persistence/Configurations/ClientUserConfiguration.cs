using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class ClientUserConfiguration : IEntityTypeConfiguration<ClientUser>
{
    public void Configure(EntityTypeBuilder<ClientUser> builder)
    {
        builder.ToTable("client_users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ClientId).HasColumnName("client_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Role).HasColumnName("role");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");

        builder.HasIndex(x => new { x.ClientId, x.UserId }).IsUnique().HasDatabaseName("uq_client_user");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Client)
            .WithMany(c => c.ClientUsers)
            .HasForeignKey(x => x.ClientId)
            .HasConstraintName("fk_client_users_client");

        builder.HasOne(x => x.User)
            .WithMany(u => u.ClientUsers)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_client_users_user");
    }
}
