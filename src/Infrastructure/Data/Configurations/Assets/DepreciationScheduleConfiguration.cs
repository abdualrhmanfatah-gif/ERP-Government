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

        builder.Property(e => e.DepreciationMethod)
            .HasMaxLength(50);

        builder.Property(e => e.DepreciationBase)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.DepreciationRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.AccumulatedDepreciation)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.NetBookValue)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ReversalReason)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReversalOf)
            .WithMany()
            .HasForeignKey(e => e.ReversalOfId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.FiscalYearId);
        builder.HasIndex(e => e.FiscalPeriodId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ReversalOfId);
    }
}
