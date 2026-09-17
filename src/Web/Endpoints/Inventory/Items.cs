using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Inventory.Common;
using ERP_Government.Application.Inventory.Items.Commands.CreateItem;
using ERP_Government.Application.Inventory.Items.Commands.ToggleItemActive;
using ERP_Government.Application.Inventory.Items.Commands.UpdateItem;
using ERP_Government.Application.Inventory.Items.Enums;
using ERP_Government.Application.Inventory.Items.Queries.GetItemById;
using ERP_Government.Application.Inventory.Items.Queries.GetItems;
using ERP_Government.Application.Inventory.ItemUnits.Queries.GetItemUnitsByItemId;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Inventory;

public class Items : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.ItemsView)
            .Produces<PaginatedList<ItemResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.ItemsView)
            .Produces<ItemDetailResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.ItemsCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.ItemsUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/toggle-active", HandleToggleActive)
            .RequireAuthorization(PermissionCodes.ItemsUpdate)
            .Produces<Result>();
        group.MapGet("/{itemId:int}/units", HandleGetItemUnits)
            .RequireAuthorization(PermissionCodes.ItemsView)
            .Produces<IReadOnlyList<ItemUnitResponse>>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        int? categoryId = null,
        int? unitId = null,
        ItemType? itemType = null,
        bool? isActive = null,
        bool? underReorderLevel = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetItemsQuery(
            search, categoryId, unitId, itemType, isActive, underReorderLevel, page, pageSize));
        return Results.Ok(new PaginatedList<ItemResponse>(
            result.Items.Select(i => i.ToResponse()).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize));
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var item = await sender.Send(new GetItemByIdQuery(id));
        if (!item.Succeeded)
            return item.ToProblemDetails();

        var response = item.Value!.ToDetailResponse();
        var itemUnits = await sender.Send(new GetItemUnitsByItemIdQuery(id));
        response = response with { ItemUnits = itemUnits.Select(iu => iu.ToResponse()).ToList() };

        return Results.Ok(response);
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateItemRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/Items/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateItemRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleToggleActive(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new ToggleItemActiveCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetItemUnits(
        ISender sender,
        int itemId)
    {
        var result = await sender.Send(new GetItemUnitsByItemIdQuery(itemId));
        return Results.Ok(result.Select(iu => iu.ToResponse()).ToList());
    }
}

public record CreateItemRequest(
    string Name,
    string? NameEn,
    string? Description,
    int? CategoryId,
    int UnitId,
    int? SupplierId,
    string? Barcode,
    string ItemType,
    decimal? OpeningStock,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    bool IsActive)
{
    public CreateItemCommand ToCommand() => new(
        Name, NameEn, Description, CategoryId, UnitId, SupplierId, Barcode,
        ItemType, OpeningStock, MinimumStock, MaximumStock, ReorderLevel,
        ReorderQuantity, LeadTimeDays, IsActive);
}

public record UpdateItemRequest(
    string Name,
    string? NameEn,
    string? Description,
    int? CategoryId,
    int UnitId,
    int? SupplierId,
    string? Barcode,
    string ItemType,
    decimal? OpeningStock,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    bool IsActive)
{
    public UpdateItemCommand ToCommand(int id) => new(
        id, Name, NameEn, Description, CategoryId, UnitId, SupplierId, Barcode,
        ItemType, OpeningStock, MinimumStock, MaximumStock, ReorderLevel,
        ReorderQuantity, LeadTimeDays, IsActive);
}
