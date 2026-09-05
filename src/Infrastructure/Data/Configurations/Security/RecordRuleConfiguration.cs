using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class RecordRuleConfiguration : IEntityTypeConfiguration<RecordRule>
{
    public void Configure(EntityTypeBuilder<RecordRule> builder)
    {
        builder.ToTable("RecordRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EntityName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.RuleType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.AccessScope)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.DomainFilter)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.EntityName, e.RoleId, e.RuleType })
            .IsUnique();

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
