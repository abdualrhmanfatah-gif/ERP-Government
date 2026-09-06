using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterDetail;

[Authorize(Policy = PermissionCodes.ReportingViewDisbursementRegister)]
public record GetDisbursementRegisterDetailQuery : IRequest<DisbursementRegisterDetailDto>
{
    public int PaymentOrderId { get; init; }
}
