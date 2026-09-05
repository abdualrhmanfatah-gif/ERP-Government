using ERP_Government.Domain.Accounting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Accounting;

public class JournalConfiguration : IEntityTypeConfiguration<Journal>
{
    public void Configure(EntityTypeBuilder<Journal> builder)
    {
        builder.ToTable("Journals");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.AccountId);

        builder.HasIndex(e => e.SuspenseAccountId);

        builder.HasIndex(e => e.SequenceId);

        builder.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SuspenseAccount)
            .WithMany()
            .HasForeignKey(e => e.SuspenseAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
