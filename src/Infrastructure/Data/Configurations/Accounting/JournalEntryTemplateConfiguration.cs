using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class JournalEntryTemplateConfiguration : IEntityTypeConfiguration<JournalEntryTemplate>
{
    public void Configure(EntityTypeBuilder<JournalEntryTemplate> builder)
    {
        builder.ToTable("JournalEntryTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TemplateName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.JournalId);

        builder.HasOne(e => e.Journal)
            .WithMany()
            .HasForeignKey(e => e.JournalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
