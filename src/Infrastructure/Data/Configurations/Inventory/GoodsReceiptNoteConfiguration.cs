using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Inventory;

public class GoodsReceiptNoteConfiguration : IEntityTypeConfiguration<GoodsReceiptNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNote> builder)
    {
        builder.ToTable("GoodsReceiptNotes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GRNNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.PurchaseOrderNumber)
            .HasMaxLength(100);

        builder.Property(e => e.InvoiceNumber)
            .HasMaxLength(100);

        builder.Property(e => e.TotalQuantity)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(23,6)");

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.GRNNumber)
            .IsUnique();

        builder.HasIndex(e => e.PurchaseOrderId);
        builder.HasIndex(e => e.WarehouseId);
        builder.HasIndex(e => e.LocationId);
    }
}
