using ERP_Government.Application.Security.Common.DTOs;
using ERP_Government.Application.Security.Commands.Roles;
using ERP_Government.Application.Security.Commands.RolePermissions;
using ERP_Government.Application.Security.Queries.Roles;
using ERP_Government.Application.Security.Queries.RolePermissions;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Security;

public class Roles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetRoles)
            .Produces<List<SecurityRoleDto>>()
            .RequireAuthorization(PermissionCodes.RolesView);

        groupBuilder.MapGet("/{id:int}", GetRoleById)
            .Produces<SecurityRoleDto?>()
            .RequireAuthorization(PermissionCodes.RolesView);

        groupBuilder.MapPost("/", CreateRole)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.RolesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateRole)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.RolesUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteRole)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.RolesDelete);

        // FEATURE-008 — RolePermission endpoints
        groupBuilder.MapGet("/{roleId:int}/permissions", GetRolePermissions)
            .Produces<List<RolePermissionDto>>()
            .RequireAuthorization(PermissionCodes.RolesView);

        groupBuilder.MapPost("/{roleId:int}/permissions", AssignRolePermission)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.RolesUpdate);

        groupBuilder.MapDelete("/{roleId:int}/permissions/{permissionId:int}", RemoveRolePermission)
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(PermissionCodes.RolesUpdate);
    }

    [EndpointSummary("Get all roles")]
    public static async Task<List<SecurityRoleDto>> GetRoles(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetRolesQuery());
    }

    [EndpointSummary("Get role by ID")]
    public static async Task<IResult> GetRoleById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetRoleByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }

    [EndpointSummary("Create a new role")]
    public static async Task<IResult> CreateRole(
        [FromServices] ISender sender,
        [FromBody] CreateRoleCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    [EndpointSummary("Update a role")]
    public static async Task<IResult> UpdateRole(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateRoleCommand command)
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

    [EndpointSummary("Delete a role")]
    public static async Task<IResult> DeleteRole(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteRoleCommand { Id = id });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    // FEATURE-008 — RolePermission endpoints
    [EndpointSummary("Get permissions assigned to a role")]
    public static async Task<List<RolePermissionDto>> GetRolePermissions(
        [FromServices] ISender sender,
        int roleId)
    {
        return await sender.Send(new GetRolePermissionsQuery { RoleId = roleId });
    }

    [EndpointSummary("Assign a permission to a role")]
    public static async Task<IResult> AssignRolePermission(
        [FromServices] ISender sender,
        int roleId,
        [FromBody] AssignRolePermissionCommand command)
    {
        if (roleId != command.RoleId)
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

    [EndpointSummary("Remove a permission from a role")]
    public static async Task<IResult> RemoveRolePermission(
        [FromServices] ISender sender,
        int roleId,
        int permissionId)
    {
        var result = await sender.Send(new RemoveRolePermissionCommand { RoleId = roleId, PermissionId = permissionId });
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}
