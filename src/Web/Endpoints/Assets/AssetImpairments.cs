using ERP_Government.Application.Assets.AssetTransactions.Impairments.Commands;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetImpairments : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.AssetImpairmentsView)
            .Produces<List<AssetImpairmentResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.AssetImpairmentsView)
            .Produces<AssetImpairmentResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.AssetImpairmentsCreate)
            .Produces<int>();
        group.MapPost("/{id:int}/reverse", HandleReverse)
            .RequireAuthorization(PermissionCodes.AssetImpairmentsPost)
            .Produces<int>();
    }

    private static async Task<IResult> HandleGetAll(ISender sender)
    {
        var result = await sender.Send(new GetAssetImpairmentsQuery());
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetImpairmentByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateAssetImpairmentCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/AssetImpairments/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleReverse(ISender sender, int id)
    {
        var result = await sender.Send(new ReverseAssetImpairmentCommand(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
