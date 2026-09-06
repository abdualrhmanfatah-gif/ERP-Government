using ERP_Government.Application.Budgeting.Commands.Budgets;
using ERP_Government.Application.Budgeting.Commands.BudgetItems;
using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.BudgetItems;
using ERP_Government.Application.Budgeting.Queries.Budgets;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class Budgets : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        // ─── Budget endpoints ───────────────────────────────────────
        groupBuilder.MapGet("/", GetBudgets)
            .Produces<List<BudgetDto>>()
            .RequireAuthorization(PermissionCodes.BudgetsView);

        groupBuilder.MapGet("/{id:int}", GetBudgetById)
            .Produces<BudgetDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.BudgetsView);

        groupBuilder.MapPost("/", CreateBudget)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsCreate);

        groupBuilder.MapPut("/{id:int}", UpdateBudget)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsUpdate);

        groupBuilder.MapPatch("/{id:int}/submit", SubmitBudget)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsSubmit);

        groupBuilder.MapPatch("/{id:int}/approve", ApproveBudget)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsApprove);

        groupBuilder.MapPatch("/{id:int}/activate", ActivateBudget)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsActivate);

        groupBuilder.MapPatch("/{id:int}/suspend", SuspendBudget)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsSuspend);

        groupBuilder.MapPatch("/{id:int}/close", CloseBudget)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsClose);

        groupBuilder.MapPatch("/{id:int}/cancel", CancelBudget)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetsCancel);

        // ─── BudgetItem endpoints (nested under Budget) ─────────────
        groupBuilder.MapGet("/{id:int}/items", GetBudgetItems)
            .Produces<List<BudgetItemDto>>()
            .RequireAuthorization(PermissionCodes.BudgetItemsView);

        groupBuilder.MapGet("/{id:int}/items/tree", GetBudgetItemsTree)
            .Produces<List<BudgetItemDto>>()
            .RequireAuthorization(PermissionCodes.BudgetItemsView);

        groupBuilder.MapPost("/{id:int}/items", CreateBudgetItem)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemsCreate);

        groupBuilder.MapPut("/{budgetId:int}/items/{itemId:int}", UpdateBudgetItem)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemsUpdate);

        groupBuilder.MapDelete("/{budgetId:int}/items/{itemId:int}", DeleteBudgetItem)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemsDelete);

        groupBuilder.MapPatch("/{budgetId:int}/items/{itemId:int}/move", MoveBudgetItem)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemsMove);
    }

    // ─── Budget handlers ────────────────────────────────────────────

    [EndpointSummary("Get all budgets with optional filters")]
    public static async Task<List<BudgetDto>> GetBudgets(
        [FromServices] ISender sender,
        [AsParameters] GetBudgetsListQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get budget by ID")]
    public static async Task<BudgetDto> GetBudgetById(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBudgetByIdQuery(id));
    }

    [EndpointSummary("Create a new budget")]
    public static async Task<IResult> CreateBudget(
        [FromServices] ISender sender,
        [FromBody] CreateBudgetRequest body)
    {
        var result = await sender.Send(new CreateBudgetCommand(
            body.BudgetName, body.FiscalYearId, body.FundId, body.BudgetTypeId));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/Budgets/{result.Value}", result.Value);
    }

    [EndpointSummary("Update a budget")]
    public static async Task<IResult> UpdateBudget(
        [FromServices] ISender sender,
        int id,
        [FromBody] UpdateBudgetRequest body)
    {
        var result = await sender.Send(new UpdateBudgetCommand(
            id, body.BudgetNumber, body.BudgetName, body.FiscalYearId, body.FundId, body.BudgetTypeId, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Submit a budget for approval")]
    public static async Task<IResult> SubmitBudget(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetActionRequest body)
    {
        var result = await sender.Send(new SubmitBudgetCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Approve a submitted budget")]
    public static async Task<IResult> ApproveBudget(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetActionRequest body)
    {
        var result = await sender.Send(new ApproveBudgetCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Activate an approved budget")]
    public static async Task<IResult> ActivateBudget(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetActionRequest body)
    {
        var result = await sender.Send(new ActivateBudgetCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Suspend an active budget")]
    public static async Task<IResult> SuspendBudget(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetActionRequest body)
    {
        var result = await sender.Send(new SuspendBudgetCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Close an active or suspended budget")]
    public static async Task<IResult> CloseBudget(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetActionRequest body)
    {
        var result = await sender.Send(new CloseBudgetCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Cancel a draft, submitted, or suspended budget")]
    public static async Task<IResult> CancelBudget(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetActionRequest body)
    {
        var result = await sender.Send(new CancelBudgetCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    // ─── BudgetItem handlers ────────────────────────────────────────

    [EndpointSummary("Get budget items for a budget")]
    public static async Task<List<BudgetItemDto>> GetBudgetItems(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBudgetItemsListQuery(id));
    }

    [EndpointSummary("Get budget items as hierarchical tree for a budget")]
    public static async Task<List<BudgetItemDto>> GetBudgetItemsTree(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBudgetItemsTreeQuery(id));
    }

    [EndpointSummary("Create a new budget item under a budget")]
    public static async Task<IResult> CreateBudgetItem(
        [FromServices] ISender sender,
        int id,
        [FromBody] CreateBudgetItemRequest body)
    {
        var result = await sender.Send(new CreateBudgetItemCommand(
            id, body.ItemCode, body.ItemName, body.ParentId, body.AccountId, body.FundId, body.CostCenterId, body.BudgetClassificationId, body.Remarks, body.AllowOverrun));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/Budgets/{id}/items/{result.Value}", result.Value);
    }

    [EndpointSummary("Update a budget item")]
    public static async Task<IResult> UpdateBudgetItem(
        [FromServices] ISender sender,
        int budgetId,
        int itemId,
        [FromBody] UpdateBudgetItemRequest body)
    {
        var result = await sender.Send(new UpdateBudgetItemCommand(
            itemId, body.ItemName, body.Remarks, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Delete a budget item")]
    public static async Task<IResult> DeleteBudgetItem(
        [FromServices] ISender sender,
        int budgetId,
        int itemId)
    {
        var result = await sender.Send(new DeleteBudgetItemCommand(itemId));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Move a budget item to a new parent")]
    public static async Task<IResult> MoveBudgetItem(
        [FromServices] ISender sender,
        int budgetId,
        int itemId,
        [FromBody] MoveBudgetItemRequest body)
    {
        var result = await sender.Send(new MoveBudgetItemCommand(
            itemId, body.NewParentId, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

// ─── Request DTOs ────────────────────────────────────────────────

public record CreateBudgetRequest(
    string BudgetName,
    int FiscalYearId,
    int FundId,
    int BudgetTypeId);

public record UpdateBudgetRequest(
    string BudgetNumber,
    string BudgetName,
    int FiscalYearId,
    int FundId,
    int BudgetTypeId,
    byte[] RowVersion);

public record BudgetActionRequest(byte[] RowVersion);

public record CreateBudgetItemRequest(
    string ItemCode,
    string ItemName,
    int? ParentId,
    int? AccountId,
    int? FundId,
    int? CostCenterId,
    int? BudgetClassificationId,
    string? Remarks,
    bool? AllowOverrun);

public record UpdateBudgetItemRequest(
    string ItemName,
    string? Remarks,
    byte[] RowVersion);

public record MoveBudgetItemRequest(
    int? NewParentId,
    byte[] RowVersion);
