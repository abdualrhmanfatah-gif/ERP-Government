using ERP_Government.Application.Budgeting.Commands.BudgetTypes;
using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.BudgetTypes;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class BudgetTypes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBudgetTypes)
            .Produces<List<BudgetTypeDto>>()
            .RequireAuthorization(PermissionCodes.BudgetTypesView);

        groupBuilder.MapGet("/{id:int}", GetBudgetTypeById)
            .Produces<BudgetTypeDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.BudgetTypesView);

        groupBuilder.MapPost("/", CreateBudgetType)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTypesCreate);

        groupBuilder.MapPut("/{id:int}", UpdateBudgetType)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTypesUpdate);

        groupBuilder.MapPatch("/{id:int}/toggle-active", ToggleBudgetTypeActive)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTypesToggleActive);
    }

    [EndpointSummary("Get all budget types")]
    public static async Task<List<BudgetTypeDto>> GetBudgetTypes(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetBudgetTypesListQuery());
    }

    [EndpointSummary("Get budget type by ID")]
    public static async Task<BudgetTypeDto> GetBudgetTypeById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBudgetTypeByIdQuery(id));
    }

    [EndpointSummary("Create a new budget type")]
    public static async Task<IResult> CreateBudgetType(
        [FromServices] ISender sender,
        [FromBody] CreateBudgetTypeRequest body)
    {
        var result = await sender.Send(new CreateBudgetTypeCommand(
            body.Code, body.Name, body.Description, body.ControlMethod, body.AllowOverrun));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/BudgetTypes/{result.Value}", result.Value);
    }

    [EndpointSummary("Update a budget type")]
    public static async Task<IResult> UpdateBudgetType(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateBudgetTypeRequest body)
    {
        var result = await sender.Send(new UpdateBudgetTypeCommand(
            id, body.Code, body.Name, body.Description, body.ControlMethod, body.AllowOverrun, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Toggle budget type active status")]
    public static async Task<IResult> ToggleBudgetTypeActive(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetTypeToggleActiveRequest body)
    {
        var result = await sender.Send(new ToggleBudgetTypeActiveCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

public record CreateBudgetTypeRequest(
    string Code,
    string Name,
    string? Description,
    ERP_Government.Domain.Budgeting.Enums.BudgetControlMethod ControlMethod,
    bool AllowOverrun);

public record UpdateBudgetTypeRequest(
    string Code,
    string Name,
    string? Description,
    ERP_Government.Domain.Budgeting.Enums.BudgetControlMethod ControlMethod,
    bool AllowOverrun,
    byte[] RowVersion);

public record BudgetTypeToggleActiveRequest(byte[] RowVersion);
