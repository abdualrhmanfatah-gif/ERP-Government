using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetTransferDetailConfiguration : IEntityTypeConfiguration<AssetTransferDetail>
{
    public void Configure(EntityTypeBuilder<AssetTransferDetail> builder)
    {
        builder.ToTable("AssetTransferDetails");

        builder.Ignore(e => e.Id);
        builder.HasKey(e => e.AssetTransactionId);

        builder.HasOne(e => e.AssetTransaction)
            .WithMany()
            .HasForeignKey(e => e.AssetTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FromLocation)
            .WithMany()
            .HasForeignKey(e => e.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToLocation)
            .WithMany()
            .HasForeignKey(e => e.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FromEmployee)
            .WithMany()
            .HasForeignKey(e => e.FromEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToEmployee)
            .WithMany()
            .HasForeignKey(e => e.ToEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FromDepartment)
            .WithMany()
            .HasForeignKey(e => e.FromDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToDepartment)
            .WithMany()
            .HasForeignKey(e => e.ToDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.FromLocationId);
        builder.HasIndex(e => e.ToLocationId);
        builder.HasIndex(e => e.FromEmployeeId);
        builder.HasIndex(e => e.ToEmployeeId);
        builder.HasIndex(e => e.FromDepartmentId);
        builder.HasIndex(e => e.ToDepartmentId);
    }
}
