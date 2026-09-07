using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.TrialBalance.GetLedgerMovement;

[Authorize(Policy = PermissionCodes.ReportingViewTrialBalanceReport)]
public record GetLedgerMovementQuery : IRequest<LedgerMovementDto>
{
    public int AccountId { get; init; }
    public int FiscalYearId { get; init; }
}
