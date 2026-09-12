using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionDetail;
using ERP_Government.Application.Reporting.BudgetExecution.GetBudgetExecutionReport;
using ERP_Government.Application.Reporting.Common;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Web.Endpoints.Reporting;

public class BudgetExecutionReports : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetBudgetExecutionReport)
            .RequireAuthorization(PermissionCodes.ReportingViewBudgetExecution)
            .Produces<BudgetExecutionReportDto>();

        group.MapGet("/{budgetItemId:int}/detail", GetBudgetExecutionDetail)
            .RequireAuthorization(PermissionCodes.ReportingViewBudgetExecution)
            .Produces<BudgetExecutionDetailDto>();

        group.MapGet("/export", ExportBudgetExecutionReport)
            .RequireAuthorization(PermissionCodes.ReportingExportReports);
    }

    [EndpointSummary("Get budget execution report")]
    public static async Task<BudgetExecutionReportDto> GetBudgetExecutionReport(
        [FromServices] ISender sender,
        [AsParameters] GetBudgetExecutionReportQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get budget execution detail for a budget line")]
    public static async Task<BudgetExecutionDetailDto> GetBudgetExecutionDetail(
        ISender sender,
        int budgetItemId,
        [AsParameters] GetBudgetExecutionDetailQuery query)
    {
        return await sender.Send(query with { BudgetItemId = budgetItemId });
    }

    [EndpointSummary("Export budget execution report to Excel or PDF")]
    public static async Task<IResult> ExportBudgetExecutionReport(
        ISender sender,
        [FromServices] IApplicationDbContext context,
        [FromQuery] string format,
        [AsParameters] GetBudgetExecutionReportQuery query)
    {
        var result = await sender.Send(query);
        var currencyCode = await context.Currencies
            .Where(c => c.IsBase)
            .Select(c => c.Code)
            .FirstOrDefaultAsync();

        // RC-4: exporting data that includes the currently open fiscal period is partial data.
        var isPartialData = await context.FiscalYears
            .AnyAsync(fy => fy.Id == query.FiscalYearId
                         && fy.Status == Domain.FinancialSettings.Enums.FiscalYearStatus.Open);

        var stream = new MemoryStream();
        var isPdf = format?.ToLower() == "pdf";
        var exporter = isPdf
            ? (ERP_Government.Application.Accounting.Reports.Common.IReportExporter)new ERP_Government.Infrastructure.Services.PdfReportExporter()
            : new ERP_Government.Infrastructure.Services.ExcelReportExporter();

        var reportResult = result.ToReportResult(currencyCode, isPartialData);

        if (isPdf)
        {
            await exporter.ExportPdfAsync(reportResult, "تقرير تنفيذ الموازنة", stream);
        }
        else
        {
            await exporter.ExportExcelAsync(reportResult, "تقرير تنفيذ الموازنة", stream);
        }
        stream.Position = 0;

        var extension = isPdf ? "pdf" : "xlsx";
        var contentType = isPdf
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return Results.File(stream, contentType, $"BudgetExecution-{DateTime.Now:yyyyMMdd}.{extension}");
    }
}
