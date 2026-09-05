using ERP_Government.Domain.Workflow.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Workflow;

public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Version)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasIndex(e => new { e.EntityName, e.Version })
            .IsUnique();

        builder.Ignore(e => e.DomainEvents);
    }
}