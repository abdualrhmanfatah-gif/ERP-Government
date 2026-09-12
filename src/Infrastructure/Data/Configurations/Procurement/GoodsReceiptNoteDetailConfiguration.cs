using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class GoodsReceiptNoteDetailConfiguration : IEntityTypeConfiguration<GoodsReceiptNoteDetail>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNoteDetail> builder)
    {
        builder.ToTable("GoodsReceiptNoteDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderedQuantity)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.ReceivedQuantity)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(e => e.AcceptedQuantity)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.RejectedQuantity)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.RemainingQuantity)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.UnitCost)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.TotalCost)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.BatchNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.GoodsReceiptNote)
            .WithMany(g => g.Details)
            .HasForeignKey(e => e.GRNId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PurchaseOrderDetail)
            .WithMany()
            .HasForeignKey(e => e.PurchaseOrderDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.GRNId);
        builder.HasIndex(e => e.PurchaseOrderDetailId);
        builder.HasIndex(e => e.ItemId);
    }
}
