using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetRevaluationDetailConfiguration : IEntityTypeConfiguration<AssetRevaluationDetail>
{
    public void Configure(EntityTypeBuilder<AssetRevaluationDetail> builder)
    {
        builder.ToTable("AssetRevaluationDetails");

        builder.Ignore(e => e.Id);
        builder.HasKey(e => e.AssetTransactionId);

        builder.Property(e => e.RevaluationMethod)
            .HasMaxLength(50);

        builder.Property(e => e.OldBookValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.NewBookValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RevaluationAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RevaluationType)
            .HasMaxLength(50);

        builder.Property(e => e.AppraiserName)
            .HasMaxLength(200);

        builder.Property(e => e.ReportNumber)
            .HasMaxLength(100);

        builder.HasOne(e => e.AssetTransaction)
            .WithMany()
            .HasForeignKey(e => e.AssetTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
