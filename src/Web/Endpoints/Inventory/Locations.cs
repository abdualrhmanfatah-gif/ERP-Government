using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Inventory.Common;
using ERP_Government.Application.Inventory.Locations.Commands.CreateLocation;
using ERP_Government.Application.Inventory.Locations.Commands.ToggleLocationActive;
using ERP_Government.Application.Inventory.Locations.Commands.UpdateLocation;
using ERP_Government.Application.Inventory.Locations.Queries.GetLocationById;
using ERP_Government.Application.Inventory.Locations.Queries.GetLocations;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Inventory;

public class Locations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.LocationsView)
            .Produces<IReadOnlyList<LocationResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.LocationsView)
            .Produces<LocationResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.LocationsCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.LocationsUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/activate", HandleActivate)
            .RequireAuthorization(PermissionCodes.LocationsUpdate)
            .Produces<Result<int>>();
        group.MapPatch("/{id:int}/deactivate", HandleDeactivate)
            .RequireAuthorization(PermissionCodes.LocationsUpdate)
            .Produces<Result<int>>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        bool? isActive = null,
        int? parentLocationId = null)
    {
        var result = await sender.Send(new GetLocationsQuery(search, isActive, parentLocationId));
        return Results.Ok(result.Select(l => l.ToResponse()).ToList());
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetLocationByIdQuery(id));
        return result.Succeeded
            ? Results.Ok(result.Value!.ToResponse())
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateLocationRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/Locations/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateLocationRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleActivate(
        ISender sender,
        int id,
        ToggleActiveRequest request)
    {
        var result = await sender.Send(new ToggleLocationActiveCommand(id, true, request.RowVersion));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleDeactivate(
        ISender sender,
        int id,
        ToggleActiveRequest request)
    {
        var result = await sender.Send(new ToggleLocationActiveCommand(id, false, request.RowVersion));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}

public record CreateLocationRequest(
    string Code,
    string Name,
    string? Barcode,
    int? ParentLocationId,
    string? City,
    string? Address,
    decimal? Capacity,
    bool IsActive)
{
    public CreateLocationCommand ToCommand() => new(
        Code, Name, Barcode, ParentLocationId, City, Address, Capacity, IsActive);
}

public record UpdateLocationRequest(
    string Code,
    string Name,
    string? Barcode,
    int? ParentLocationId,
    string? City,
    string? Address,
    decimal? Capacity,
    byte[] RowVersion)
{
    public UpdateLocationCommand ToCommand(int id) => new(
        id, Code, Name, Barcode, ParentLocationId, City, Address, Capacity, RowVersion);
}

public record ToggleActiveRequest(byte[] RowVersion);
