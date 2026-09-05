namespace ERP_Government.Domain.BackgroundJobs.Common;

/// <summary>
/// Retry policy for a background job.
/// </summary>
public record BackgroundJobRetryPolicy
{
    public int MaxRetries { get; init; } = 5;

    /// <summary>
    /// Backoff delays in order. Element index = retry number (0-based).
    /// If retries exceed array length, last element is used.
    /// </summary>
    public TimeSpan[] BackoffSchedule { get; init; } =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10)
    ];

    public TimeSpan GetBackoffDelay(int retryCount)
    {
        var index = Math.Min(retryCount - 1, BackoffSchedule.Length - 1);
        return index >= 0 ? BackoffSchedule[index] : TimeSpan.Zero;
    }
}
