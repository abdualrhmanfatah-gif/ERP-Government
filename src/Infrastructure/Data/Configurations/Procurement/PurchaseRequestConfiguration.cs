using ERP_Government.Domain.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Procurement;

public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("PurchaseRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RequestNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.TotalEstimatedCost)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RequesterName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.RequestNumber)
            .IsUnique();

        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestDate);
    }
}
