using ERP_Government.Application.Budgeting.Commands.FinancialControl.LapseFiscalYear;
using ERP_Government.Application.Budgeting.Commands.FinancialControl.ReopenFiscalYear;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.FinancialControl;

public class YearClosing : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/Lapse", HandleLapse)
            .RequireAuthorization(PermissionCodes.FinancialControlLapseYear)
            .Produces<LapseFiscalYearResponse>();
        group.MapPost("/Reopen", HandleReopen)
            .RequireAuthorization(PermissionCodes.FinancialControlLapseYear)
            .Produces<ReopenFiscalYearResponse>();
    }

    private static async Task<IResult> HandleLapse(
        ISender sender,
        LapseFiscalYearRequest request)
    {
        var result = await sender.Send(new LapseFiscalYearCommand(request.FiscalYearId));
        return result.Succeeded && result.Value is not null
            ? Results.Ok(result.Value.ToResponse())
            : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleReopen(
        ISender sender,
        ReopenFiscalYearRequest request)
    {
        var result = await sender.Send(new ReopenFiscalYearCommand(request.FiscalYearId));
        return result.Succeeded && result.Value is not null
            ? Results.Ok(result.Value.ToResponse())
            : Results.BadRequest(result.Errors);
    }
}

public record LapseFiscalYearRequest(int FiscalYearId);
public record ReopenFiscalYearRequest(int FiscalYearId);

public record LapseFiscalYearResponse(
    int YearClosingRunId,
    int FiscalYearId,
    string FiscalYearName,
    decimal LapsedAppropriationTotal,
    decimal LapsedEncumbranceTotal);

public record ReopenFiscalYearResponse(
    int YearClosingRunId,
    int FiscalYearId,
    decimal RestoredBudgetTotal,
    decimal RestoredEncumbranceTotal);

internal static class YearClosingMappingExtensions
{
    public static LapseFiscalYearResponse ToResponse(this LapseFiscalYearResult result) => new(
        result.YearClosingRunId,
        result.FiscalYearId,
        result.FiscalYearName,
        result.LapsedAppropriationTotal,
        result.LapsedEncumbranceTotal);

    public static ReopenFiscalYearResponse ToResponse(this ReopenFiscalYearResult result) => new(
        result.YearClosingRunId,
        result.FiscalYearId,
        result.RestoredBudgetTotal,
        result.RestoredEncumbranceTotal);
}
