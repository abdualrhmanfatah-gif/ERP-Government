using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.Commands.MarkAllNotificationsAsRead;

[Authorize(Policy = PermissionCodes.NotificationsMarkRead)]
public class MarkAllNotificationsAsReadCommand : IRequest<Result>
{
}

public class MarkAllNotificationsAsReadCommandHandler(
    INotificationService notificationService,
    IUser user) : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
{
    public async Task<Result> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id!.Value;
        await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Result.Success();
    }
}
