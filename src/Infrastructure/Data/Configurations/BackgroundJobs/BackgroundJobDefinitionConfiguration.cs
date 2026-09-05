using ERP_Government.Domain.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.BackgroundJobs;

public class BackgroundJobDefinitionConfiguration : IEntityTypeConfiguration<BackgroundJobDefinition>
{
    public void Configure(EntityTypeBuilder<BackgroundJobDefinition> builder)
    {
        builder.ToTable("BackgroundJobDefinitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TypeName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.DisplayName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.ScheduleType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ScheduleValue)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.MaxRetries)
            .HasDefaultValue(5);

        builder.Property(e => e.TimeoutSeconds)
            .HasDefaultValue(300);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(e => e.TypeName)
            .IsUnique()
            .HasDatabaseName("IX_BackgroundJobDefinitions_TypeName");

        builder.Ignore(e => e.DomainEvents);
    }
}
