using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class StockTakeConfiguration : IEntityTypeConfiguration<StockTake>
{
    public void Configure(EntityTypeBuilder<StockTake> builder)
    {
        builder.ToTable("StockTakes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.StockTakeNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.StockTakeType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.StockTakeNumber)
            .IsUnique();

        builder.HasOne(e => e.Warehouse)
            .WithMany()
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.WarehouseId);
        builder.HasIndex(e => e.LocationId);
    }
}
