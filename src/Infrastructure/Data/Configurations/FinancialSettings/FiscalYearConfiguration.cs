using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.FinancialSettings;

public class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
{
    public void Configure(EntityTypeBuilder<FiscalYear> builder)
    {
        builder.ToTable("FiscalYears");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.IsClosed)
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.YearNumber)
            .IsUnique();
    }
}
