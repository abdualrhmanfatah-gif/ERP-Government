using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class ItemUnitConfiguration : IEntityTypeConfiguration<ItemUnit>
{
    public void Configure(EntityTypeBuilder<ItemUnit> builder)
    {
        builder.ToTable("ItemUnits");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversionFactor)
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.Item)
            .WithMany()
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Unit)
            .WithMany()
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ItemId);
        builder.HasIndex(e => e.UnitId);
    }
}
