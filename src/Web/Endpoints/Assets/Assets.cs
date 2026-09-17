using ERP_Government.Application.Assets.AssetAttributes.Values;
using ERP_Government.Application.Assets.Assets.Commands.CreateAsset;
using ERP_Government.Application.Assets.Assets.Commands.DeactivateAsset;
using ERP_Government.Application.Assets.Assets.Commands.UpdateAsset;
using ERP_Government.Application.Assets.Assets.Common;
using ERP_Government.Application.Assets.Assets.Queries.GetAssetById;
using ERP_Government.Application.Assets.Assets.Queries.GetAssets;
using ERP_Government.Application.Assets.Traceability.Queries;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.Common.Security;
using ERP_Government.Web.Infrastructure;

namespace ERP_Government.Web.Endpoints.Assets;

public class Assets : IEndpointGroup
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleGetAll)
            .RequireAuthorization(PermissionCodes.AssetsView)
            .Produces<PaginatedList<AssetResponse>>();
        group.MapGet("/{id:int}", HandleGetById)
            .RequireAuthorization(PermissionCodes.AssetsView)
            .Produces<AssetDetailResponse?>();
        group.MapGet("/{id:int}/transactions", HandleGetHistory)
            .RequireAuthorization(PermissionCodes.AssetsView)
            .Produces<List<TransactionSummaryResponse>>();
        group.MapPost("/", HandleCreate)
            .RequireAuthorization(PermissionCodes.AssetsCreate)
            .Produces<int>();
        group.MapPut("/{id:int}", HandleUpdate)
            .RequireAuthorization(PermissionCodes.AssetsUpdate)
            .Produces<Result>();
        group.MapPost("/{id:int}/deactivate", HandleDeactivate)
            .RequireAuthorization(PermissionCodes.AssetsUpdate)
            .Produces<Result>();
    }

    private static async Task<IResult> HandleGetAll(
        ISender sender,
        string? search = null,
        string? status = "Active",
        int? assetGroupId = null,
        int? locationId = null,
        int? employeeId = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await sender.Send(new GetAssetsQuery(
            search, status, assetGroupId, locationId, employeeId, page, pageSize));
        return Results.Ok(result);
    }

    private static async Task<IResult> HandleGetById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetByIdQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleGetHistory(ISender sender, int id)
    {
        var result = await sender.Send(new GetAssetHistoryQuery(id));
        return result.Succeeded ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleCreate(
        ISender sender, CreateAssetRequest request)
    {
        var result = await sender.Send(request.ToCommand());
        return result.Succeeded
            ? Results.Created($"/api/Assets/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleUpdate(
        ISender sender, int id, UpdateAssetRequest request)
    {
        if (id != request.Id)
            return Results.Problem(
                detail: "عدم تطابق المعرف.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                type: "about:blank");

        var result = await sender.Send(request.ToCommand());
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }

    private static async Task<IResult> HandleDeactivate(
        ISender sender, int id, DeactivateAssetRequest request)
    {
        var result = await sender.Send(new DeactivateAssetCommand(id, request.RowVersion));
        return result.Succeeded ? Results.Ok() : result.ToProblemDetails();
    }
}

public record CreateAssetRequest(
    string Name,
    string? Description,
    int AssetGroupId,
    int? LocationId,
    int? EmployeeId,
    string? AssetTag,
    string? Barcode,
    string? SerialNumber,
    int CurrencyId,
    int? ExchangeRateId,
    decimal OriginalValue,
    decimal? AcquisitionCost,
    DateOnly PurchaseDate,
    DateOnly DepreciationStartDate,
    string AcquisitionType,
    int? UsefulLifeYears,
    string? Notes,
    List<AssetAttributeValueRequest>? AttributeValues)
{
    public CreateAssetCommand ToCommand() => new(
        Name, Description, AssetGroupId, LocationId, EmployeeId,
        AssetTag, Barcode, SerialNumber, CurrencyId, ExchangeRateId, OriginalValue, AcquisitionCost,
        PurchaseDate, DepreciationStartDate, AcquisitionType, UsefulLifeYears, Notes,
        AttributeValues?.Select(v => v.ToInput()).ToList());
}

public record UpdateAssetRequest(
    int Id,
    string Name,
    string? Description,
    int AssetGroupId,
    int? LocationId,
    int? EmployeeId,
    string? AssetTag,
    string? Barcode,
    string? SerialNumber,
    int CurrencyId,
    int? ExchangeRateId,
    decimal OriginalValue,
    decimal? AcquisitionCost,
    DateOnly PurchaseDate,
    DateOnly DepreciationStartDate,
    string AcquisitionType,
    int? UsefulLifeYears,
    string? Notes,
    string? Status,
    byte[] RowVersion,
    List<AssetAttributeValueRequest>? AttributeValues)
{
    public UpdateAssetCommand ToCommand() => new(
        Id, Name, Description, AssetGroupId, LocationId, EmployeeId,
        AssetTag, Barcode, SerialNumber, CurrencyId, ExchangeRateId, OriginalValue, AcquisitionCost,
        PurchaseDate, DepreciationStartDate, AcquisitionType, UsefulLifeYears, Notes, Status, RowVersion,
        AttributeValues?.Select(v => v.ToInput()).ToList());
}

public record AssetAttributeValueRequest(
    int AssetAttributeDefinitionId,
    string? TextValue,
    int? IntegerValue,
    decimal? DecimalValue,
    DateOnly? DateValue,
    bool? BooleanValue)
{
    public AttributeValueInput ToInput() => new(
        AssetAttributeDefinitionId, TextValue, IntegerValue, DecimalValue, DateValue, BooleanValue);
}

public record DeactivateAssetRequest(byte[] RowVersion);
