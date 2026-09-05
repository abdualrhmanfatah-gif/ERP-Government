using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("SecurityAuditLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventCategory)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EntityName)
            .HasMaxLength(50);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        builder.Property(e => e.DeviceInfo)
            .HasMaxLength(300);

        builder.Property(e => e.SessionId)
            .HasMaxLength(100);

        builder.Property(e => e.FailureReason)
            .HasMaxLength(500);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => new { e.EventCategory, e.Timestamp });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
