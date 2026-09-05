using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetMovementConfiguration : IEntityTypeConfiguration<AssetMovement>
{
    public void Configure(EntityTypeBuilder<AssetMovement> builder)
    {
        builder.ToTable("AssetMovements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.MovementNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.MovementType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ReferenceType)
            .HasMaxLength(50);

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.OldValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.NewValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.MovementNumber)
            .IsUnique();

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.MovementType);
        builder.HasIndex(e => e.MovementDate);
        builder.HasIndex(e => e.ReferenceType);
        builder.HasIndex(e => e.ReferenceId);
    }
}
