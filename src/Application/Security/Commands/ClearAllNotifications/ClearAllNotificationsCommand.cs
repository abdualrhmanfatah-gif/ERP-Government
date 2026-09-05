using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.Commands.ClearAllNotifications;

[Authorize(Policy = PermissionCodes.NotificationsDelete)]
public class ClearAllNotificationsCommand : IRequest<Result>
{
}

public class ClearAllNotificationsCommandHandler(
    INotificationService notificationService,
    IUser user) : IRequestHandler<ClearAllNotificationsCommand, Result>
{
    public async Task<Result> Handle(
        ClearAllNotificationsCommand request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id!.Value;
        await notificationService.ClearAllAsync(userId, cancellationToken);
        return Result.Success();
    }
}
