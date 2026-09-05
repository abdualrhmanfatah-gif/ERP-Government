using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class YearClosingRunConfiguration : IEntityTypeConfiguration<YearClosingRun>
{
    public void Configure(EntityTypeBuilder<YearClosingRun> builder)
    {
        builder.ToTable("YearClosingRuns");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RunAt)
            .IsRequired();

        builder.Property(e => e.LapsedAppropriationTotal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.LapsedEncumbranceTotal)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.FiscalYearId);

        builder.HasIndex(e => new { e.FiscalYearId, e.Status })
            .IsUnique()
            .HasFilter("[Status] = 0");

        builder.HasOne(e => e.FiscalYear)
            .WithMany()
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
