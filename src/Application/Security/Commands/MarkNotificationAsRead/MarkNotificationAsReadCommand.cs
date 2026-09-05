using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Security.Commands.MarkNotificationAsRead;

[Authorize(Policy = PermissionCodes.NotificationsMarkRead)]
public class MarkNotificationAsReadCommand : IRequest<Result>
{
    public long NotificationId { get; init; }
}

public class MarkNotificationAsReadCommandHandler(
    INotificationService notificationService,
    IUser user) : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    public async Task<Result> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id!.Value;
        await notificationService.MarkAsReadAsync(userId, request.NotificationId, cancellationToken);
        return Result.Success();
    }
}
