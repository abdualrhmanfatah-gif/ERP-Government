using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Payments;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Completed);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.PaymentNumber)
            .IsUnique();

        builder.HasIndex(e => e.DisbursementRequestId)
            .IsUnique();

        builder.HasIndex(e => e.PaymentOrderId);

        builder.HasOne(e => e.DisbursementRequest)
            .WithMany()
            .HasForeignKey(e => e.DisbursementRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PaymentOrder)
            .WithMany()
            .HasForeignKey(e => e.PaymentOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
