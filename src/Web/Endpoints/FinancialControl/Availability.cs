using ERP_Government.Application.Budgeting.Common;
using ERP_Government.Application.Budgeting.Queries.FinancialControl.GetAvailabilityBreakdown;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.FinancialControl;

public class Availability : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{budgetItemId:int}", HandleGetAvailabilityBreakdown)
            .RequireAuthorization(PermissionCodes.BudgetItemsView)
            .Produces<AvailabilityBreakdownResponse>();
    }

    private static async Task<IResult> HandleGetAvailabilityBreakdown(
        ISender sender,
        int budgetItemId,
        int fiscalYearId)
    {
        var result = await sender.Send(new GetAvailabilityBreakdownQuery(budgetItemId, fiscalYearId));
        return Results.Ok(result.ToResponse());
    }
}

public record AvailabilityBreakdownResponse(
    int BudgetItemId,
    string BudgetItemCode,
    int FiscalYearId,
    string FiscalYearName,
    List<AvailabilityBreakdownLineResponse> Breakdown,
    AvailabilityBreakdownTotalResponse Totals);

public record AvailabilityBreakdownLineResponse(
    int FundId,
    string FundCode,
    string FundName,
    int? ProgramId,
    string? ProgramCode,
    string? ProgramName,
    int? ProjectId,
    string? ProjectCode,
    string? ProjectName,
    int BudgetItemId,
    string ItemCode,
    decimal AppropriationAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);

public record AvailabilityBreakdownTotalResponse(
    decimal AppropriationAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);

internal static class AvailabilityMappingExtensions
{
    public static AvailabilityBreakdownResponse ToResponse(this AvailabilityBreakdownResult result) => new(
        result.BudgetItemId,
        result.BudgetItemCode,
        result.FiscalYearId,
        result.FiscalYearName,
        result.Breakdown.Select(b => new AvailabilityBreakdownLineResponse(
            b.FundId, b.FundCode, b.FundName,
            b.ProgramId, b.ProgramCode, b.ProgramName,
            b.ProjectId, b.ProjectCode, b.ProjectName,
            b.BudgetItemId, b.ItemCode,
            b.AppropriationAmount, b.EncumberedAmount, b.PaidAmount, b.AvailableAmount)).ToList(),
        new AvailabilityBreakdownTotalResponse(
            result.Totals.AppropriationAmount,
            result.Totals.EncumberedAmount,
            result.Totals.PaidAmount,
            result.Totals.AvailableAmount));
}
