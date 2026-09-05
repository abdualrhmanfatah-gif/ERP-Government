using ERP_Government.Domain.Parties.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Parties;

public class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> builder)
    {
        builder.ToTable("Parties");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PartyCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.NameAr)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.NameEn)
            .HasMaxLength(500);

        builder.Property(e => e.TaxNumber)
            .HasMaxLength(50);

        builder.Property(e => e.NationalId)
            .HasMaxLength(50);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.Address)
            .HasMaxLength(1000);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.PartyCode)
            .IsUnique();

        builder.HasIndex(e => new { e.PartyType, e.IsActive });

        builder.HasIndex(e => e.TaxNumber);

        builder.HasIndex(e => e.NameAr);

        builder.Ignore(e => e.DomainEvents);
    }
}
