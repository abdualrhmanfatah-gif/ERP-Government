using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class SupplierInvoiceConfiguration : IEntityTypeConfiguration<SupplierInvoice>
{
    public void Configure(EntityTypeBuilder<SupplierInvoice> builder)
    {
        builder.ToTable("SupplierInvoices");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvoiceNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.SupplierInvoiceNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.SupplierPartyId)
            .IsRequired();

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(e => e.ExchangeRate)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.SubTotal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.DiscountAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.TaxAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ShippingCost)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.OtherCharges)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.GrandTotal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.InvoiceNumber)
            .IsUnique();

        builder.HasIndex(e => new { e.SupplierPartyId, e.SupplierInvoiceNumber })
            .IsUnique();

        builder.HasOne(e => e.PurchaseOrder)
            .WithMany()
            .HasForeignKey(e => e.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PurchaseOrderId);
        builder.HasIndex(e => e.SupplierPartyId);
        builder.HasIndex(e => e.Status);
    }
}
