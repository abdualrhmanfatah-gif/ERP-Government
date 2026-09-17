using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.Inventory.Common;
using ERP_Government.Application.Inventory.Units.Commands.CreateUnit;
using ERP_Government.Application.Inventory.Units.Commands.ToggleUnitActive;
using ERP_Government.Application.Inventory.Units.Commands.UpdateUnit;
using ERP_Government.Application.Inventory.Units.Queries.GetUnitById;
using ERP_Government.Application.Inventory.Units.Queries.GetUnits;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Inventory;

public class Units : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.UnitsView)
            .Produces<IReadOnlyList<UnitResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.UnitsView)
            .Produces<UnitResponse?>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.UnitsCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.UnitsUpdate)
            .Produces<Result>();
        group.MapPatch("/{id:int}/toggle-active", HandleToggleActive)
            .RequireAuthorization(PermissionCodes.UnitsUpdate)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        bool? isActive = null)
    {
        var result = await sender.Send(new GetUnitsQuery(search, isActive));
        return Results.Ok(result.Select(u => u.ToResponse()).ToList());
    }

    private static async Task<IResult> HandleGetById(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new GetUnitByIdQuery(id));
        return result.Succeeded
            ? Results.Ok(result.Value!.ToResponse())
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender,
        CreateUnitRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/Units/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender,
        int id,
        UpdateUnitRequest request)
    {
        var result = await sender.Send(request.ToCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleToggleActive(
        ISender sender,
        int id)
    {
        var result = await sender.Send(new ToggleUnitActiveCommand(id));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}

public record CreateUnitRequest(
    string Code,
    string Name,
    string? NameAr,
    string? UnitType,
    int? BaseUnitId,
    decimal? ConversionToBase,
    bool IsActive)
{
    public CreateUnitCommand ToCommand() => new(
        Code, Name, NameAr, UnitType, BaseUnitId, ConversionToBase, IsActive);
}

public record UpdateUnitRequest(
    string Code,
    string Name,
    string? NameAr,
    string? UnitType,
    int? BaseUnitId,
    decimal? ConversionToBase,
    bool IsActive)
{
    public UpdateUnitCommand ToCommand(int id) => new(
        id, Code, Name, NameAr, UnitType, BaseUnitId, ConversionToBase, IsActive);
}
