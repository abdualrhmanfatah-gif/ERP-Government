using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class DepositSlip48Configuration : IEntityTypeConfiguration<DepositSlip48>
{
    public void Configure(EntityTypeBuilder<DepositSlip48> builder)
    {
        builder.ToTable("DepositSlips48");

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

        builder.HasMany(s => s.Checks)
            .WithOne(c => c.DepositSlip48)
            .HasForeignKey(c => c.DepositSlip48Id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
