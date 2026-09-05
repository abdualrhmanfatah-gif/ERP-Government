using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class PostingRuleLineConfiguration : IEntityTypeConfiguration<PostingRuleLine>
{
    public void Configure(EntityTypeBuilder<PostingRuleLine> builder)
    {
        builder.ToTable("PostingRuleLines");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AccountSource)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.AmountSource)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.PostingRuleId);

        builder.HasIndex(e => e.FixedAccountId);

        builder.HasOne(e => e.PostingRule)
            .WithMany()
            .HasForeignKey(e => e.PostingRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FixedAccount)
            .WithMany()
            .HasForeignKey(e => e.FixedAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
