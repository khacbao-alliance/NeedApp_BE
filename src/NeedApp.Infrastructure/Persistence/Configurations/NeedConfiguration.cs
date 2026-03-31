using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class NeedConfiguration : IEntityTypeConfiguration<Need>
{
    public void Configure(EntityTypeBuilder<Need> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Description).IsRequired().HasMaxLength(2000);
        builder.Property(n => n.Location).HasMaxLength(300);
        builder.Property(n => n.Budget).HasColumnType("decimal(18,2)");
        builder.Property(n => n.Status).HasConversion<string>();

        builder.HasOne(n => n.User)
            .WithMany(u => u.Needs)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Category)
            .WithMany(c => c.Needs)
            .HasForeignKey(n => n.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
