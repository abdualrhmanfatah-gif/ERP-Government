using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class RevenueReceiptConfiguration : IEntityTypeConfiguration<RevenueReceipt>
{
    public void Configure(EntityTypeBuilder<RevenueReceipt> builder)
    {
        builder.ToTable("RevenueReceipts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReceiptNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ReceiptType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.PayerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.PayerNationalId)
            .HasMaxLength(50);

        builder.Property(e => e.AmountTotal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ExternalTransactionRef)
            .HasMaxLength(100);

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.ReceiptNumber)
            .IsUnique();

        builder.HasIndex(e => e.FundId);

        builder.HasIndex(e => e.BudgetClassificationId);

        builder.HasIndex(e => e.CurrencyId);

        builder.Property(e => e.PaymentMethod).HasDefaultValue(PaymentMethod.Other);

        builder.HasIndex(e => e.JournalEntryId);

        builder.Ignore(e => e.DomainEvents);
    }
}
