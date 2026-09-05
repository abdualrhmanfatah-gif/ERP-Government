using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class SecurityRoleConfiguration : IEntityTypeConfiguration<SecurityRole>
{
    public void Configure(EntityTypeBuilder<SecurityRole> builder)
    {
        builder.ToTable("SecurityRoles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.RoleLevel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.IsAdmin)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasOne(e => e.ExclusiveWithRole)
            .WithMany()
            .HasForeignKey(e => e.ExclusiveWithRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
