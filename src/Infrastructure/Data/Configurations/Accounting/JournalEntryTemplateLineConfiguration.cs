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

        builder.Property(e => e.ExchangeRate)
            .HasColumnType("decimal(18,6)");

        builder.Property(e => e.Debit)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Credit)
            .HasColumnType("decimal(23,2)");

        builder.Property(e => e.Description)
            .HasMaxLength(200);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.TemplateId);

        builder.HasIndex(e => e.AccountId);

        builder.HasIndex(e => e.CostCenterId);

        builder.HasIndex(e => e.CurrencyId);

        builder.HasOne(e => e.Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CostCenter)
            .WithMany()
            .HasForeignKey(e => e.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
