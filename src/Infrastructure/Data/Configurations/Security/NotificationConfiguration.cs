using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NotificationType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.DocumentType)
            .HasMaxLength(50);

        builder.Property(e => e.Priority)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.IsRead });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
