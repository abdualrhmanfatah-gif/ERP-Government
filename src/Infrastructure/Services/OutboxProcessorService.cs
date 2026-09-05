using System.Text.Json;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP_Government.Infrastructure.Services;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly OutboxOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly TimeSpan[] BackoffSchedule =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10)
    ];

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger,
        IOptions<OutboxOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessorService starting. PollInterval={Interval}ms, BatchSize={Batch}, MaxRetries={Retries}",
            _options.PollIntervalMs, _options.BatchSize, _options.MaxRetries);

        // Orphan recovery: reset stale Processing events on startup
        await ResetStaleProcessingEventsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
                await CleanupProcessedMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "OutboxProcessorService encountered an error");
            }

            await Task.Delay(_options.PollIntervalMs, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessorService stopping");
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var messages = await context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending
                     && (m.NextRetryAt == null || m.NextRetryAt <= DateTimeOffset.UtcNow))
            .OrderBy(m => m.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        _logger.LogDebug("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            await ProcessSingleMessageAsync(message, context, cancellationToken);
        }
    }

    private async Task ProcessSingleMessageAsync(
        OutboxMessage message,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        message.Status = OutboxMessageStatus.Processing;
        await context.SaveChangesAsync(cancellationToken);

        try
        {
            var eventType = Type.GetType(message.TypeName);
            if (eventType is null)
            {
                throw new InvalidOperationException($"Type not found: {message.TypeName}");
            }

            var payload = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions);
            if (payload is null)
            {
                throw new InvalidOperationException($"Deserialization returned null for type: {message.TypeName}");
            }

            // Publish via MediatR — this is where the actual event handling happens
            var mediator = _serviceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(payload, cancellationToken);

            message.Status = OutboxMessageStatus.Processed;
            message.ProcessedAt = DateTimeOffset.UtcNow;
            message.ErrorMessage = null;

            _logger.LogDebug("Outbox message {Id} published successfully", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Outbox message {Id} publish failed (attempt {RetryCount})", message.Id, message.RetryCount + 1);

            message.RetryCount++;
            message.ErrorMessage = ex.Message;

            if (message.RetryCount >= _options.MaxRetries)
            {
                message.Status = OutboxMessageStatus.Failed;
                message.NextRetryAt = null;
                _logger.LogError("Outbox message {Id} failed after {MaxRetries} retries", message.Id, _options.MaxRetries);
            }
            else
            {
                message.Status = OutboxMessageStatus.Pending;
                message.NextRetryAt = DateTimeOffset.UtcNow.Add(GetBackoffDelay(message.RetryCount));
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CleanupProcessedMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = DateTimeOffset.UtcNow.AddDays(-_options.RetentionDays);
        var staleMessages = await context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Processed && m.ProcessedAt < cutoff)
            .ToListAsync(cancellationToken);

        if (staleMessages.Count > 0)
        {
            context.OutboxMessages.RemoveRange(staleMessages);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Cleaned up {Count} processed outbox messages older than {Days} days", staleMessages.Count, _options.RetentionDays);
        }
    }

    public static TimeSpan GetBackoffDelay(int retryCount)
    {
        var index = Math.Min(retryCount - 1, BackoffSchedule.Length - 1);
        return BackoffSchedule[index];
    }

    private async Task ResetStaleProcessingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-5);
        var staleEvents = await context.AccountingEvents
            .Where(e => e.Status == Domain.Accounting.Enums.EventStatus.Posted && e.LastModified < cutoff)
            .ToListAsync(cancellationToken);

        if (staleEvents.Count > 0)
        {
            foreach (var evt in staleEvents)
            {
                evt.Status = Domain.Accounting.Enums.EventStatus.Pending;
                evt.LastModified = DateTimeOffset.UtcNow;
            }
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Orphan recovery: reset {Count} stale Processing events to Pending", staleEvents.Count);
        }
    }
}
