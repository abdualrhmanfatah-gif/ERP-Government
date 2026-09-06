using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.DisbursementRegister.GetDisbursementRegisterQuery;

[Authorize(Policy = PermissionCodes.ReportingViewDisbursementRegister)]
public record GetDisbursementRegisterQuery : IRequest<DisbursementRegisterDto>
{
    public int FiscalYearId { get; init; }
    public int? FiscalPeriodId { get; init; }
    public int? FundId { get; init; }
    public string? Status { get; init; }
    public int? ApproverId { get; init; }
}
