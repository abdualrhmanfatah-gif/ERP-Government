using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Organization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.ExchangeRate)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.Debit)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Credit)
            .HasColumnType("decimal(23,2)");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_JournalEntryLines_DebitCreditXOR",
            "(([Debit] > 0 AND [Credit] = 0) OR ([Debit] = 0 AND [Credit] > 0))"));

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.JournalEntryId);

        builder.HasIndex(e => e.AccountId);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasIndex(e => e.CostCenterId);

        builder.HasIndex(e => e.PaymentOrderId);

        // Performance index for General Ledger report (running balance computation)
        builder.HasIndex(e => new { e.AccountId, e.JournalEntryId })
            .HasDatabaseName("IX_JournalEntryLines_AccountId_JournalEntryId");

        // Same-module FK
        builder.HasOne(e => e.JournalEntry)
            .WithMany()
            .HasForeignKey(e => e.JournalEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
