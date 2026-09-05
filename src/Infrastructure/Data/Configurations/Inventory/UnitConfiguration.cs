using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.NameAr)
            .HasMaxLength(100);

        builder.Property(e => e.UnitType)
            .HasMaxLength(50);

        builder.Property(e => e.ConversionToBase)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne(e => e.BaseUnit)
            .WithMany()
            .HasForeignKey(e => e.BaseUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.BaseUnitId);
    }
}
