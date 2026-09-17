using ERP_Government.Application.Security.Common.DTOs;
using ERP_Government.Application.Security.Queries.Permissions;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Security;

public class Permissions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetPermissions)
            .Produces<List<SecurityPermissionDto>>()
            .RequireAuthorization(PermissionCodes.PermissionsView);

        groupBuilder.MapGet("/{id:int}", GetPermissionById)
            .Produces<SecurityPermissionDto?>()
            .RequireAuthorization(PermissionCodes.PermissionsView);
    }

    [EndpointSummary("Get all permissions")]
    public static async Task<List<SecurityPermissionDto>> GetPermissions(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetPermissionsQuery());
    }

    [EndpointSummary("Get permission by ID")]
    public static async Task<IResult> GetPermissionById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetPermissionByIdQuery { Id = id });
        return result.Succeeded ? Results.Ok(result.Value!) : result.ToProblemDetails();
    }
}
