using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetDisposalConfiguration : IEntityTypeConfiguration<AssetDisposal>
{
    public void Configure(EntityTypeBuilder<AssetDisposal> builder)
    {
        builder.ToTable("AssetDisposals");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DisposalNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DisposalMethod)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.BuyerName)
            .HasMaxLength(200);

        builder.Property(e => e.BuyerContact)
            .HasMaxLength(200);

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.BookValueAtDisposal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.AccumulatedDepreciationAtDisposal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.SaleProceeds)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.DisposalCost)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.NetProceeds)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.GainOrLoss)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.DisposalNumber)
            .IsUnique();

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.DisposalDate);
    }
}
