using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Payments;

public class DisbursementRequestConfiguration : IEntityTypeConfiguration<DisbursementRequest>
{
    public void Configure(EntityTypeBuilder<DisbursementRequest> builder)
    {
        builder.ToTable("DisbursementRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RequestNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(DisbursementRequestStatus.Draft);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.RequestNumber)
            .IsUnique();

        builder.HasIndex(e => e.PaymentOrderId)
            .IsUnique();

        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.PaymentOrder)
            .WithMany()
            .HasForeignKey(e => e.PaymentOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
