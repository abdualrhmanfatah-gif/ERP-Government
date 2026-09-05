namespace ERP_Government.Domain.Common;

public class OutboxMessage : BaseEntity
{
    public string TypeName { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string? AggregateId { get; init; }
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
}
