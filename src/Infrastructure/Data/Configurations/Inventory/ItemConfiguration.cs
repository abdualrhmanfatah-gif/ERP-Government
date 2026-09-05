using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NameEn)
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Barcode)
            .HasMaxLength(100);

        builder.Property(e => e.ItemType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.OpeningStock)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.AvailableQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.ReservedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.AverageCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.MinimumStock)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.MaximumStock)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.ReorderLevel)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.ReorderQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.Barcode)
            .IsUnique();

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Unit)
            .WithMany()
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.UnitId);
    }
}
