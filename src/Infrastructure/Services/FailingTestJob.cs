using ERP_Government.Domain.BackgroundJobs.Common;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Test job that always throws. Used for integration testing retry behavior.
/// </summary>
public class FailingTestJob : IBackgroundJob
{
    public string DisplayName => "Failing Test Job";

    public int TimeoutSeconds => 30;

    public BackgroundJobSchedule GetSchedule() => BackgroundJobSchedule.Interval(60);

    public BackgroundJobRetryPolicy GetRetryPolicy() => new()
    {
        MaxRetries = 3,
        BackoffSchedule =
        [
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(30)
        ]
    };

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Test job intentionally failed for retry testing");
    }
}
