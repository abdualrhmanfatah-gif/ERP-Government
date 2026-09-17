using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Inventory.Common;
using ERP_Government.Application.Inventory.Warehouses.Commands.CreateWarehouse;
using ERP_Government.Application.Inventory.Warehouses.Commands.ToggleWarehouseActive;
using ERP_Government.Application.Inventory.Warehouses.Commands.UpdateWarehouse;
using ERP_Government.Application.Inventory.Warehouses.Queries.GetWarehouseById;
using ERP_Government.Application.Inventory.Warehouses.Queries.GetWarehouses;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Inventory;

public class Warehouses : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.WarehousesView)
            .Produces<IReadOnlyList<WarehouseResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.WarehousesView)
            .Produces<WarehouseResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.WarehousesCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.WarehousesUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/toggle-active", HandleToggleActive)
            .RequireAuthorization(PermissionCodes.WarehousesUpdate)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        bool? isActive = null)
    {
        var result = await sender.Send(new GetWarehousesQuery(search, isActive));
        return Results.Ok(result.Select(w => w.ToResponse()).ToList());
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetWarehouseByIdQuery(id));
        return result.Succeeded
            ? Results.Ok(result.Value!.ToResponse())
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateWarehouseRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/Warehouses/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateWarehouseRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleToggleActive(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new ToggleWarehouseActiveCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}

public record CreateWarehouseRequest(
    string Code,
    string Name,
    int? LocationId,
    int? ManagerId,
    string? Address,
    string? City,
    string? Phone,
    string? Email,
    decimal? TotalCapacity,
    decimal? CurrentLoad,
    bool IsActive)
{
    public CreateWarehouseCommand ToCommand() => new(
        Code, Name, LocationId, ManagerId, Address, City, Phone, Email,
        TotalCapacity, CurrentLoad, IsActive);
}

public record UpdateWarehouseRequest(
    string Code,
    string Name,
    int? LocationId,
    int? ManagerId,
    string? Address,
    string? City,
    string? Phone,
    string? Email,
    decimal? TotalCapacity,
    decimal? CurrentLoad,
    bool IsActive)
{
    public UpdateWarehouseCommand ToCommand(int id) => new(
        id, Code, Name, LocationId, ManagerId, Address, City, Phone, Email,
        TotalCapacity, CurrentLoad, IsActive);
}
