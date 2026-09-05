using ERP_Government.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_Government.Infrastructure.Data.Configurations.Common;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TypeName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.Payload)
            .IsRequired();

        builder.Property(e => e.AggregateId)
            .HasMaxLength(100);

        builder.Property(e => e.CorrelationId)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.RetryCount)
            .HasDefaultValue(0);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(int.MaxValue); // nvarchar(max)

        // Indexes for polling query performance
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_OutboxMessages_Status_CreatedAt");

        builder.HasIndex(e => e.AggregateId)
            .HasDatabaseName("IX_OutboxMessages_AggregateId");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("IX_OutboxMessages_CorrelationId");

        builder.HasIndex(e => e.NextRetryAt)
            .HasDatabaseName("IX_OutboxMessages_NextRetryAt");

        builder.Ignore(e => e.DomainEvents);
    }
}
