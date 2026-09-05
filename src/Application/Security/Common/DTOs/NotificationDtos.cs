namespace ERP_Government.Application.Security.Common.DTOs;

public class NotificationDto
{
    public long Id { get; init; }
    public int UserId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? DocumentType { get; init; }
    public int? DocumentId { get; init; }
    public string Priority { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public DateTimeOffset Created { get; init; }
}

public class UnreadCountDto
{
    public int Count { get; init; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public class CreateNotificationRequest
{
    public int UserId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Priority { get; init; } = "Normal";
    public string? DocumentType { get; init; }
    public int? DocumentId { get; init; }
}
