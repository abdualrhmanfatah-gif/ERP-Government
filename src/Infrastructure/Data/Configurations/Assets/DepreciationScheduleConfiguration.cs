using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class DepreciationScheduleConfiguration : IEntityTypeConfiguration<DepreciationSchedule>
{
    public void Configure(EntityTypeBuilder<DepreciationSchedule> builder)
    {
        builder.ToTable("DepreciationSchedules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Method)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.DepreciationBase)
            .HasPrecision(23, 6);

        builder.Property(e => e.Rate)
            .HasPrecision(18, 6);

        builder.Property(e => e.ResidualValue)
            .HasPrecision(23, 6);

        builder.Property(e => e.OpeningBookValue)
            .HasPrecision(23, 6);

        builder.Property(e => e.OpeningAccumulatedDepreciation)
            .HasPrecision(23, 6);

        builder.Property(e => e.DepreciationAmount).HasPrecision(23, 6);
        builder.Property(e => e.ClosingAccumulatedDepreciation).HasPrecision(23, 6);
        builder.Property(e => e.ClosingBookValue).HasPrecision(23, 6);

        builder.HasOne(e => e.DepreciationRun)
            .WithMany(e => e.Schedules)
            .HasForeignKey(e => e.DepreciationRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DepreciationRunId);
        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => new { e.DepreciationRunId, e.AssetId }).IsUnique();
    }
}
