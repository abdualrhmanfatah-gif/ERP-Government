using ERP_Government.Domain.Committees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Committees;

public class CommitteeAssignmentConfiguration : IEntityTypeConfiguration<CommitteeAssignment>
{
    public void Configure(EntityTypeBuilder<CommitteeAssignment> builder)
    {
        builder.ToTable("CommitteeAssignments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AssignmentType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.DecisionNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.CommitteeId);

        builder.HasIndex(e => e.PurchaseOrderId);

        builder.HasOne(e => e.Committee)
            .WithMany()
            .HasForeignKey(e => e.CommitteeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
