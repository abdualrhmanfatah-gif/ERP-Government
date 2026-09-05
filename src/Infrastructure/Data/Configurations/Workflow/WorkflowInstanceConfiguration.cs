using ERP_Government.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Workflow;

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.StartedAt)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.EntityName, e.EntityId, e.Status })
            .IsUnique()
            .HasFilter("[Status] = 'InProgress'");

        builder.HasOne(e => e.Definition)
            .WithMany(d => d.Instances)
            .HasForeignKey(e => e.DefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentStep)
            .WithMany(s => s.CurrentInstances)
            .HasForeignKey(e => e.CurrentStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}