using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class BudgetItemMonthlyPlanConfiguration : IEntityTypeConfiguration<BudgetItemMonthlyPlan>
{
    public void Configure(EntityTypeBuilder<BudgetItemMonthlyPlan> builder)
    {
        builder.ToTable("BudgetItemMonthlyPlans");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Month)
            .IsRequired();

        builder.Property(e => e.PlannedAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.BudgetItemId, e.Month })
            .IsUnique();

        builder.HasIndex(e => e.BudgetItemId);

        builder.Ignore(e => e.DomainEvents);
    }
}
