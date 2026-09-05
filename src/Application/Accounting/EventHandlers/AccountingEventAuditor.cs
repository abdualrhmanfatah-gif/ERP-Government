using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Manages AccountingEvent status transitions: Pending → Processing → Completed/Failed.
/// Handles retry logic (max 3 automatic retries).
/// </summary>
public class AccountingEventAuditor
{
    private readonly IApplicationDbContext _context;

    public AccountingEventAuditor(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Marks AccountingEvent as Processing.
    /// </summary>
    public Task MarkProcessingAsync(AccountingEvent accountingEvent, CancellationToken cancellationToken = default)
    {
        accountingEvent.Status = EventStatus.Posted;
        // SaveChangesAsync removed — persistence handled by OutboxProcessorService transaction scope.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks AccountingEvent as Completed with timestamp.
    /// </summary>
    public Task MarkCompletedAsync(AccountingEvent accountingEvent, CancellationToken cancellationToken = default)
    {
        accountingEvent.Status = EventStatus.Reversed;
        accountingEvent.ProcessedAt = DateTimeOffset.UtcNow;
        // SaveChangesAsync removed — persistence handled by OutboxProcessorService transaction scope.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks AccountingEvent as Failed with error message and increments RetryCount.
    /// Returns true if retry is allowed (RetryCount < 3).
    /// </summary>
    public Task<bool> MarkFailedAsync(AccountingEvent accountingEvent, string errorMessage, CancellationToken cancellationToken = default)
    {
        accountingEvent.Status = EventStatus.Posted;
        accountingEvent.ErrorMessage = errorMessage;
        accountingEvent.ProcessedAt = DateTimeOffset.UtcNow;
        accountingEvent.RetryCount++;
        // SaveChangesAsync removed — persistence handled by OutboxProcessorService transaction scope.
        return Task.FromResult(accountingEvent.RetryCount < 3);
    }

    /// <summary>
    /// Resets a Failed AccountingEvent for manual retry.
    /// </summary>
    public Task ResetForRetryAsync(AccountingEvent accountingEvent, CancellationToken cancellationToken = default)
    {
        accountingEvent.Status = EventStatus.Pending;
        accountingEvent.RetryCount = 0;
        accountingEvent.ErrorMessage = null;
        accountingEvent.ProcessedAt = null;
        // SaveChangesAsync removed — persistence handled by OutboxProcessorService transaction scope.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Finds a pending AccountingEvent by source and locks it for processing.
    /// </summary>
    public async Task<AccountingEvent?> FindPendingAsync(string sourceDocumentType, int sourceDocumentId, EventType eventType, CancellationToken cancellationToken = default)
    {
        return await _context.AccountingEvents
            .FirstOrDefaultAsync(e =>
                e.SourceDocumentType == sourceDocumentType &&
                e.SourceDocumentId == sourceDocumentId &&
                e.EventType == eventType &&
                e.Status == EventStatus.Pending,
                cancellationToken);
    }

    /// <summary>
    /// Resets AccountingEvents stuck in Processing status for more than 5 minutes to Pending.
    /// Called on OutboxProcessorService startup to recover from crashes.
    /// </summary>
    public async Task<int> ResetStaleProcessingEventsAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-5);
        var staleEvents = await _context.AccountingEvents
            .Where(e => e.Status == EventStatus.Posted && e.LastModified < cutoff)
            .ToListAsync(cancellationToken);

        foreach (var evt in staleEvents)
        {
            evt.Status = EventStatus.Pending;
            evt.LastModified = DateTimeOffset.UtcNow;
        }

        return staleEvents.Count;
    }
}
