using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class SupplierInvoiceDetailConfiguration : IEntityTypeConfiguration<SupplierInvoiceDetail>
{
    public void Configure(EntityTypeBuilder<SupplierInvoiceDetail> builder)
    {
        builder.ToTable("SupplierInvoiceDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Quantity)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(23,2)")
            .IsRequired();

        builder.Property(e => e.DiscountAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.TaxAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.LineTotal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.SupplierInvoice)
            .WithMany(s => s.Details)
            .HasForeignKey(e => e.SupplierInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PurchaseOrderDetail)
            .WithMany()
            .HasForeignKey(e => e.PurchaseOrderDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.SupplierInvoiceId);
        builder.HasIndex(e => e.PurchaseOrderDetailId);
        builder.HasIndex(e => e.ItemId);
    }
}
