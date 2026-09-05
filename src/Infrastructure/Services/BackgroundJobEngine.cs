using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.BackgroundJobs.Common;
using ERP_Government.Domain.BackgroundJobs.Entities;
using ERP_Government.Domain.BackgroundJobs.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Core background job engine — polls for due jobs, executes them, manages state transitions.
/// Replaces ad-hoc BackgroundService implementations with a single unified engine.
/// </summary>
public class BackgroundJobEngine : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundJobEngine> _logger;
    private readonly BackgroundJobOptions _options;

    public BackgroundJobEngine(
        IServiceProvider serviceProvider,
        ILogger<BackgroundJobEngine> logger,
        IOptions<BackgroundJobOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "BackgroundJobEngine starting. PollInterval={Interval}ms, BatchSize={Batch}, Timeout={Timeout}s",
            _options.PollIntervalMs, _options.BatchSize, _options.TimeoutSeconds);

        // On startup: recover orphaned Running jobs
        await RecoverOrphanedJobsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueJobsAsync(stoppingToken);
                await PurgeOldLogsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "BackgroundJobEngine encountered an error");
            }

            await Task.Delay(_options.PollIntervalMs, stoppingToken);
        }

        _logger.LogInformation("BackgroundJobEngine stopping");
    }

    private async Task RecoverOrphanedJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<BackgroundJobStore>();
        var timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        var orphans = await store.GetOrphanedRunningJobsAsync(timeout, cancellationToken);

        foreach (var orphan in orphans)
        {
            _logger.LogWarning(
                "Recovering orphaned job instance {InstanceId} (started at {StartedAt})",
                orphan.Id, orphan.StartedAt);

            await store.UpdateStatusAsync(orphan, BackgroundJobStatus.Failed,
                "Orphaned — application restarted during execution", cancellationToken);
        }

        if (orphans.Count > 0)
        {
            _logger.LogInformation("Recovered {Count} orphaned job instances", orphans.Count);
        }
    }

    private async Task ProcessDueJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<BackgroundJobStore>();

        var dueJobs = await store.GetDueJobsAsync(_options.BatchSize, cancellationToken);

        if (dueJobs.Count == 0) return;

        _logger.LogDebug("Processing {Count} due jobs", dueJobs.Count);

        foreach (var jobInstance in dueJobs)
        {
            if (cancellationToken.IsCancellationRequested) break;

            await ExecuteJobAsync(jobInstance, cancellationToken);
        }
    }

    private async Task ExecuteJobAsync(BackgroundJobInstance instance, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<BackgroundJobStore>();

        // Load definition
        var definition = await scope.ServiceProvider.GetRequiredService<IApplicationDbContext>()
            .BackgroundJobDefinitions
            .FirstOrDefaultAsync(d => d.Id == instance.JobDefinitionId, cancellationToken);

        if (definition == null)
        {
            _logger.LogError("Job definition {DefinitionId} not found for instance {InstanceId}",
                instance.JobDefinitionId, instance.Id);
            return;
        }

        // Find the registered IBackgroundJob implementation
        var jobType = Type.GetType(definition.TypeName);
        if (jobType == null || !typeof(IBackgroundJob).IsAssignableFrom(jobType))
        {
            _logger.LogError("Job type {TypeName} not found or does not implement IBackgroundJob", definition.TypeName);
            await store.UpdateStatusAsync(instance, BackgroundJobStatus.Failed,
                $"Job type not found: {definition.TypeName}", cancellationToken);
            return;
        }

        var job = (IBackgroundJob)scope.ServiceProvider.GetRequiredService(jobType);

        // Transition to Running
        await store.UpdateStatusAsync(instance, BackgroundJobStatus.Running, cancellationToken: cancellationToken);

        // Create execution log
        var log = await store.CreateExecutionLogAsync(instance.Id, instance.RetryCount + 1, cancellationToken);

        // Execute with timeout
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(definition.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        var attemptNumber = instance.RetryCount + 1;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Executing job {DisplayName} (instance {InstanceId}, attempt {Attempt})",
                definition.DisplayName, instance.Id, attemptNumber);

            await job.ExecuteAsync(linkedCts.Token);
            sw.Stop();

            // Success
            await store.CompleteExecutionLogAsync(log, BackgroundJobExecutionStatus.Running,
                cancellationToken: cancellationToken);
            await store.UpdateStatusAsync(instance, BackgroundJobStatus.Completed, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Job {DisplayName} completed (instance {InstanceId}, duration {Duration}ms)",
                definition.DisplayName, instance.Id, sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            sw.Stop();
            _logger.LogWarning("Job {DisplayName} timed out after {Duration}ms (instance {InstanceId})",
                definition.DisplayName, sw.ElapsedMilliseconds, instance.Id);

            await store.CompleteExecutionLogAsync(log, BackgroundJobExecutionStatus.Failed,
                "Execution timed out", cancellationToken: cancellationToken);
            await store.UpdateStatusAsync(instance, BackgroundJobStatus.Failed,
                $"Timed out after {sw.ElapsedMilliseconds}ms", cancellationToken);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            _logger.LogInformation("Job {DisplayName} cancelled (instance {InstanceId})",
                definition.DisplayName, instance.Id);

            await store.CompleteExecutionLogAsync(log, BackgroundJobExecutionStatus.Failed,
                "Cancelled", cancellationToken: cancellationToken);
            await store.UpdateStatusAsync(instance, BackgroundJobStatus.Cancelled,
                "Cancelled by operator", cancellationToken);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Job {DisplayName} failed (instance {InstanceId}, attempt {Attempt})",
                definition.DisplayName, instance.Id, attemptNumber);

            await store.CompleteExecutionLogAsync(log, BackgroundJobExecutionStatus.Failed,
                ex.Message, ex.StackTrace, cancellationToken);

            // Check if retries remain
            var retryPolicy = job.GetRetryPolicy();
            if (instance.RetryCount < retryPolicy.MaxRetries)
            {
                var delay = retryPolicy.GetBackoffDelay(instance.RetryCount + 1);
                await store.ScheduleRetryAsync(instance, delay, cancellationToken);

                _logger.LogInformation(
                    "Scheduled retry for {DisplayName} (instance {InstanceId}) at {NextRun} in {Delay}s",
                    definition.DisplayName, instance.Id, instance.ScheduledAt, delay.TotalSeconds);
            }
            else
            {
                await store.UpdateStatusAsync(instance, BackgroundJobStatus.Failed,
                    $"Failed after {retryPolicy.MaxRetries} retries: {ex.Message}", cancellationToken);

                _logger.LogError("Job {DisplayName} exhausted all retries (instance {InstanceId})",
                    definition.DisplayName, instance.Id);
            }
        }
    }

    private async Task PurgeOldLogsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<BackgroundJobStore>();

        await store.PurgeOldLogsAsync(_options.RetentionDays, cancellationToken);
    }
}
