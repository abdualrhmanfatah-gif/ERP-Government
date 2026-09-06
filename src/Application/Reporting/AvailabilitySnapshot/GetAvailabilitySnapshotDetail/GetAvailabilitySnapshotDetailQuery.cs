using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotDetail;

[Authorize(Policy = PermissionCodes.ReportingViewAvailabilitySnapshot)]
public record GetAvailabilitySnapshotDetailQuery : IRequest<AvailabilitySnapshotDetailDto>
{
    public int BudgetItemId { get; init; }
    public int FiscalYearId { get; init; }
    public int? FundId { get; init; }
}
