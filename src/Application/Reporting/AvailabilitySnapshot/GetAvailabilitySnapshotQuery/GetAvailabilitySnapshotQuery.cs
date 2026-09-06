using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.AvailabilitySnapshot.GetAvailabilitySnapshotQuery;

[Authorize(Policy = PermissionCodes.ReportingViewAvailabilitySnapshot)]
public record GetAvailabilitySnapshotQuery : IRequest<AvailabilitySnapshotDto>
{
    public int FiscalYearId { get; init; }
    public int? BudgetItemId { get; init; }
    public int? FundId { get; init; }
}
