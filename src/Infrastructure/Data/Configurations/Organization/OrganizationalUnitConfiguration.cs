using ERP_Government.Domain.Organization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Organization;

public class OrganizationalUnitConfiguration : IEntityTypeConfiguration<OrganizationalUnit>
{
    public void Configure(EntityTypeBuilder<OrganizationalUnit> builder)
    {
        builder.ToTable("OrganizationalUnits");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.ParentPath)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.ParentId);

        builder.HasOne(e => e.Parent)
            .WithMany()
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
