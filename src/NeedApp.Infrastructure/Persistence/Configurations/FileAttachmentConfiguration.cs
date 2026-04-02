using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeedApp.Domain.Entities;

namespace NeedApp.Infrastructure.Persistence.Configurations;

public class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.ToTable("file_attachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.MessageId).HasColumnName("message_id");
        builder.Property(x => x.FileName).HasColumnName("file_name").IsRequired();
        builder.Property(x => x.CloudinaryPublicId).HasColumnName("cloudinary_public_id").IsRequired();
        builder.Property(x => x.Url).HasColumnName("url").IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type");
        builder.Property(x => x.FileSize).HasColumnName("file_size");
        builder.Property(x => x.UploadedBy).HasColumnName("uploaded_by");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(x => x.MessageId).HasDatabaseName("idx_file_attachments_message");

        builder.HasOne(x => x.Message).WithMany(m => m.Files).HasForeignKey(x => x.MessageId);
        builder.HasOne(x => x.Uploader).WithMany().HasForeignKey(x => x.UploadedBy).IsRequired(false);
    }
}
