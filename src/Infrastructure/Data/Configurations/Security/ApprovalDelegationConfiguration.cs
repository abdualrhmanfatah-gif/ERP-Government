using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class ApprovalDelegationConfiguration : IEntityTypeConfiguration<ApprovalDelegation>
{
    public void Configure(EntityTypeBuilder<ApprovalDelegation> builder)
    {
        builder.ToTable("ApprovalDelegations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityType)
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.DelegatorUser)
            .WithMany()
            .HasForeignKey(e => e.DelegatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DelegateUser)
            .WithMany()
            .HasForeignKey(e => e.DelegateUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
