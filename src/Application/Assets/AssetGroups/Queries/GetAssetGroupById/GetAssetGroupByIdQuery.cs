using ERP_Government.Application.Assets.AssetGroups.Common;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroupById;

[Authorize(Policy = PermissionCodes.AssetGroupsView)]
public record GetAssetGroupByIdQuery(int Id) : IRequest<Result<AssetGroupResponse>>;

public class GetAssetGroupByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetGroupByIdQuery, Result<AssetGroupResponse>>
{
    public async Task<Result<AssetGroupResponse>> Handle(
        GetAssetGroupByIdQuery request,
        CancellationToken cancellationToken)
    {
        var group = await context.AssetGroups
            .Include(g => g.ParentAssetGroup)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (group is null)
            return Result<AssetGroupResponse>.Failure(ErrorCodes.Assets.AssetGroupNotFound, ErrorCategory.NotFound, "المجموعة غير موجودة");

        var hasChildren = await context.AssetGroups.AnyAsync(g => g.ParentAssetGroupId == group.Id, cancellationToken);

        return Result<AssetGroupResponse>.Success(group.ToResponse(hasChildren));
    }
}
