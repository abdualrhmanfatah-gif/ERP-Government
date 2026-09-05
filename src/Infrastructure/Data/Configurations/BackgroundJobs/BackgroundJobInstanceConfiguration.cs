using ERP_Government.Domain.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.BackgroundJobs;

public class BackgroundJobInstanceConfiguration : IEntityTypeConfiguration<BackgroundJobInstance>
{
    public void Configure(EntityTypeBuilder<BackgroundJobInstance> builder)
    {
        builder.ToTable("BackgroundJobInstances");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.RetryCount)
            .HasDefaultValue(0);

        builder.Property(e => e.IdempotencyKey)
            .HasMaxLength(256);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(int.MaxValue); // nvarchar(max)

        builder.Property(e => e.TriggeredBy)
            .HasMaxLength(64)
            .IsRequired();

        // Unique constraint: one instance per definition per scheduled time
        builder.HasIndex(e => new { e.JobDefinitionId, e.ScheduledAt })
            .IsUnique()
            .HasDatabaseName("IX_BackgroundJobInstances_DefinitionId_ScheduledAt");

        // Polling query optimization
        builder.HasIndex(e => new { e.Status, e.ScheduledAt })
            .HasDatabaseName("IX_BackgroundJobInstances_Status_ScheduledAt");

        // FK to BackgroundJobDefinition
        builder.HasOne(e => e.JobDefinition)
            .WithMany()
            .HasForeignKey(e => e.JobDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}
