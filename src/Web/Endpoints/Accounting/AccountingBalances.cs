using ERP_Government.Application.Accounting.Commands.AccountBalances.Finalize;
using ERP_Government.Application.Accounting.Commands.AccountBalances.Unfinalize;
using ERP_Government.Application.Accounting.Commands.Balances.RebuildAccountBalances;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.Balances.GetAccountBalances;
using ERP_Government.Application.Accounting.Queries.Balances.ReconcileBalances;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Accounting;

public class AccountingBalances : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetAccountBalances)
            .RequireAuthorization(PermissionCodes.BalancesRead);

        group.MapGet("/reconcile", ReconcileBalances)
            .RequireAuthorization(PermissionCodes.BalancesRead);

        group.MapPost("/rebuild", RebuildAccountBalances)
            .RequireAuthorization(PermissionCodes.BalancesRebuild);

        group.MapPost("/finalize", FinalizePeriod)
            .RequireAuthorization(PermissionCodes.BalancesFinalize);

        group.MapPost("/unfinalize", UnfinalizePeriod)
            .RequireAuthorization(PermissionCodes.BalancesUnfinalize);
    }

    private static async Task<IResult> GetAccountBalances(
        [FromServices] ISender sender,
        [AsParameters] GetAccountBalancesQuery query)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> ReconcileBalances(
        [FromServices] ISender sender,
        [AsParameters] ReconcileBalancesQuery query)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> RebuildAccountBalances(
        [FromServices] ISender sender,
        RebuildAccountBalancesCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(new { success = true, message = "تم إعادة بناء الأرصدة بنجاح" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> FinalizePeriod(
        [FromServices] ISender sender,
        FinalizePeriodCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(new { success = true, message = "تم إغلاق الفترة" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }

    private static async Task<IResult> UnfinalizePeriod(
        [FromServices] ISender sender,
        UnfinalizePeriodCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(new { success = true, message = "تم فتح الفترة" })
            : Results.BadRequest(new { success = false, errors = result.Errors });
    }
}
