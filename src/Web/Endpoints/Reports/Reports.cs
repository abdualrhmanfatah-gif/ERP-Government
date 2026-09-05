using ERP_Government.Application.Accounting.Reports.BalanceSheet;
using ERP_Government.Application.Accounting.Reports.CashFlowStatement;
using ERP_Government.Application.Accounting.Reports.Common;
using ERP_Government.Application.Accounting.Reports.GeneralLedger;
using ERP_Government.Application.Accounting.Reports.IncomeStatement;
using ERP_Government.Application.Accounting.Reports.TrialBalance;
using ERP_Government.Application.Common.Security;
using ERP_Government.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Reports;

public class Reports : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/balance-sheet", GetBalanceSheet)
            .Produces<BalanceSheetDto>()
            .RequireAuthorization(PermissionCodes.ViewBalanceSheet);

        groupBuilder.MapGet("/income-statement", GetIncomeStatement)
            .Produces<IncomeStatementDto>()
            .RequireAuthorization(PermissionCodes.ViewIncomeStatement);

        groupBuilder.MapGet("/general-ledger", GetGeneralLedger)
            .Produces<GeneralLedgerDto>()
            .RequireAuthorization(PermissionCodes.ViewGeneralLedger);

        groupBuilder.MapGet("/cash-flow", GetCashFlowStatement)
            .Produces<CashFlowStatementDto>()
            .RequireAuthorization(PermissionCodes.ViewCashFlow);

        groupBuilder.MapGet("/trial-balance", GetTrialBalance)
            .Produces<TrialBalanceDto>()
            .RequireAuthorization(PermissionCodes.ViewTrialBalance);

        groupBuilder.MapGet("/{reportType}/export", ExportReport)
            .RequireAuthorization(PermissionCodes.ExportReports);
    }

    [EndpointSummary("Get balance sheet as of a date")]
    public static async Task<BalanceSheetDto> GetBalanceSheet(
        [FromServices] ISender sender,
        [AsParameters] GetBalanceSheetQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get income statement for a date range")]
    public static async Task<IncomeStatementDto> GetIncomeStatement(
        [FromServices] ISender sender,
        [AsParameters] GetIncomeStatementQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get general ledger for accounts")]
    public static async Task<GeneralLedgerDto> GetGeneralLedger(
        [FromServices] ISender sender,
        [AsParameters] GetGeneralLedgerQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get cash flow statement for a date range")]
    public static async Task<CashFlowStatementDto> GetCashFlowStatement(
        [FromServices] ISender sender,
        [AsParameters] GetCashFlowStatementQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get trial balance for a fiscal period")]
    public static async Task<TrialBalanceDto> GetTrialBalance(
        [FromServices] ISender sender,
        [AsParameters] GetTrialBalanceQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Export report to Excel or PDF")]
    public static async Task<IResult> ExportReport(
        string reportType,
        [FromQuery] string format,
        [FromServices] ISender sender,
        [FromQuery] DateOnly? asOfDate,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] int? accountId,
        [FromQuery] string? accountCode,
        [FromQuery] int? fiscalYearId,
        [FromQuery] int? fiscalPeriodId)
    {
        ReportResult reportResult;
        string reportName;

        switch (reportType.ToLower())
        {
            case "balance-sheet":
                reportName = "Balance Sheet";
                var bsResult = await sender.Send(new GetBalanceSheetQuery { AsOfDate = (asOfDate ?? DateOnly.FromDateTime(DateTime.Today)).ToString("yyyy-MM-dd") });
                reportResult = new ReportResult { Currency = bsResult.Currency, GeneratedAt = bsResult.GeneratedAt, Sections = [] };
                break;
            case "income-statement":
                reportName = "Income Statement";
                var isResult = await sender.Send(new GetIncomeStatementQuery { StartDate = (startDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30))).ToString("yyyy-MM-dd"), EndDate = (endDate ?? DateOnly.FromDateTime(DateTime.Today)).ToString("yyyy-MM-dd") });
                reportResult = new ReportResult { Currency = isResult.Currency, GeneratedAt = isResult.GeneratedAt, Sections = [] };
                break;
            case "general-ledger":
                reportName = "General Ledger";
                var glResult = await sender.Send(new GetGeneralLedgerQuery { AccountId = accountId, AccountCode = accountCode, StartDate = startDate?.ToString("yyyy-MM-dd"), EndDate = endDate?.ToString("yyyy-MM-dd") });
                reportResult = new ReportResult { Currency = glResult.Currency, GeneratedAt = glResult.GeneratedAt, TotalLines = glResult.TotalLines, Sections = [] };
                break;
            case "cash-flow":
                reportName = "Cash Flow Statement";
                var cfResult = await sender.Send(new GetCashFlowStatementQuery { StartDate = (startDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30))).ToString("yyyy-MM-dd"), EndDate = (endDate ?? DateOnly.FromDateTime(DateTime.Today)).ToString("yyyy-MM-dd") });
                reportResult = new ReportResult { Currency = cfResult.Currency, GeneratedAt = cfResult.GeneratedAt, Sections = [] };
                break;
            case "trial-balance":
                reportName = "Trial Balance";
                var tbResult = await sender.Send(new GetTrialBalanceQuery { FiscalYearId = fiscalYearId ?? 0, FiscalPeriodId = fiscalPeriodId ?? 0 });
                reportResult = new ReportResult { Currency = tbResult.Currency, GeneratedAt = tbResult.GeneratedAt, Sections = tbResult.Sections };
                break;
            default:
                return Results.BadRequest($"Unknown report type: {reportType}");
        }

        var stream = new MemoryStream();
        var exporter = format?.ToLower() == "pdf" ? (IReportExporter)new PdfReportExporter() : new ExcelReportExporter();
        await exporter.ExportExcelAsync(reportResult, reportName, stream);
        stream.Position = 0;

        var contentType = format?.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var extension = format?.ToLower() == "pdf" ? "pdf" : "xlsx";
        return Results.File(stream, contentType, $"{reportName}-{DateTime.Now:yyyyMMdd}.{extension}");
    }
}
