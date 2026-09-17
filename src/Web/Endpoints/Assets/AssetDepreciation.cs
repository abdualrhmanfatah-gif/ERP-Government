using ERP_Government.Application.Assets.Depreciation.Post.Commands;
using ERP_Government.Application.Assets.Depreciation.Queries.GetAssetDepreciationSchedules;
using ERP_Government.Application.Assets.Depreciation.Queries.GetDepreciationRunById;
using ERP_Government.Application.Assets.Depreciation.Queries.GetDepreciationRuns;
using ERP_Government.Application.Assets.Depreciation.Run.Commands;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetDepreciation : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetRuns)
            .RequireAuthorization(PermissionCodes.AssetDepreciationView)
            .Produces<PaginatedList<DepreciationRunResponse>>();
        group.MapGet("/{id:int}", HandleGetRunById)
            .RequireAuthorization(PermissionCodes.AssetDepreciationView)
            .Produces<DepreciationRunDetailResponse>();
        group.MapGet("/schedules/asset/{assetId:int}", HandleGetAssetSchedules)
            .RequireAuthorization(PermissionCodes.AssetDepreciationView)
            .Produces<PaginatedList<AssetDepreciationScheduleResponse>>();

        group.MapPost("/", HandleRun)
            .RequireAuthorization(PermissionCodes.AssetDepreciationRun)
            .Produces<RunDepreciationResult>();
        group.MapPost("/preview", HandlePreview)
            .RequireAuthorization(PermissionCodes.AssetDepreciationView)
            .Produces<PreviewDepreciationResult>();
        group.MapPost("/{id:int}/post", HandlePost)
            .RequireAuthorization(PermissionCodes.AssetDepreciationPost)
            .Produces<int>();
        group.MapDelete("/{id:int}", HandleDelete)
            .RequireAuthorization(PermissionCodes.AssetDepreciationRun);
    }

    private static async Task<IResult> HandleGetRuns(
        ISender sender,
        [AsParameters] GetDepreciationRunsQuery query)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetRunById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetDepreciationRunByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetAssetSchedules(
        ISender sender,
        int assetId)
    {
        var result = await sender.Send(new GetAssetDepreciationSchedulesQuery(assetId));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleRun(
        ISender sender,
        RunDepreciationCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandlePreview(
        ISender sender,
        PreviewDepreciationCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandlePost(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new PostDepreciationCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleDelete(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new DeleteDepreciationRunCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}
