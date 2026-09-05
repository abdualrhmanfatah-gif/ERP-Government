using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("Quotations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.QuotationNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.ExchangeRate)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.SubTotal)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.DiscountAmount)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.TaxAmount)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.ShippingCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.OtherCharges)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.GrandTotal)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.PaymentTerms)
            .HasMaxLength(500);

        builder.Property(e => e.DeliveryTerms)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.SelectionReason)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.QuotationNumber)
            .IsUnique();

        builder.HasIndex(e => e.RFQId);
        builder.HasIndex(e => e.SupplierId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsSelected);
    }
}
