using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PONumber)
            .HasMaxLength(20)
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

        builder.Property(e => e.PaymentTerms)
            .HasMaxLength(2000);

        builder.Property(e => e.DeliveryTerms)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.PONumber)
            .IsUnique();

        builder.HasOne(e => e.PurchaseRequest)
            .WithMany()
            .HasForeignKey(e => e.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Quotation)
            .WithMany()
            .HasForeignKey(e => e.QuotationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PurchaseRequestId);
        builder.HasIndex(e => e.QuotationId);
        builder.HasIndex(e => e.SupplierPartyId);
        builder.HasIndex(e => e.Status);
    }
}
