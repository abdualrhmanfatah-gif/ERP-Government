using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.FinancialSettings;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Symbol)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(e => e.DecimalPlaces)
            .HasDefaultValue(2);

        builder.Property(e => e.RoundingPrecision)
            .HasColumnType("decimal(18,6)")
            .HasDefaultValue(0.01m);

        builder.Property(e => e.IsBase)
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();
    }
}
