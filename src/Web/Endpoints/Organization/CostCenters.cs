using ERP_Government.Application.Organization.Common.DTOs;
using ERP_Government.Application.Organization.Commands.CostCenters;
using ERP_Government.Application.Organization.Queries.CostCenters;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Organization;

public class CostCenters : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetCostCenters)
            .Produces<List<CostCenterDto>>()
            .RequireAuthorization(PermissionCodes.CostCentersView);

        groupBuilder.MapGet("/{id:int}", GetCostCenterById)
            .Produces<CostCenterDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.CostCentersView);

        groupBuilder.MapPost("/", CreateCostCenter)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CostCentersCreate);

        groupBuilder.MapPut("/{id:int}", UpdateCostCenter)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CostCentersUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteCostCenter)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.CostCentersDelete);
    }

    [EndpointSummary("Get all cost centers")]
    public static async Task<List<CostCenterDto>> GetCostCenters(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetCostCentersQuery());
    }

    [EndpointSummary("Get cost center by ID")]
    public static async Task<CostCenterDto> GetCostCenterById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetCostCenterByIdQuery { Id = id });
    }

    [EndpointSummary("Create a new cost center")]
    public static async Task<IResult> CreateCostCenter(
        [FromServices] ISender sender,
        [FromBody] CreateCostCenterCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Update a cost center")]
    public static async Task<IResult> UpdateCostCenter(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateCostCenterRequest body)
    {
        var result = await sender.Send(new UpdateCostCenterCommand
        {
            Id = id,
            Code = body.Code,
            Name = body.Name,
            OrganizationUnitId = body.OrganizationUnitId,
            BudgetLimit = body.BudgetLimit,
            IsActive = body.IsActive
        });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Delete a cost center")]
    public static async Task<IResult> DeleteCostCenter(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteCostCenterCommand { Id = id });
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

public record UpdateCostCenterRequest(
    string Code,
    string Name,
    int? OrganizationUnitId,
    decimal? BudgetLimit,
    bool IsActive);