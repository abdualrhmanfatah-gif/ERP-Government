using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class GoodsReceiptNoteDetailConfiguration : IEntityTypeConfiguration<GoodsReceiptNoteDetail>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNoteDetail> builder)
    {
        builder.ToTable("GoodsReceiptNoteDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversionFactor)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.OrderedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.ReceivedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.AcceptedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.RejectedQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.UnitCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.TotalCost)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.BatchNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.GoodsReceiptNote)
            .WithMany()
            .HasForeignKey(e => e.GRNId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Item)
            .WithMany()
            .HasForeignKey(e => e.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Unit)
            .WithMany()
            .HasForeignKey(e => e.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.GRNId);
        builder.HasIndex(e => e.ItemId);
    }
}
