using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class ApprovalRuleConfiguration : IEntityTypeConfiguration<ApprovalRule>
{
    public void Configure(EntityTypeBuilder<ApprovalRule> builder)
    {
        builder.ToTable("ApprovalRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DocumentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.AmountThreshold)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ApproverRole)
            .HasMaxLength(50);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.DocumentType, e.FundId, e.Sequence })
            .IsUnique();

        builder.HasOne(e => e.ApproverRoleNavigation)
            .WithMany()
            .HasForeignKey(e => e.ApproverRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
