using ERP_Government.Domain.Committees.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Committees;

public class CommitteeMemberConfiguration : IEntityTypeConfiguration<CommitteeMember>
{
    public void Configure(EntityTypeBuilder<CommitteeMember> builder)
    {
        builder.ToTable("CommitteeMembers");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.MemberName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.MemberRole)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.CommitteeId);

        builder.HasIndex(e => e.EmployeeId);

        builder.HasOne(e => e.Committee)
            .WithMany()
            .HasForeignKey(e => e.CommitteeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
