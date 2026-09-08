using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Handles domain events by matching active PostingRules and generating JournalEntries directly
/// (DEP-026 — AccountingEvent staging entity removed; outbox shape retained via OutboxMessages).
/// No-rules matches are skipped with a warning; generation exceptions are logged and swallowed so
/// the originating document save completes (failure is observable via application logs).
/// </summary>
public class PostingPipelineHandler : INotificationHandler<BaseEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly PostingRuleMatcher _matcher;
    private readonly JournalEntryGenerator _journalEntryGenerator;
    private readonly ILogger<PostingPipelineHandler> _logger;

    public PostingPipelineHandler(
        IApplicationDbContext context,
        PostingRuleMatcher matcher,
        JournalEntryGenerator journalEntryGenerator,
        ILogger<PostingPipelineHandler> logger)
    {
        _context = context;
        _matcher = matcher;
        _journalEntryGenerator = journalEntryGenerator;
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

        // Match PostingRules
        var rules = await _matcher.MatchAsync(eventType.ToString(), cancellationToken);

        if (rules.Count == 0)
        {
            _logger.LogWarning("No posting rule found: EventType={EventType}, CorrelationId={CorrelationId}",
                eventType, correlationId);
            return;
        }

        try
        {
            var documentDate = DateOnly.FromDateTime(DateTime.Today);
            var entryCount = 0;

            foreach (var rule in rules)
            {
                var journalEntry = await _journalEntryGenerator.GenerateJournalEntryAsync(
                    eventType,
                    rule,
                    sourceEntity,
                    documentDate,
                    cancellationToken);
                entryCount++;

                _logger.LogDebug("JournalEntry generated: JournalEntryId={JournalEntryId}, EntryNumber={EntryNumber}, JournalId={JournalId}, CorrelationId={CorrelationId}",
                    journalEntry.Id, journalEntry.EntryNumber, journalEntry.JournalId, correlationId);
            }

            _logger.LogInformation("Posting pipeline completed: JournalEntriesGenerated={JournalEntriesGenerated}, CorrelationId={CorrelationId}",
                entryCount, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Posting pipeline failed: EventType={EventType}, SourceTable={SourceTable}, SourceId={SourceId}, CorrelationId={CorrelationId}",
                eventType, sourceEntity.SourceEntityType, sourceEntity.SourceEntityId, correlationId);
        }
    }

    private static EventType MapEventType(string eventName) => EventTypeMapper.MapFrom(eventName);
}
