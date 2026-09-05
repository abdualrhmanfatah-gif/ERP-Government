using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class FinalAccountLineConfiguration : IEntityTypeConfiguration<FinalAccountLine>
{
    public void Configure(EntityTypeBuilder<FinalAccountLine> builder)
    {
        builder.ToTable("FinalAccountLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DimensionCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DimensionName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.BudgetedAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ActualAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Variance)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.FinalAccountId, e.Dimension, e.DimensionId })
            .IsUnique();

        builder.HasIndex(e => e.FinalAccountId);

        builder.HasOne(e => e.FinalAccount)
            .WithMany(f => f.Lines)
            .HasForeignKey(e => e.FinalAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
