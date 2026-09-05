using ERP_Government.Domain.BackgroundJobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.BackgroundJobs;

public class BackgroundJobExecutionLogConfiguration : IEntityTypeConfiguration<BackgroundJobExecutionLog>
{
    public void Configure(EntityTypeBuilder<BackgroundJobExecutionLog> builder)
    {
        builder.ToTable("BackgroundJobExecutionLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.AttemptNumber)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(int.MaxValue); // nvarchar(max)

        builder.Property(e => e.StackTrace)
            .HasMaxLength(int.MaxValue); // nvarchar(max)

        // Index for querying by instance
        builder.HasIndex(e => e.JobInstanceId)
            .HasDatabaseName("IX_BackgroundJobExecutionLogs_InstanceId");

        // FK to BackgroundJobInstance
        builder.HasOne(e => e.JobInstance)
            .WithMany(i => i.ExecutionLogs)
            .HasForeignKey(e => e.JobInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}
