using ERP_Government.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Payments;

public class PaymentOrderDeductionConfiguration : IEntityTypeConfiguration<PaymentOrderDeduction>
{
    public void Configure(EntityTypeBuilder<PaymentOrderDeduction> builder)
    {
        builder.ToTable("PaymentOrderDeductions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DeductionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DeductionCode)
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.DeductionPercent)
            .HasColumnType("decimal(5,2)")
            .HasSentinel(null);

        builder.Property(e => e.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.PaymentOrderId, e.LineNumber })
            .IsUnique();

        builder.HasIndex(e => e.PaymentOrderId);

        builder.HasIndex(e => e.AccountId);

        builder.HasOne(e => e.PaymentOrder)
            .WithMany()
            .HasForeignKey(e => e.PaymentOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
