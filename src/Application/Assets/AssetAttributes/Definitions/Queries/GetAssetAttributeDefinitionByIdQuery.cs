using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetAttributes.Definitions.Queries;

[Authorize(Policy = PermissionCodes.AssetGroupsView)]
public record GetAssetAttributeDefinitionByIdQuery(int Id) : IRequest<Result<AssetAttributeDefinitionDetailDto>>;

public record AssetAttributeDefinitionDetailDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    string? Unit,
    int SortOrder,
    AssetAttributeDataType AttributeDataType,
    bool IsActive,
    int LinkedValueCount,
    byte[] RowVersion);

public class GetAssetAttributeDefinitionByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetAttributeDefinitionByIdQuery, Result<AssetAttributeDefinitionDetailDto>>
{
    public async Task<Result<AssetAttributeDefinitionDetailDto>> Handle(
        GetAssetAttributeDefinitionByIdQuery request, CancellationToken ct)
    {
        var entity = await context.AssetAttributeDefinitions.FindAsync([request.Id], ct);
        if (entity is null)
            return Result<AssetAttributeDefinitionDetailDto>.Failure(
                ErrorCodes.Assets.AttributeDefinitionNotFound, ErrorCategory.NotFound, "المواصفة غير موجودة");

        var linkedValueCount = await context.AssetAttributeValues
            .CountAsync(v => v.AssetAttributeDefinitionId == request.Id, ct);

        var dto = new AssetAttributeDefinitionDetailDto(
            entity.Id, entity.Code, entity.Name, entity.Description, entity.Unit,
            entity.SortOrder, entity.AttributeDataType, entity.IsActive,
            linkedValueCount, entity.RowVersion);

        return Result<AssetAttributeDefinitionDetailDto>.Success(dto);
    }
}
