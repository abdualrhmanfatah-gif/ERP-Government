using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Inventory.Common;
using ERP_Government.Application.Inventory.ItemCategories.Commands.CreateItemCategory;
using ERP_Government.Application.Inventory.ItemCategories.Commands.ToggleItemCategoryActive;
using ERP_Government.Application.Inventory.ItemCategories.Commands.UpdateItemCategory;
using ERP_Government.Application.Inventory.ItemCategories.Queries.GetItemCategories;
using ERP_Government.Application.Inventory.ItemCategories.Queries.GetItemCategoryById;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Inventory;

public class ItemCategories : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.ItemCategoriesView)
            .Produces<IReadOnlyList<ItemCategoryResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.ItemCategoriesView)
            .Produces<ItemCategoryResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.ItemCategoriesCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.ItemCategoriesUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/toggle-active", HandleToggleActive)
            .RequireAuthorization(PermissionCodes.ItemCategoriesUpdate)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        bool? isActive = null)
    {
        var result = await sender.Send(new GetItemCategoriesQuery(search, isActive));
        return Results.Ok(result.Select(c => c.ToResponse()).ToList());
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetItemCategoryByIdQuery(id));
        return result is not null ? Results.Ok(result.ToResponse()) : Results.NotFound();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateItemCategoryRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/ItemCategories/{result.Value}", result.Value)
            : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateItemCategoryRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> HandleToggleActive(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new ToggleItemCategoryActiveCommand(id));
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }
}

public record CreateItemCategoryRequest(
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    int? ParentItemCategoryId,
    int? ExpenseAccountId,
    int? InventoryAccountId,
    int? TaxAccountId,
    string? TaxClass,
    bool IsActive)
{
    public CreateItemCategoryCommand ToCommand() => new(
        Code, Name, NameEn, Description, ParentItemCategoryId,
        ExpenseAccountId, InventoryAccountId, TaxAccountId, TaxClass, IsActive);
}

public record UpdateItemCategoryRequest(
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    int? ParentItemCategoryId,
    int? ExpenseAccountId,
    int? InventoryAccountId,
    int? TaxAccountId,
    string? TaxClass,
    bool IsActive)
{
    public UpdateItemCategoryCommand ToCommand(int id) => new(
        id, Code, Name, NameEn, Description, ParentItemCategoryId,
        ExpenseAccountId, InventoryAccountId, TaxAccountId, TaxClass, IsActive);
}
