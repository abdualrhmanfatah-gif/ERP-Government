using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.AcquisitionType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.OriginalValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.AcquisitionCost)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ResidualValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RelinquishmentValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.AccumulatedDepreciation)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.CurrentValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.AssetTag)
            .HasMaxLength(50);

        builder.Property(e => e.Barcode)
            .HasMaxLength(100);

        builder.Property(e => e.SerialNumber)
            .HasMaxLength(100);

        builder.Property(e => e.ImageUrl)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne(e => e.AssetGroup)
            .WithMany()
            .HasForeignKey(e => e.AssetGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetGroupId);
        builder.HasIndex(e => e.LocationId);
        builder.HasIndex(e => e.FundId);
        builder.HasIndex(e => e.CostCenterId);
        builder.HasIndex(e => e.CustodianId);
        builder.HasIndex(e => e.Status);
    }
}
