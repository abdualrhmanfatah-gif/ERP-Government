using ERP_Government.Domain.Banking.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Banking;

public class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.ToTable("BankStatements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.BalanceStart)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.BalanceEnd)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.BalanceEndComputed)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.ImportSource)
            .HasMaxLength(100);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.BankAccountId);

        builder.HasIndex(e => e.JournalId);

        builder.Ignore(e => e.DomainEvents);
    }
}
