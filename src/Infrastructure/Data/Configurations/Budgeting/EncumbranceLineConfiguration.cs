using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class EncumbranceLineConfiguration : IEntityTypeConfiguration<EncumbranceLine>
{
    public void Configure(EntityTypeBuilder<EncumbranceLine> builder)
    {
        builder.ToTable("EncumbranceLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.LiquidatedAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.CancelledAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.EncumbranceId);

        builder.HasIndex(e => e.BudgetItemId);

        builder.HasOne(e => e.Encumbrance)
            .WithMany()
            .HasForeignKey(e => e.EncumbranceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BudgetItem)
            .WithMany()
            .HasForeignKey(e => e.BudgetItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
