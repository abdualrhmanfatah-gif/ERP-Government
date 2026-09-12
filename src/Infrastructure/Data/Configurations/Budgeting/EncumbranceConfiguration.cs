using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class EncumbranceConfiguration : IEntityTypeConfiguration<Encumbrance>
{
    public void Configure(EntityTypeBuilder<Encumbrance> builder)
    {
        builder.ToTable("Encumbrances");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EncumbranceNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EncumbranceType)
            .HasConversion<int>();

        builder.Property(e => e.DocumentType)
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.ReversalReason)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.EncumbranceNumber)
            .IsUnique();

        builder.HasIndex(e => e.PurchaseOrderId);

        builder.HasIndex(e => e.ReversalOfId);

        builder.Ignore(e => e.DomainEvents);
    }
}
