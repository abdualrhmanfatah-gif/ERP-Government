using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class CashFlowMappingRuleConfiguration : IEntityTypeConfiguration<CashFlowMappingRule>
{
    public void Configure(EntityTypeBuilder<CashFlowMappingRule> builder)
    {
        builder.ToTable("CashFlowMappingRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(200);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.AccountGroupId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasOne(e => e.AccountGroup)
            .WithMany()
            .HasForeignKey(e => e.AccountGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
