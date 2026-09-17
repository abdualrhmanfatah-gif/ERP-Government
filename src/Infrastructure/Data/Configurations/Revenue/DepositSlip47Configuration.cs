using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class DepositSlip47Configuration : IEntityTypeConfiguration<DepositSlip47>
{
    public void Configure(EntityTypeBuilder<DepositSlip47> builder)
    {
        builder.ToTable("DepositSlips47");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SlipNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(s => s.SlipNumber)
            .IsUnique();

        builder.Property(s => s.TotalAmount)
            .HasPrecision(23, 2);

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        builder.HasMany(s => s.ReceiptVouchers)
            .WithOne(v => v.DepositSlip47)
            .HasForeignKey(v => v.DepositSlip47Id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
