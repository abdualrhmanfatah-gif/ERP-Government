using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Domain.Assets.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP_Government.Application.Common.Security;

namespace ERP_Government.Application.Assets.AssetAttributes.Definitions.Queries;

[Authorize(Policy = PermissionCodes.AssetGroupsView)]
public record GetAssetAttributeDefinitionsQuery(
    string? Search = null,
    AssetAttributeDataType? DataType = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedList<AssetAttributeDefinitionDto>>>;

public record AssetAttributeDefinitionDto(
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

public class GetAssetAttributeDefinitionsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAssetAttributeDefinitionsQuery, Result<PaginatedList<AssetAttributeDefinitionDto>>>
{
    public async Task<Result<PaginatedList<AssetAttributeDefinitionDto>>> Handle(
        GetAssetAttributeDefinitionsQuery request, CancellationToken ct)
    {
        var query = context.AssetAttributeDefinitions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(d => d.Name.Contains(search) || d.Code.Contains(search));
        }

        if (request.DataType.HasValue)
            query = query.Where(d => d.AttributeDataType == request.DataType.Value);

        if (request.IsActive.HasValue)
            query = query.Where(d => d.IsActive == request.IsActive.Value);
        else
            query = query.Where(d => d.IsActive);

        query = query.OrderBy(d => d.SortOrder).ThenBy(d => d.Name);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new AssetAttributeDefinitionDto(
                d.Id, d.Code, d.Name, d.Description, d.Unit, d.SortOrder,
                d.AttributeDataType, d.IsActive,
                context.AssetAttributeValues.Count(v => v.AssetAttributeDefinitionId == d.Id),
                d.RowVersion))
            .ToListAsync(ct);

        return Result<PaginatedList<AssetAttributeDefinitionDto>>.Success(
            new PaginatedList<AssetAttributeDefinitionDto>(items, totalCount, request.Page, request.PageSize));
    }
}
