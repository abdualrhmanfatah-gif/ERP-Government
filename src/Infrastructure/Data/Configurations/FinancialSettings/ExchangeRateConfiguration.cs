using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.FinancialSettings;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");

        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.BaseCurrency)
            .WithMany()
            .HasForeignKey(e => e.BaseCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.RateDate)
            .IsRequired();

        builder.Property(e => e.RateType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Rate)
            .HasColumnType("decimal(18,6)")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.BaseCurrencyId, e.CurrencyId, e.RateDate, e.RateType })
            .IsUnique();
    }
}
