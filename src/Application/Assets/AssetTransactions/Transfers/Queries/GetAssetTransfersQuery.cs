using ERP_Government.Application.Assets.AssetTransactions.Transfers.Common;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetTransactions.Transfers.Queries;

[Authorize(Policy = PermissionCodes.AssetTransfersView)]
public record GetAssetTransfersQuery(
    string? Search = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedList<AssetTransferListItemResponse>>;

public class GetAssetTransfersQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetTransfersQuery, PaginatedList<AssetTransferListItemResponse>>
{
    public async Task<PaginatedList<AssetTransferListItemResponse>> Handle(
        GetAssetTransfersQuery request,
        CancellationToken ct)
    {
        var query =
            from t in context.AssetTransactions
            where t.TransactionType == AssetTransactionType.Transfer
            join a in context.Assets on t.AssetId equals a.Id
            join d in context.AssetTransferDetails on t.Id equals d.AssetTransactionId
            join fl in context.Locations on d.FromLocationId equals (int?)fl.Id into fromLocations
            from fl in fromLocations.DefaultIfEmpty()
            join tl in context.Locations on d.ToLocationId equals (int?)tl.Id into toLocations
            from tl in toLocations.DefaultIfEmpty()
            join fe in context.Employees on d.FromEmployeeId equals (int?)fe.Id into fromEmployees
            from fe in fromEmployees.DefaultIfEmpty()
            join te in context.Employees on d.ToEmployeeId equals (int?)te.Id into toEmployees
            from te in toEmployees.DefaultIfEmpty()
            select new
            {
                t,
                AssetCode = a.Code,
                AssetName = a.Name,
                FromLocationName = fl != null ? fl.Name : null,
                ToLocationName = tl != null ? tl.Name : null,
                FromEmployeeName = fe != null ? fe.Name : null,
                ToEmployeeName = te != null ? te.Name : null
            };

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            !string.Equals(request.Status, "All", StringComparison.OrdinalIgnoreCase) &&
            Enum.TryParse<AssetTransactionStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(row => row.t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(row =>
                row.t.TransactionNumber.ToLower().Contains(search) ||
                row.AssetCode.ToLower().Contains(search) ||
                row.AssetName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(row => row.t.TransactionDate)
            .ThenByDescending(row => row.t.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(row => new AssetTransferListItemResponse(
                row.t.Id,
                row.t.TransactionNumber,
                row.t.AssetId,
                row.AssetCode,
                row.AssetName,
                row.t.TransactionDate,
                row.t.Status.ToString(),
                row.FromLocationName,
                row.ToLocationName,
                row.FromEmployeeName,
                row.ToEmployeeName))
            .ToListAsync(ct);

        return new PaginatedList<AssetTransferListItemResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);
    }
}
