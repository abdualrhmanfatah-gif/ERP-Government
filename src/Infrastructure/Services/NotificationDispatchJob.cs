using ERP_Government.Domain.BackgroundJobs.Common;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Placeholder job for scheduled notification dispatch. Validates extensibility.
/// No business logic yet — infrastructure-only consumer.
/// </summary>
public class NotificationDispatchJob : IBackgroundJob
{
    public string DisplayName => "Notification Dispatch";

    public int TimeoutSeconds => 300;

    public BackgroundJobSchedule GetSchedule() => BackgroundJobSchedule.Interval(300); // Every 5 minutes

    public BackgroundJobRetryPolicy GetRetryPolicy() => new()
    {
        MaxRetries = 5,
        BackoffSchedule =
        [
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(10)
        ]
    };

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO: Implement notification dispatch logic in a future feature
        return Task.CompletedTask;
    }
}
