using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventCategory)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.DocumentType)
            .HasMaxLength(50);

        builder.Property(e => e.Action)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.FailureReason)
            .HasMaxLength(200);

        builder.Property(e => e.DeviceInfo)
            .HasMaxLength(300);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        builder.Property(e => e.SessionId)
            .HasMaxLength(100);

        builder.Property(e => e.ChangeSummary)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.Action);
        builder.HasIndex(e => new { e.DocumentType, e.DocumentId });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
