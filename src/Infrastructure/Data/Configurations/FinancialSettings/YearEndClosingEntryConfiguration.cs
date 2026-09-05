using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.FinancialSettings;

public class YearEndClosingEntryConfiguration : IEntityTypeConfiguration<YearEndClosingEntry>
{
    public void Configure(EntityTypeBuilder<YearEndClosingEntry> builder)
    {
        builder.ToTable("YearEndClosingEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClosingEntryNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(e => e.FiscalYear)
            .WithMany()
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.IsReversal)
            .HasDefaultValue(false);

        builder.HasOne(e => e.ReversalOf)
            .WithMany()
            .HasForeignKey(e => e.ReversalOfId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.ClosingEntryNumber)
            .IsUnique();
    }
}
