using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class RecurringEntryExecutionLogConfiguration : IEntityTypeConfiguration<RecurringEntryExecutionLog>
{
    public void Configure(EntityTypeBuilder<RecurringEntryExecutionLog> builder)
    {
        builder.ToTable("RecurringEntryExecutionLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExecutionDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.TriggeredBy)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Unique index: one successful log per entry per date
        builder.HasIndex(e => new { e.RecurringEntryId, e.ExecutionDate })
            .IsUnique();

        builder.HasIndex(e => e.RecurringEntryId);

        builder.HasIndex(e => e.Status);

        // FK: RecurringEntry
        builder.HasOne(e => e.RecurringEntry)
            .WithMany(r => r.ExecutionLogs)
            .HasForeignKey(e => e.RecurringEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: GeneratedJournalEntry (optional)
        builder.HasOne(e => e.GeneratedJournalEntry)
            .WithMany()
            .HasForeignKey(e => e.GeneratedJournalEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
