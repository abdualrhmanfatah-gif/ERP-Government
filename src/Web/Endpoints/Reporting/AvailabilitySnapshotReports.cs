using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotDetail;
using ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotQuery;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Reporting;

public class AvailabilitySnapshotReports : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetAvailabilitySnapshot)
            .RequireAuthorization(PermissionCodes.ReportingViewAvailabilitySnapshot)
            .Produces<AvailabilitySnapshotDto>();

        group.MapGet("/{budgetItemId:int}/detail", GetAvailabilitySnapshotDetail)
            .RequireAuthorization(PermissionCodes.ReportingViewAvailabilitySnapshot)
            .Produces<AvailabilitySnapshotDetailDto>();

        group.MapGet("/export", ExportAvailabilitySnapshot)
            .RequireAuthorization(PermissionCodes.ReportingExportReports);
    }

    [EndpointSummary("Get budget availability snapshot")]
    public static async Task<AvailabilitySnapshotDto> GetAvailabilitySnapshot(
        [FromServices] ISender sender,
        [AsParameters] GetAvailabilitySnapshotQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get availability snapshot detail for a budget line")]
    public static async Task<AvailabilitySnapshotDetailDto> GetAvailabilitySnapshotDetail(
        ISender sender,
        int budgetItemId,
        [AsParameters] GetAvailabilitySnapshotDetailQuery query)
    {
        return await sender.Send(query with { BudgetItemId = budgetItemId });
    }

    [EndpointSummary("Export availability snapshot report to Excel or PDF")]
    public static async Task<IResult> ExportAvailabilitySnapshot(
        ISender sender,
        [FromQuery] string format,
        [AsParameters] GetAvailabilitySnapshotQuery query)
    {
        var result = await sender.Send(query);
        var stream = new MemoryStream();
        var exporter = format?.ToLower() == "pdf"
            ? (ERP_Government.Application.Accounting.Reports.Common.IReportExporter)new ERP_Government.Infrastructure.Services.PdfReportExporter()
            : new ERP_Government.Infrastructure.Services.ExcelReportExporter();

        var reportResult = new ERP_Government.Application.Accounting.Reports.Common.ReportResult
        {
            Currency = "SAR",
            GeneratedAt = DateTimeOffset.UtcNow,
            Sections =
            [
                new ERP_Government.Application.Accounting.Reports.Common.ReportSection
                {
                    Title = "Availability Snapshot",
                    Lines = result.Breakdown.Select(l => new ERP_Government.Application.Accounting.Reports.Common.ReportLine
                    {
                        AccountCode = result.ItemCode,
                        AccountName = $"{result.ItemName} - {l.FundCode}",
                        Debit = l.PaidAmount,
                        Credit = 0,
                        Balance = l.AvailableAmount
                    }).ToList(),
                    Total = result.Totals.AvailableAmount
                }
            ]
        };

        await exporter.ExportExcelAsync(reportResult, "Availability Snapshot", stream);
        stream.Position = 0;

        var extension = format?.ToLower() == "pdf" ? "pdf" : "xlsx";
        var contentType = format?.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return Results.File(stream, contentType, $"AvailabilitySnapshot-{DateTime.Now:yyyyMMdd}.{extension}");
    }
}
