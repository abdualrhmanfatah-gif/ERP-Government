using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterDetail;
using ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;
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
}
