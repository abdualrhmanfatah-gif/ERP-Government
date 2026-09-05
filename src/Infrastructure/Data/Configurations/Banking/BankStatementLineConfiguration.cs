using ERP_Government.Domain.Banking.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Banking;

public class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("BankStatementLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Debit)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Credit)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Balance)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.Reference)
            .HasMaxLength(100);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.StatementId, e.LineNumber })
            .IsUnique();

        builder.HasIndex(e => e.StatementId);

        builder.HasIndex(e => e.JournalEntryLineId);

        builder.HasOne(e => e.Statement)
            .WithMany()
            .HasForeignKey(e => e.StatementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
