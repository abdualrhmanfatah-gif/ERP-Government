using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class ReceiptVoucherLineConfiguration : IEntityTypeConfiguration<ReceiptVoucherLine>
{
    public void Configure(EntityTypeBuilder<ReceiptVoucherLine> builder)
    {
        builder.ToTable("ReceiptVoucherLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Amount)
            .HasPrecision(23, 2)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasMaxLength(200);

        builder.Property(l => l.RowVersion)
            .IsRowVersion();

        builder.HasOne(l => l.ReceiptVoucher)
            .WithMany(v => v.Lines)
            .HasForeignKey(l => l.ReceiptVoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.RevenueAccount)
            .WithMany()
            .HasForeignKey(l => l.RevenueAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
