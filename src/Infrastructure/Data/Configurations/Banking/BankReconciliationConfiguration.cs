using ERP_Government.Domain.Banking.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Banking;

public class BankReconciliationConfiguration : IEntityTypeConfiguration<BankReconciliation>
{
    public void Configure(EntityTypeBuilder<BankReconciliation> builder)
    {
        builder.ToTable("BankReconciliations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.BookBalance)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.StatementBalance)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.AdjustedBalance)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Difference)
            .HasColumnType("decimal(23,2)")
            .HasSentinel(null);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.BankAccountId);

        builder.HasIndex(e => e.StatementId);

        builder.HasIndex(e => e.PreparedById);

        builder.HasIndex(e => e.ApprovedById);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500);

        builder.HasOne(e => e.Statement)
            .WithMany()
            .HasForeignKey(e => e.StatementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
