using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Reporting.TrialBalance.GetLedgerMovement;
using ERP_Government.Application.Reporting.TrialBalance.GetTrialBalanceReport;
using ERP_Government.Application.Reporting.Common;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Reporting;

public class TrialBalanceReports : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetTrialBalanceReport)
            .RequireAuthorization(PermissionCodes.ReportingViewTrialBalanceReport)
            .Produces<TrialBalanceReportDto>();

        group.MapGet("/{accountId:int}/ledger-movement", GetLedgerMovement)
            .RequireAuthorization(PermissionCodes.ReportingViewTrialBalanceReport)
            .Produces<LedgerMovementDto>();

        group.MapGet("/export", ExportTrialBalanceReport)
            .RequireAuthorization(PermissionCodes.ReportingExportReports);
    }

    [EndpointSummary("Get trial balance report")]
    public static async Task<TrialBalanceReportDto> GetTrialBalanceReport(
        [FromServices] ISender sender,
        [AsParameters] GetTrialBalanceReportQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get ledger movement for an account")]
    public static async Task<LedgerMovementDto> GetLedgerMovement(
        [FromServices] ISender sender,
        int accountId,
        [AsParameters] GetLedgerMovementQuery query)
    {
        return await sender.Send(query with { AccountId = accountId });
    }

    [EndpointSummary("Export trial balance report to Excel or PDF")]
    public static async Task<IResult> ExportTrialBalanceReport(
        [FromServices] ISender sender,
        [FromQuery] string format,
        [AsParameters] GetTrialBalanceReportQuery query)
    {
        var result = await sender.Send(query);
        var stream = new MemoryStream();
        var exporter = format?.ToLower() == "pdf"
            ? (ERP_Government.Application.Accounting.Reports.Common.IReportExporter)new ERP_Government.Infrastructure.Services.PdfReportExporter()
            : new ERP_Government.Infrastructure.Services.ExcelReportExporter();

        var reportResult = result.ToReportResult();

        await exporter.ExportExcelAsync(reportResult, "Trial Balance", stream);
        stream.Position = 0;

        var extension = format?.ToLower() == "pdf" ? "pdf" : "xlsx";
        var contentType = format?.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return Results.File(stream, contentType, $"TrialBalance-{DateTime.Now:yyyyMMdd}.{extension}");
    }
}
