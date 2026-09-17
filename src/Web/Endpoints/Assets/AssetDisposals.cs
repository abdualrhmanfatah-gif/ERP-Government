using ERP_Government.Application.Assets.AssetTransactions.Disposals.Commands;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetDisposals : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.AssetDisposalsView)
            .Produces<List<AssetDisposalResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.AssetDisposalsView)
            .Produces<AssetDisposalResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.AssetDisposalsCreate)
            .Produces<int>();
        group.MapPost("/{id:int}/approve", HandleApprove)
            .RequireAuthorization(PermissionCodes.AssetDisposalsApprove)
            .Produces<int>();
        group.MapPost("/{id:int}/post", HandlePost)
            .RequireAuthorization(PermissionCodes.AssetDisposalsPost)
            .Produces<int>();
    }

    private static async Task<IResult> HandleGetAll(ISender sender)
    {
        var result = await sender.Send(new GetAssetDisposalsQuery());
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetDisposalByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateAssetDisposalCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/AssetDisposals/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleApprove(ISender sender, int id)
    {
        var result = await sender.Send(new ApproveAssetDisposalCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandlePost(ISender sender, int id)
    {
        var result = await sender.Send(new PostAssetDisposalCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
