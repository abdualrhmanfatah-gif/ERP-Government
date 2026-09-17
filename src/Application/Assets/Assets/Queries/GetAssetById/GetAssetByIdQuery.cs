using ERP_Government.Application.Assets.Assets.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.Assets.Queries.GetAssetById;

[Authorize(Policy = PermissionCodes.AssetsView)]
public record GetAssetByIdQuery(int Id) : IRequest<Result<AssetDetailResponse>>;

public class GetAssetByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetByIdQuery, Result<AssetDetailResponse>>
{
    public async Task<Result<AssetDetailResponse>> Handle(
        GetAssetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await context.Assets
            .Include(a => a.AssetGroup)
            .Include(a => a.Location)
            .Include(a => a.Employee!)
            .ThenInclude(e => e.OrganizationalUnit)
            .Include(a => a.Currency)
            .Include(a => a.ExchangeRate)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result<AssetDetailResponse>.Failure(
                ErrorCodes.Assets.AssetNotFound,
                ErrorCategory.NotFound,
                $"الأصل بالمعرف {request.Id} غير موجود");

        var attributeValues = await context.AssetAttributeValues
            .Include(v => v.AssetAttributeDefinition)
            .Where(v => v.AssetId == request.Id)
            .OrderBy(v => v.AssetAttributeDefinitionId)
            .ToListAsync(cancellationToken);

        var auditUserIds = new[] { entity.CreatedBy, entity.LastModifiedBy }
            .Select(value => int.TryParse(value, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var auditUserNames = auditUserIds.Count == 0
            ? []
            : await context.Users
                .Where(u => auditUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Login, cancellationToken);

        var response = entity.ToDetailResponse(attributeValues);

        if (entity.CreatedBy is not null
            && int.TryParse(entity.CreatedBy, out var createdById)
            && auditUserNames.TryGetValue(createdById, out var createdByName))
        {
            response = response with { CreatedBy = createdByName };
        }

        if (entity.LastModifiedBy is not null
            && int.TryParse(entity.LastModifiedBy, out var modifiedById)
            && auditUserNames.TryGetValue(modifiedById, out var modifiedByName))
        {
            response = response with { LastModifiedBy = modifiedByName };
        }

        return Result<AssetDetailResponse>.Success(response);
    }
}
