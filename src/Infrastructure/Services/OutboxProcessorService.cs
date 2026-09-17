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

    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(5);

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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RecoverStalledMessagesAsync(stoppingToken);
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

    private async Task RecoverStalledMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var stalledMessages = await context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Processing
                     && m.LeaseExpiry != null
                     && m.LeaseExpiry < DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        if (stalledMessages.Count == 0) return;

        foreach (var message in stalledMessages)
        {
            message.Status = OutboxMessageStatus.Pending;
            message.LeaseExpiry = null;
            _logger.LogWarning("Recovered stalled outbox message {Id} (lease expired)", message.Id);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var now = DateTimeOffset.UtcNow;
        var messages = await context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending
                     && (m.NextRetryAt == null || m.NextRetryAt <= now))
            .OrderBy(m => m.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        _logger.LogDebug("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            var claimed = await ClaimMessageAsync(message, context, cancellationToken);
            if (claimed)
            {
                await ProcessSingleMessageAsync(message, context, cancellationToken);
            }
        }
    }

    private async Task<bool> ClaimMessageAsync(
        OutboxMessage message,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        if (message.Status != OutboxMessageStatus.Pending) return false;

        message.Status = OutboxMessageStatus.Processing;
        message.LeaseExpiry = DateTimeOffset.UtcNow.Add(LeaseDuration);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task RenewLeaseAsync(
        OutboxMessage message,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        message.LeaseExpiry = DateTimeOffset.UtcNow.Add(LeaseDuration);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessSingleMessageAsync(
        OutboxMessage message,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var startTime = DateTimeOffset.UtcNow;
        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var heartbeatTask = StartHeartbeatAsync(message, context, heartbeatCts.Token);

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

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(payload, cancellationToken);

            message.Status = OutboxMessageStatus.Processed;
            message.ProcessedAt = DateTimeOffset.UtcNow;
            message.LeaseExpiry = null;
            message.ErrorMessage = null;

            _logger.LogDebug("Outbox message {Id} published successfully", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Outbox message {Id} publish failed (attempt {RetryCount})", message.Id, message.RetryCount + 1);

            message.RetryCount++;
            message.ErrorMessage = ex.Message;
            message.LeaseExpiry = null;

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
        finally
        {
            await heartbeatCts.CancelAsync();
            try { await heartbeatTask; } catch (OperationCanceledException) { }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task StartHeartbeatAsync(
        OutboxMessage message,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(HeartbeatInterval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                await RenewLeaseAsync(message, context, cancellationToken);
                _logger.LogDebug("Renewed lease for outbox message {Id}", message.Id);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to renew lease for outbox message {Id}", message.Id);
                break;
            }
        }
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
}
