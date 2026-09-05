namespace ERP_Government.Infrastructure.Services;

public class OutboxOptions
{
    public int PollIntervalMs { get; set; } = 5000;
    public int BatchSize { get; set; } = 50;
    public int MaxRetries { get; set; } = 5;
    public int RetentionDays { get; set; } = 7;
}
