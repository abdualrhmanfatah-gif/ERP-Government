using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class RevenueClaimConfiguration : IEntityTypeConfiguration<RevenueClaim>
{
    public void Configure(EntityTypeBuilder<RevenueClaim> builder)
    {
        builder.ToTable("RevenueClaims");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClaimNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.ClaimNumber)
            .IsUnique();

        builder.Property(c => c.TotalAmount)
            .HasPrecision(23, 2)
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasMaxLength(500);

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        builder.HasOne(c => c.Party)
            .WithMany()
            .HasForeignKey(c => c.PartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.CollectionOrders)
            .WithOne(o => o.RevenueClaim)
            .HasForeignKey(o => o.RevenueClaimId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
