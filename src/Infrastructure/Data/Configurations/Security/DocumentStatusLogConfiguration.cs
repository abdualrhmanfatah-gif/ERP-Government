using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class DocumentStatusLogConfiguration : IEntityTypeConfiguration<DocumentStatusLog>
{
    public void Configure(EntityTypeBuilder<DocumentStatusLog> builder)
    {
        builder.ToTable("DocumentStatusLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.FromStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ToStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(1000);

        builder.HasIndex(e => new { e.EntityName, e.DocumentId });

        builder.HasIndex(e => e.ChangedAt);

        builder.Ignore(e => e.DomainEvents);
    }
}
