using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable("AccountBalances");

        builder.HasKey(e => e.Id);

        // Money fields — decimal(23,2) per project convention
        builder.Property(e => e.OpeningDebit).HasColumnType("decimal(23,2)");
        builder.Property(e => e.OpeningCredit).HasColumnType("decimal(23,2)");
        builder.Property(e => e.Debit).HasColumnType("decimal(23,2)");
        builder.Property(e => e.Credit).HasColumnType("decimal(23,2)");
        builder.Property(e => e.ClosingDebit).HasColumnType("decimal(23,2)");
        builder.Property(e => e.ClosingCredit).HasColumnType("decimal(23,2)");

        // RowVersion for optimistic concurrency
        builder.Property(e => e.RowVersion).IsRowVersion();

        // Unique composite index: one balance per account per period per currency (FR-002)
        builder.HasIndex(e => new { e.AccountId, e.FiscalYearId, e.FiscalPeriodId, e.CurrencyId })
            .IsUnique();

        // Non-unique indexes for query performance (FR-003)
        builder.HasIndex(e => e.FiscalYearId);
        builder.HasIndex(e => e.FiscalPeriodId);
        builder.HasIndex(e => e.AccountId);

        // FK relationships — all DeleteBehavior.Restrict per project convention
        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FiscalYear)
            .WithMany()
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FiscalPeriod)
            .WithMany()
            .HasForeignKey(e => e.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
