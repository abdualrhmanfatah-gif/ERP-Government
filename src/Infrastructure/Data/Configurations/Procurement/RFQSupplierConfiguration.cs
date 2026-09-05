using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class RFQSupplierConfiguration : IEntityTypeConfiguration<RFQSupplier>
{
    public void Configure(EntityTypeBuilder<RFQSupplier> builder)
    {
        builder.ToTable("RFQSuppliers");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.RequestForQuotation)
            .WithMany()
            .HasForeignKey(e => e.RFQId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.RFQId);
        builder.HasIndex(e => e.SupplierId);
        builder.HasIndex(e => e.Status);
    }
}
