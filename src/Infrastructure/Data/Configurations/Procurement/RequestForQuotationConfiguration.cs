using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class RequestForQuotationConfiguration : IEntityTypeConfiguration<RequestForQuotation>
{
    public void Configure(EntityTypeBuilder<RequestForQuotation> builder)
    {
        builder.ToTable("RequestForQuotations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RFQNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.TermsAndConditions)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.RFQNumber)
            .IsUnique();

        builder.HasIndex(e => e.PurchaseRequestId);
        builder.HasIndex(e => e.Status);
    }
}
