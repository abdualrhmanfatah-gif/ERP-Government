using ERP_Government.Application.Assets.AssetTransactions.Transfers.Commands;
using ERP_Government.Application.Assets.AssetTransactions.Transfers.Common;
using ERP_Government.Application.Assets.AssetTransactions.Transfers.Queries;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetTransfers : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.AssetTransfersView)
            .Produces<PaginatedList<AssetTransferListItemResponse>>();

        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.AssetTransfersView)
            .Produces<AssetTransferDetailResponse>();

        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.AssetTransfersCreate)
            .Produces<int>(StatusCodes.Status201Created);

        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.AssetTransfersCreate)
            .Produces<int>();

        group.MapPost("/{id:int}/execute", HandleExecute)
            .RequireAuthorization(PermissionCodes.AssetTransfersExecute)
            .Produces<int>();

        group.MapPost("/{id:int}/cancel", HandleCancel)
            .RequireAuthorization(PermissionCodes.AssetTransfersCreate)
            .Produces<int>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search,
        string? status,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetAssetTransfersQuery(search, status, page, pageSize));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetTransferByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateAssetTransferCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/AssetTransfers/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateTransferRequest request)
    {
        var result = await sender.Send(new UpdateAssetTransferCommand(
            id,
            request.TransactionDate,
            request.ToLocationId,
            request.ToEmployeeId,
            request.Notes,
            request.RowVersion));

        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleExecute(
        ISender sender,
        int id,
        ExecuteTransferRequest request)
    {
        var result = await sender.Send(new ExecuteAssetTransferCommand(
            id,
            request.RowVersion,
            request.AssetRowVersion));

        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCancel(
        ISender sender,
        int id,
        CancelTransferRequest request)
    {
        var result = await sender.Send(new CancelAssetTransferCommand(id, request.RowVersion));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}

public record UpdateTransferRequest(
    DateOnly TransactionDate,
    int? ToLocationId,
    int? ToEmployeeId,
    string? Notes,
    byte[] RowVersion);

public record ExecuteTransferRequest(byte[] RowVersion, byte[] AssetRowVersion);

public record CancelTransferRequest(byte[] RowVersion);
