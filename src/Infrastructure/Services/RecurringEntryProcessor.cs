using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.BackgroundJobs.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Processes RecurringEntry entities where NextExecutionDate <= today and Status = Active.
/// Generates complete journal entries (JournalEntry + JournalEntryLines) from linked JournalEntryTemplate.
/// Per-entry transaction isolation, idempotent execution, full audit trail.
/// </summary>
public class RecurringEntryProcessor : IBackgroundJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringEntryProcessor> _logger;

    public RecurringEntryProcessor(IServiceProvider serviceProvider, ILogger<RecurringEntryProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public string DisplayName => "Recurring Entry Processor";

    public int TimeoutSeconds => 600; // 10 minutes

    public BackgroundJobSchedule GetSchedule()
    {
        // Run daily at midnight
        return BackgroundJobSchedule.Cron("0 0 * * *");
    }

    public BackgroundJobRetryPolicy GetRetryPolicy() => new()
    {
        MaxRetries = 3,
        BackoffSchedule =
        [
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(5)
        ]
    };

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var executionLogService = new RecurringEntryExecutionLogService(context);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var dueEntries = await context.RecurringEntries
            .Where(r => r.Status == RecurringEntryStatus.Active
                     && r.NextExecutionDate <= today
                     && r.IsActive)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Processing {Count} recurring entries", dueEntries.Count);

        var successCount = 0;
        var failCount = 0;
        var skipCount = 0;

        foreach (var entry in dueEntries)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                _logger.LogDebug("Processing recurring entry {EntryNumber} (ID: {Id})",
                    entry.EntryNumber, entry.Id);

                var result = await ProcessEntryAsync(entry, executionLogService, context, today, cancellationToken);

                switch (result)
                {
                    case ProcessResult.Success:
                        successCount++;
                        break;
                    case ProcessResult.Failed:
                        failCount++;
                        break;
                    case ProcessResult.Skipped:
                        skipCount++;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process recurring entry {EntryNumber}", entry.EntryNumber);
                failCount++;
                // Do NOT throw — per-entry isolation, continue to next entry
            }
        }

        _logger.LogInformation(
            "Completed processing recurring entries: {Success} succeeded, {Failed} failed, {Skipped} skipped",
            successCount, failCount, skipCount);
    }

    private async Task<ProcessResult> ProcessEntryAsync(
        RecurringEntry entry,
        RecurringEntryExecutionLogService executionLogService,
        IApplicationDbContext context,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        // Step 1: Idempotency check — skip if already processed successfully for this date
        if (await executionLogService.HasSuccessfulLogAsync(entry.Id, entry.NextExecutionDate, cancellationToken))
        {
            _logger.LogDebug("Skipping recurring entry {EntryNumber} — already processed for {Date}",
                entry.EntryNumber, entry.NextExecutionDate);
            return ProcessResult.Skipped;
        }

        // Step 2: Validate template and journal
        var template = entry.TemplateId.HasValue
            ? await context.JournalEntryTemplates
                .FirstOrDefaultAsync(t => t.Id == entry.TemplateId.Value && t.IsActive, cancellationToken)
            : null;

        if (template is null)
        {
            _logger.LogWarning("Recurring entry {EntryNumber}: Template not found or inactive — skipping",
                entry.EntryNumber);
            return ProcessResult.Skipped;
        }

        var templateLines = await context.JournalEntryTemplateLines
            .Where(l => l.TemplateId == template.Id)
            .ToListAsync(cancellationToken);

        if (templateLines.Count == 0)
        {
            _logger.LogWarning("Recurring entry {EntryNumber}: Template has zero lines — skipping",
                entry.EntryNumber);
            return ProcessResult.Skipped;
        }

        var journal = await context.Journals
            .FirstOrDefaultAsync(j => j.Id == entry.JournalId && j.IsActive, cancellationToken);

        if (journal is null)
        {
            _logger.LogWarning("Recurring entry {EntryNumber}: Journal not found or inactive — skipping",
                entry.EntryNumber);
            return ProcessResult.Skipped;
        }

        // Step 3: Resolve FiscalPeriod and FiscalYear for document date
        var documentDate = today;
        var period = await context.FiscalPeriods
            .FirstOrDefaultAsync(p => p.IsActive
                && !p.IsLockedForPosting
                && p.StartDate <= documentDate
                && p.EndDate >= documentDate, cancellationToken);

        if (period is null)
        {
            _logger.LogWarning("Recurring entry {EntryNumber}: No open FiscalPeriod for {Date} — skipping",
                entry.EntryNumber, documentDate);
            return ProcessResult.Skipped;
        }

        var fiscalYear = await context.FiscalYears
            .FirstOrDefaultAsync(f => f.Id == period.FiscalYearId && f.IsActive, cancellationToken);

        if (fiscalYear is null)
        {
            _logger.LogWarning("Recurring entry {EntryNumber}: FiscalYear not found for period — skipping",
                entry.EntryNumber);
            return ProcessResult.Skipped;
        }

        // Step 4: Create execution log (Status=Created)
        var log = await executionLogService.CreateLogAsync(
            entry.Id, entry.NextExecutionDate, "Scheduler", cancellationToken);

        try
        {
            // Step 5: Calculate amounts and validate balance BEFORE creating JournalEntry
            var calculatedAmounts = AmountCalculator.CalculateAmounts(templateLines, entry.Amount);

            var totalDebit = calculatedAmounts.Sum(a => a.Debit);
            var totalCredit = calculatedAmounts.Sum(a => a.Credit);

            if (totalDebit != totalCredit)
            {
                throw new InvalidOperationException(
                    $"Imbalanced entry: total Debit ({totalDebit}) != total Credit ({totalCredit})");
            }

            // Step 6: Generate JournalEntry
            var entryNumber = await GenerateEntryNumberAsync(context, cancellationToken);

            var journalEntry = new JournalEntry
            {
                EntryNumber = entryNumber,
                DocumentDate = documentDate,
                PostingDate = documentDate,
                EntryStatus = EntryStatus.Draft,
                JournalId = entry.JournalId,
                PeriodId = period.Id,
                FiscalYearId = fiscalYear.Id,
                IsSystemGenerated = true,
                Narration = entry.DescriptionTemplate ?? $"Auto-generated from recurring entry {entry.EntryNumber}"
            };

            context.JournalEntries.Add(journalEntry);
            await context.SaveChangesAsync(cancellationToken); // Save to get journalEntry.Id

            // Step 7: Generate JournalEntryLines
            var journalEntryLines = JournalEntryLineGenerator.GenerateLines(journalEntry, templateLines, calculatedAmounts, entry);

            foreach (var line in journalEntryLines)
            {
                context.JournalEntryLines.Add(line);
            }

            await context.SaveChangesAsync(cancellationToken); // Save JournalEntryLines

            // Step 8: Update execution log to Success
            await executionLogService.UpdateLogSuccessAsync(log.Id, journalEntry.Id, cancellationToken);

            // Step 9: Update RecurringEntry
            entry.LastExecutedAt = DateTime.UtcNow;
            entry.GeneratedJournalEntryId = journalEntry.Id;
            entry.NextExecutionDate = CalculateNextExecutionDate(entry);

            // Step 10: Auto-complete if past EndDate
            if (entry.EndDate.HasValue && entry.NextExecutionDate > entry.EndDate.Value)
            {
                entry.Status = RecurringEntryStatus.Completed;
            }

            await context.SaveChangesAsync(cancellationToken); // Save RecurringEntry changes

            _logger.LogInformation(
                "Recurring entry {EntryNumber} processed: JournalEntry {JournalEntryEntryNumber} created",
                entry.EntryNumber, journalEntry.EntryNumber);

            return ProcessResult.Success;
        }
        catch (Exception ex)
        {
            // Step 11: Log failure — update execution log to Failed
            _logger.LogError(ex, "Failed to process recurring entry {EntryNumber}", entry.EntryNumber);

            try
            {
                await executionLogService.UpdateLogFailedAsync(log.Id, ex.Message, cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to update execution log for entry {EntryNumber}", entry.EntryNumber);
            }

            // NextExecutionDate is NOT advanced — entry remains due for retry
            return ProcessResult.Failed;
        }
    }

    private static async Task<string> GenerateEntryNumberAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var year = DateTime.Today.Year;
        var prefix = $"AUTO-{year}-";

        var last = await context.JournalEntries
            .Where(m => m.EntryNumber.StartsWith(prefix))
            .OrderByDescending(m => m.EntryNumber)
            .Select(m => m.EntryNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (last is null)
            return $"{prefix}000001";

        var seq = int.Parse(last.Substring(prefix.Length));
        return $"{prefix}{(seq + 1):D6}";
    }

    private static DateOnly CalculateNextExecutionDate(RecurringEntry entry)
    {
        return entry.Frequency switch
        {
            RecurringFrequency.Weekly => entry.NextExecutionDate.AddDays(7),
            RecurringFrequency.Monthly => entry.NextExecutionDate.AddMonths(1),
            RecurringFrequency.Quarterly => entry.NextExecutionDate.AddMonths(3),
            RecurringFrequency.Yearly => entry.NextExecutionDate.AddYears(1),
            _ => entry.NextExecutionDate.AddDays(7)
        };
    }

    private enum ProcessResult
    {
        Success,
        Failed,
        Skipped
    }
}
