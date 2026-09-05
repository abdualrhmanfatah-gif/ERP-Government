using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FileName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.MimeType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.FileHash)
            .HasMaxLength(100);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.UploadedBy)
            .WithMany()
            .HasForeignKey(e => e.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
