using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntryNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Ref)
            .HasMaxLength(200);

        builder.Property(e => e.EntryType)
            .HasMaxLength(20);

        builder.Property(e => e.EntryStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Narration)
            .HasMaxLength(1000);

        builder.Property(e => e.ReversalReason)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.EntryNumber)
            .IsUnique();

        builder.HasIndex(e => e.JournalId);

        builder.HasIndex(e => e.PeriodId);

        builder.HasIndex(e => e.FiscalYearId);

        builder.HasIndex(e => e.SourceEventId);

        builder.HasIndex(e => e.ReversalOfId);

        builder.HasIndex(e => e.PostedById);

        builder.HasIndex(e => e.CancelledById);

        // Performance indexes for reports
        builder.HasIndex(e => new { e.EntryStatus, e.DocumentDate })
            .HasDatabaseName("IX_JournalEntries_EntryStatus_DocumentDate");

        // Self-referential FK
        builder.HasOne(e => e.ReversalOf)
            .WithMany()
            .HasForeignKey(e => e.ReversalOfId)
            .OnDelete(DeleteBehavior.Restrict);

        // Same-module FK
        builder.HasOne(e => e.Journal)
            .WithMany()
            .HasForeignKey(e => e.JournalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SourceEvent)
            .WithMany()
            .HasForeignKey(e => e.SourceEventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cross-module FKs (Module 1)
        builder.HasOne(e => e.Period)
            .WithMany()
            .HasForeignKey(e => e.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FiscalYear)
            .WithMany()
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);

        // User navigation FKs for audit metadata
        builder.HasOne(e => e.PostedBy)
            .WithMany()
            .HasForeignKey(e => e.PostedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CancelledBy)
            .WithMany()
            .HasForeignKey(e => e.CancelledById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
