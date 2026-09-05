using ERP_Government.Domain.BackgroundJobs.Common;

namespace ERP_Government.Infrastructure.Services;

/// <summary>
/// Placeholder job for scheduled bank statement imports. Validates extensibility.
/// No business logic yet — infrastructure-only consumer.
/// </summary>
public class BankStatementImportJob : IBackgroundJob
{
    public string DisplayName => "Bank Statement Import";

    public int TimeoutSeconds => 600;

    public BackgroundJobSchedule GetSchedule() => BackgroundJobSchedule.Cron("0 2 * * *"); // Daily at 2 AM

    public BackgroundJobRetryPolicy GetRetryPolicy() => new()
    {
        MaxRetries = 3,
        BackoffSchedule =
        [
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(10)
        ]
    };

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO: Implement bank statement import logic in a future feature
        return Task.CompletedTask;
    }
}
