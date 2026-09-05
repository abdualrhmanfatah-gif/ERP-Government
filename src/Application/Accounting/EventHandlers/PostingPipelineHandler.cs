using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Orchestrates the posting pipeline: AccountingEvent creation → PostingRule matching → JournalEntry generation → Retry handling.
/// This is the main entry point called by DomainEventHandler for each published domain event.
/// </summary>
public class PostingPipelineHandler : INotificationHandler<BaseEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly PostingRuleMatcher _matcher;
    private readonly JournalEntryGenerator _journalEntryGenerator;
    private readonly AccountingEventAuditor _auditor;
    private readonly RetryPolicy _retryPolicy;
    private readonly ILogger<PostingPipelineHandler> _logger;

    public PostingPipelineHandler(
        IApplicationDbContext context,
        PostingRuleMatcher matcher,
        JournalEntryGenerator journalEntryGenerator,
        AccountingEventAuditor auditor,
        RetryPolicy retryPolicy,
        ILogger<PostingPipelineHandler> logger)
    {
        _context = context;
        _matcher = matcher;
        _journalEntryGenerator = journalEntryGenerator;
        _auditor = auditor;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }

    public async Task Handle(BaseEvent notification, CancellationToken cancellationToken)
    {
        if (notification is not IHasSourceEntity sourceEntity)
            return;

        var eventType = MapEventType(notification.GetType().Name);
        var correlationId = Guid.NewGuid();

        _logger.LogInformation("Posting pipeline started: EventType={EventType}, SourceTable={SourceTable}, SourceId={SourceId}, CorrelationId={CorrelationId}",
            eventType, sourceEntity.SourceEntityType, sourceEntity.SourceEntityId, correlationId);

        // Find or create AccountingEvent
        var accountingEvent = await _auditor.FindPendingAsync(
            sourceEntity.SourceEntityType,
            sourceEntity.SourceEntityId,
            eventType,
            cancellationToken);

        if (accountingEvent is null)
            return; // Already processed or not pending

        await _auditor.MarkProcessingAsync(accountingEvent, cancellationToken);

        try
        {
            // Match PostingRules
            var rules = await _matcher.MatchAsync(eventType.ToString(), cancellationToken);

            if (rules.Count == 0)
            {
                await _auditor.MarkFailedAsync(
                    accountingEvent,
                    $"No posting rule found for event type: {eventType}",
                    cancellationToken);
                _logger.LogWarning("No posting rule found: EventType={EventType}, CorrelationId={CorrelationId}",
                    eventType, correlationId);
                return;
            }

            // Generate JournalEntry for each matched rule
            var documentDate = DateOnly.FromDateTime(DateTime.Today);
            var entryCount = 0;

            foreach (var rule in rules)
            {
                var journalEntry = await _journalEntryGenerator.GenerateJournalEntryAsync(
                    accountingEvent,
                    rule,
                    sourceEntity,
                    documentDate,
                    cancellationToken);
                entryCount++;

                _logger.LogDebug("JournalEntry generated: JournalEntryId={JournalEntryId}, EntryNumber={EntryNumber}, JournalId={JournalId}, CorrelationId={CorrelationId}",
                    journalEntry.Id, journalEntry.EntryNumber, journalEntry.JournalId, correlationId);
            }

            await _auditor.MarkCompletedAsync(accountingEvent, cancellationToken);

            _logger.LogInformation("Posting pipeline completed: AccountingEventId={AccountingEventId}, JournalEntriesGenerated={JournalEntriesGenerated}, CorrelationId={CorrelationId}",
                accountingEvent.Id, entryCount, correlationId);
        }
        catch (Exception ex)
        {
            var canRetry = await _auditor.MarkFailedAsync(
                accountingEvent,
                ex.Message,
                cancellationToken);

            _logger.LogError(ex, "Posting pipeline failed: AccountingEventId={AccountingEventId}, CanRetry={CanRetry}, CorrelationId={CorrelationId}",
                accountingEvent.Id, canRetry, correlationId);

            if (!canRetry && _retryPolicy.RequiresManualRetry(accountingEvent))
            {
                _logger.LogWarning("Manual retry required: AccountingEventId={AccountingEventId}, RetryCount={RetryCount}, CorrelationId={CorrelationId}",
                    accountingEvent.Id, accountingEvent.RetryCount, correlationId);
            }
        }
    }

    private static EventType MapEventType(string eventName) => EventTypeMapper.MapFrom(eventName);
}
