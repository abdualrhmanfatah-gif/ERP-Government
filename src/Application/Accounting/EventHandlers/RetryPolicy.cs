using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Determines retry eligibility for Failed AccountingEvents.
/// Max 3 automatic retries. Manual retry resets state.
/// </summary>
public class RetryPolicy
{
    public const int MaxRetries = 3;

    /// <summary>
    /// Returns true if the AccountingEvent is eligible for automatic retry.
    /// </summary>
    public bool CanRetry(AccountingEvent accountingEvent)
    {
        return accountingEvent.Status == EventStatus.Posted &&
               accountingEvent.RetryCount < MaxRetries;
    }

    /// <summary>
    /// Returns true if the AccountingEvent has exhausted all retries
    /// and requires manual operator intervention.
    /// </summary>
    public bool RequiresManualRetry(AccountingEvent accountingEvent)
    {
        return accountingEvent.Status == EventStatus.Posted &&
               accountingEvent.RetryCount >= MaxRetries;
    }

    /// <summary>
    /// Validates that a manual retry reset is allowed.
    /// Only Failed events with RetryCount >= MaxRetries can be manually retried.
    /// </summary>
    public bool CanManualRetry(AccountingEvent accountingEvent)
    {
        return accountingEvent.Status == EventStatus.Posted;
    }
}
