using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class DepositSlipConfiguration : IEntityTypeConfiguration<DepositSlip>
{
    public void Configure(EntityTypeBuilder<DepositSlip> builder)
    {
        builder.ToTable("DepositSlips");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SlipNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.FormType)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(23,2)")
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.SlipNumber)
            .IsUnique();

        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => e.FormType);

        builder.HasIndex(e => e.SlipDate);

        builder.Ignore(e => e.DomainEvents);
    }
}
