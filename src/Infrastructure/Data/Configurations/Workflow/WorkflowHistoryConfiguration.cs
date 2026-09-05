using ERP_Government.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Workflow;

public class WorkflowHistoryConfiguration : IEntityTypeConfiguration<WorkflowHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowHistory> builder)
    {
        builder.ToTable("WorkflowHistory");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Decision)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(500);

        builder.Property(e => e.EvaluationSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.HasOne(e => e.WorkflowInstance)
            .WithMany(i => i.History)
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Step)
            .WithMany(s => s.HistoryRecords)
            .HasForeignKey(e => e.StepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}