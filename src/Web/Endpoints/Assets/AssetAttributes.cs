using ERP_Government.Application.Assets.AssetAttributes.Bindings.Commands;
using ERP_Government.Application.Assets.AssetAttributes.Definitions.Commands;
using ERP_Government.Application.Assets.AssetAttributes.Definitions.Queries;
using ERP_Government.Application.Common.Security;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class AssetAttributes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/definitions", HandleGetDefinitions)
            .RequireAuthorization(PermissionCodes.AssetGroupsView)
            .Produces<List<AssetAttributeDefinitionDto>>();
        group.MapGet("/definitions/{id:int}", HandleGetDefinitionById)
            .RequireAuthorization(PermissionCodes.AssetGroupsView)
            .Produces<AssetAttributeDefinitionDetailDto>();
        group.MapPost("/definitions", HandleCreateDefinition)
            .RequireAuthorization(PermissionCodes.AssetGroupsCreate)
            .Produces<int>();
        group.MapPut("/definitions/{id:int}", HandleUpdateDefinition)
            .RequireAuthorization(PermissionCodes.AssetGroupsUpdate)
            .Produces<int>();
        group.MapPut("/groups/{groupId:int}/attributes", HandleSetBindings)
            .RequireAuthorization(PermissionCodes.AssetGroupsUpdate)
            .Produces<int>();
    }

    private static async Task<IResult> HandleGetDefinitions(
        ISender sender,
        string? search = null,
        AssetAttributeDataType? dataType = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetAssetAttributeDefinitionsQuery(search, dataType, isActive, page, pageSize));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetDefinitionById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetAttributeDefinitionByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreateDefinition(
        ISender sender,
        CreateAssetAttributeDefinitionCommand command)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Created($"/definitions/{result.Value}", result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdateDefinition(
        ISender sender,
        int id,
        UpdateAssetAttributeDefinitionCommand command)
    {
        if (id != command.Id) return Results.BadRequest("Mismatched ID");
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleSetBindings(
        ISender sender,
        int groupId,
        SetGroupAttributeBindingsCommand command)
    {
        if (groupId != command.AssetGroupId) return Results.BadRequest("Mismatched group ID");
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
