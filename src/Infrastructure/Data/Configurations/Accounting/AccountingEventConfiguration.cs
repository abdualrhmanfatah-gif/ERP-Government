using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class AccountingEventConfiguration : IEntityTypeConfiguration<AccountingEvent>
{
    public void Configure(EntityTypeBuilder<AccountingEvent> builder)
    {
        builder.ToTable("AccountingEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.SourceDocumentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.EventCategory)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.JournalEntryId);

        // Unique constraint: prevent double-posting
        builder.HasIndex(e => new { e.EventType, e.SourceDocumentType, e.SourceDocumentId, e.Status })
            .IsUnique()
            .HasFilter("[Status] = 'Posted'");

        // Same-module FK (set after processing)
        builder.HasOne(e => e.JournalEntry)
            .WithMany()
            .HasForeignKey(e => e.JournalEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
