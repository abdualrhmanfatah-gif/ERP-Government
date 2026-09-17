using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetDisposalDetailConfiguration : IEntityTypeConfiguration<AssetDisposalDetail>
{
    public void Configure(EntityTypeBuilder<AssetDisposalDetail> builder)
    {
        builder.ToTable("AssetDisposalDetails");

        builder.Ignore(e => e.Id);
        builder.HasKey(e => e.AssetTransactionId);

        builder.Property(e => e.DisposalMethod)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.BookValueAtDisposal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.AccumulatedDepreciationAtDisposal)
            .HasPrecision(23, 6);

        builder.Property(e => e.SaleProceeds)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.DisposalCost)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.NetProceeds)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.GainOrLoss)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.BuyerName)
            .HasMaxLength(200);

        builder.Property(e => e.BuyerContact)
            .HasMaxLength(100);

        builder.HasOne(e => e.AssetTransaction)
            .WithMany()
            .HasForeignKey(e => e.AssetTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
