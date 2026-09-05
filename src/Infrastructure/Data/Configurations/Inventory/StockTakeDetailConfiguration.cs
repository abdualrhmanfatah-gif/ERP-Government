using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class StockTakeDetailConfiguration : IEntityTypeConfiguration<StockTakeDetail>
{
    public void Configure(EntityTypeBuilder<StockTakeDetail> builder)
    {
        builder.ToTable("StockTakeDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversionFactor)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.SystemQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.CountedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.QuantityDifference)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.UnitCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.DifferenceAmount)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.StockTake)
            .WithMany()
            .HasForeignKey(e => e.StockTakeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Item)
            .WithMany()
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Unit)
            .WithMany()
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.StockTakeId);
        builder.HasIndex(e => e.ItemId);
    }
}
