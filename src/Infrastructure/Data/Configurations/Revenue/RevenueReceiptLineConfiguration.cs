using ERP_Government.Domain.Revenue.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Revenue;

public class RevenueReceiptLineConfiguration : IEntityTypeConfiguration<RevenueReceiptLine>
{
    public void Configure(EntityTypeBuilder<RevenueReceiptLine> builder)
    {
        builder.ToTable("RevenueReceiptLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.ReceiptId);

        builder.HasIndex(e => e.AccountId);

        builder.HasOne(e => e.Receipt)
            .WithMany()
            .HasForeignKey(e => e.ReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
