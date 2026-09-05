using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(100);

        builder.Property(e => e.TotalCapacity)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.CurrentLoad)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne(e => e.Location)
            .WithMany()
            .HasForeignKey(e => e.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.LocationId);
    }
}
