# Feature Specification: Asset Transfers (نقل الأصول)

**Feature Branch**: `060-asset-transfers`

**Created**: 2026-09-16

**Status**: Draft

**Input**: User description: "ابدا بتنفيذ الميزه" — تنفيذ ميزة إدارة نقل الأصول حسب دراسة `plan/feature-context-asset-transfers.md` (القرارات محسومة) و`specs/058-asset-module-spec/spec.md` (US3 + FR-040…053 + FR-120…122)

## Decisions Inherited (محسومة)

| # | القرار | المصدر |
|---|--------|--------|
| D1 | التنفيذ هو الاعتماد: `Draft → Executed` مباشرة، بلا حالة Approved وبلا صلاحية Approve للنقل | Q1 |
| D2 | الوجهة إلزامية: يجب توفير موقع أو حارس جديد على الأقل، ويجب أن يختلف عن الحالة الحالية | Q2 |
| D3 | تعديل المسودة وإلغاؤها ضمن v1 | Q3 |
| D4 | المصطلح المعتمد في الواجهة «نقل الأصول» | Q4 |
| D5 | لقطة القسم من موظف الحارس: `FromDepartment` عند التنفيذ، `ToDepartment` من حارس الوجهة (أو الحارس الحالي عند عدم تغييره) | 058 FR-038b |
| D6 | لا قيد محاسبي للنقل: `IsPosted=false` و`JournalEntryId=null` دائماً | 058 |
| D7 | أصل واحد لكل مستند | 058 FR-002 |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Execute a Transfer (Priority: P1)

As an **asset custodian administrator (أمين الأصول)**, I want to create a transfer draft from an asset card (from-values pre-filled from the card) and execute it, so that the card reflects the destination and history preserves the source.

**Why this priority**: Core of the feature — the most frequent operational transaction.

**Independent Test**: Create a draft for an active asset, execute it, verify card update + snapshot values + no journal entry; re-transfer the same asset and verify history retention.

**Acceptance Scenarios**:

1. **Given** an active asset with location/custodian, **When** the user opens a new transfer, **Then** the system pre-fills "من" values from the card and the draft saves with `Draft` status and no change to the asset
2. **Given** a draft with destination (new location or custodian different from current), **When** executed, **Then** the transfer becomes `Executed`, `OccurredAt` is set at execution time, from/to values and department snapshots are final, and the card's location/custodian are updated
3. **Given** an executed transfer, **When** execution is repeated (double-submit/retry), **Then** the same success result returns and no second effect occurs (idempotent)
4. **Given** a draft, **When** another user edits the asset (location/custodian) before execution, **Then** execution is rejected with a conflict and the draft must be reloaded/edited — no silent overwrite
5. **Given** a transfer of an asset with a custodian employee, **When** executed, **Then** `FromDepartmentId`/`ToDepartmentId` are snapshotted from the custodian employees' departments at execution and never change later
6. **Given** a completed transfer, **When** the asset history is viewed, **Then** the transfer appears with its number, date, status, from/to values and no journal link

### User Story 2 - Manage the Draft (Priority: P2)

As an **asset custodian administrator**, I want to edit or cancel a transfer draft, so that mistakes are corrected before execution and abandoned documents are closed.

**Independent Test**: Edit a draft's destination, cancel a draft, verify non-draft documents reject both.

**Acceptance Scenarios**:

1. **Given** a draft, **When** edited (date/destination/notes), **Then** from-values are re-snapshotted from the card and the draft updates with optimistic concurrency (`RowVersion`)
2. **Given** a draft, **When** cancelled, **Then** status becomes `Cancelled` and no card effect occurs
3. **Given** an executed or cancelled transfer, **When** edit/cancel is attempted, **Then** it is rejected

### User Story 3 - Browse and Review Transfers (Priority: P2)

As an **auditor (مراجع)**, I want to list and open transfers with search and filters, so that movements are traceable.

**Independent Test**: Create several transfers, filter by status and search by number/asset, open a detail and verify all snapshot values and row versions are shown.

**Acceptance Scenarios**:

1. **Given** transfers exist, **When** the list is opened, **Then** it shows number, asset, date, from/to locations and status, paginated, newest first
2. **Given** the list, **When** searching by document number or asset code/name, **Then** results filter accordingly; filtering by status works
3. **Given** a transfer detail, **When** opened, **Then** from/to location, custodian, department names, occurrence time, notes, and both concurrency tokens are available

### User Story 4 - Validation Guards (Priority: P1)

As the system owner, I want transfer rules enforced server-side, so that no invalid movement corrupts the register.

**Acceptance Scenarios**:

1. **Given** a create/update with no destination fields, **Then** validation fails with a field-level Arabic error
2. **Given** a create/update whose destination equals the asset's current location and custodian, **Then** validation fails ("لا يوجد تغيير")
3. **Given** a non-existent or inactive location or employee as destination, **Then** validation fails
4. **Given** a non-Active asset, **Then** creating a transfer fails
5. **Given** an invalid document number generation, **Then** no partial document is created

## Requirements *(mandatory)*

### Functional Requirements

**Document**
- **FR-001**: Transfers MUST be single-asset documents of `AssetTransactionType.Transfer` with a server-generated number (`AssetTransfer` sequence, prefix `TRF`) allocated at draft creation
- **FR-002**: Draft creation/editing MUST NOT change the asset card or the ledger (`IsPosted=false`, `JournalEntryId=null`)
- **FR-003**: Only an asset with status `Active` can be transferred; a non-Active asset MUST be rejected
- **FR-004**: Destination MUST include a new location and/or a new custodian employee; at least one provided value MUST differ from the asset's current value (D2)
- **FR-005**: A destination location/employee MUST exist and be active; otherwise validation fails
- **FR-006**: From-values (location, custodian) MUST be snapshotted from the asset card at draft save time; unchanged destination fields are filled with current values so "remove vs unchanged" stays distinguishable
- **FR-007**: Execution MUST be allowed only from `Draft`; it MUST verify the header token AND the asset card token; a mismatch MUST return a concurrency conflict (409)
- **FR-008**: Execution MUST re-validate that the card still matches the draft's from-values; a mismatch MUST return a conflict and MUST NOT overwrite (058 FR-052)
- **FR-009**: A repeat execution of an already `Executed` transfer MUST return the same success result without a second effect (058 FR-122)
- **FR-010**: Execution MUST set `OccurredAt` and department snapshots (`FromDepartmentId` = current custodian employee's department, `ToDepartmentId` = destination custodian employee's department or the current one when the custodian is unchanged), update the card (location + custodian), and persist everything in one consistent operation
- **FR-011**: Editing a draft MUST be allowed only from `Draft`, MUST re-snapshot from-values from the card, and MUST enforce the header token
- **FR-012**: Cancelling a draft MUST be allowed only from `Draft`, sets status `Cancelled`, and MUST NOT touch the card

**Queries & UI**
- **FR-013**: A paginated list endpoint MUST support search (document number, asset code, asset name) and status filter, ordered newest first
- **FR-014**: A detail endpoint MUST return header + transfer detail with names, occurrence time, notes, and both concurrency tokens (transfer, asset)
- **FR-015**: Screens MUST use the term «نقل الأصول» in navigation and labels, Arabic-first RTL, with create/edit/cancel/execute actions per status
- **FR-016**: Errors MUST follow the unified contract: stable codes, Arabic messages, 404 for missing transfer, 409 for concurrency/source conflicts

**Authorization**
- **FR-017**: Reads use `AssetTransfers.View`; create/edit/cancel use `AssetTransfers.Create`; execution uses `AssetTransfers.Execute` — no new permissions

### Key Entities

- **AssetTransaction** (existing) — unified header: number, asset, type, date, status, currency, notes, RowVersion. New enum member `Cancelled = 7`.
- **AssetTransferDetail** (existing) — occurrence time, from/to location, from/to employee, from/to department snapshots.

## Success Criteria *(mandatory)*

- **SC-001**: A transfer draft + execution runs end-to-end from the UI with card update and no ledger entries
- **SC-002**: 0 duplicate effects on repeated execution; 0 silent overwrites on concurrent edit (tests)
- **SC-003**: 100% of invalid destinations (missing/no-change/inactive) rejected server-side with Arabic field errors
- **SC-004**: Executed transfer appears in the asset history with no journal link
- **SC-005**: All new backend tests pass; frontend lint + build clean

## Assumptions

- No schema changes: the 13-table model from DEP-030 already contains `AssetTransactions` and `AssetTransferDetails`; only the `AssetTransactionStatus` enum gains `Cancelled = 7` (int column, no migration)
- Location/employee lookup endpoints exist for the destination pickers (`/api/Locations`, `/api/Employees`)
- Existing `GetAssetHistoryQuery` already surfaces transfers in the asset card history
- NSwag client regeneration is automatic via the frontend prebuild
