using ERP_Government.Application.Assets.AssetGroups.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroupDetail;

[Authorize(Policy = PermissionCodes.AssetGroupsView)]
public record GetAssetGroupDetailQuery(int Id) : IRequest<Result<AssetGroupDetailResponse>>;

public class GetAssetGroupDetailQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetGroupDetailQuery, Result<AssetGroupDetailResponse>>
{
    public async Task<Result<AssetGroupDetailResponse>> Handle(
        GetAssetGroupDetailQuery request,
        CancellationToken cancellationToken)
    {
        var group = await context.AssetGroups
            .Include(g => g.ParentAssetGroup)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (group is null)
            return Result<AssetGroupDetailResponse>.Failure(ErrorCodes.Assets.AssetGroupNotFound, ErrorCategory.NotFound, "المجموعة غير موجودة");

        var bindings = await context.AssetGroupAttributes
            .Where(b => b.AssetGroupId == request.Id)
            .Select(b => new AssetGroupAttributeBindingResponse(
                b.AssetAttributeDefinitionId,
                b.AssetAttributeDefinition!.Code,
                b.AssetAttributeDefinition.Name,
                b.AssetAttributeDefinition.AttributeDataType,
                b.IsRequired,
                b.SortOrder))
            .ToListAsync(cancellationToken);

        return Result<AssetGroupDetailResponse>.Success(group.ToDetailResponse(bindings));
    }
}
