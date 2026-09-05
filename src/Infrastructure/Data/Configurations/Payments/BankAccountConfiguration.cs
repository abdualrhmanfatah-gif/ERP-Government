using ERP_Government.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Payments;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.BankName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.AccountNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Iban)
            .HasMaxLength(50);

        builder.Property(e => e.SwiftCode)
            .HasMaxLength(20);

        builder.Property(e => e.BranchName)
            .HasMaxLength(200);

        builder.Property(e => e.BranchCode)
            .HasMaxLength(50);

        builder.Property(e => e.MaxDailyLimit)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.MaxTransactionLimit)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.OpeningBalance)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.CurrentBalance)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Iban)
            .IsUnique();

        builder.HasIndex(e => e.CurrencyId);

        builder.HasIndex(e => e.FundId);

        builder.HasIndex(e => e.GlAccountId);

        builder.Ignore(e => e.DomainEvents);
    }
}
