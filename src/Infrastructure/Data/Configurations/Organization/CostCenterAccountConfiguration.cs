using ERP_Government.Domain.Organization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Organization;

public class CostCenterAccountConfiguration : IEntityTypeConfiguration<CostCenterAccount>
{
    public void Configure(EntityTypeBuilder<CostCenterAccount> builder)
    {
        builder.ToTable("CostCenterAccounts");

        builder.HasKey(e => new { e.CostCenterId, e.AccountId });

        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
