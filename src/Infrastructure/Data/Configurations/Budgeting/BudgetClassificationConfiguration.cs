using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class BudgetClassificationConfiguration : IEntityTypeConfiguration<BudgetClassification>
{
    public void Configure(EntityTypeBuilder<BudgetClassification> builder)
    {
        builder.ToTable("BudgetClassifications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.ParentId);

        builder.Ignore(e => e.DomainEvents);
    }
}
