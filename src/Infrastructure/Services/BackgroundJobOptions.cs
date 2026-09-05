namespace ERP_Government.Infrastructure.Services;

public class BackgroundJobOptions
{
    /// <summary>Poll interval in milliseconds.</summary>
    public int PollIntervalMs { get; set; } = 5000;

    /// <summary>Maximum jobs to process per poll cycle.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>Default maximum retries for new job types.</summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>Default timeout in seconds for new job types.</summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>Days to retain completed execution logs.</summary>
    public int RetentionDays { get; set; } = 90;
}
