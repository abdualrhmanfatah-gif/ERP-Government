using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.PhysicalCounts.Queries;

public record AssetCountLineResponse(
    int Id,
    int AssetId,
    string AssetCode,
    string AssetName,
    int? SystemLocationId,
    string? SystemLocation,
    int? PhysicalLocationId,
    string? PhysicalLocation,
    int? SystemEmployeeId,
    string? SystemCustodian,
    int? PhysicalEmployeeId,
    string? PhysicalCustodian,
    string? SystemStatus,
    string? PhysicalStatus,
    int IsFound,
    bool? IsMatch,
    string? DiscrepancyNotes,
    byte[] RowVersion);

public record AssetCountDetailResponse(
    int Id,
    string DocumentNumber,
    DateOnly CountDate,
    string CountType,
    string Status,
    string ResolvedScopeLabel,
    int? LocationId,
    int? DepartmentId,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int? CountedById,
    int? ReviewedById,
    string? Notes,
    byte[] RowVersion,
    List<AssetCountLineResponse> Lines);

[Authorize(Policy = PermissionCodes.AssetCountsView)]
public record GetAssetCountByIdQuery(int Id) : IRequest<Result<AssetCountDetailResponse>>;

public class GetAssetCountByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetCountByIdQuery, Result<AssetCountDetailResponse>>
{
    public async Task<Result<AssetCountDetailResponse>> Handle(
        GetAssetCountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var count = await context.AssetPhysicalCounts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (count is null)
            return Result<AssetCountDetailResponse>.Failure(
                ErrorCodes.Assets.CountNotFound,
                ErrorCategory.NotFound,
                "وثيقة الجرد غير موجودة");

        var lines = await context.AssetPhysicalCountDetails
            .Where(l => l.AssetPhysicalCountId == request.Id)
            .OrderBy(l => l.AssetId)
            .ToListAsync(cancellationToken);

        var assetIds = lines.Select(l => l.AssetId).Distinct().ToList();
        var assets = await context.Assets
            .Where(a => assetIds.Contains(a.Id))
            .Select(a => new { a.Id, a.Code, a.Name })
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var locationIds = lines
            .SelectMany(l => new[] { l.SystemLocationId, l.PhysicalLocationId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var locations = await context.Locations
            .Where(l => locationIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Name, cancellationToken);

        var employeeIds = lines
            .SelectMany(l => new[] { l.SystemEmployeeId, l.PhysicalEmployeeId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var employees = await context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

        var lineResponses = lines.Select(l =>
        {
            assets.TryGetValue(l.AssetId, out var asset);
            return new AssetCountLineResponse(
                l.Id,
                l.AssetId,
                asset?.Code ?? string.Empty,
                asset?.Name ?? string.Empty,
                l.SystemLocationId,
                l.SystemLocationId.HasValue && locations.TryGetValue(l.SystemLocationId.Value, out var sl) ? sl : null,
                l.PhysicalLocationId,
                l.PhysicalLocationId.HasValue && locations.TryGetValue(l.PhysicalLocationId.Value, out var pl) ? pl : null,
                l.SystemEmployeeId,
                l.SystemEmployeeId.HasValue && employees.TryGetValue(l.SystemEmployeeId.Value, out var se) ? se : null,
                l.PhysicalEmployeeId,
                l.PhysicalEmployeeId.HasValue && employees.TryGetValue(l.PhysicalEmployeeId.Value, out var pe) ? pe : null,
                l.SystemStatus,
                l.PhysicalStatus,
                (int)l.IsFound,
                l.IsMatch,
                l.DiscrepancyNotes,
                l.RowVersion);
        }).ToList();

        return Result<AssetCountDetailResponse>.Success(new AssetCountDetailResponse(
            count.Id,
            count.CountNumber,
            count.CountDate,
            count.CountType,
            count.Status.ToString(),
            count.ResolvedScopeLabel,
            count.LocationId,
            count.DepartmentId,
            count.StartedAt,
            count.CompletedAt,
            count.CountedById,
            count.ReviewedById,
            count.Notes,
            count.RowVersion,
            lineResponses));
    }
}
