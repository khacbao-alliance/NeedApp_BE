using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Infrastructure.Persistence.Converters;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Email).HasColumnName("email").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.Role).HasColumnName("role").HasColumnType("user_role")
            .HasConversion(PostgresEnumConverter.ForNullableEnum<UserRole>());
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");

        builder.Property(x => x.PasswordHash).HasColumnName("password_hash");
        builder.Property(x => x.GoogleId).HasColumnName("google_id");

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
