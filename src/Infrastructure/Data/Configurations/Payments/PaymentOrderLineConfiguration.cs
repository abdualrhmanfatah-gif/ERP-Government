using ERP_Government.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Payments;

public class PaymentOrderLineConfiguration : IEntityTypeConfiguration<PaymentOrderLine>
{
    public void Configure(EntityTypeBuilder<PaymentOrderLine> builder)
    {
        builder.ToTable("PaymentOrderLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.LineType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.TaxAmount)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.ExchangeRate)
            .HasColumnType("decimal(18,6)")
            .HasSentinel(null);

        builder.Property(e => e.AllocationStatus)
            .HasMaxLength(20);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.PaymentOrderId, e.LineNumber })
            .IsUnique();

        builder.HasIndex(e => e.PaymentOrderId);

        builder.HasIndex(e => e.AccountId);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasIndex(e => e.FundId);

        builder.HasIndex(e => e.AppropriationId);

        builder.HasIndex(e => e.OrganizationUnitId);

        builder.HasIndex(e => e.CostCenterId);

        builder.HasIndex(e => e.ProjectId);

        builder.HasOne(e => e.PaymentOrder)
            .WithMany()
            .HasForeignKey(e => e.PaymentOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
