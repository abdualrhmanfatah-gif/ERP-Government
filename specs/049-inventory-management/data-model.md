# Data Model: Inventory Management

**Date**: 2026-09-11

## Entities (Existing Domain — No Schema Changes)

### Item

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | yes | PK, BaseAuditableEntity |
| Code | string | yes | Unique, generated via IDocumentSequenceService "ITEM-{D6}" |
| Name | string | yes | Arabic name |
| NameEn | string? | no | English name |
| Description | string? | no | Free text |
| CategoryId | int? | no | FK to ItemCategory |
| UnitId | int | yes | FK to Unit (base unit) |
| SupplierId | int? | no | FK to Party |
| Barcode | string? | no | Unique, LTR display |
| ItemType | string | yes | Enum: Goods, Service, Raw Material, Consumable, Fixed Asset |
| OpeningStock | decimal? | no | >= 0 |
| AvailableQuantity | decimal? | no | Computed from stock transactions |
| ReservedQuantity | decimal? | no | Computed from stock transactions |
| AverageCost | decimal? | no | Computed |
| MinimumStock | decimal? | no | >= 0, <= MaximumStock if both provided |
| MaximumStock | decimal? | no | >= 0 |
| ReorderLevel | decimal? | no | >= 0 |
| ReorderQuantity | decimal? | no | >= 0 |
| LeadTimeDays | int? | no | >= 0 |
| IsActive | bool | yes | Default true |
| RowVersion | byte[] | yes | Optimistic concurrency |

### ItemCategory

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | yes | PK |
| Code | string | yes | Unique |
| Name | string | yes | Arabic name |
| NameEn | string? | no | English name |
| Description | string? | no | Free text |
| ParentItemCategoryId | int? | no | FK to self (hierarchical) |
| Level | int? | no | Computed depth |
| Breadcrumb | string? | no | Computed path |
| ExpenseAccountId | int? | no | FK to Account |
| InventoryAccountId | int? | no | FK to Account |
| TaxAccountId | int? | no | FK to Account |
| TaxClass | string? | no | Tax classification |
| IsActive | bool | yes | Default true |

### Unit

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | yes | PK |
| Code | string | yes | Unique |
| Name | string | yes | Arabic name |
| NameAr | string? | no | Alternative Arabic name |
| UnitType | string? | no | Type classification |
| BaseUnitId | int? | no | FK to self (conversion reference) |
| ConversionToBase | decimal? | no | > 0 if BaseUnitId provided |
| IsActive | bool | yes | Default true |

### ItemUnit

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | yes | PK |
| ItemId | int | yes | FK to Item |
| UnitId | int | yes | FK to Unit |
| ConversionFactor | decimal | yes | > 0 |
| IsBase | bool | yes | One per item |

### Warehouse

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| Id | int | yes | PK |
| Code | string | yes | Unique |
| Name | string | yes | Arabic name |
| LocationId | int? | no | FK to Location |
| ManagerId | int? | no | FK to Employee |
| Address | string? | no | Free text |
| City | string? | no | Free text |
| Phone | string? | no | Free text |
| Email | string? | no | Valid email format |
| TotalCapacity | decimal? | no | >= 0 |
| CurrentLoad | decimal? | no | >= 0, <= TotalCapacity |
| IsActive | bool | yes | Default true |

## Enums (Application Layer)

### ItemType

```
Goods = "Goods"
Service = "Service"
RawMaterial = "RawMaterial"
Consumable = "Consumable"
FixedAsset = "FixedAsset"
```

## Relationships

```
ItemCategory 0..1──0..* Item (via CategoryId)
Unit 0..1──0..* Item (via UnitId)
Item 1──0..* ItemUnit (via ItemId)
Unit 1──0..* ItemUnit (via UnitId)
ItemCategory 0..1──0..* ItemCategory (self-ref via ParentItemCategoryId)
Unit 0..1──0..* Unit (self-ref via BaseUnitId)
Location 0..1──0..* Warehouse (via LocationId)
```

## Validation Rules (from spec)

| Entity | Field | Rule |
|--------|-------|------|
| Item | Code | Required, unique |
| Item | Name | Required |
| Item | UnitId | Required |
| Item | ItemType | Required, must be valid enum value |
| Item | OpeningStock | >= 0 |
| Item | MinimumStock | >= 0 |
| Item | MaximumStock | >= 0 |
| Item | MinimumStock <= MaximumStock | If both provided |
| ItemCategory | Code | Required, unique |
| ItemCategory | Name | Required |
| ItemCategory | ParentItemCategoryId | Cannot be self |
| Unit | Code | Required, unique |
| Unit | Name | Required |
| Unit | ConversionToBase | > 0 if BaseUnitId provided |
| ItemUnit | ItemId | Required |
| ItemUnit | UnitId | Required |
| ItemUnit | ConversionFactor | > 0 |
| ItemUnit | IsBase | One base unit per item |
| Warehouse | Code | Required, unique |
| Warehouse | Name | Required |
| Warehouse | Email | Valid format if provided |
| Warehouse | TotalCapacity | >= 0 |
| Warehouse | CurrentLoad | >= 0 |
| Warehouse | CurrentLoad <= TotalCapacity | If both provided |
