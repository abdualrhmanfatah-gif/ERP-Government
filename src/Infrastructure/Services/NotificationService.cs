using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common.DTOs;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Infrastructure.Services;

public class NotificationService(ApplicationDbContext context) : INotificationService
{
    public async Task<long> CreateAsync(CreateNotificationRequest request, CancellationToken ct)
    {
        var notificationType = Enum.Parse<NotificationType>(request.NotificationType);
        var priority = Enum.Parse<NotificationPriority>(request.Priority);

        var entity = new Notification
        {
            UserId = request.UserId,
            NotificationType = notificationType,
            Title = request.Title,
            Message = request.Message,
            Priority = priority,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            IsRead = false
        };

        context.Notifications.Add(entity);
        await context.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task<PaginatedResult<NotificationDto>> GetUserNotificationsAsync(int userId, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Created);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                NotificationType = n.NotificationType.ToString(),
                Title = n.Title,
                Message = n.Message,
                DocumentType = n.DocumentType,
                DocumentId = n.DocumentId,
                Priority = n.Priority.ToString(),
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                Created = n.Created
            })
            .ToListAsync(ct);

        return new PaginatedResult<NotificationDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct)
    {
        return await context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task MarkAsReadAsync(int userId, long notificationId, CancellationToken ct)
    {
        var entity = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (entity is null)
            return;

        if (!entity.IsRead)
        {
            entity.IsRead = true;
            entity.ReadAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken ct)
    {
        var unread = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (unread.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int userId, long notificationId, CancellationToken ct)
    {
        var entity = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (entity is null)
            return;

        context.Notifications.Remove(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task ClearAllAsync(int userId, CancellationToken ct)
    {
        var all = await context.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync(ct);

        if (all.Count == 0)
            return;

        context.Notifications.RemoveRange(all);
        await context.SaveChangesAsync(ct);
    }
}
