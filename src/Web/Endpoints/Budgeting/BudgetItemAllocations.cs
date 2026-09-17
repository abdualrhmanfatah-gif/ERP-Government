using ERP_Government.Application.Budgeting.Commands.BudgetItemAllocations;
using ERP_Government.Application.Budgeting.Queries.BudgetItemAllocations;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class BudgetItemAllocations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBudgetItemAllocations)
            .Produces<List<BudgetItemAllocationDto>>()
            .RequireAuthorization(PermissionCodes.BudgetItemAllocationsView);

        groupBuilder.MapGet("/{id:int}", GetBudgetItemAllocationById)
            .Produces<BudgetItemAllocationDetailDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.BudgetItemAllocationsView);

        groupBuilder.MapPost("/", CreateBudgetItemAllocation)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemAllocationsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateBudgetItemAllocation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemAllocationsUpdate);

        groupBuilder.MapDelete("/{id:int}", DeleteBudgetItemAllocation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemAllocationsDelete);
    }

    public static async Task<List<BudgetItemAllocationDto>> GetBudgetItemAllocations(
        [FromServices] ISender sender,
        int budgetId)
    {
        return await sender.Send(new GetBudgetItemAllocationsListQuery(budgetId));
    }

    public static async Task<IResult> GetBudgetItemAllocationById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetBudgetItemAllocationByIdQuery(id));
        return result.Succeeded
            ? Results.Ok(result.Value!)
            : result.ToProblemDetails();
    }

    public static async Task<IResult> CreateBudgetItemAllocation(
        [FromServices] ISender sender,
        [FromBody] CreateBudgetItemAllocationRequest body)
    {
        var result = await sender.Send(new CreateBudgetItemAllocationCommand(
            body.BudgetId, body.BudgetItemId, body.ProposedAmount, body.Remarks));
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.Created($"/api/BudgetItemAllocations/{result.Value}", result.Value);
    }

    public static async Task<IResult> UpdateBudgetItemAllocation(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateBudgetItemAllocationRequest body)
    {
        var result = await sender.Send(new UpdateBudgetItemAllocationCommand(
            id, body.ProposedAmount, body.Remarks, body.RowVersion));
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }

    public static async Task<IResult> DeleteBudgetItemAllocation(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteBudgetItemAllocationCommand(id));
        if (!result.Succeeded)
            return result.ToProblemDetails();
        return Results.NoContent();
    }
}

public record CreateBudgetItemAllocationRequest(
    int BudgetId,
    int BudgetItemId,
    decimal ProposedAmount,
    string? Remarks);

public record UpdateBudgetItemAllocationRequest(
    decimal ProposedAmount,
    string? Remarks,
    byte[] RowVersion);
