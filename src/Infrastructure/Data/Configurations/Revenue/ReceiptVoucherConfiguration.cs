using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class ReceiptVoucherConfiguration : IEntityTypeConfiguration<ReceiptVoucher>
{
    public void Configure(EntityTypeBuilder<ReceiptVoucher> builder)
    {
        builder.ToTable("ReceiptVouchers");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VoucherNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(v => v.VoucherNumber)
            .IsUnique();

        builder.Property(v => v.ReceivedFrom)
            .HasMaxLength(200);

        builder.Property(v => v.Notes)
            .HasMaxLength(500);

        builder.Property(v => v.CancellationReason)
            .HasMaxLength(500);

        builder.Property(v => v.RowVersion)
            .IsRowVersion();

        builder.HasOne(v => v.CollectionOrder)
            .WithMany(o => o.ReceiptVouchers)
            .HasForeignKey(v => v.CollectionOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Party)
            .WithMany()
            .HasForeignKey(v => v.PartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.DepositSlip47)
            .WithMany(s => s.ReceiptVouchers)
            .HasForeignKey(v => v.DepositSlip47Id)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(v => v.Lines)
            .WithOne(l => l.ReceiptVoucher)
            .HasForeignKey(l => l.ReceiptVoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Checks)
            .WithOne(c => c.ReceiptVoucher)
            .HasForeignKey(c => c.ReceiptVoucherId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
