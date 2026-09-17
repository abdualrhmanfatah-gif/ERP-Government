using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetAttributeValueConfiguration : IEntityTypeConfiguration<AssetAttributeValue>
{
    public void Configure(EntityTypeBuilder<AssetAttributeValue> builder)
    {
        builder.ToTable("AssetAttributeValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TextValue)
            .HasMaxLength(1000);

        builder.Property(e => e.IntegerValue);

        builder.Property(e => e.DecimalValue)
            .HasPrecision(18, 4);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.AssetId, e.AssetAttributeDefinitionId })
            .IsUnique();

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssetAttributeDefinition)
            .WithMany()
            .HasForeignKey(e => e.AssetAttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.AssetAttributeDefinitionId);
    }
}
