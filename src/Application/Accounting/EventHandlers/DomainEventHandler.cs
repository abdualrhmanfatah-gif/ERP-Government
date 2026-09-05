using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Events.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Accounting.EventHandlers;

/// <summary>
/// Handles all domain events by creating AccountingEvent records for durable tracking.
/// Runs inside the SaveChanges transaction — if processing fails, the originating write rolls back.
/// </summary>
public class DomainEventHandler : INotificationHandler<BaseEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DomainEventHandler> _logger;

    public DomainEventHandler(IApplicationDbContext context, ILogger<DomainEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(BaseEvent notification, CancellationToken cancellationToken)
    {
        if (notification is not IHasSourceEntity sourceEntity)
            return; // Events without source entity metadata are ignored

        var accountingEvent = new AccountingEvent
        {
            EventType = MapEventType(notification.GetType().Name),
            SourceDocumentType = sourceEntity.SourceEntityType,
            SourceDocumentId = sourceEntity.SourceEntityId,
            Status = EventStatus.Pending,
            RetryCount = 0
        };

        _context.AccountingEvents.Add(accountingEvent);
        // SaveChangesAsync removed — persistence handled by OutboxProcessorService transaction scope.
        // Nested SaveChanges would re-trigger DispatchDomainEventsInterceptor, causing infinite recursion.

        _logger.LogDebug("Domain event captured: EventType={EventType}, SourceDocumentType={SourceDocumentType}, SourceDocumentId={SourceDocumentId}",
            accountingEvent.EventType, accountingEvent.SourceDocumentType, accountingEvent.SourceDocumentId);
    }

    private static EventType MapEventType(string eventName) => EventTypeMapper.MapFrom(eventName);
}
