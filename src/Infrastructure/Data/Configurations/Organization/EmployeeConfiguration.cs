using ERP_Government.Domain.Organization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Organization;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.JobTitle)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.JobGrade)
            .HasMaxLength(20);

        builder.Property(e => e.EmploymentStatus)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.EmployeeNumber)
            .IsUnique();

        builder.HasIndex(e => e.OrganizationalUnitId);

        builder.HasIndex(e => e.UserId);

        builder.HasOne(e => e.OrganizationalUnit)
            .WithMany()
            .HasForeignKey(e => e.OrganizationalUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
