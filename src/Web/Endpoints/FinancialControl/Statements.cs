using ERP_Government.Application.Budgeting.Queries.FinancialControl.GetCollectionStatement;
using ERP_Government.Application.Budgeting.Queries.FinancialControl.GetDisbursementStatement;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.FinancialControl;

public class Statements : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/Collection", HandleCollection)
            .RequireAuthorization(PermissionCodes.FinancialControlLapseYear)
            .Produces<CollectionStatementResponse>();
        group.MapGet("/Disbursement", HandleDisbursement)
            .RequireAuthorization(PermissionCodes.FinancialControlLapseYear)
            .Produces<DisbursementStatementResponse>();
    }

    private static async Task<IResult> HandleCollection(
        ISender sender,
        int fiscalYearId)
    {
        var result = await sender.Send(new GetCollectionStatementQuery(fiscalYearId));
        return Results.Ok(new CollectionStatementResponse(
            result.FiscalYearId,
            result.FiscalYearName,
            result.Collections.Select(c => new CollectionLineResponse(
                c.FundCode, c.FundName, c.Date, c.Amount, c.Source)).ToList(),
            result.Subtotals,
            result.GrandTotal,
            result.IsClosed));
    }

    private static async Task<IResult> HandleDisbursement(
        ISender sender,
        int fiscalYearId)
    {
        var result = await sender.Send(new GetDisbursementStatementQuery(fiscalYearId));
        return Results.Ok(new DisbursementStatementResponse(
            result.FiscalYearId,
            result.FiscalYearName,
            result.Disbursements.Select(d => new DisbursementLineResponse(
                d.FundCode, d.FundName, d.Date, d.Amount, d.Payee)).ToList(),
            result.Subtotals,
            result.GrandTotal,
            result.IsClosed));
    }
}

public record CollectionStatementResponse(
    int FiscalYearId,
    string FiscalYearName,
    List<CollectionLineResponse> Collections,
    Dictionary<string, decimal> Subtotals,
    decimal GrandTotal,
    bool IsClosed);

public record CollectionLineResponse(
    string FundCode,
    string FundName,
    DateOnly Date,
    decimal Amount,
    string Source);

public record DisbursementStatementResponse(
    int FiscalYearId,
    string FiscalYearName,
    List<DisbursementLineResponse> Disbursements,
    Dictionary<string, decimal> Subtotals,
    decimal GrandTotal,
    bool IsClosed);

public record DisbursementLineResponse(
    string FundCode,
    string FundName,
    DateOnly Date,
    decimal Amount,
    string Payee);
