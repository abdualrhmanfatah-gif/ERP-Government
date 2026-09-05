using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Level)
            .HasColumnType("tinyint");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.AccountGroupId);

        builder.HasIndex(e => e.ParentId);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasOne(e => e.AccountGroup)
            .WithMany()
            .HasForeignKey(e => e.AccountGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Parent)
            .WithMany()
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
