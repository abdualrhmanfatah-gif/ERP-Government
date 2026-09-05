namespace ERP_Government.Domain.Events.Common;

/// <summary>
/// Common properties for all domain events that trigger accounting postings.
/// </summary>
public interface IHasSourceEntity
{
    /// <summary>
    /// Primary key of the source entity that raised this event.
    /// </summary>
    int SourceEntityId { get; }

    /// <summary>
    /// Entity type name (e.g., "PurchaseOrder", "PaymentOrder").
    /// Used as SourceTable in AccountingEvent.
    /// </summary>
    string SourceEntityType { get; }

    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
