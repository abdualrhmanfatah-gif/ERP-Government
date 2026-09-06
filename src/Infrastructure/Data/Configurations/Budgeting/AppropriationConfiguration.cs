using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class AppropriationConfiguration : IEntityTypeConfiguration<Appropriation>
{
    public void Configure(EntityTypeBuilder<Appropriation> builder)
    {
        builder.ToTable("Appropriations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AppropriationNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.AppropriationType)
            .HasConversion<int>();

        builder.Property(e => e.DocumentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.AppropriationNumber)
            .IsUnique();

        builder.HasIndex(e => e.BudgetId);

        builder.HasIndex(e => e.BudgetItemId);

        builder.HasIndex(e => e.TargetBudgetItemId);

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
