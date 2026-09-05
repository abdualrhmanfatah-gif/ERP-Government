using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.Commands.DeleteNotification;

[Authorize(Policy = PermissionCodes.NotificationsDelete)]
public class DeleteNotificationCommand : IRequest<Result>
{
    public long NotificationId { get; init; }
}

public class DeleteNotificationCommandHandler(
    INotificationService notificationService,
    IUser user) : IRequestHandler<DeleteNotificationCommand, Result>
{
    public async Task<Result> Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id!.Value;
        await notificationService.DeleteAsync(userId, request.NotificationId, cancellationToken);
        return Result.Success();
    }
}
