using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class RequestFileConfiguration : IEntityTypeConfiguration<RequestFile>
{
    public void Configure(EntityTypeBuilder<RequestFile> builder)
    {
        builder.ToTable("files");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Url).HasColumnName("url");
        builder.Property(x => x.FileName).HasColumnName("file_name");
        builder.Property(x => x.RequestId).HasColumnName("request_id");
        builder.Property(x => x.CommentId).HasColumnName("comment_id");
        builder.Property(x => x.UploadedBy).HasColumnName("uploaded_by");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.Request)
            .WithMany(r => r.Files)
            .HasForeignKey(x => x.RequestId)
            .HasConstraintName("fk_files_request")
            .IsRequired(false);

        builder.HasOne(x => x.Comment)
            .WithMany(c => c.Files)
            .HasForeignKey(x => x.CommentId)
            .HasConstraintName("fk_files_comment")
            .IsRequired(false);
    }
}
