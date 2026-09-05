using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetImpairmentConfiguration : IEntityTypeConfiguration<AssetImpairment>
{
    public void Configure(EntityTypeBuilder<AssetImpairment> builder)
    {
        builder.ToTable("AssetImpairments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ImpairmentNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ImpairmentReason)
            .HasMaxLength(50);

        builder.Property(e => e.ImpairmentDescription)
            .HasMaxLength(2000);

        builder.Property(e => e.AssessedBy)
            .HasMaxLength(200);

        builder.Property(e => e.AssessmentReportNumber)
            .HasMaxLength(100);

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.ReversalReason)
            .HasMaxLength(500);

        builder.Property(e => e.CarryingAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RecoverableAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ImpairmentLoss)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.ImpairmentNumber)
            .IsUnique();

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReversalOf)
            .WithMany()
            .HasForeignKey(e => e.ReversalOfId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.ImpairmentDate);
        builder.HasIndex(e => e.ReversalOfId);
    }
}
