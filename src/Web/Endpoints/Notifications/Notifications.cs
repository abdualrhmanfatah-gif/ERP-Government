using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Commands.CreateNotification;
using ERP_Government.Application.Security.Commands.DeleteNotification;
using ERP_Government.Application.Security.Commands.ClearAllNotifications;
using ERP_Government.Application.Security.Commands.MarkNotificationAsRead;
using ERP_Government.Application.Security.Commands.MarkAllNotificationsAsRead;
using ERP_Government.Application.Security.Common.DTOs;
using ERP_Government.Application.Security.Queries.GetNotifications;
using ERP_Government.Application.Security.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Notifications;

public class Notifications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetNotifications)
            .Produces<PaginatedResult<NotificationDto>>()
            .RequireAuthorization(PermissionCodes.NotificationsView);

        groupBuilder.MapGet("/unread-count", GetUnreadCount)
            .Produces<UnreadCountDto>()
            .RequireAuthorization(PermissionCodes.NotificationsView);

        groupBuilder.MapPut("/{id:long}/read", MarkAsRead)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.NotificationsMarkRead);

        groupBuilder.MapPut("/read-all", MarkAllAsRead)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.NotificationsMarkRead);

        groupBuilder.MapDelete("/{id:long}", DeleteNotification)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.NotificationsDelete);

        groupBuilder.MapDelete("/all", ClearAll)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.NotificationsDelete);
    }

    [EndpointSummary("Get paginated notifications for current user")]
    public static async Task<PaginatedResult<NotificationDto>> GetNotifications(
        [FromServices] ISender sender,
        [AsParameters] GetNotificationsQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get unread notification count")]
    public static async Task<UnreadCountDto> GetUnreadCount(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetUnreadCountQuery());
    }

    [EndpointSummary("Mark notification as read")]
    public static async Task<IResult> MarkAsRead(
        [FromServices] ISender sender,
        long id)
    {
        await sender.Send(new MarkNotificationAsReadCommand { NotificationId = id });
        return Results.NoContent();
    }

    [EndpointSummary("Mark all notifications as read")]
    public static async Task<IResult> MarkAllAsRead(
        [FromServices] ISender sender)
    {
        await sender.Send(new MarkAllNotificationsAsReadCommand());
        return Results.NoContent();
    }

    [EndpointSummary("Delete notification")]
    public static async Task<IResult> DeleteNotification(
        [FromServices] ISender sender,
        long id)
    {
        await sender.Send(new DeleteNotificationCommand { NotificationId = id });
        return Results.NoContent();
    }

    [EndpointSummary("Clear all notifications")]
    public static async Task<IResult> ClearAll(
        [FromServices] ISender sender)
    {
        await sender.Send(new ClearAllNotificationsCommand());
        return Results.NoContent();
    }
}
