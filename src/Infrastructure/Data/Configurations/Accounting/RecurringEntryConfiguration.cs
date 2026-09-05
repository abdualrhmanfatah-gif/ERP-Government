using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class RecurringEntryConfiguration : IEntityTypeConfiguration<RecurringEntry>
{
    public void Configure(EntityTypeBuilder<RecurringEntry> builder)
    {
        builder.ToTable("RecurringEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntryNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.DescriptionTemplate)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.EntryNumber)
            .IsUnique();

        builder.HasIndex(e => e.TemplateId);

        builder.HasIndex(e => e.JournalId);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasIndex(e => e.FundId);

        builder.HasIndex(e => e.CostCenterId);

        builder.HasIndex(e => e.ProjectId);

        builder.HasIndex(e => e.GeneratedJournalEntryId);

        // Same-module FKs
        builder.HasOne(e => e.Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Journal)
            .WithMany()
            .HasForeignKey(e => e.JournalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.GeneratedJournalEntry)
            .WithMany()
            .HasForeignKey(e => e.GeneratedJournalEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cross-module FKs (Module 3)
        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deferred FK to Module 5 — Funds table not yet implemented
        // builder.HasOne(e => e.Fund)
        //     .WithMany()
        //     .HasForeignKey(e => e.FundId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // Execution history
        builder.HasMany(e => e.ExecutionLogs)
            .WithOne(r => r.RecurringEntry)
            .HasForeignKey(r => r.RecurringEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
