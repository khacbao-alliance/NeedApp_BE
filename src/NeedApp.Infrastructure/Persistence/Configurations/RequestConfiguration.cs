using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(500);
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.ClientId).HasColumnName("client_id");
        builder.Property(x => x.AssignedTo).HasColumnName("assigned_to");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.Priority).HasColumnName("priority");
        builder.Property(x => x.IntakeQuestionSetId).HasColumnName("intake_question_set_id");
        builder.Property(x => x.IntakeProgress).HasColumnName("intake_progress");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");

        builder.HasIndex(x => x.ClientId).HasDatabaseName("idx_requests_client");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_requests_status");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Client).WithMany(c => c.Requests).HasForeignKey(x => x.ClientId);
        builder.HasOne(x => x.AssignedUser).WithMany(u => u.AssignedRequests).HasForeignKey(x => x.AssignedTo).IsRequired(false);
        builder.HasOne(x => x.IntakeQuestionSet).WithMany(s => s.Requests).HasForeignKey(x => x.IntakeQuestionSetId).IsRequired(false);
    }
}
