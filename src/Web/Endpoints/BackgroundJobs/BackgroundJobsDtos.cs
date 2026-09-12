namespace ERP_Government.Web.Endpoint.BackgroundJobs;

public class BackgroundJobsResponse
{
    public List<BackgroundJobDto> Jobs { get; set; } = [];
}

public class BackgroundJobDto
{
    public int Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = string.Empty;
    public string ScheduleValue { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTimeOffset? LastExecutedAt { get; set; }
    public DateTimeOffset? NextScheduledAt { get; set; }
    public int LastRetryCount { get; set; }
    public string? LastError { get; set; }
}

public class BackgroundJobHistoryResponse
{
    public int JobId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public List<BackgroundJobInstanceDto> Instances { get; set; } = [];
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}

public class BackgroundJobInstanceDto
{
    public int InstanceId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
    public List<ExecutionAttemptDto> Attempts { get; set; } = [];
}

public class ExecutionAttemptDto
{
    public int AttemptNumber { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class CancelBackgroundJobResponse
{
    public int InstanceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? CancelledAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}
