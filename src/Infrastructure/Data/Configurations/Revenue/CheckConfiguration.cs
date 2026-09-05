using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class CheckConfiguration : IEntityTypeConfiguration<Check>
{
    public void Configure(EntityTypeBuilder<Check> builder)
    {
        builder.ToTable("Checks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.BankName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CheckNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)")
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.ReceiptVoucherId);

        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => e.ReplacementVoucherId);

        builder.HasOne(e => e.ReceiptVoucher)
            .WithMany(v => v.Checks)
            .HasForeignKey(e => e.ReceiptVoucherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReplacementVoucher)
            .WithMany()
            .HasForeignKey(e => e.ReplacementVoucherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
