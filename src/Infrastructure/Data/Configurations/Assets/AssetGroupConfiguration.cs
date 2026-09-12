using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetGroupConfiguration : IEntityTypeConfiguration<AssetGroup>
{
    public void Configure(EntityTypeBuilder<AssetGroup> builder)
    {
        builder.ToTable("AssetGroups");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.DepreciationMethod)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.AssetCategory)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DepreciationRate)
            .HasPrecision(18, 4);

        builder.Property(e => e.ResidualValuePercentage)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne(e => e.ParentAssetGroup)
            .WithMany()
            .HasForeignKey(e => e.ParentAssetGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
