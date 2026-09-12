using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class FundConfiguration : IEntityTypeConfiguration<Fund>
{
    public void Configure(EntityTypeBuilder<Fund> builder)
    {
        builder.ToTable("Funds");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FundNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.FundName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FundType)
            .HasConversion<int>();

        builder.Property(e => e.FundCategory)
            .HasConversion<int>();

        builder.Property(e => e.LegalAuthority)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.FundNumber)
            .IsUnique();

        builder.HasIndex(e => e.DefaultRevenueAccountId);

        builder.Ignore(e => e.DomainEvents);
    }
}
