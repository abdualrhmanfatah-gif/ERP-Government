using ERP_Government.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Workflow;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("WorkflowSteps");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.StepOrder)
            .IsRequired();

        builder.Property(e => e.StepType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.ConditionExpression)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(e => new { e.DefinitionId, e.StepOrder })
            .IsUnique();

        builder.HasOne(e => e.Definition)
            .WithMany(d => d.Steps)
            .HasForeignKey(e => e.DefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.EscalateToStep)
            .WithMany(s => s.EscalationTargets)
            .HasForeignKey(e => e.EscalateToStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}