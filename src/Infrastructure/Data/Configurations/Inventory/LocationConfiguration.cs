using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Barcode)
            .HasMaxLength(100);

        builder.Property(e => e.Breadcrumb)
            .HasMaxLength(1000);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.Capacity)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne(e => e.ParentLocation)
            .WithMany()
            .HasForeignKey(e => e.ParentLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ParentLocationId);
    }
}
