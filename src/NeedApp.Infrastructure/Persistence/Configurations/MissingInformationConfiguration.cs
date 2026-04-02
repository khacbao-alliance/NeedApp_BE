using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class MissingInformationConfiguration : IEntityTypeConfiguration<MissingInformation>
{
    public void Configure(EntityTypeBuilder<MissingInformation> builder)
    {
        builder.ToTable("missing_information");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RequestId).HasColumnName("request_id");
        builder.Property(x => x.Question).HasColumnName("question").IsRequired();
        builder.Property(x => x.Answer).HasColumnName("answer");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.AssignedTo).HasColumnName("assigned_to");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.RequestId).HasDatabaseName("idx_missing_request");

        builder.HasOne(x => x.Request)
            .WithMany(r => r.MissingInformations)
            .HasForeignKey(x => x.RequestId)
            .HasConstraintName("fk_mi_request")
            .IsRequired(false);
    }
}
