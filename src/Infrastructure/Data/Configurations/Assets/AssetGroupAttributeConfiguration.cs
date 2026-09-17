using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetGroupAttributeConfiguration : IEntityTypeConfiguration<AssetGroupAttribute>
{
    public void Configure(EntityTypeBuilder<AssetGroupAttribute> builder)
    {
        builder.ToTable("AssetGroupAttributes");

        builder.Ignore(e => e.Id);
        builder.HasKey(e => new { e.AssetGroupId, e.AssetAttributeDefinitionId });

        builder.HasOne(e => e.AssetGroup)
            .WithMany()
            .HasForeignKey(e => e.AssetGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssetAttributeDefinition)
            .WithMany()
            .HasForeignKey(e => e.AssetAttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
