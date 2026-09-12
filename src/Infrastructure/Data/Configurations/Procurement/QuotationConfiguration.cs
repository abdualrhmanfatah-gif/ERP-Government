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

        builder.Property(e => e.TechnicalScore)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.FinancialScore)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.SelectionReason)
            .HasMaxLength(2000);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(2000);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.QuotationNumber)
            .IsUnique();

        builder.HasIndex(e => e.SupplierPartyId);
        builder.HasIndex(e => e.Status);
    }
}
