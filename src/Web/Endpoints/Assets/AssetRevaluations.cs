using ERP_Government.Application.Assets.AssetTransactions.Revaluations.Commands;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetRevaluations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.AssetRevaluationsView)
            .Produces<List<AssetRevaluationResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.AssetRevaluationsView)
            .Produces<AssetRevaluationResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.AssetRevaluationsCreate)
            .Produces<int>();
    }

    private static async Task<IResult> HandleGetAll(ISender sender)
    {
        var result = await sender.Send(new GetAssetRevaluationsQuery());
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetRevaluationByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateAssetRevaluationCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/AssetRevaluations/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }
}
