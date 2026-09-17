using ERP_Government.Application.Assets.Assets.Common;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Assets.Queries.GetAssets;

[Authorize(Policy = PermissionCodes.AssetsView)]
public record GetAssetsQuery(
    string? Search = null,
    string? Status = "Active",
    int? AssetGroupId = null,
    int? LocationId = null,
    int? EmployeeId = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PaginatedList<AssetResponse>>;

public class GetAssetsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetsQuery, PaginatedList<AssetResponse>>
{
    public async Task<PaginatedList<AssetResponse>> Handle(
        GetAssetsQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Asset> query = context.Assets
            .Include(a => a.AssetGroup)
            .Include(a => a.Location)
            .Include(a => a.Employee)
            .Include(a => a.Currency)
            .Include(a => a.ExchangeRate);

        if (!string.Equals(request.Status, "All", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        if (request.AssetGroupId.HasValue)
            query = query.Where(a => a.AssetGroupId == request.AssetGroupId.Value);

        if (request.LocationId.HasValue)
            query = query.Where(a => a.LocationId == request.LocationId.Value);

        if (request.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == request.EmployeeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.Code.ToLower().Contains(search) ||
                a.Name.ToLower().Contains(search) ||
                (a.AssetTag != null && a.AssetTag.ToLower().Contains(search)) ||
                (a.Barcode != null && a.Barcode.ToLower().Contains(search)) ||
                (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.Code)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<AssetResponse>(
            items.Select(a => a.ToResponse()).ToList(),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
