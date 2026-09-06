using ERP_Government.Application.Common.Security;
using MediatR;

namespace ERP_Government.Application.Reporting.RevenueCollections.GetRevenueCollectionsDetail;

[Authorize(Policy = PermissionCodes.ReportingViewRevenueCollections)]
public record GetRevenueCollectionsDetailQuery : IRequest<RevenueCollectionsDetailDto>
{
    public int ReceiptVoucherId { get; init; }
}
