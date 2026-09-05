using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class FieldSecurityPolicyConfiguration : IEntityTypeConfiguration<FieldSecurityPolicy>
{
    public void Configure(EntityTypeBuilder<FieldSecurityPolicy> builder)
    {
        builder.ToTable("FieldSecurityPolicies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FieldName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.AccessLevel)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.MaskingFormat)
            .HasMaxLength(50);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.EntityName, e.FieldName, e.RoleId })
            .IsUnique();

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
