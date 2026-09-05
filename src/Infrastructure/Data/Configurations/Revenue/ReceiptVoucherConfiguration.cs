using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class ReceiptVoucherConfiguration : IEntityTypeConfiguration<ReceiptVoucher>
{
    public void Configure(EntityTypeBuilder<ReceiptVoucher> builder)
    {
        builder.ToTable("ReceiptVouchers");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.VoucherNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ReceivedFrom)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(500);

        builder.Property(e => e.PaymentMethod)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.VoucherNumber)
            .IsUnique();

        builder.HasIndex(e => e.PartyId);

        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => e.VoucherDate);

        builder.HasIndex(e => e.DepositSlipId);

        builder.HasOne(e => e.Party)
            .WithMany()
            .HasForeignKey(e => e.PartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DepositSlip)
            .WithMany(s => s.ReceiptVouchers)
            .HasForeignKey(e => e.DepositSlipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
