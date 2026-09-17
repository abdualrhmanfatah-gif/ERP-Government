# Data Model: Asset Transfers

No new tables and no migration. The 13-table model from DEP-030 already carries the transfer structures; this feature adds one enum member and DTOs.

## Domain changes

### `AssetTransactionStatus` (src/Domain/Assets/Enums/AssetTransactionStatus.cs)

```csharp
Draft = 0,
Approved = 1,   // unused by transfers (D1)
Executed = 2,   // terminal success for transfers
Posting = 3,
Posted = 4,
PostingFailed = 5,
Reversed = 6,
Cancelled = 7   // NEW — terminal, no card effect
```

Additive int member; the column has no check constraint → no EF migration required.

## Existing entities used as-is

### `AssetTransaction` (header)

| Field | Transfer use |
|-------|--------------|
| `TransactionNumber` | `TRF-{D6}` from `AssetTransfer` sequence |
| `AssetId` | single asset (D7) |
| `TransactionType` | `Transfer` |
| `TransactionDate` | effect date entered by the user |
| `Status` | Draft → Executed / Cancelled |
| `CurrencyId` | copy of the asset's currency (kept for the unified header) |
| `JournalEntryId` / `IsPosted` | always null / false (D6) |
| `Notes`, `RowVersion` | user notes; header concurrency token |

### `AssetTransferDetail` (1:1)

| Field | Semantics |
|-------|-----------|
| `OccurredAt` | set at execution (FR-010); draft value is provisional |
| `FromLocationId`, `FromEmployeeId` | snapshotted from the card at draft save; verified at execution (FR-008) |
| `ToLocationId`, `ToEmployeeId` | validated destination; unchanged fields carry current values (FR-006) |
| `FromDepartmentId` | custodian employee's department at execution (FR-010) |
| `ToDepartmentId` | destination custodian's department, or current custodian's when unchanged (D5) |

## DTOs (Application layer)

### `AssetTransferListItemResponse`
`Id, DocumentNumber, AssetId, AssetCode, AssetName, TransactionDate, Status, FromLocationName, ToLocationName, FromEmployeeName, ToEmployeeName`

### `AssetTransferDetailResponse`
Header/list fields plus:
`CurrencyId, Notes, OccurredAt, FromLocationId, FromEmployeeId, FromDepartmentId, FromDepartmentName, ToLocationId, ToEmployeeId, ToDepartmentId, ToDepartmentName, RowVersion, AssetRowVersion`

`RowVersion` and `AssetRowVersion` are base64 strings over JSON (byte[] default serializer) and are required by execute.

## Status transitions (transfers only)

```
Draft ──execute──▶ Executed
  │
  └──cancel──▶ Cancelled

Executed / Cancelled: terminal (edit/cancel/execute rejected; repeat execute = same success result)
```

## Validation rules (server-side)

| Rule | Error |
|------|-------|
| Asset exists | `Assets.AssetNotFound` (404) |
| Asset status Active (create/update; execute re-checked via source comparison) | `Assets.InvalidStatusTransition` |
| Destination present and different from current | `Assets.InvalidTransferDestination` |
| Destination location exists/active | `Inventory.LocationNotFound` / `Inventory.LocationInactive` |
| Destination employee exists/active | `Organization.EmployeeNotFound` (+ validation message) |
| Draft-only edit/cancel/execute | `Assets.InvalidStatusTransition` |
| Header/card token mismatch | `Request.ConcurrencyConflict` (409) |
| From-values no longer match the card | `Assets.TransferSourceChanged` (409) |
