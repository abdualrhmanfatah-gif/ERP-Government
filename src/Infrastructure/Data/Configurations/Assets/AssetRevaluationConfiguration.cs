using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetRevaluationConfiguration : IEntityTypeConfiguration<AssetRevaluation>
{
    public void Configure(EntityTypeBuilder<AssetRevaluation> builder)
    {
        builder.ToTable("AssetRevaluations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RevaluationNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.RevaluationMethod)
            .HasMaxLength(50);

        builder.Property(e => e.RevaluationType)
            .HasMaxLength(50);

        builder.Property(e => e.Appraiser)
            .HasMaxLength(200);

        builder.Property(e => e.AppraisalReportNumber)
            .HasMaxLength(100);

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(10);

        builder.Property(e => e.OldBookValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.NewBookValue)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RevaluationAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.RevaluationNumber)
            .IsUnique();

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.RevaluationDate);
    }
}
