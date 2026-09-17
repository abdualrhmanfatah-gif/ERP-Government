using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetPhysicalCountDetailConfiguration : IEntityTypeConfiguration<AssetPhysicalCountDetail>
{
    public void Configure(EntityTypeBuilder<AssetPhysicalCountDetail> builder)
    {
        builder.ToTable("AssetPhysicalCountDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SystemStatus)
            .HasMaxLength(50);

        builder.Property(e => e.PhysicalStatus)
            .HasMaxLength(50);

        builder.Property(e => e.DiscrepancyNotes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.AssetPhysicalCountId, e.AssetId })
            .IsUnique();

        builder.HasOne(e => e.AssetPhysicalCount)
            .WithMany()
            .HasForeignKey(e => e.AssetPhysicalCountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SystemLocation)
            .WithMany()
            .HasForeignKey(e => e.SystemLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PhysicalLocation)
            .WithMany()
            .HasForeignKey(e => e.PhysicalLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SystemEmployee)
            .WithMany()
            .HasForeignKey(e => e.SystemEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PhysicalEmployee)
            .WithMany()
            .HasForeignKey(e => e.PhysicalEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetPhysicalCountId);
        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.SystemLocationId);
        builder.HasIndex(e => e.PhysicalLocationId);
        builder.HasIndex(e => e.SystemEmployeeId);
        builder.HasIndex(e => e.PhysicalEmployeeId);
    }
}
