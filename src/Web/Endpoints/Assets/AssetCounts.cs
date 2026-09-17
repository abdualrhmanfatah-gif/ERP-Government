using ERP_Government.Application.Assets.PhysicalCounts.Commands;
using ERP_Government.Application.Assets.PhysicalCounts.Queries;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetCounts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.AssetCountsView)
            .Produces<PaginatedList<AssetCountResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.AssetCountsView)
            .Produces<AssetCountDetailResponse>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.AssetCountsCreate)
            .Produces<int>();
        group.MapPost("/{id:int}/start", HandleStart)
            .RequireAuthorization(PermissionCodes.AssetCountsExecute)
            .Produces<int>();
        group.MapPut("/{id:int}/lines/{lineId:int}", HandleUpdateLine)
            .RequireAuthorization(PermissionCodes.AssetCountsExecute)
            .Produces<int>();
        group.MapPost("/{id:int}/complete", HandleComplete)
            .RequireAuthorization(PermissionCodes.AssetCountsExecute)
            .Produces<int>();
        group.MapPost("/{id:int}/review", HandleReview)
            .RequireAuthorization(PermissionCodes.AssetCountsReview)
            .Produces<int>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        string? status = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetAssetCountsQuery(search, status, page, pageSize));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetCountByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateAssetPhysicalCountCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/AssetCounts/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleStart(ISender sender, int id)
    {
        var result = await sender.Send(new StartAssetPhysicalCountCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdateLine(
        ISender sender,
        int id,
        int lineId,
        UpdateAssetPhysicalCountLineRequest request)
    {
        var command = new UpdateAssetPhysicalCountLineCommand(
            id,
            lineId,
            request.IsFound,
            request.PhysicalLocationId,
            request.PhysicalEmployeeId,
            request.PhysicalStatus,
            request.DiscrepancyNotes,
            request.RowVersion);

        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleComplete(ISender sender, int id)
    {
        var result = await sender.Send(new CompleteAssetPhysicalCountCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleReview(ISender sender, int id)
    {
        var result = await sender.Send(new ReviewAssetPhysicalCountCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}

public record UpdateAssetPhysicalCountLineRequest(
    CountFoundState IsFound,
    int? PhysicalLocationId,
    int? PhysicalEmployeeId,
    string? PhysicalStatus,
    string? DiscrepancyNotes,
    byte[] RowVersion);
