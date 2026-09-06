using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterDetail;
using ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;
using ERP_Government.Application.Reporting.Common;
using ERP_Government.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP_Government.Web.Endpoints.Reporting;

public class DisbursementRegisterReports : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetDisbursementRegister)
            .RequireAuthorization(PermissionCodes.ReportingViewDisbursementRegister)
            .Produces<DisbursementRegisterDto>();

        group.MapGet("/{paymentOrderId:int}/detail", GetDisbursementRegisterDetail)
            .RequireAuthorization(PermissionCodes.ReportingViewDisbursementRegister)
            .Produces<DisbursementRegisterDetailDto>();

        group.MapGet("/export", ExportDisbursementRegister)
            .RequireAuthorization(PermissionCodes.ReportingExportReports);
    }

    [EndpointSummary("Get disbursement register report")]
    public static async Task<DisbursementRegisterDto> GetDisbursementRegister(
        [FromServices] ISender sender,
        [AsParameters] GetDisbursementRegisterQuery query)
    {
        return await sender.Send(query);
    }

    [EndpointSummary("Get disbursement register detail for a payment order")]
    public static async Task<DisbursementRegisterDetailDto> GetDisbursementRegisterDetail(
        ISender sender,
        int paymentOrderId,
        [AsParameters] GetDisbursementRegisterDetailQuery query)
    {
        return await sender.Send(query with { PaymentOrderId = paymentOrderId });
    }

    [EndpointSummary("Export disbursement register report to Excel or PDF")]
    public static async Task<IResult> ExportDisbursementRegister(
        ISender sender,
        [FromQuery] string format,
        [AsParameters] GetDisbursementRegisterQuery query)
    {
        var result = await sender.Send(query);
        var stream = new MemoryStream();
        var exporter = format?.ToLower() == "pdf"
            ? (ERP_Government.Application.Accounting.Reports.Common.IReportExporter)new ERP_Government.Infrastructure.Services.PdfReportExporter()
            : new ERP_Government.Infrastructure.Services.ExcelReportExporter();

        var reportResult = result.ToReportResult();

        await exporter.ExportExcelAsync(reportResult, "Disbursement Register", stream);
        stream.Position = 0;

        var extension = format?.ToLower() == "pdf" ? "pdf" : "xlsx";
        var contentType = format?.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return Results.File(stream, contentType, $"DisbursementRegister-{DateTime.Now:yyyyMMdd}.{extension}");
    }
}
