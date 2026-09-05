using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class PurchaseOrderDetailConfiguration : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
    {
        builder.ToTable("PurchaseOrderDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversionFactor)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.OrderedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.ReceivedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.RemainingQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.DiscountPercent)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.DiscountAmount)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.NetUnitPrice)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.LineTotal)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.TaxPercent)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.TaxAmount)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.LineTotalWithTax)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.Status)
            .HasMaxLength(50);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.PurchaseOrder)
            .WithMany()
            .HasForeignKey(e => e.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PurchaseOrderId);
        builder.HasIndex(e => e.ItemId);
        builder.HasIndex(e => e.Status);
    }
}
