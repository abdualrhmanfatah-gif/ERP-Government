using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryTemplateLineConfiguration : IEntityTypeConfiguration<JournalEntryTemplateLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryTemplateLine> builder)
    {
        builder.ToTable("JournalEntryTemplateLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DebitAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.CreditAmount)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Description)
            .HasMaxLength(200);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.TemplateId);

        builder.HasIndex(e => e.AccountId);

        builder.HasIndex(e => e.FundId);

        builder.HasIndex(e => e.CostCenterId);

        builder.HasIndex(e => e.ProjectId);

        builder.HasIndex(e => e.OrganizationUnitId);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasOne(e => e.Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cross-module FKs (Module 3)
        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.OrganizationUnit)
            .WithMany()
            .HasForeignKey(e => e.OrganizationUnitId)
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

        builder.Ignore(e => e.DomainEvents);
    }
}
