using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class PostingRuleConfiguration : IEntityTypeConfiguration<PostingRule>
{
    public void Configure(EntityTypeBuilder<PostingRule> builder)
    {
        builder.ToTable("PostingRules");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(50)
            .IsRequired();

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
