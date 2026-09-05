using ERP_Government.Application.Budgeting.Commands.BudgetClassifications;
using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.BudgetClassifications;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class BudgetClassifications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBudgetClassifications)
            .Produces<List<BudgetClassificationDto>>()
            .RequireAuthorization(PermissionCodes.BudgetClassificationsView);

        groupBuilder.MapGet("/tree", GetBudgetClassificationsTree)
            .Produces<List<BudgetClassificationDto>>()
            .RequireAuthorization(PermissionCodes.BudgetClassificationsView);

        groupBuilder.MapGet("/{id:int}", GetBudgetClassificationById)
            .Produces<BudgetClassificationDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.BudgetClassificationsView);

        groupBuilder.MapPost("/", CreateBudgetClassification)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetClassificationsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateBudgetClassification)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetClassificationsUpdate);

        groupBuilder.MapPatch("/{id:int}/toggle-active", ToggleBudgetClassificationActive)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetClassificationsToggleActive);
    }

    [EndpointSummary("Get all budget classifications (flat list)")]
    public static async Task<List<BudgetClassificationDto>> GetBudgetClassifications(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetBudgetClassificationsListQuery());
    }

    [EndpointSummary("Get budget classifications as hierarchical tree")]
    public static async Task<List<BudgetClassificationDto>> GetBudgetClassificationsTree(
        [FromServices] ISender sender)
    {
        return await sender.Send(new GetBudgetClassificationsTreeQuery());
    }

    [EndpointSummary("Get budget classification by ID")]
    public static async Task<BudgetClassificationDto> GetBudgetClassificationById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBudgetClassificationByIdQuery(id));
    }

    [EndpointSummary("Create a new budget classification")]
    public static async Task<IResult> CreateBudgetClassification(
        [FromServices] ISender sender,
        [FromBody] CreateBudgetClassificationRequest body)
    {
        var result = await sender.Send(new CreateBudgetClassificationCommand(
            body.Code, body.Name, body.ParentId));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/BudgetClassifications/{result.Value}", result.Value);
    }

    [EndpointSummary("Update a budget classification")]
    public static async Task<IResult> UpdateBudgetClassification(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateBudgetClassificationRequest body)
    {
        var result = await sender.Send(new UpdateBudgetClassificationCommand(
            id, body.Code, body.Name, body.ParentId, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Toggle budget classification active status")]
    public static async Task<IResult> ToggleBudgetClassificationActive(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetClassificationToggleActiveRequest body)
    {
        var result = await sender.Send(new ToggleBudgetClassificationActiveCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

public record CreateBudgetClassificationRequest(
    string Code,
    string Name,
    int? ParentId);

public record UpdateBudgetClassificationRequest(
    string Code,
    string Name,
    int? ParentId,
    byte[] RowVersion);

public record BudgetClassificationToggleActiveRequest(byte[] RowVersion);
