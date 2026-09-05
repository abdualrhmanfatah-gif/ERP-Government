using ERP_Government.Domain.FinancialSettings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.FinancialSettings;

public class DocumentSequenceConfiguration : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("DocumentSequences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.DocumentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(e => e.FiscalYear)
            .WithMany()
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.CurrentNumber)
            .HasDefaultValue(1);

        builder.Property(e => e.ResetPolicy)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
