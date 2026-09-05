using ERP_Government.Domain.Organization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Organization;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.BudgetLimit)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.OrganizationUnitId);

        builder.HasOne(e => e.OrganizationUnit)
            .WithMany()
            .HasForeignKey(e => e.OrganizationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
