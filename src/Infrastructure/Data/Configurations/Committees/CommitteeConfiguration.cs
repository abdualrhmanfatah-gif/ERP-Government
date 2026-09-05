using ERP_Government.Domain.Committees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Committees;

public class CommitteeConfiguration : IEntityTypeConfiguration<Committee>
{
    public void Configure(EntityTypeBuilder<Committee> builder)
    {
        builder.ToTable("Committees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CommitteeNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.CommitteeType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.FormationDecisionNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.CommitteeNumber)
            .IsUnique();

        builder.Ignore(e => e.DomainEvents);
    }
}
