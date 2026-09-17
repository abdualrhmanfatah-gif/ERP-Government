using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Security.Commands.Users;
using ERP_Government.Application.Security.Commands.UserPermissions;
using ERP_Government.Application.Security.Common.DTOs;
using ERP_Government.Application.Security.Queries.Users;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Web.Endpoint.Security;

// T016 — Users endpoints
public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetUsers)
            .Produces<List<UserDto>>()
            .RequireAuthorization(PermissionCodes.UsersView);

        groupBuilder.MapGet("/{id:int}", GetUserById)
            .Produces<UserDetailDto?>()
            .RequireAuthorization(PermissionCodes.UsersView);

        groupBuilder.MapPost("/", CreateUser)
            .Produces<int>()
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.UsersCreate);

        groupBuilder.MapPut("/{id:int}", UpdateUser)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.UsersUpdate);

        groupBuilder.MapPost("/{id:int}/deactivate", DeactivateUser)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.UsersDeactivate);

        groupBuilder.MapPost("/{id:int}/reactivate", ReactivateUser)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.UsersDeactivate);

        groupBuilder.MapGet("/{id:int}/sessions", GetUserSessions)
            .Produces<List<UserSessionDto>>()
            .RequireAuthorization(PermissionCodes.UsersManageSessions);

        groupBuilder.MapPost("/{id:int}/sessions", CreateUserSession)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        groupBuilder.MapPost("/sessions", CreateCurrentUserSession)
            .Produces<int>(StatusCodes.Status201Created)
            .RequireAuthorization();

        groupBuilder.MapPost("/{id:int}/sessions/{sessionId:int}/revoke", RevokeSession)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.UsersManageSessions);

        groupBuilder.MapPost("/{id:int}/sessions/revoke-all", RevokeAllSessions)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.UsersManageSessions);

        groupBuilder.MapPost("/{id:int}/reset-failed-login-attempts", ResetFailedLoginAttempts)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.UsersResetLogin);

        // FEATURE-008 — User-Role endpoints
        groupBuilder.MapPut("/{id:int}/role", SetUserRole)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.UsersManageRoles);

        // FEATURE-008 — User-Permission endpoints
        groupBuilder.MapGet("/{id:int}/permissions", GetUserPermissions)
            .Produces<List<UserPermissionDto>>()
            .RequireAuthorization(PermissionCodes.UsersView);

        groupBuilder.MapPost("/{id:int}/permissions", AssignUserPermission)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.UsersManageRoles);

        groupBuilder.MapDelete("/{id:int}/permissions/{permissionId:int}", RemoveUserPermission)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.UsersManageRoles);
    }

    [EndpointSummary("Get all users with optional filters")]
    public static async Task<List<UserDto>> GetUsers(
        [FromServices] ISender sender,
        [AsParameters] GetUsersQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get user by ID")]
    public static async Task<IResult> GetUserById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetUserByIdQuery { Id = id });
        return result.Succeeded
            ? Results.Ok(result.Value!)
            : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new user")]
    public static async Task<IResult> CreateUser(
        [FromServices] ISender sender,
        [FromBody] CreateUserCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Created($"/api/Users/{result.Value}", result.Value);
    }

    [EndpointSummary("Update a user")]
    public static async Task<IResult> UpdateUser(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Deactivate a user")]
    public static async Task<IResult> DeactivateUser(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeactivateUserCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Reactivate a user")]
    public static async Task<IResult> ReactivateUser(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new ReactivateUserCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Create user session")]
    public static async Task<IResult> CreateUserSession(
        [FromServices] ISender sender,
        int id,
        HttpContext http,
        [FromBody] CreateSessionRequest? request)
    {
        var ip = http.Connection.RemoteIpAddress?.ToString()
            ?? http.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? "0.0.0.0";
        var ua = http.Request.Headers.UserAgent.FirstOrDefault();

        var result = await sender.Send(new CreateUserSessionCommand
        {
            UserId = id,
            IpAddress = ip,
            UserAgent = ua,
            DeviceFingerprint = request?.DeviceFingerprint
        });

        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Created($"/api/Users/{id}/sessions/{result.Value}", result.Value);
    }

    public record CreateSessionRequest(string? DeviceFingerprint);

    [EndpointSummary("Create current user session")]
    public static async Task<IResult> CreateCurrentUserSession(
        [FromServices] ISender sender,
        HttpContext http,
        [FromServices] ERP_Government.Infrastructure.Data.ApplicationDbContext db,
        [FromBody] CreateSessionRequest? request)
    {
        var userIdClaim = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var domainUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (domainUser is null)
            return Results.Unauthorized();

        var ip = http.Connection.RemoteIpAddress?.ToString()
            ?? http.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? "0.0.0.0";
        var ua = http.Request.Headers.UserAgent.FirstOrDefault();

        var result = await sender.Send(new CreateUserSessionCommand
        {
            UserId = domainUser.Id,
            IpAddress = ip,
            UserAgent = ua,
            DeviceFingerprint = request?.DeviceFingerprint
        });

        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Created($"/api/Users/{domainUser.Id}/sessions/{result.Value}", result.Value);
    }

    [EndpointSummary("Get user sessions")]
    public static async Task<List<UserSessionDto>> GetUserSessions(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetUserSessionsQuery { UserId = id });
    }

    [EndpointSummary("Revoke a specific session")]
    public static async Task<IResult> RevokeSession(
        [FromServices] ISender sender,
        int id,
        int sessionId)
    {
        var result = await sender.Send(new RevokeSessionCommand { UserId = id, SessionId = sessionId });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Revoke all user sessions")]
    public static async Task<IResult> RevokeAllSessions(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new RevokeAllSessionsCommand { UserId = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Reset failed login attempts")]
    public static async Task<IResult> ResetFailedLoginAttempts(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new ResetFailedLoginAttemptsCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    // FEATURE-008 — User-Role endpoints
    [EndpointSummary("Set user's single role")]
    public static async Task<IResult> SetUserRole(
        [FromServices] ISender sender,
        int id,
        [FromBody] SetUserRoleCommand command)
    {
        if (id != command.UserId)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    // FEATURE-008 — User-Permission endpoints
    [EndpointSummary("Get effective permissions for a user (role + overrides)")]
    public static async Task<List<EffectivePermissionDto>> GetUserPermissions(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetUserPermissionsQuery { UserId = id });
    }

    [EndpointSummary("Assign a permission directly to a user")]
    public static async Task<IResult> AssignUserPermission(
        [FromServices] ISender sender,
        int id,
        [FromBody] AssignUserPermissionCommand command)
    {
        if (id != command.UserId)
            return Results.Problem(
                detail: "ID mismatch.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Remove a direct permission from a user")]
    public static async Task<IResult> RemoveUserPermission(
        [FromServices] ISender sender,
        int id,
        int permissionId)
    {
        var result = await sender.Send(new RemoveUserPermissionCommand { UserId = id, PermissionId = permissionId });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
