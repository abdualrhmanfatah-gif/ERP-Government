using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetPhysicalCountConfiguration : IEntityTypeConfiguration<AssetPhysicalCount>
{
    public void Configure(EntityTypeBuilder<AssetPhysicalCount> builder)
    {
        builder.ToTable("AssetPhysicalCounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CountNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CountType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.CountNumber)
            .IsUnique();

        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.Status);
    }
}
