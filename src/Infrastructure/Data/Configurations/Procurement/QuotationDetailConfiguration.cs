using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class QuotationDetailConfiguration : IEntityTypeConfiguration<QuotationDetail>
{
    public void Configure(EntityTypeBuilder<QuotationDetail> builder)
    {
        builder.ToTable("QuotationDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversionFactor)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.Quantity)
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

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.Quotation)
            .WithMany()
            .HasForeignKey(e => e.QuotationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.QuotationId);
        builder.HasIndex(e => e.ItemId);
    }
}
