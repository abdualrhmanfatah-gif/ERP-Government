using ERP_Government.Domain.Banking.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Banking;

public class BankReconciliationLineConfiguration : IEntityTypeConfiguration<BankReconciliationLine>
{
    public void Configure(EntityTypeBuilder<BankReconciliationLine> builder)
    {
        builder.ToTable("BankReconciliationLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.LineType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.ReconciliationId);

        builder.HasIndex(e => e.BankStatementLineId);

        builder.HasIndex(e => e.JournalEntryLineId);

        builder.HasOne(e => e.Reconciliation)
            .WithMany()
            .HasForeignKey(e => e.ReconciliationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BankStatementLine)
            .WithMany()
            .HasForeignKey(e => e.BankStatementLineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
