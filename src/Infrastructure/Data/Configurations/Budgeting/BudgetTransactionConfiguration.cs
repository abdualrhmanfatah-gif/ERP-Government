using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class BudgetTransactionConfiguration : IEntityTypeConfiguration<BudgetTransaction>
{
    public void Configure(EntityTypeBuilder<BudgetTransaction> builder)
    {
        builder.ToTable("BudgetTransactions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TransactionNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TransactionType)
            .HasConversion<int>();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Direction)
            .HasConversion<int>();

        builder.Property(e => e.DocumentType)
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.ReversalReason)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.TransactionNumber)
            .IsUnique();

        builder.HasIndex(e => e.BudgetId);
        builder.HasIndex(e => e.BudgetItemAllocationId);

        builder.HasOne(e => e.Budget)
            .WithMany()
            .HasForeignKey(e => e.BudgetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BudgetItemAllocation)
            .WithMany()
            .HasForeignKey(e => e.BudgetItemAllocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReversalOf)
            .WithMany()
            .HasForeignKey(e => e.ReversalOfId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
