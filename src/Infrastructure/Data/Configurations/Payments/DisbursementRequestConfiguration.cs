using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Payments;

public class DisbursementRequestConfiguration : IEntityTypeConfiguration<DisbursementRequest>
{
    public void Configure(EntityTypeBuilder<DisbursementRequest> builder)
    {
        builder.ToTable("DisbursementRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RequestNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RequestedByName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.BeneficiaryName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.RequestedAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Purpose)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(DisbursementRequestStatus.Draft);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.RequestNumber)
            .IsUnique();

        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => e.RequestedById);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasIndex(e => e.FinancialYearId);

        builder.Ignore(e => e.DomainEvents);

        builder.HasOne<ERP_Government.Domain.Accounting.Entities.JournalEntry>()
            .WithMany()
            .HasForeignKey(e => e.AccrualJournalEntryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
