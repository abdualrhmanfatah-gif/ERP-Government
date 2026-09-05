using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class ReceiptVoucherLineConfiguration : IEntityTypeConfiguration<ReceiptVoucherLine>
{
    public void Configure(EntityTypeBuilder<ReceiptVoucherLine> builder)
    {
        builder.ToTable("ReceiptVoucherLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(200);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.ReceiptVoucherId);

        builder.HasIndex(e => e.RevenueAccountId);

        builder.HasOne(e => e.ReceiptVoucher)
            .WithMany(v => v.Lines)
            .HasForeignKey(e => e.ReceiptVoucherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
