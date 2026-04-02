using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class IntakeQuestionSetConfiguration : IEntityTypeConfiguration<IntakeQuestionSet>
{
    public void Configure(EntityTypeBuilder<IntakeQuestionSet> builder)
    {
        builder.ToTable("intake_question_sets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.IsDefault).HasColumnName("is_default");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedBy).HasColumnName("created_by");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.UpdatedBy).HasColumnName("updated_by");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class IntakeQuestionConfiguration : IEntityTypeConfiguration<IntakeQuestion>
{
    public void Configure(EntityTypeBuilder<IntakeQuestion> builder)
    {
        builder.ToTable("intake_questions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.QuestionSetId).HasColumnName("question_set_id");
        builder.Property(x => x.Content).HasColumnName("content").IsRequired();
        builder.Property(x => x.OrderIndex).HasColumnName("order_index");
        builder.Property(x => x.IsRequired).HasColumnName("is_required");
        builder.Property(x => x.Placeholder).HasColumnName("placeholder");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => new { x.QuestionSetId, x.OrderIndex }).HasDatabaseName("idx_intake_questions_set");

        builder.HasOne(x => x.QuestionSet).WithMany(s => s.Questions).HasForeignKey(x => x.QuestionSetId);
    }
}
