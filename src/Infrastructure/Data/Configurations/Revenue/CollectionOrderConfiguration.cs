using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class CollectionOrderConfiguration : IEntityTypeConfiguration<CollectionOrder>
{
    public void Configure(EntityTypeBuilder<CollectionOrder> builder)
    {
        builder.ToTable("CollectionOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.AuthorizedAmount)
            .HasPrecision(23, 2)
            .IsRequired();

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        builder.Property(o => o.RowVersion)
            .IsRowVersion();

        builder.HasOne(o => o.RevenueClaim)
            .WithMany(c => c.CollectionOrders)
            .HasForeignKey(o => o.RevenueClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.ReceiptVouchers)
            .WithOne(v => v.CollectionOrder)
            .HasForeignKey(v => v.CollectionOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
