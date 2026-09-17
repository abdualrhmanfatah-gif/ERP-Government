# Research: Asset Register CRUD

**Feature**: 055-asset-register-crud | **Date**: 2026-09-15

## R1: Asset Entity Field Mapping

**Decision**: Use existing `Asset.cs` entity as-is (35 properties). No schema changes required.

**Rationale**: The entity already contains all fields needed for F2: Code, Name, Description, AssetGroupId, LocationId, FundId, CostCenterId, CustodianId, AssetTag, Barcode, SerialNumber, ImageUrl, CurrencyCode, OriginalValue, AcquisitionCost, ResidualValue, RelinquishmentValue, AccumulatedDepreciation, CurrentValue, PurchaseDate, ActivationDate, DepreciationStartDate, LastDepreciationDate, Status, AcquisitionType, UsefulLifeYears, IsFullyDepreciated, Notes, IsActive, RowVersion.

**Alternatives considered**: Adding new fields — rejected because the entity already has 35 fields covering identification, financials, depreciation tracking, and lifecycle.

## R2: Status Field Type

**Decision**: Status remains a `string` property (not an enum). Allowed values defined in spec: Draft, Active, UnderMaintenance, Disposed, WrittenOff.

**Rationale**: The existing entity uses `string Status` with `HasMaxLength(50)`. Changing to enum would require migration + all downstream features (F3–F9) to align. String-based validation in FluentValidation is sufficient and matches the established pattern.

**Alternatives considered**: Converting to `AssetStatus` enum — rejected because it would require migration, affect all 9 Asset module DbSets, and break the existing pattern where Status is a free-form string validated at the application layer.

## R3: AcquisitionType Field Type

**Decision**: AcquisitionType remains a `string` property. Allowed values: Purchase, Grant, Transfer, Donation, Inherited.

**Rationale**: Same reasoning as R2 — existing pattern is string-based validation.

## R4: Deactivation Permission

**Decision**: Deactivation uses `Assets.Update` permission (not a separate `Assets.Delete`).

**Rationale**: `AssetsDelete` does not exist in PermissionCodes. Deactivation is a soft-delete (status change to Disposed), which is semantically an update. This aligns with the AssetGroups pattern where deactivation is a separate permission but assets don't have one.

**Alternatives considered**: Adding `Assets.Delete` permission — rejected because it would require seeding a new permission, updating the permission matrix, and the operation is semantically an update (status transition).

## R5: DocumentSequenceService Integration

**Decision**: Use `IDocumentSequenceService.GenerateNextNumberAsync("Asset")` in `CreateAssetCommandHandler`.

**Rationale**: The "AST" prefix is already registered in the PrefixMap (line ~48 of DocumentSequenceService.cs). The service uses RowVersion-based optimistic concurrency for sequence generation. Error handling wraps `DocumentSequenceException` → returns `Result.Failure`.

**Alternatives considered**: Manual code generation — rejected because the centralized service handles concurrency and is the established pattern (used by Items, Journals, etc.).

## R6: Frontend Pattern Alignment

**Decision**: Follow the inventory items pattern exactly: types.ts, schemas.ts, hooks/useAssets.ts, AssetForm.tsx, page components.

**Rationale**: The inventory items CRUD is the closest existing pattern. It demonstrates: React Query hooks, Zod validation, reusable form (create/edit/read-only modes), skeleton loading, DataGrid with filters, RTL-first layout. Reusing this pattern minimizes new conventions.

**Alternatives considered**: Creating a new pattern — rejected because it would violate Principle X (shared UI patterns) and increase maintenance burden.

## R7: Field Locking After Activation

**Decision**: Implement field locking in the frontend form component based on asset status. Server-side validation in `UpdateAssetCommandHandler` enforces the same rules.

**Rationale**: FR-004 requires OriginalValue, PurchaseDate, and AssetGroup to become read-only when status is Active or beyond. The frontend locks the fields visually; the backend rejects updates to locked fields for defense-in-depth.

**Alternatives considered**: Frontend-only locking — rejected because it violates Principle III (server-side enforcement).

## R8: Default List Filter

**Decision**: `GetAssetsQuery` defaults to `Status = "Active"` unless `Status` filter is explicitly provided or `ShowAll = true`.

**Rationale**: Spec clarification confirmed Active-only default with "Show All" toggle. This matches the Items pattern where `IsActive = true` is the default.

## R9: Error Codes

**Decision**: Add new error codes to `ErrorCodes.Assets` for asset-specific operations.

**New codes needed**:
- `Assets.AssetNotFound` — asset not found by ID
- `Assets.DuplicateAssetTag` — AssetTag uniqueness violation
- `Assets.DuplicateBarcode` — Barcode uniqueness violation
- `Assets.DuplicateSerialNumber` — SerialNumber uniqueness violation
- `Assets.InvalidStatusTransition` — Status transition not allowed
- `Assets.FieldLockedAfterActivation` — Attempting to edit locked field
- `Assets.CannotDeactivate` — Deactivation blocked (e.g., pending depreciation)

**Rationale**: Follows the existing pattern of domain-specific error codes with `ErrorCategory` classification.
