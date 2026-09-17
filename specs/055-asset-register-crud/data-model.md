# Data Model: Asset Register CRUD

**Feature**: 055-asset-register-crud | **Date**: 2026-09-15

## Entity: Asset

**Table**: `Assets` | **Source**: `src/Domain/Assets/Entities/Asset.cs`

### Fields

| Property | Type | Nullable | Max Length | Constraints | Notes |
|----------|------|----------|------------|-------------|-------|
| Id | int | No | — | PK, auto-increment | Inherited from BaseEntity |
| Code | string | No | 50 | Unique index, required | Auto-generated: AST-xxxxxx |
| Name | string | No | 200 | Required | Asset display name |
| Description | string | Yes | 500 | — | Free-text description |
| AssetGroupId | int | No | — | FK → AssetGroups (Restrict), indexed | Required classification |
| LocationId | int | Yes | — | FK → Locations, indexed | Physical location |
| FundId | int | Yes | — | FK → Funds, indexed | Funding source |
| CostCenterId | int | Yes | — | FK → CostCenters, indexed | Cost allocation |
| CustodianId | int | Yes | — | FK → Users, indexed | Responsible person |
| AssetTag | string | Yes | 50 | Unique (nullable) | Physical tag on asset |
| Barcode | string | Yes | 100 | Unique (nullable) | Barcode identifier |
| SerialNumber | string | Yes | 100 | Unique (nullable) | Manufacturer serial |
| ImageUrl | nvarchar(max) | Yes | — | — | Deferred to v2 |
| CurrencyCode | string | Yes | 10 | — | Currency for values |
| OriginalValue | decimal(23,2) | No | — | Required | Acquisition cost |
| AcquisitionCost | decimal(23,2) | Yes | — | — | Additional costs |
| ResidualValue | decimal(23,2) | Yes | — | — | Salvage value |
| RelinquishmentValue | decimal(23,2) | Yes | — | — | Disposal value |
| AccumulatedDepreciation | decimal(23,2) | No | — | Default 0 | Running total |
| CurrentValue | decimal(23,2) | Yes | — | — | Book value |
| PurchaseDate | DateOnly | No | — | Required | Acquisition date |
| ActivationDate | DateOnly | Yes | — | — | Set when activated (F3) |
| DepreciationStartDate | DateOnly | No | — | Required | When depreciation begins |
| LastDepreciationDate | DateOnly | Yes | — | — | Last depreciation run |
| Status | string | No | 50 | Required, indexed | Draft/Active/UnderMaintenance/Disposed/WrittenOff |
| AcquisitionType | string | No | 50 | Required | Purchase/Grant/Transfer/Donation/Inherited |
| UsefulLifeYears | int | Yes | — | — | Depreciation period |
| IsFullyDepreciated | bool | No | — | Default false | Depreciation complete |
| Notes | string | Yes | 2000 | — | Free-text notes |
| IsActive | bool | No | — | Default true | Soft-delete flag |
| RowVersion | byte[] | No | — | Optimistic concurrency | Auto-managed by EF |
| Created | DateTimeOffset | No | — | Audit | Inherited |
| CreatedBy | string | Yes | — | Audit | Inherited |
| LastModified | DateTimeOffset | No | — | Audit | Inherited |
| LastModifiedBy | string | Yes | — | Audit | Inherited |

### Relationships

```
Asset ──FK──> AssetGroup (Restrict)
Asset ──FK──> Location (nullable)
Asset ──FK──> Fund (nullable)
Asset ──FK──> CostCenter (nullable)
Asset ──FK──> User/Custodian (nullable)
```

### Indexes

| Index | Columns | Type |
|-------|---------|------|
| PK_Assets | Id | Primary |
| IX_Assets_Code | Code | Unique |
| IX_Assets_Status | Status | Non-unique |
| IX_Assets_AssetGroupId | AssetGroupId | Non-unique |
| IX_Assets_LocationId | LocationId | Non-unique |
| IX_Assets_FundId | FundId | Non-unique |
| IX_Assets_CostCenterId | CostCenterId | Non-unique |
| IX_Assets_CustodianId | CustodianId | Non-unique |

### Unique Constraints (Application-Level)

| Field | Scope | Validation |
|-------|-------|------------|
| Code | Global | Auto-generated, unique index |
| AssetTag | Global (nullable) | FluentValidation async check |
| Barcode | Global (nullable) | FluentValidation async check |
| SerialNumber | Global (nullable) | FluentValidation async check |

### State Transitions

```
Draft ──────> Active
Active ─────> UnderMaintenance
Active ─────> Disposed
UnderMaintenance ──> Active
UnderMaintenance ──> Disposed
```

All other transitions are rejected by `UpdateAssetCommandValidator` and `DeactivateAssetCommandValidator`.

### Field Locking Rules

When `Status` is `Active`, `UnderMaintenance`, `Disposed`, or `WrittenOff`:
- `OriginalValue` → read-only
- `PurchaseDate` → read-only
- `AssetGroupId` → read-only

Enforced both client-side (form disabled state) and server-side (update command validation).

## Entity: AssetGroup (Reference)

**Table**: `AssetGroups` | **Source**: Existing seed data

| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| Code | string(50) | Unique |
| Name | string(200) | Required |
| Description | string(500) | Optional |
| ParentAssetGroupId | int? | Self-referencing FK |
| IsActive | bool | Toggle active/inactive |

**Referenced by**: Asset.AssetGroupId (Restrict delete)

## Enums (String-Based Validation)

### AssetStatus

| Value | Arabic Label | Description |
|-------|-------------|-------------|
| Draft | مسودة | Newly created, not yet activated |
| Active | نشط | In service, depreciation running |
| UnderMaintenance |Under الصيانة | Under repair/maintenance |
| Disposed | متخلص | Removed from service |
| WrittenOff | مُشطوب | Accounting write-off |

### AcquisitionType

| Value | Arabic Label |
|-------|-------------|
| Purchase | شراء |
| Grant | منحة |
| Transfer | تحويل |
| Donation | هدية |
| Inherited | موروث |
