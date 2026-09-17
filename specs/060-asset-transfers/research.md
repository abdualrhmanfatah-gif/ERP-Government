# Research: Asset Transfers

## R1 — Idempotent execution (058 FR-122)

**Question**: How does a repeated execute request return the same result without a second effect?

**Decision**: Execute loads the transaction. If status is already `Executed`, return `Result.Success(transaction.Id)` immediately — no token checks, no writes. A "double-submit" therefore always resolves to success, while a genuine second transfer requires a new draft. If status is `Cancelled`, return a validation failure (`Assets.InvalidStatusTransition`). Only `Draft` proceeds with token checks and writes.

**Rationale**: The execute request carries no data inputs (the destination lives in the draft), so an already-executed document is unambiguously the same effect. This is deterministic, testable with EF InMemory, and matches FR-122.

## R2 — Concurrency verification (058 FR-121)

**Question**: How to verify header AND card tokens under EF InMemory (tests) and SQL Server (runtime)?

**Decision**: Two layers:
1. Manual comparison: request tokens vs loaded entity `RowVersion` (`SequenceEqual`). Mismatch → `Assets.TransferSourceChanged`… no — mismatch → `ErrorCodes.Request.ConcurrencyConflict` (Conflict category, 409) with an Arabic message naming the document.
2. Set `Entry(entity).Property(e => e.RowVersion).OriginalValue = request.RowVersion` for header and card, and catch `DbUpdateConcurrencyException` → same conflict result. This closes the load-to-save race on SQL Server.

**Rationale**: The manual layer is the only one EF InMemory can exercise; the EF token layer is real-DB protection. Same pattern exists in `UpdateAssetGroupCommand`/`ToggleAssetGroupActiveCommand` (catch) — this extends it with the pre-check for testability.

## R3 — Stale-source re-validation (058 FR-052)

**Question**: What exactly is compared at execution?

**Decision**: Compare the draft detail's `FromLocationId` and `FromEmployeeId` with the card's current `LocationId` and `EmployeeId`. Any difference → `Assets.TransferSourceChanged` (Conflict, 409, Arabic message asking to reload the draft). Recovery path: editing the draft re-snapshots from-values from the card (FR-011 in spec.md), so the user can re-save and execute.

**Rationale**: These are the only card fields a transfer asserts; the card token check (R2) already covers unrelated concurrent edits.

## R4 — Destination validation (Q2 decision)

**Question**: What makes a destination valid?

**Decision** (server-side, in a shared `AssetTransferRules` helper used by create and update):
1. At least one of `ToLocationId` / `ToEmployeeId` is provided → else `Assets.InvalidTransferDestination` (Validation).
2. At least one provided value differs from the card's current value → else `Assets.InvalidTransferDestination` ("لا يوجد تغيير").
3. Provided location exists and `IsActive` → else `ErrorCodes.Inventory.LocationNotFound` / `LocationInactive`.
4. Provided employee exists and `IsActive` → else `ErrorCodes.Organization.EmployeeNotFound` (existing code; verify name at implementation) / a validation message.

**Rationale**: Q2 chose "destination mandatory". Distinguishing "equal to current" from "missing" gives precise Arabic messages, and reuses existing error codes for referenced entities.

## R5 — Department snapshots (058 FR-038b)

**Question**: When and from where are department snapshots taken?

**Decision**: Always at execution:
- `FromDepartmentId` = department of the card's custodian employee at execution (null if none).
- `ToDepartmentId` = department of the destination custodian employee when provided; otherwise the department of the card's (unchanged) custodian employee.
- `FromLocationId` / `FromEmployeeId` stay the draft's saved from-values (which execution has just verified still match the card).
- `OccurredAt` = `DateTimeOffset.UtcNow` at execution.

**Rationale**: Matches the existing `TransferDepartmentSnapshotTests` expectations (departments re-snapshot at execution; locations/custodian from draft values) while adding the occurrence-time update required by FR-050.

## R6 — Status model

**Question**: Cancel needs a terminal "cancelled" state; the enum lacks one.

**Decision**: Add `Cancelled = 7` to `AssetTransactionStatus`. Int column, no check constraint → no migration. `Approved` remains unused for transfers (D1 — execute is the approval).

## R7 — Frontend lookups

**Question**: What endpoints feed the destination pickers?

**Decision**: Reuse existing list endpoints: `GET /api/Locations` (Inventory, created by the in-flight locations feature) for locations, `GET /api/Employees` for custodian employees, `GET /api/Assets?search=&status=Active` for the asset picker. Reuse the custom `@/shared/api` client and TanStack Query hooks per the disposals pattern (no hand-written nswag dependency for these screens — matches current asset pages).
