using ERP_Government.Application.Organization.Common.DTOs;
using ERP_Government.Application.Organization.Commands.OrganizationalUnits;
using ERP_Government.Application.Organization.Queries.OrganizationalUnits;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Organization;

public class OrganizationalUnits : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetOrganizationalUnits)
            .Produces<List<OrganizationalUnitDto>>()
            .RequireAuthorization(PermissionCodes.OrgUnitsView);

        groupBuilder.MapGet("/{id:int}", GetOrganizationalUnitById)
            .Produces<OrganizationalUnitDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.OrgUnitsView);

        groupBuilder.MapPost("/", CreateOrganizationalUnit)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.OrgUnitsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateOrganizationalUnit)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.OrgUnitsUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteOrganizationalUnit)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.OrgUnitsDelete);
    }

    [EndpointSummary("Get all organizational units")]
    public static async Task<List<OrganizationalUnitDto>> GetOrganizationalUnits(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetOrganizationalUnitsQuery());
    }

    [EndpointSummary("Get organizational unit by ID")]
    public static async Task<OrganizationalUnitDto> GetOrganizationalUnitById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetOrganizationalUnitByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new organizational unit")]
    public static async Task<IResult> CreateOrganizationalUnit(
        [FromServices] ISender sender,
        [FromBody] CreateOrganizationalUnitCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Update an organizational unit")]
    public static async Task<IResult> UpdateOrganizationalUnit(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateOrganizationalUnitRequest body)
    {
        var result = await sender.Send(new UpdateOrganizationalUnitCommand
        {
            Id = id,
            Code = body.Code,
            Name = body.Name,
            ParentId = body.ParentId,
            IsActive = body.IsActive
        });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Delete an organizational unit")]
    public static async Task<IResult> DeleteOrganizationalUnit(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteOrganizationalUnitCommand { Id = id });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

public record UpdateOrganizationalUnitRequest(
    string Code,
    string Name,
    int? ParentId,
    bool IsActive);
