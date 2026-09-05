using ERP_Government.Domain.Budgeting.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Budgeting;

public class FinalAccountConfiguration : IEntityTypeConfiguration<FinalAccount>
{
    public void Configure(EntityTypeBuilder<FinalAccount> builder)
    {
        builder.ToTable("FinalAccounts");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GeneratedAt)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.FiscalYearId)
            .IsUnique();

        builder.HasOne(e => e.FiscalYear)
            .WithMany()
            .HasForeignKey(e => e.FiscalYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
