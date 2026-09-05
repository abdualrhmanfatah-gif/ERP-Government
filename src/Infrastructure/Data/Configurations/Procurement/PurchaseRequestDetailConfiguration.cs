using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class PurchaseRequestDetailConfiguration : IEntityTypeConfiguration<PurchaseRequestDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseRequestDetail> builder)
    {
        builder.ToTable("PurchaseRequestDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversionFactor)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.RequestedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.ApprovedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.UnitCostEstimate)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.TotalCostEstimate)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .HasMaxLength(50);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.PurchaseRequest)
            .WithMany()
            .HasForeignKey(e => e.PurchaseRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PurchaseRequestId);
        builder.HasIndex(e => e.ItemId);
    }
}
