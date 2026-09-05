using ERP_Government.Application.Budgeting.Commands.BudgetItems;
using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.BudgetItems;
using ERP_Government.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoint.Budgeting;

public class BudgetItems : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/{id:int}/availability", GetAvailability)
            .Produces<BudgetAvailabilitySummary>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(PermissionCodes.BudgetItemsView);

        groupBuilder.MapGet("/{id:int}/monthly-plan", GetMonthlyPlan)
            .Produces<List<MonthlyPlanDto>>()
            .RequireAuthorization(PermissionCodes.BudgetItemsView);

        groupBuilder.MapPut("/{id:int}/monthly-plan", SaveMonthlyPlan)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(PermissionCodes.BudgetItemsUpdate);
    }

    public static async Task<IResult> GetAvailability(
        [FromServices] ISender sender,
        int id)
    {
        var result = await sender.Send(new GetBudgetItemAvailabilityQuery(id));
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    public static async Task<List<MonthlyPlanDto>> GetMonthlyPlan(
        [FromServices] ISender sender,
        int id)
    {
        return await sender.Send(new GetBudgetItemMonthlyPlanQuery(id));
    }

    public static async Task<IResult> SaveMonthlyPlan(
        [FromServices] ISender sender,
        int id,
        [FromBody] SaveMonthlyPlanRequest body)
    {
        var result = await sender.Send(new SaveBudgetItemMonthlyPlanCommand(id, body.Entries));
        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);
        return Results.NoContent();
    }
}

public record SaveMonthlyPlanRequest(List<MonthlyPlanEntry> Entries);
