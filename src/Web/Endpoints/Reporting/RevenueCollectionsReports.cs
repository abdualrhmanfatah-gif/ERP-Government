using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsDetail;
using ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsReport;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Reporting;

public class RevenueCollectionsReports : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetRevenueCollectionsReport)
            .RequireAuthorization(PermissionCodes.ReportingViewRevenueCollections)
            .Produces<RevenueCollectionsReportDto>();

        group.MapGet("/{receiptVoucherId:int}/detail", GetRevenueCollectionsDetail)
            .RequireAuthorization(PermissionCodes.ReportingViewRevenueCollections)
            .Produces<RevenueCollectionsDetailDto>();

        group.MapGet("/export", ExportRevenueCollectionsReport)
            .RequireAuthorization(PermissionCodes.ReportingExportReports);
    }

    [EndpointSummary("Get revenue collections report")]
    public static async Task<RevenueCollectionsReportDto> GetRevenueCollectionsReport(
        [FromServices] ISender sender,
        [AsParameters] GetRevenueCollectionsReportQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get revenue collections detail for a receipt voucher")]
    public static async Task<RevenueCollectionsDetailDto> GetRevenueCollectionsDetail(
        ISender sender,
        int receiptVoucherId,
        [AsParameters] GetRevenueCollectionsDetailQuery query)
    {
        return await sender.Send(query with { ReceiptVoucherId = receiptVoucherId });
    }

    [EndpointSummary("Export revenue collections report to Excel or PDF")]
    public static async Task<IResult> ExportRevenueCollectionsReport(
        ISender sender,
        [FromQuery] string format,
        [AsParameters] GetRevenueCollectionsReportQuery query)
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
                    Title = "Revenue Collections",
                    Lines = result.Lines.Select(l => new ERP_Government.Application.Accounting.Reports.Common.ReportLine
                    {
                        AccountCode = l.AccountCode,
                        AccountName = $"{l.AccountName} - {l.PartyName}",
                        Debit = l.Amount,
                        Credit = 0,
                        Balance = l.Amount
                    }).ToList(),
                    Total = result.Totals.TotalAmount
                }
            ]
        };

        await exporter.ExportExcelAsync(reportResult, "Revenue Collections", stream);
        stream.Position = 0;

        var extension = format?.ToLower() == "pdf" ? "pdf" : "xlsx";
        var contentType = format?.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return Results.File(stream, contentType, $"RevenueCollections-{DateTime.Now:yyyyMMdd}.{extension}");
    }
}
