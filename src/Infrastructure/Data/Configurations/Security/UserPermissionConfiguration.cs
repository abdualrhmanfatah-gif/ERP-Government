using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Reason)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.UserId, e.PermissionId })
            .IsUnique();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovedBy)
            .WithMany()
            .HasForeignKey(e => e.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
