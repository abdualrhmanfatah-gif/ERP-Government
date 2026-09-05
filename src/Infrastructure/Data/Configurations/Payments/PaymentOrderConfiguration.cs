using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Payments;

public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("PaymentOrders");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentOrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.PaymentOrderType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ExchangeRate)
            .HasColumnType("decimal(18,6)")
            .HasSentinel(null);

        builder.Property(e => e.AmountGross)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.DeductionAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.BeneficiaryName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.BeneficiaryIban)
            .HasMaxLength(50);

        builder.Property(e => e.BeneficiaryAccountNumber)
            .HasMaxLength(100);

        builder.Property(e => e.BeneficiaryBankName)
            .HasMaxLength(200);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.BudgetCheckStatus)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.TreasuryStatus)
            .HasMaxLength(20);

        builder.Property(e => e.TreasuryReference)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.PaymentOrderNumber)
            .IsUnique();

        builder.HasIndex(e => e.VendorId);

        builder.HasIndex(e => e.FundId);

        builder.HasIndex(e => e.FiscalYearId);

        builder.HasIndex(e => e.AppropriationId);

        builder.HasIndex(e => e.BudgetClassificationId);

        builder.HasIndex(e => e.CostCenterId);

        builder.HasIndex(e => e.ProjectId);

        builder.HasIndex(e => e.PurchaseOrderId);

        builder.HasIndex(e => e.EncumbranceId);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasIndex(e => e.BankAccountId);

        builder.Property(e => e.PaymentMethod).HasDefaultValue(PaymentMethod.Other);

        builder.HasIndex(e => e.JournalEntryId);

        builder.HasIndex(e => e.AccountingEventId);

        builder.Ignore(e => e.DomainEvents);
    }
}
