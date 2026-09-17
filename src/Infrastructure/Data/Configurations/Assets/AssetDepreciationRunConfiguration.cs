using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetDepreciationRunConfiguration : IEntityTypeConfiguration<AssetDepreciationRun>
{
    public void Configure(EntityTypeBuilder<AssetDepreciationRun> builder)
    {
        builder.ToTable("AssetDepreciationRuns");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RunNumber).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.TotalDepreciation).HasPrecision(23, 6);
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.PostedBy).HasMaxLength(450);
        builder.Property(e => e.ReversedBy).HasMaxLength(450);
        builder.Property(e => e.ReversalReason).HasMaxLength(500);
        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasIndex(e => e.RunNumber).IsUnique();
        builder.HasIndex(e => new { e.FiscalYearId, e.FiscalPeriodId }).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.JournalEntryId).IsUnique().HasFilter("[JournalEntryId] IS NOT NULL");
        builder.HasIndex(e => e.ReversalJournalEntryId).IsUnique().HasFilter("[ReversalJournalEntryId] IS NOT NULL");

        builder.HasOne(e => e.FiscalYear).WithMany().HasForeignKey(e => e.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FiscalPeriod).WithMany().HasForeignKey(e => e.FiscalPeriodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.JournalEntry).WithMany().HasForeignKey(e => e.JournalEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReversalJournalEntry).WithMany().HasForeignKey(e => e.ReversalJournalEntryId).OnDelete(DeleteBehavior.Restrict);
    }
}
