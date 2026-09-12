using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.BudgetNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.BudgetName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.BudgetNumber)
            .IsUnique();

        builder.HasIndex(e => e.BudgetTypeId);

        builder.HasIndex(e => e.FiscalYearId);

        builder.HasIndex(e => e.FundId);

        builder.Ignore(e => e.DomainEvents);
    }
}
