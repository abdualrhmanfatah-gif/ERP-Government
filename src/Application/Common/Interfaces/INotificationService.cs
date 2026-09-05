using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Common.Interfaces;

public interface INotificationService
{
    Task<long> CreateAsync(CreateNotificationRequest request, CancellationToken ct);
    Task<PaginatedResult<NotificationDto>> GetUserNotificationsAsync(int userId, int page, int pageSize, CancellationToken ct);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken ct);
    Task MarkAsReadAsync(int userId, long notificationId, CancellationToken ct);
    Task MarkAllAsReadAsync(int userId, CancellationToken ct);
    Task DeleteAsync(int userId, long notificationId, CancellationToken ct);
    Task ClearAllAsync(int userId, CancellationToken ct);
}
