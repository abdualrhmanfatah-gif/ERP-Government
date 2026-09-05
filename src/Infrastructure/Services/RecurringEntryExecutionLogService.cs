using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Encapsulates CRUD operations for RecurringEntryExecutionLog.
/// Used by RecurringEntryProcessor for audit trail management.
/// </summary>
public class RecurringEntryExecutionLogService
{
    private readonly IApplicationDbContext _context;

    public RecurringEntryExecutionLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new execution log record with Status=Created.
    /// </summary>
    public async Task<RecurringEntryExecutionLog> CreateLogAsync(
        int recurringEntryId,
        DateOnly executionDate,
        string triggeredBy = "Scheduler",
        CancellationToken cancellationToken = default)
    {
        var log = new RecurringEntryExecutionLog
        {
            RecurringEntryId = recurringEntryId,
            ExecutionDate = executionDate,
            Status = RecurringEntryExecutionStatus.Created,
            StartedAt = DateTimeOffset.UtcNow,
            TriggeredBy = triggeredBy
        };

        _context.RecurringEntryExecutionLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);

        return log;
    }

    /// <summary>
    /// Updates an execution log to Success status with the generated JournalEntry reference.
    /// </summary>
    public async Task UpdateLogSuccessAsync(
        int logId,
        int journalEntryId,
        CancellationToken cancellationToken = default)
    {
        var log = await _context.RecurringEntryExecutionLogs.FindAsync([logId], cancellationToken)
            ?? throw new InvalidOperationException($"Execution log {logId} not found");

        log.Status = RecurringEntryExecutionStatus.Success;
        log.GeneratedJournalEntryId = journalEntryId;
        log.CompletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an execution log to Failed status with error details.
    /// </summary>
    public async Task UpdateLogFailedAsync(
        int logId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var log = await _context.RecurringEntryExecutionLogs.FindAsync([logId], cancellationToken)
            ?? throw new InvalidOperationException($"Execution log {logId} not found");

        log.Status = RecurringEntryExecutionStatus.Failed;
        log.ErrorMessage = errorMessage;
        log.CompletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a successful execution log already exists for the given entry and date.
    /// Used for idempotency — prevents duplicate JournalEntry generation.
    /// </summary>
    public async Task<bool> HasSuccessfulLogAsync(
        int recurringEntryId,
        DateOnly executionDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.RecurringEntryExecutionLogs
            .AnyAsync(l =>
                l.RecurringEntryId == recurringEntryId
                && l.ExecutionDate == executionDate
                && l.Status == RecurringEntryExecutionStatus.Success,
                cancellationToken);
    }
}
