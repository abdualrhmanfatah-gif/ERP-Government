using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Assets;

public class AssetImpairmentDetailConfiguration : IEntityTypeConfiguration<AssetImpairmentDetail>
{
    public void Configure(EntityTypeBuilder<AssetImpairmentDetail> builder)
    {
        builder.ToTable("AssetImpairmentDetails");

        builder.Ignore(e => e.Id);
        builder.HasKey(e => e.AssetTransactionId);

        builder.Property(e => e.CarryingAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.RecoverableAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.LossAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.AssessorName)
            .HasMaxLength(200);

        builder.Property(e => e.ReportNumber)
            .HasMaxLength(100);

        builder.Property(e => e.ReversalReason)
            .HasMaxLength(500);

        builder.HasOne(e => e.AssetTransaction)
            .WithMany()
            .HasForeignKey(e => e.AssetTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReversalOfTransaction)
            .WithMany()
            .HasForeignKey(e => e.ReversalOfTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ReversalOfTransactionId)
            .IsUnique()
            .HasFilter("[ReversalOfTransactionId] IS NOT NULL");
    }
}
