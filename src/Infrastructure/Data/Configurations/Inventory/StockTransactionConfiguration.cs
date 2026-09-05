using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TransactionNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TransactionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ReferenceType)
            .HasMaxLength(50);

        builder.Property(e => e.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(e => e.ConversionFactor)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.Quantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.UnitCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.TotalCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.QuantityBefore)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.QuantityAfter)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.ReversalReason)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.TransactionNumber)
            .IsUnique();

        builder.HasOne(e => e.Item)
            .WithMany()
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Warehouse)
            .WithMany()
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Unit)
            .WithMany()
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReversalOf)
            .WithMany()
            .HasForeignKey(e => e.ReversalOfId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ItemId);
        builder.HasIndex(e => e.WarehouseId);
        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.TransactionDate);
        builder.HasIndex(e => e.ReferenceType);
        builder.HasIndex(e => e.ReferenceId);
        builder.HasIndex(e => e.ReversalOfId);
    }
}
