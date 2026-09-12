using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ApproveBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CancelBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.CreateBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.DeleteBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.PostBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.ReverseBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.SubmitBudgetTransaction;
using ERP_Government.Application.Budgeting.Commands.BudgetTransactions.UpdateBudgetTransaction;
using ERP_Government.Application.Budgeting.Queries.BudgetTransactions.GetBudgetTransactionById;
using ERP_Government.Application.Budgeting.Queries.BudgetTransactions.GetBudgetTransactionsList;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Budgeting.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class BudgetTransactions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetBudgetTransactions)
            .Produces<List<BudgetTransactionListItemDto>>()
            .RequireAuthorization(PermissionCodes.BudgetTransactionsView);

        groupBuilder.MapGet("/{id:int}", GetBudgetTransactionById)
            .Produces<BudgetTransactionDetailDto>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.BudgetTransactionsView);

        groupBuilder.MapPost("/", CreateBudgetTransaction)
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTransactionsCreate);

        groupBuilder.MapPatch("/{id:int}/submit", SubmitBudgetTransaction)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTransactionsSubmit);

        groupBuilder.MapPatch("/{id:int}/approve", ApproveBudgetTransaction)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTransactionsApprove);

        groupBuilder.MapPatch("/{id:int}/post", PostBudgetTransaction)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTransactionsPost);

        groupBuilder.MapPatch("/{id:int}/cancel", CancelBudgetTransaction)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTransactionsCancel);

        groupBuilder.MapPatch("/{id:int}/reverse", ReverseBudgetTransaction)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetTransactionsReverse);
    }

    public static async Task<List<BudgetTransactionListItemDto>> GetBudgetTransactions(
        [FromServices] ISender sender,
        int? budgetItemAllocationId,
        BudgetTransactionType? transactionType,
        BudgetTransactionStatus? status)
    {
        return await sender.Send(new GetBudgetTransactionsListQuery(budgetItemAllocationId, transactionType, status));
    }

    public static async Task<IResult> GetBudgetTransactionById(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetBudgetTransactionByIdQuery(id));
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    public static async Task<IResult> CreateBudgetTransaction(
        [FromServices] ISender sender,
        [FromBody] CreateBudgetTransactionRequest body)
    {
        var result = await sender.Send(new CreateBudgetTransactionCommand(
            body.BudgetItemAllocationId, body.TransactionType, body.TransactionDate,
            body.Amount, body.Direction, body.DocumentType, body.DocumentId, body.Description));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/BudgetTransactions/{result.Value}", result.Value);
    }

    public static async Task<IResult> SubmitBudgetTransaction(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetTransactionActionRequest body)
    {
        var result = await sender.Send(new SubmitBudgetTransactionCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> ApproveBudgetTransaction(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetTransactionActionRequest body)
    {
        var result = await sender.Send(new ApproveBudgetTransactionCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> PostBudgetTransaction(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetTransactionActionRequest body)
    {
        var result = await sender.Send(new PostBudgetTransactionCommand(id, body.RowVersion));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> CancelBudgetTransaction(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetTransactionActionRequest body)
    {
        var result = await sender.Send(new CancelBudgetTransactionCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    public static async Task<IResult> ReverseBudgetTransaction(
        [FromServices] ISender sender,
        int id,
        [FromBody] BudgetTransactionActionRequest body)
    {
        var result = await sender.Send(new ReverseBudgetTransactionCommand(id, body.RowVersion, body.Reason));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.Created($"/api/BudgetTransactions/{result.Value}", result.Value);
    }
}

public record CreateBudgetTransactionRequest(
    int BudgetItemAllocationId,
    BudgetTransactionType TransactionType,
    DateOnly TransactionDate,
    decimal Amount,
    TransactionDirection Direction,
    string? DocumentType,
    int? DocumentId,
    string? Description);

public record BudgetTransactionActionRequest(byte[] RowVersion, string? Reason = null);
