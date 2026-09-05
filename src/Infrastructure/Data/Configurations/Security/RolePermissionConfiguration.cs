using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

// FEATURE-008 — RolePermission EF configuration
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(e => new { e.RoleId, e.PermissionId });

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}
