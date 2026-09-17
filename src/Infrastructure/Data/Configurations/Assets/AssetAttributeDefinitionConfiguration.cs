using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetAttributeDefinitionConfiguration : IEntityTypeConfiguration<AssetAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AssetAttributeDefinition> builder)
    {
        builder.ToTable("AssetAttributeDefinitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Unit)
            .HasMaxLength(50);

        builder.Property(e => e.SortOrder);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();
    }
}
