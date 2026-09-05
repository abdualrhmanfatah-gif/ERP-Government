using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class DocumentAttachmentRequirementConfiguration : IEntityTypeConfiguration<DocumentAttachmentRequirement>
{
    public void Configure(EntityTypeBuilder<DocumentAttachmentRequirement> builder)
    {
        builder.ToTable("DocumentAttachmentRequirements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DocumentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.AttachmentTypeCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TitleAr)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.DocumentType, e.AttachmentTypeCode })
            .IsUnique();

        builder.HasIndex(e => e.DocumentType);

        builder.Ignore(e => e.DomainEvents);
    }
}
