using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.ToTable("ApprovalHistory");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DocumentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.RequiredRole)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Decision)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(500);

        builder.Property(e => e.EvaluationSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.DocumentType, e.DocumentId });
        builder.HasIndex(e => e.ApproverUserId);
        builder.HasIndex(e => e.DecisionAt);

        builder.HasOne(e => e.ApproverUser)
            .WithMany()
            .HasForeignKey(e => e.ApproverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
