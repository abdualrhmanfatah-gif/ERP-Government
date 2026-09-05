using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.FinancialSettings;

public class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("FiscalPeriods");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.FiscalYear)
            .WithMany(e => e.Periods)
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.IsLockedForPosting)
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.FiscalYearId, e.PeriodNumber })
            .IsUnique();
    }
}
