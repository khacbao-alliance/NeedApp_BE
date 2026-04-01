using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;
using NeedApp.Domain.Enums;
using NeedApp.Infrastructure.Persistence.Converters;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RequestId).HasColumnName("request_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Content).HasColumnName("content");
        builder.Property(x => x.Type).HasColumnName("type").HasColumnType("comment_type")
            .HasConversion(PostgresEnumConverter.ForNullableEnum<CommentType>());
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");

        builder.HasIndex(x => x.RequestId).HasDatabaseName("idx_comments_request");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Request)
            .WithMany(r => r.Comments)
            .HasForeignKey(x => x.RequestId)
            .HasConstraintName("fk_comments_request");

        builder.HasOne(x => x.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_comments_user");
    }
}
