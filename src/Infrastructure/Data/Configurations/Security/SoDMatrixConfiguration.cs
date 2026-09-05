using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class SoDMatrixConfiguration : IEntityTypeConfiguration<SoDMatrix>
{
    public void Configure(EntityTypeBuilder<SoDMatrix> builder)
    {
        builder.ToTable("SoDMatrix");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RiskLevel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ActionOnViolation)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.PermissionAId, e.PermissionBId })
            .IsUnique();

        builder.HasOne(e => e.PermissionA)
            .WithMany()
            .HasForeignKey(e => e.PermissionAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PermissionB)
            .WithMany()
            .HasForeignKey(e => e.PermissionBId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
