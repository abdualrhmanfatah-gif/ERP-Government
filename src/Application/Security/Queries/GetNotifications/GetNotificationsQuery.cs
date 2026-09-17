using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Common.DTOs;
using FluentValidation;

namespace ERP_Government.Application.Security.Queries.GetNotifications;

[Authorize(Policy = PermissionCodes.NotificationsView)]
public class GetNotificationsQuery : IRequest<PaginatedResult<NotificationDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public class GetNotificationsQueryHandler(
    INotificationService notificationService,
    IUser user) : IRequestHandler<GetNotificationsQuery, PaginatedResult<NotificationDto>>
{
    public async Task<PaginatedResult<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id!.Value;

        return await notificationService.GetUserNotificationsAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}

public class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
