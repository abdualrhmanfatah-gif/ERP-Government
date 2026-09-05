using ERP_Government.Domain.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Security;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Login)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(200);

        builder.Property(e => e.MfaMethod)
            .HasMaxLength(20);

        builder.Property(e => e.AccountType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => e.Login)
            .IsUnique();

        builder.HasIndex(e => e.DepartmentId);

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
