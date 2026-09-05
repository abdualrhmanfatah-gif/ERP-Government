using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class SecurityPermissionConfiguration : IEntityTypeConfiguration<SecurityPermission>
{
    public void Configure(EntityTypeBuilder<SecurityPermission> builder)
    {
        builder.ToTable("SecurityPermissions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Module)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.PermissionLevel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.DataScope)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => new { e.Module, e.Action })
            .IsUnique();

        builder.Ignore(e => e.DomainEvents);
    }
}
