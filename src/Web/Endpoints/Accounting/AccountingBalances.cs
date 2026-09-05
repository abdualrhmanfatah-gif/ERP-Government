using ERP_Government.Application.Accounting.Commands.Balances.FinalizePeriod;
using ERP_Government.Application.Accounting.Commands.Balances.RebuildAccountBalances;
using ERP_Government.Application.Accounting.Commands.Balances.UnfinalizePeriod;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.Balances.GetAccountBalances;
using ERP_Government.Application.Accounting.Queries.Balances.ReconcileBalances;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class AccountingBalances : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", GetAccountBalances)
            .Produces<List<AccountBalanceDto>>()
            .RequireAuthorization(PermissionCodes.BalancesRead);

        groupBuilder.MapGet("/reconcile", ReconcileBalances)
            .Produces<ReconciliationResultDto>()
            .RequireAuthorization(PermissionCodes.BalancesRead);

        groupBuilder.MapPost("/rebuild", RebuildAccountBalances)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BalancesRebuild);

        groupBuilder.MapPost("/finalize", FinalizePeriod)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BalancesFinalize);

        groupBuilder.MapPost("/unfinalize", UnfinalizePeriod)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BalancesUnfinalize);
    }

    [EndpointSummary("Get account balances for a period")]
    public static async Task<List<AccountBalanceDto>> GetAccountBalances(
        [FromServices] ISender sender,
        [AsParameters] GetAccountBalancesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Reconcile materialized balances against source JournalEntryLines")]
    public static async Task<ReconciliationResultDto> ReconcileBalances(
        [FromServices] ISender sender,
        [AsParameters] ReconcileBalancesQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Rebuild account balances from posted journal entries")]
    public static async Task<IResult> RebuildAccountBalances(
        [FromServices] ISender sender,
        [FromBody] RebuildAccountBalancesCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Finalize account balances for a period")]
    public static async Task<IResult> FinalizePeriod(
        [FromServices] ISender sender,
        [FromBody] FinalizePeriodCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }

    [EndpointSummary("Unfinalize account balances for a period")]
    public static async Task<IResult> UnfinalizePeriod(
        [FromServices] ISender sender,
        [FromBody] UnfinalizePeriodCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}
