using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsReport;

[Authorize(Policy = PermissionCodes.ReportingViewRevenueCollections)]
public record GetRevenueCollectionsReportQuery : IRequest<RevenueCollectionsReportDto>
{
    public int FiscalYearId { get; init; }
    public int? FiscalPeriodId { get; init; }
    public int? FundId { get; init; }
    public int? RevenueAccountId { get; init; }
    public int? PartyId { get; init; }
    public string? PaymentMethod { get; init; }
}
