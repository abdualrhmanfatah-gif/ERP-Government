using ERP_Government.Application.Assets.AssetGroups.Common;
using ERP_Government.Application.Assets.AssetGroups.Commands.CreateAssetGroup;
using ERP_Government.Application.Assets.AssetGroups.Commands.ToggleAssetGroupActive;
using ERP_Government.Application.Assets.AssetGroups.Commands.UpdateAssetGroup;
using ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroupById;
using ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroupDetail;
using ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroups;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetGroups : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.AssetGroupsView)
            .Produces<List<AssetGroupResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.AssetGroupsView)
            .Produces<AssetGroupResponse?>();
        group.MapGet("/{id:int}/detail", HandleGetDetail)
            .RequireAuthorization(PermissionCodes.AssetGroupsView)
            .Produces<AssetGroupDetailResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.AssetGroupsCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.AssetGroupsUpdate)
            .Produces<int>();
        group.MapPost("/{id:int}/deactivate", HandleDeactivate)
            .RequireAuthorization(PermissionCodes.AssetGroupsDeactivate)
            .Produces<int>();
        group.MapPost("/{id:int}/activate", HandleActivate)
            .RequireAuthorization(PermissionCodes.AssetGroupsActivate)
            .Produces<int>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        bool? isActive = null,
        int? parentId = null)
    {
        var result = await sender.Send(new GetAssetGroupsQuery(search, isActive, parentId));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetAssetGroupByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetDetail(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetAssetGroupDetailQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateAssetGroupRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/AssetGroups/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateAssetGroupRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleDeactivate(
        ISender sender,
        int id,
        ToggleActiveRequest request)
    {
        var result = await sender.Send(new ToggleAssetGroupActiveCommand(id, false, request.RowVersion));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleActivate(
        ISender sender,
        int id,
        ToggleActiveRequest request)
    {
        var result = await sender.Send(new ToggleAssetGroupActiveCommand(id, true, request.RowVersion));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}

public record CreateAssetGroupRequest(
    string Code,
    string Name,
    string? Description,
    int? ParentAssetGroupId,
    string AssetCategory,
    bool IsDepreciable,
    string DepreciationMethod,
    decimal? DepreciationRate,
    int? DefaultUsefulLifeYears,
    decimal? ResidualValuePercentage,
    int? AssetAccountId,
    int? AccumulatedDepreciationAccountId,
    int? DepreciationExpenseAccountId,
    int? DisposalAccountId)
{
    public CreateAssetGroupCommand ToCommand() => new(
        Code, Name, Description, ParentAssetGroupId, AssetCategory, IsDepreciable,
        DepreciationMethod, DepreciationRate, DefaultUsefulLifeYears, ResidualValuePercentage,
        AssetAccountId, AccumulatedDepreciationAccountId, DepreciationExpenseAccountId, DisposalAccountId);
}

public record UpdateAssetGroupRequest(
    string Name,
    string? Description,
    int? ParentAssetGroupId,
    string AssetCategory,
    bool IsDepreciable,
    string DepreciationMethod,
    decimal? DepreciationRate,
    int? DefaultUsefulLifeYears,
    decimal? ResidualValuePercentage,
    int? AssetAccountId,
    int? AccumulatedDepreciationAccountId,
    int? DepreciationExpenseAccountId,
    int? DisposalAccountId,
    byte[] RowVersion)
{
    public UpdateAssetGroupCommand ToCommand(int id) => new(
        id, Name, Description, ParentAssetGroupId, AssetCategory, IsDepreciable,
        DepreciationMethod, DepreciationRate, DefaultUsefulLifeYears, ResidualValuePercentage,
        AssetAccountId, AccumulatedDepreciationAccountId, DepreciationExpenseAccountId, DisposalAccountId, RowVersion);
}

public record ToggleActiveRequest(byte[] RowVersion);
