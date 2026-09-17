using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class CheckConfiguration : IEntityTypeConfiguration<Check>
{
    public void Configure(EntityTypeBuilder<Check> builder)
    {
        builder.ToTable("Checks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.BankName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.CheckNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Amount)
            .HasPrecision(23, 2)
            .IsRequired();

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        builder.HasOne(c => c.ReceiptVoucher)
            .WithMany(v => v.Checks)
            .HasForeignKey(c => c.ReceiptVoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.DepositSlip48)
            .WithMany(s => s.Checks)
            .HasForeignKey(c => c.DepositSlip48Id)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.ReplacementVoucher)
            .WithMany()
            .HasForeignKey(c => c.ReplacementVoucherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
