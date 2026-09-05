using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
    public void Configure(EntityTypeBuilder<BudgetItem> builder)
    {
        builder.ToTable("BudgetItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ItemCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ItemName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.BudgetId, e.ItemCode })
            .IsUnique();

        builder.HasIndex(e => e.BudgetId);

        builder.HasIndex(e => e.ParentId);

        builder.HasIndex(e => e.AccountId);

        builder.HasIndex(e => e.FundId);

        builder.HasIndex(e => e.CostCenterId);

        builder.HasIndex(e => e.BudgetClassificationId);

        builder.Ignore(e => e.DomainEvents);
    }
}
