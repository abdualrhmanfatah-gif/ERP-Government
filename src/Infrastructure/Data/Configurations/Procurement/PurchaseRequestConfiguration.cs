using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RequestNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.RequestType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Priority)
            .HasMaxLength(50);

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500);

        builder.Property(e => e.TotalQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.EstimatedTotalCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.RequestNumber)
            .IsUnique();

        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestDate);
    }
}
