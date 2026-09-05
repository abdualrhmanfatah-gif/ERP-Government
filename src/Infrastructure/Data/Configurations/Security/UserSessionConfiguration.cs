using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SessionTokenHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.RefreshTokenHash)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.UserAgent)
            .HasMaxLength(300);

        builder.Property(e => e.DeviceFingerprint)
            .HasMaxLength(200);

        builder.Property(e => e.LogoutReason)
            .HasMaxLength(50);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.SessionTokenHash)
            .IsUnique();

        builder.HasIndex(e => e.RefreshTokenHash)
            .IsUnique();

        builder.HasIndex(e => e.UserId);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
