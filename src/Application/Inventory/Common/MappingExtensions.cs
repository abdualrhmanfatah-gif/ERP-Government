using ERP_Government.Domain.Inventory.Entities;
using Unit = ERP_Government.Domain.Inventory.Entities.Unit;

namespace ERP_Government.Application.Inventory.Common;

public record ItemResponse(
    int Id,
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    int? CategoryId,
    string? CategoryName,
    int UnitId,
    string UnitName,
    string? Barcode,
    string ItemType,
    decimal? OpeningStock,
    decimal? AvailableQuantity,
    decimal? ReservedQuantity,
    decimal? AverageCost,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    bool IsActive);

public record ItemDetailResponse(
    int Id,
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    int? CategoryId,
    string? CategoryName,
    int UnitId,
    string UnitName,
    int? SupplierId,
    string? Barcode,
    string ItemType,
    decimal? OpeningStock,
    decimal? AvailableQuantity,
    decimal? ReservedQuantity,
    decimal? AverageCost,
    decimal? MinimumStock,
    decimal? MaximumStock,
    decimal? ReorderLevel,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    bool IsActive,
    List<ItemUnitResponse> ItemUnits);

public record ItemCategoryResponse(
    int Id,
    string Code,
    string Name,
    string? NameEn,
    string? Description,
    int? ParentItemCategoryId,
    string? ParentName,
    int? Level,
    string? Breadcrumb,
    bool IsActive);

public record UnitResponse(
    int Id,
    string Code,
    string Name,
    string? NameAr,
    string? UnitType,
    int? BaseUnitId,
    string? BaseUnitName,
    decimal? ConversionToBase,
    bool IsActive);

public record ItemUnitResponse(
    int Id,
    int ItemId,
    int UnitId,
    string UnitName,
    decimal ConversionFactor,
    bool IsBase);

public record WarehouseResponse(
    int Id,
    string Code,
    string Name,
    int? LocationId,
    string? LocationName,
    int? ManagerId,
    string? ManagerName,
    string? Address,
    string? City,
    string? Phone,
    string? Email,
    decimal? TotalCapacity,
    decimal? CurrentLoad,
    bool IsActive);

public record LocationResponse(
    int Id,
    string Code,
    string Name,
    string? Barcode,
    int? ParentLocationId,
    string? ParentName,
    int? Level,
    string? Breadcrumb,
    string? City,
    string? Address,
    decimal? Capacity,
    bool IsActive,
    byte[] RowVersion);

public static class InventoryMappingExtensions
{
    public static ItemResponse ToResponse(this Item item) => new(
        item.Id,
        item.Code,
        item.Name,
        item.NameEn,
        item.Description,
        item.CategoryId,
        item.Category?.Name,
        item.UnitId,
        item.Unit?.Name ?? string.Empty,
        item.Barcode,
        item.ItemType,
        item.OpeningStock,
        item.AvailableQuantity,
        item.ReservedQuantity,
        item.AverageCost,
        item.MinimumStock,
        item.MaximumStock,
        item.ReorderLevel,
        item.ReorderQuantity,
        item.LeadTimeDays,
        item.IsActive);

    public static ItemDetailResponse ToDetailResponse(this Item item) => new(
        item.Id,
        item.Code,
        item.Name,
        item.NameEn,
        item.Description,
        item.CategoryId,
        item.Category?.Name,
        item.UnitId,
        item.Unit?.Name ?? string.Empty,
        item.SupplierId,
        item.Barcode,
        item.ItemType,
        item.OpeningStock,
        item.AvailableQuantity,
        item.ReservedQuantity,
        item.AverageCost,
        item.MinimumStock,
        item.MaximumStock,
        item.ReorderLevel,
        item.ReorderQuantity,
        item.LeadTimeDays,
        item.IsActive,
        []);

    public static ItemCategoryResponse ToResponse(this ItemCategory category) => new(
        category.Id,
        category.Code,
        category.Name,
        category.NameEn,
        category.Description,
        category.ParentItemCategoryId,
        category.ParentItemCategory?.Name,
        category.Level,
        category.Breadcrumb,
        category.IsActive);

    public static UnitResponse ToResponse(this Unit unit) => new(
        unit.Id,
        unit.Code,
        unit.Name,
        unit.NameAr,
        unit.UnitType,
        unit.BaseUnitId,
        unit.BaseUnit?.Name,
        unit.ConversionToBase,
        unit.IsActive);

    public static ItemUnitResponse ToResponse(this ItemUnit itemUnit) => new(
        itemUnit.Id,
        itemUnit.ItemId,
        itemUnit.UnitId,
        itemUnit.Unit?.Name ?? string.Empty,
        itemUnit.ConversionFactor,
        itemUnit.IsBase);

    public static WarehouseResponse ToResponse(this Warehouse warehouse) => new(
        warehouse.Id,
        warehouse.Code,
        warehouse.Name,
        warehouse.LocationId,
        warehouse.Location?.Name,
        warehouse.ManagerId,
        null,
        warehouse.Address,
        warehouse.City,
        warehouse.Phone,
        warehouse.Email,
        warehouse.TotalCapacity,
        warehouse.CurrentLoad,
        warehouse.IsActive);

    public static LocationResponse ToResponse(this Location location) => new(
        location.Id,
        location.Code,
        location.Name,
        location.Barcode,
        location.ParentLocationId,
        location.ParentLocation?.Name,
        location.Level,
        location.Breadcrumb,
        location.City,
        location.Address,
        location.Capacity,
        location.IsActive,
        location.RowVersion);
}
