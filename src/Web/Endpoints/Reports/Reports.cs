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
        [FromServices] ERP_Government.Infrastructure.Services.PdfReportExporter pdfExporter,
        [FromServices] ERP_Government.Infrastructure.Services.ExcelReportExporter excelExporter,
        [FromQuery] DateOnly? asOfDate,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] int? accountId,
        [FromQuery] string? accountCode,
        [FromQuery] int? fiscalYearId,
        [FromQuery] int? fiscalPeriodId,
        [FromQuery] string? pageSize,
        [FromQuery] bool? isLandscape)
    {
        ReportResult reportResult;
        string reportName;

        switch (reportType.ToLower())
        {
            case "balance-sheet":
                reportName = "Balance Sheet";
                var bsResult = await sender.Send(new GetBalanceSheetQuery
                {
                    AsOfDate = (asOfDate ?? DateOnly.FromDateTime(DateTime.Today)).ToString("yyyy-MM-dd"),
                    FiscalPeriodId = fiscalPeriodId
                });
                var bsSections = new List<ReportSection>();
                AddBalanceSheetSection(bsSections, "الأصول المتداولة", "Current Assets", bsResult.CurrentAssets);
                AddBalanceSheetSection(bsSections, "الأصول غير المتداولة", "Non-current Assets", bsResult.NonCurrentAssets);
                AddBalanceSheetSection(bsSections, "الخصوم المتداولة", "Current Liabilities", bsResult.CurrentLiabilities);
                AddBalanceSheetSection(bsSections, "الخصوم غير المتداولة", "Non-current Liabilities", bsResult.NonCurrentLiabilities);
                AddBalanceSheetSection(bsSections, "حقوق الملكية / صافي الأصول", "Equity / Net Assets", bsResult.Equity);
                reportResult = new ReportResult { Currency = bsResult.Currency, GeneratedAt = bsResult.GeneratedAt, Sections = bsSections, PaperSize = pageSize, IsLandscape = isLandscape ?? true };
                break;
            case "income-statement":
                reportName = "Income Statement";
                var isResult = await sender.Send(new GetIncomeStatementQuery { StartDate = (startDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30))).ToString("yyyy-MM-dd"), EndDate = (endDate ?? DateOnly.FromDateTime(DateTime.Today)).ToString("yyyy-MM-dd") });
                var isSections = new List<ReportSection>();
                if (isResult.Revenue?.Sections?.Count > 0)
                    isSections.Add(new ReportSection { Title = "الإيرادات", TitleEn = "Revenue", Lines = isResult.Revenue.Sections.SelectMany(s => s.Lines ?? []).Select(l => new ReportLine { AccountCode = l.AccountCode ?? "", AccountName = l.AccountName ?? "", Debit = l.Debit, Credit = l.Credit, Balance = l.Balance }).ToList(), Total = isResult.Revenue.Total });
                if (isResult.Expenses?.Sections?.Count > 0)
                    isSections.Add(new ReportSection { Title = "المصروفات", TitleEn = "Expenses", Lines = isResult.Expenses.Sections.SelectMany(s => s.Lines ?? []).Select(l => new ReportLine { AccountCode = l.AccountCode ?? "", AccountName = l.AccountName ?? "", Debit = l.Debit, Credit = l.Credit, Balance = l.Balance }).ToList(), Total = isResult.Expenses.Total });
                reportResult = new ReportResult { Currency = isResult.Currency, GeneratedAt = isResult.GeneratedAt, Sections = isSections, PaperSize = pageSize, IsLandscape = isLandscape ?? true };
                break;
            case "general-ledger":
                reportName = "General Ledger";
                var glResult = await sender.Send(new GetGeneralLedgerQuery { AccountId = accountId, AccountCode = accountCode, StartDate = startDate?.ToString("yyyy-MM-dd"), EndDate = endDate?.ToString("yyyy-MM-dd") });
                var glLines = glResult.Lines?.Select(l => new ReportLine { AccountCode = l.AccountCode ?? "", AccountName = l.AccountName ?? "", Debit = l.Debit, Credit = l.Credit, Balance = l.RunningBalance }).ToList() ?? [];
                reportResult = new ReportResult { Currency = glResult.Currency, GeneratedAt = glResult.GeneratedAt, TotalLines = glResult.TotalLines, Sections = [new ReportSection { Title = "دفتر الأستاذ العام", TitleEn = "General Ledger", Lines = glLines, Total = 0 }], PaperSize = pageSize, IsLandscape = isLandscape ?? true };
                break;
            case "cash-flow":
                reportName = "Cash Flow Statement";
                var cfResult = await sender.Send(new GetCashFlowStatementQuery { StartDate = (startDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30))).ToString("yyyy-MM-dd"), EndDate = (endDate ?? DateOnly.FromDateTime(DateTime.Today)).ToString("yyyy-MM-dd") });
                var cfSections = new List<ReportSection>();
                if (cfResult.Operating?.Items?.Count > 0)
                    cfSections.Add(new ReportSection { Title = "التدفقات التشغيلية", TitleEn = "Operating", Lines = cfResult.Operating.Items.Select(i => new ReportLine { AccountCode = "", AccountName = i.Description ?? "", Balance = i.Amount }).ToList(), Total = cfResult.Operating.Total });
                if (cfResult.Investing?.Items?.Count > 0)
                    cfSections.Add(new ReportSection { Title = "التدفقات الاستثمارية", TitleEn = "Investing", Lines = cfResult.Investing.Items.Select(i => new ReportLine { AccountCode = "", AccountName = i.Description ?? "", Balance = i.Amount }).ToList(), Total = cfResult.Investing.Total });
                if (cfResult.Financing?.Items?.Count > 0)
                    cfSections.Add(new ReportSection { Title = "التدفقات التمويلية", TitleEn = "Financing", Lines = cfResult.Financing.Items.Select(i => new ReportLine { AccountCode = "", AccountName = i.Description ?? "", Balance = i.Amount }).ToList(), Total = cfResult.Financing.Total });
                reportResult = new ReportResult { Currency = cfResult.Currency, GeneratedAt = cfResult.GeneratedAt, Sections = cfSections, PaperSize = pageSize, IsLandscape = isLandscape ?? true };
                break;
            case "trial-balance":
                reportName = "Trial Balance";
                var tbResult = await sender.Send(new GetTrialBalanceQuery { FiscalYearId = fiscalYearId ?? 0, FiscalPeriodId = fiscalPeriodId ?? 0 });
                reportResult = new ReportResult { Currency = tbResult.Currency, GeneratedAt = tbResult.GeneratedAt, Sections = tbResult.Sections, PaperSize = pageSize, IsLandscape = isLandscape ?? true };
                break;
            default:
                return Results.BadRequest($"Unknown report type: {reportType}");
        }

        var stream = new MemoryStream();
        if (format?.ToLower() == "pdf")
        {
            await pdfExporter.ExportPdfAsync(reportResult, reportName, stream);
        }
        else
        {
            await excelExporter.ExportExcelAsync(reportResult, reportName, stream);
        }
        stream.Position = 0;

        var contentType = format?.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var extension = format?.ToLower() == "pdf" ? "pdf" : "xlsx";
        return Results.File(stream, contentType, $"{reportName}-{DateTime.Now:yyyyMMdd}.{extension}");
    }

    private static void AddBalanceSheetSection(
        List<ReportSection> sections,
        string title,
        string titleEn,
        BalanceSheetGroup group)
    {
        foreach (var section in group.Sections.Where(section => section.Lines.Count > 0 || section.Total != 0))
        {
            sections.Add(new ReportSection
            {
                Title = $"{title} - {section.Title}",
                TitleEn = $"{titleEn} - {section.TitleEn}",
                Lines = section.Lines
                    .Select(line => new ReportLine
                    {
                        AccountCode = line.AccountCode ?? "",
                        AccountName = line.AccountName ?? "",
                        Debit = line.Debit,
                        Credit = line.Credit,
                        Balance = line.Balance
                    })
                    .ToList(),
                Total = section.Total
            });
        }
    }

}
