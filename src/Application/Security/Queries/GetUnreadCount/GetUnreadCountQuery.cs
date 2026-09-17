using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;

namespace ERP_Government.Application.Security.Queries.GetUnreadCount;

[Authorize(Policy = PermissionCodes.NotificationsView)]
public class GetUnreadCountQuery : IRequest<UnreadCountDto>
{
}

public class GetUnreadCountQueryHandler(
    INotificationService notificationService,
    IUser user) : IRequestHandler<GetUnreadCountQuery, UnreadCountDto>
{
    public async Task<UnreadCountDto> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        // IUser.Id is now the domain user ID directly (int?)
        if (user.Id is null)
            return new UnreadCountDto { Count = 0 };

        var count = await notificationService.GetUnreadCountAsync(user.Id.Value, cancellationToken);

        return new UnreadCountDto { Count = count };
    }
}
