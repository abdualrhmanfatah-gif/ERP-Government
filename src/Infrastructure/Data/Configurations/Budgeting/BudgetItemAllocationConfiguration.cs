using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class BudgetItemAllocationConfiguration : IEntityTypeConfiguration<BudgetItemAllocation>
{
    public void Configure(EntityTypeBuilder<BudgetItemAllocation> builder)
    {
        builder.ToTable("BudgetItemAllocations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProposedAmount)
            .HasColumnType("decimal(23,2)")
            .IsRequired();

        builder.Property(e => e.ApprovedAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Remarks)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.BudgetId, e.BudgetItemId })
            .IsUnique();

        builder.HasIndex(e => e.BudgetId);

        builder.HasIndex(e => e.BudgetItemId);

        builder.HasOne(e => e.Budget)
            .WithMany()
            .HasForeignKey(e => e.BudgetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BudgetItem)
            .WithMany()
            .HasForeignKey(e => e.BudgetItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
