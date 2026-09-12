using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Inventory.Common;
using ERP_Government.Application.Inventory.ItemUnits.Commands.AddItemUnit;
using ERP_Government.Application.Inventory.ItemUnits.Commands.RemoveItemUnit;
using ERP_Government.Application.Inventory.ItemUnits.Commands.UpdateItemUnit;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Inventory;

public class ItemUnits : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/items/{itemId:int}/units", HandleGetByItemId)
            .RequireAuthorization(PermissionCodes.ItemsView)
            .Produces<IReadOnlyList<ItemUnitResponse>>();
        group.MapPost("/items/{itemId:int}/units", HandleCreate)
            .RequireAuthorization(PermissionCodes.ItemsUpdate)
            .Produces<int>();
        group.MapPut("/items/{itemId:int}/units/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.ItemsUpdate)
            .Produces<Result>();
        group.MapDelete("/items/{itemId:int}/units/{id:int}", HandleDelete)
            .RequireAuthorization(PermissionCodes.ItemsUpdate)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetByItemId(
        ISender sender,
        int itemId)
    {
        var result = await sender.Send(new ERP_Government.Application.Inventory.ItemUnits.Queries.GetItemUnitsByItemId.GetItemUnitsByItemIdQuery(itemId));
        return Results.Ok(result.Select(iu => iu.ToResponse()).ToList());
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        int itemId,
        AddItemUnitRequest request)
    {
        var result = await sender.Send(request.ToCommand(itemId));
        return result.Succeeded
            ? Results.Created($"/api/Items/{itemId}/units/{result.Value}", result.Value)
            : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int itemId,
        int id,
        UpdateItemUnitRequest request)
    {
        var result = await sender.Send(request.ToCommand(id, itemId));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleDelete(
        ISender sender,
        int itemId,
        int id)
    {
        var result = await sender.Send(new RemoveItemUnitCommand(id, itemId));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }
}

public record AddItemUnitRequest(
    int UnitId,
    decimal ConversionFactor,
    bool IsBase)
{
    public AddItemUnitCommand ToCommand(int itemId) => new(
        itemId, UnitId, ConversionFactor, IsBase);
}

public record UpdateItemUnitRequest(
    decimal ConversionFactor,
    bool IsBase)
{
    public UpdateItemUnitCommand ToCommand(int id, int itemId) => new(
        id, itemId, ConversionFactor, IsBase);
}
