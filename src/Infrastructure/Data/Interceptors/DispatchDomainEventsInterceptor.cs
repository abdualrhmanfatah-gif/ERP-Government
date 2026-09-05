using System.Text.Json;
using ERP_Government.Domain.Common;
using ERP_Government.Domain.Events.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP_Government.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        PersistDomainEventsToOutbox(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        PersistDomainEventsToOutbox(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void PersistDomainEventsToOutbox(DbContext? context)
    {
        if (context is null) return;

        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage
        {
            TypeName = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName!,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions),
            AggregateId = ExtractAggregateId(domainEvent),
            CorrelationId = Guid.NewGuid(),
            Status = OutboxMessageStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }

    private static string? ExtractAggregateId(object domainEvent)
    {
        if (domainEvent is IHasSourceEntity hasSource)
            return hasSource.SourceEntityId.ToString();

        return null;
    }
}
