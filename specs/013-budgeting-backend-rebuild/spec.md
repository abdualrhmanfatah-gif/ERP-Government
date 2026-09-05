> STATUS: SUPERSEDED by specs/015-budgeting-backend-completion — 013's open tasks are re-covered by 015. Implemented work (71/122) stays in code.

# Feature Specification: Rebuild Budgeting Module Backend

**Feature Branch**: `013-budgeting-backend-rebuild`

**Created**: 2026-09-05

**Status**: Superseded by specs/015-budgeting-backend-completion

**Input**: User description: "Rebuild Budgeting module from scratch - 7 tables (BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance), enum statuses, zero derived/duplicate columns, computed availability, full backend rebuild"

## Context

Current Budgeting module uses string statuses, stored snapshot amounts, stale dimensions, and denormalized FK chains. Rebuild replaces entire schema. Principle: actual expenditure derives from posted Moves (journal entries), never stored on budget tables. Availability computed at transaction time (Constitution Principle III/V). All derived or duplicated columns REMOVED: no Level on Classification (computed), no IsReversed on Encumbrance (computed), no Fund/FiscalYear/Budget/BudgetItem denormalized FKs on Appropriation/Encumbrance (joined via Appropriation->Budget), no CreatedById (audit CreatedBy is source), no ApprovedById/At on entities (ApprovalHistory is source of truth per Principle VIII), no Budget.IsActive (Status covers Cancelled/Closed). Liquidations + BudgetLedgerEntries already deleted (prior spec).

Goal: Tear down and rebuild Budgeting: 7 tables, 8 enums, computed availability engine, derived-value projections, all Application layer, Web endpoints. All existing Budgeting data wiped — fresh start. Frontend delivered by 5 separate frontend specs (FE-1..FE-5) — out of scope here.

## Clarifications

### Session 2026-09-05

- Q: Which roles should receive which budgeting permissions — fixed role-to-permission mapping or permission codes only? → A: Fixed mapping with 5 roles (Admin, BudgetOfficer, Approver, ProcurementOfficer, Analyst) with explicit permission sets seeded.
- Q: Is Transfer AppropriationType restricted to Draft-only or usable at any Active state? → A: Transfer restricted to Draft only — once submitted, all fund movement uses Adjustment.
- Q: Where should Warning control method over-availability warnings be recorded? → A: Audit log — warning events recorded alongside authorization decisions per Constitution Principle VII.

## Scope

- **IN**: Drop + recreate 7 tables, 8 enums, availability engine, derived-field query projections (Level, IsReversed, contextual FKs, approval info), all Application commands/queries/DTOs, endpoints + permissions, DocumentSequence prefixes (BGT/APR/ENC) kept, seeds, unit + functional tests, OpenAPI regen, docs.
- **OUT**: FiscalYears, Currencies, Accounts, CostCenters, Suppliers, PurchaseOrders, Moves, PaymentOrders, other modules. No stored snapshots. No stored derived values. Frontend (5 separate specs).

**Data**: WIPE all existing Budgeting rows. Fresh idempotent seed after rebuild (Principle VI).

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Budget Master Data Management (Priority: P1)

A budget administrator defines the master data governing the budget cycle: budget types (with control method and overrun policy), funds, and hierarchical budget classifications.

**Why this priority**: Master data is prerequisite for all downstream operations. Budgets reference BudgetType/FiscalYear/Fund; BudgetItems reference Classifications. Without masters, no budget can be created.

**Independent Test**: Can be fully tested by creating BudgetTypes with distinct ControlMethod values, Funds with FundType/FundCategory, and multi-level Classifications and verifying Level is computed via hierarchy traversal and IsActive toggling works. All three entities are independently CRUD-verifiable.

**Acceptance Scenarios**:

1. **Given** an administrator creates a BudgetType with Code "CAPEX", Name "Capital", ControlMethod Blocking and AllowOverrun false, **When** the record is saved, **Then** it is retrievable with those values and a RowVersion for concurrency.
2. **Given** an administrator creates a BudgetClassification hierarchy Parent "1000" -> Child "1100" -> Grandchild "1110", **When** the hierarchy is queried, **Then** the DTO for "1110" reports Level 3 (computed, not stored) and parent linkage is correct; deep hierarchy (5+ levels) computes without overflow.
3. **Given** a BudgetClassification with existing BudgetItems, **When** an update violates unique Code, **Then** the system returns an explicit result failure and no duplicate is created.
4. **Given** a Fund with FiscalYearId referencing an existing FiscalYear, **When** the Fund is created, **Then** it stores the legal authority, default revenue account, and IsActive state.

---

### User Story 2 — Budget and BudgetItem Lifecycle with Tree (Priority: P1)

A budget officer creates a Budget for a fiscal year, defines its line items as a tree (e.g., Projects -> CostCenters -> Accounts), and progresses the Budget through its lifecycle states.

**Why this priority**: Budgets and BudgetItems are the allocation containers for all appropriations. Every appropriation must reference a valid Budget+BudgetItem. The lifecycle governs when appropriations may be created.

**Independent Test**: Can be fully tested by creating a Budget in Draft, submitting, approving, activating, suspending/reactivating, and closing/cancelling, and by building a BudgetItem tree with AllowOverrun inheritance (Item null -> Budget null -> BudgetType concrete). No appropriation or encumbrance is needed to verify the lifecycle and tree.

**Acceptance Scenarios**:

1. **Given** a Budget in Draft with TotalAmount 1,000,000 and BudgetType AllowOverrun false, **When** the officer submits it, **Then** Status becomes Submitted and the transition is recorded in ApprovalHistory with an evaluation snapshot.
2. **Given** a Budget in Submitted, **When** an approver approves it, **Then** Status becomes Approved; only Approved budgets can be Activated.
3. **Given** a BudgetItem with BudgetId 1, ItemCode "ITM-001", and AllowOverrun null, Budget.AllowOverrun null, BudgetType.AllowOverrun false, **When** the effective overrun policy is queried, **Then** the system resolves to false (three-level null-inherit chain).
4. **Given** a Budget in Active with child BudgetItems forming a 3-level tree, **When** the tree is queried, **Then** each node reports its computed Level and the unique constraint (BudgetId, ItemCode) is enforced.

---

### User Story 3 — Appropriation Management with Computed Availability (Priority: P1)

A budget officer creates appropriations (Original, Supplement, Reduction, Adjustment) against a BudgetItem, and the system computes availability for that item at transaction time. Transfer type is restricted to Draft status only for initial fund allocation between BudgetItems; post-submission fund movement uses Adjustment. Update and Delete are allowed only while the row is Draft.

**Why this priority**: Appropriations are the authorization to spend. Availability determines whether new appropriations are allowed and must be computed from live data (Principle III/V), not stored snapshots.

**Independent Test**: Can be fully tested by creating multiple appropriations against the same BudgetItem with different types and verifying `AvailableForAppropriation(BudgetItemId)` equals sum(Active Original/Supplement) - sum(Active Reduction) + sum(Active Adjustment signed Amount: positive Adjustments add availability, negative Adjustments subtract it) to two decimals, and that Blocking control rejects over-availability while Warning allows with a logged warning.

**Acceptance Scenarios**:

1. **Given** a BudgetItem with existing Original 100,000 and Supplement 20,000 appropriations (both Active), **When** a user attempts a Reduction of 130,000, **Then** the system rejects with an explicit failure "exceeds net available" (Blocking mode) computed at transaction time.
2. **Given** a Budget whose BudgetType has ControlMethod Warning, **When** an appropriation exceeds available amount, **Then** the operation succeeds but a warning is logged to the audit log and surfaced in the availability indicator (amber).
3. **Given** an appropriation in Draft, **When** it is submitted and approved, **Then** Status transitions Draft->PendingApproval->Approved->Active are each guarded server-side and approval decisions are recorded in ApprovalHistory.
4. **Given** an appropriation with BudgetId and BudgetItemId, **When** its DTO is queried, **Then** Fund and FiscalYear context is projected via join through Budget (no denormalized FundId/FiscalYearId stored on Appropriation).

---

### User Story 4 — Encumbrance Lifecycle with Availability Engine (Priority: P1)

A procurement or budget officer creates encumbrances against an appropriation, progresses them through PartiallyReleased/PartiallyLiquidated states, and reverses them. The appropriationId supplied at creation is used to resolve the BudgetItem; the system enforces availability against the whole BudgetItem net total via the BudgetType control method.

**Why this priority**: Encumbrances reserve budget before payment. Incorrect availability computation leads to over-commitment. This is the core budget-control enforcement point (Principle V).

**Independent Test**: Can be fully tested by creating encumbrances under appropriations for a BudgetItem with known net appropriated amount, verifying `AvailableForEncumbrance(AppropriationId)` resolves the BudgetItem from the supplied AppropriationId and equals the whole-BudgetItem net minus the sum of Active/PartiallyReleased/PartiallyLiquidated encumbrances for that BudgetItem where ReversalOfId is null, and that Blocking rejects while Warning allows.

**Acceptance Scenarios**:

1. **Given** a BudgetItem with net appropriated 50,000 and an existing Active encumbrance of 30,000 under one of its appropriations (ReversalOfId null), **When** a new encumbrance of 25,000 is created against another appropriation for the same BudgetItem and the budget's effective BudgetType ControlMethod is Blocking, **Then** the creation is rejected with an explicit availability failure.
2. **Given** an Active encumbrance, **When** it is partially released, **Then** Status transitions Active->PartiallyReleased and the released amount is reflected in subsequent availability computations.
3. **Given** an encumbrance with ReversalOfId pointing to another encumbrance, **When** the original encumbrance's DTO is queried, **Then** IsReversed projects as true via existence check (never stored) and availability is restored by excluding the reversed amount.
4. **Given** an encumbrance referencing a Suspended appropriation, **When** creation is attempted, **Then** the system rejects with "appropriation not active".
5. **Given** a closed FiscalYear (joined via Appropriation->Budget->FiscalYear), **When** a new appropriation or encumbrance is attempted for that fiscal period, **Then** the system rejects.

---

### User Story 5 — Derived-Value Projections and Contextual Joins (Priority: P2)

A budget analyst views any budgeting record and sees contextual information that is never stored but computed via joins and history queries.

**Why this priority**: The rebuild principle forbids stored derived values. All contextual data must be projected correctly for the system to be usable; until projections work, detail pages are incomplete.

**Independent Test**: Can be fully tested by querying an Encumbrance and a BudgetClassification and verifying that Level, IsReversed, Fund/FiscalYear/BudgetItem context, latest ApprovalHistory decision, and creator identity are all present and match computed truth without any of those columns being stored on the entity table.

**Acceptance Scenarios**:

1. **Given** a BudgetClassification "1110" nested 3 levels deep, **When** its DTO is queried, **Then** Level is computed as 3 via recursive hierarchy traversal and exposed as a field.
2. **Given** an Encumbrance that has been reversed (another Encumbrance row points to it via ReversalOfId), **When** the original Encumbrance DTO is queried, **Then** IsReversed is true (computed via existence check, not stored).
3. **Given** an Encumbrance referencing an Appropriation, **When** the Encumbrance DTO is queried, **Then** Budget, BudgetItem, Fund, and FiscalYear context is projected through the Appropriation->Budget join path.
4. **Given** a document with an ApprovalHistory entry, **When** the associated budgeting entity DTO is queried, **Then** the latest approval decision (DecisionAt DESC) is included.
5. **Given** any budgeting entity, **When** its DTO is queried, **Then** the creator identity is available from the audit CreatedBy field.

---

### User Story 6 — Permissions and Security (Priority: P2)

System enforces role-based access control on every budgeting endpoint with a fixed role-permission mapping. Five roles are seeded: Admin (all permissions), BudgetOfficer (Budgets CRUD + Submit, BudgetItems, Appropriations Create/Read/Update, Classifications/Funds/Types Read), Approver (Budgets Submit/Approve/Suspend/Close/Cancel, Appropriations Approve/Suspend/Close/Cancel, Encumbrances Approve/Cancel), ProcurementOfficer (Encumbrances full access), Analyst (all Read permissions). Anonymous requests are denied. Wrong permissions return 403. All authorization decisions are audited.

**Why this priority**: Security is non-negotiable but does not block core functionality testing. Permissions layer wraps the business logic tested in P1 stories.

**Independent Test**: Can be fully tested by sending unauthenticated requests (expect 401) and requests with wrong permissions (expect 403) to all endpoints, and verifying audit logs record authorization decisions.

**Acceptance Scenarios**:

1. **Given** an unauthenticated request, **When** any budgeting endpoint is called, **Then** the system returns 401 Unauthorized.
2. **Given** a user without "Budgets.Create" permission (e.g., Analyst role), **When** POST /api/Budgets is called, **Then** the system returns 403 Forbidden.
3. **Given** a valid user with correct permission, **When** a budgeting action is performed, **Then** the authorization decision is recorded in the audit log.

---

### User Story 7 — Migration and Data Wipe (Priority: P1)

System performs a single migration that drops all existing Budgeting tables, recreates them per the new schema, preserves DocumentSequence rows for BGT/APR/ENC prefixes, and reseeds reference data idempotently.

**Why this priority**: Without migration, the new schema cannot be deployed. This is a prerequisite for all other stories.

**Independent Test**: Can be fully tested by running the migration against a database with existing Budgeting data, verifying all old data is wiped, new tables exist with correct schema, DocumentSequence BGT/APR/ENC rows are preserved, and seeds are idempotent.

**Acceptance Scenarios**:

1. **Given** a database with existing Budgeting tables and data, **When** the migration runs, **Then** all 7 old tables are dropped, 7 new tables are created per schema, and all old Budgeting rows are gone.
2. **Given** DocumentSequence rows with prefixes BGT, APR, ENC, **When** the migration completes, **Then** those rows are preserved and functional.
3. **Given** the migration runs twice, **When** the second run completes, **Then** the schema is identical (idempotent) and reference data seeds are not duplicated.

---

### Edge Cases

- Reduction exceeding net available: reject explicit failure.
- Encumbrance on Suspended appropriation: reject.
- AllowOverrun chain: Item null + Budget null -> BudgetType concrete value decides.
- Appropriation reversal: new row with type Adjustment (negative Amount); original row unchanged. Transfer restricted to Draft only.
- Reversal: new Encumbrance row ReversalOfId->original; original shows IsReversed=true via projection; availability restored.
- FiscalYear closed (join via Budget): no new Appropriations/Encumbrances.
- Deep Classification hierarchy (5+ levels): Level computed correctly, no recursion overflow.
- ApprovalHistory missing for legacy rows post-wipe: fresh start — no legacy.
- Concurrency conflict on RowVersion: reject with explicit failure.
- Unique constraint violation on BudgetCode/Number: reject explicit failure.
- Warning control method: over-availability logged to audit log, operation proceeds.

## Requirements *(mandatory)*

### Functional Requirements — Schema

- **FR-001 (BudgetType)**: System MUST store budget types with Code (unique, 50 chars), Name (200 chars), Description (nullable, 500 chars), ControlMethod enum (None=0, Warning=1, Blocking=2), AllowOverrun bool (default false, concrete root of override chain), IsActive (default true), RowVersion, and audit fields. System MUST NOT store OverrunRequiresApproval or IsSystemType.
- **FR-002 (Fund)**: System MUST store funds with FundNumber (unique), FundName, FundType enum (General/Special/Project), FundCategory enum (Operating/Capital), FiscalYearId (nullable FK to FiscalYears), LegalAuthority, Description (nullable), DefaultRevenueDebitAccountId (nullable FK to Accounts), IsActive, RowVersion, and audit fields.
- **FR-003 (BudgetClassification)**: System MUST store budget classifications with Code (unique), Name, ParentId (nullable self-referencing FK, Restrict), IsActive, RowVersion, and audit fields. System MUST NOT store Level — it MUST be computed at query time via recursive hierarchy traversal and exposed in DTOs.
- **FR-004 (Budget)**: System MUST store budgets with BudgetNumber (unique, BGT sequence), BudgetName, BudgetTypeId FK, FiscalYearId FK, FundId FK, TotalAmount (decimal 23,2), Status enum (Draft/Submitted/Approved/Active/Suspended/Closed/Cancelled), AllowOverrun (nullable bool, null inherits from BudgetType), EffectiveFrom (DateOnly), EffectiveTo (nullable DateOnly), Description (nullable), RowVersion, and audit fields. System MUST NOT store snapshot amounts, ControlMethod, OverrunRequiresApproval, ApprovedById/At, or IsActive.
- **FR-005 (BudgetItem)**: System MUST store budget items with ItemCode, ItemName, BudgetId FK, ParentId (nullable self-referencing FK, Restrict), AccountId (nullable FK), FundId (nullable FK), CostCenterId (nullable FK), BudgetClassificationId (nullable FK), AllowOverrun (nullable bool, null inherits Budget.AllowOverrun ?? BudgetType.AllowOverrun), IsActive, RowVersion, and audit fields. Unique constraint on (BudgetId, ItemCode). System MUST NOT store amounts, Percentage, ControlLevel, IsMandatory, ProjectId, OrganizationUnitId, or Level.
- **FR-006 (Appropriation)**: System MUST store appropriations with AppropriationNumber (unique, APR sequence), BudgetId FK, BudgetItemId FK, AppropriationType enum (Original/Supplement/Reduction/Transfer/Adjustment), DocumentType (50 chars), DocumentId (int), Amount (decimal 23,2), Status enum (Draft/PendingApproval/Approved/Active/Suspended/Closed/Cancelled/Reversed), RowVersion, and audit fields. Transfer type is restricted to Draft status only — once submitted, all fund movement uses Adjustment. System MUST NOT store FundId, FiscalYearId, CreatedById, ApprovedById/At, snapshot amounts, IsReversed, ReversalOfId, or ReversalReason. Fund/FiscalYear/BudgetItem context MUST be exposed via join projection in DTOs.
- **FR-007 (Encumbrance)**: System MUST store encumbrances with EncumbranceNumber (unique, ENC sequence), EncumbranceType enum (Commitment/Obligational/Contractual/Advance/Adjustment), AppropriationId FK (single source of budget context), VendorId (nullable FK to Suppliers), PurchaseOrderId (nullable FK to PurchaseOrders), DocumentType (50 chars), DocumentId (int), Description (nullable), EncumbranceDate (DateOnly), Amount (decimal 23,2), Status enum (Draft/PendingApproval/Approved/Active/PartiallyReleased/PartiallyLiquidated/FullyLiquidated/Closed/Cancelled/Reversed), ReversalOfId (nullable self-referencing FK, Restrict — set on reversal row, points to original), ReversalReason (nullable, 500 chars — stored on reversal row only), RowVersion, and audit fields. System MUST NOT store FundId, FiscalYearId, BudgetId, BudgetItemId, IsReversed, CreatedById, or ApprovedById/At. Budget/BudgetItem/Fund/FiscalYear context MUST be projected via join through Appropriation->Budget.
- **FR-008 (Derived Value Projections)**: System MUST expose derived values via query projections, never stored: Classification/BudgetItem Level (recursive hierarchy), Encumbrance IsReversed (existence check: exists row where ReversalOfId = this.Id), Encumbrance Budget+BudgetItem+Fund+FiscalYear context (join through Appropriation->Budget), latest approval decision per document (ApprovalHistory query by DocumentType+DocumentId, ordered by DecisionAt DESC), creator identity (audit CreatedBy). DTOs MUST include these as computed fields.

### Functional Requirements — Availability Engine

- **FR-009 (Availability Computation)**: System MUST compute AvailableForAppropriation(BudgetItemId) = sum(Appropriation.Amount where BudgetItemId, Status=Active, type in Original/Supplement/Transfer-in) - sum(type in Reduction/Transfer-out). System MUST compute AvailableForEncumbrance(AppropriationId) = netAppropriated - sum(Encumbrance.Amount where AppropriationId, Status in Active/PartiallyReleased/PartiallyLiquidated, ReversalOfId null). ControlMethod from BudgetType: None = skip check, Warning = log to audit log + allow, Blocking = reject with explicit failure. AllowOverrun resolution chain: BudgetItem.AllowOverrun ?? Budget.AllowOverrun ?? BudgetType.AllowOverrun. All computed at transaction time from current data (Principle III). Fund/FiscalYear filtering via join through Appropriation->Budget.

### Functional Requirements — Lifecycles

- **FR-010 (Budget FSM)**: Draft->Submitted->Approved->Active->Suspended<->Active->Closed; Cancelled from Draft/Submitted/Suspended. Every transition server-side guarded with explicit result failures.
- **FR-011 (Appropriation FSM)**: Draft->PendingApproval->Approved->Active->Suspended/Closed/Cancelled; Reversed from Active (new reversal row with type Adjustment, negative Amount). Every transition server-side guarded.
- **FR-012 (Encumbrance FSM)**: Draft->PendingApproval->Approved->Active->PartiallyReleased->PartiallyLiquidated->FullyLiquidated->Closed; Cancelled from Draft/PendingApproval; Reversed from Active/PartiallyReleased/PartiallyLiquidated (reversal row + reason). Every transition server-side guarded.
- **FR-013 (Approval Recording)**: Every approval transition MUST record the decision in ApprovalHistory with an evaluation snapshot (Principle VIII).

### Functional Requirements — Application Layer

- **FR-014 (Commands/Queries)**: System MUST provide: Budgets (CRUD + Submit/Approve/Activate/Suspend/Close/Cancel), BudgetItems (CRUD + tree), Appropriations (Create/Update/Delete in Draft only / Approve/Activate/Suspend/Close/Cancel/Reverse), Encumbrances (Create/Approve/Activate/Release/Liquidate transitions/Cancel/Reverse + availability check on create/approve), BudgetTypes/Funds/Classifications (CRUD + toggle IsActive).
- **FR-015 (Validation)**: All commands MUST use FluentValidation. All endpoints MUST declare [Authorize(Policy=...)]. RowVersion concurrency MUST be enforced. Availability checks MUST be server-side.
- **FR-016 (Permissions)**: System MUST enforce permissions: Budgets.*, Appropriations.*, Encumbrances.*, Funds.*, BudgetClassifications.*, BudgetTypes.*. Clean seed, no legacy permissions. Fixed role-permission mapping seeded: Admin (all permissions), BudgetOfficer (Budgets.Create/Read/Update, BudgetItems.*, Appropriations.Create/Read/Update, BudgetClassifications.Read, Funds.Read, BudgetTypes.Read), Approver (Budgets.Submit/Approve/Suspend/Close/Cancel, Appropriations.Approve/Suspend/Close/Cancel, Encumbrances.Approve/Cancel), ProcurementOfficer (Encumbrances.*), Analyst (all Read permissions). Anonymous denied. Authorization decisions audited (Principle VII).
- **FR-017 (Endpoints)**: System MUST expose REST endpoints: /api/BudgetTypes, /api/Funds, /api/BudgetClassifications, /api/Budgets (+ items sub-routes), /api/Appropriations, /api/Encumbrances, GET /api/Encumbrances/availability?appropriationId={id}. ProblemDetails contract. OpenAPI spec regenerated.

### Functional Requirements — Migration & Tests

- **FR-018 (Migration)**: Single migration: drop 7 tables (wipe), recreate per new schema, add indexes (unique Code/Number columns, FK indexes on AppropriationId/BudgetId/BudgetItemId join paths), keep DocumentSequence BGT/APR/ENC rows, reseed reference data idempotent. Down migration restores prior schema (data loss accepted per wipe decision).
- **FR-019 (Tests)**: Unit tests per command (success + failure paths). Functional tests against real database: lifecycle transitions, availability accuracy (Original+Supplement-Reduction-Adjustment nets), Blocking rejection, Warning allow+audit log, AllowOverrun null-inherit chain, Level computation deep hierarchy, IsReversed computation, concurrency conflicts, ApprovalHistory snapshots. No stub tests (Principle XI).
- **FR-020 (Docs)**: Update docs/database-schema.md (Budgeting tables rewritten, removed columns noted), feature registry (chain: Budget -> Appropriation -> Encumbrance, computed availability), Constitution compliance table (Principle VI: no denormalized derived data).

### Key Entities

- **BudgetType**: Defines budget category with control method (None/Warning/Blocking) and overrun policy. Root of AllowOverrun inheritance chain.
- **Fund**: Financial fund with type (General/Special/Project), category (Operating/Capital), optional FiscalYear link, legal authority, and default revenue account.
- **BudgetClassification**: Hierarchical classification tree (self-referencing parent). Level computed at query time, never stored.
- **Budget**: Top-level budget container linking BudgetType, FiscalYear, Fund. Has lifecycle (Draft through Closed/Cancelled) and total amount.
- **BudgetItem**: Line items within a Budget forming a tree. Links to Account, Fund, CostCenter, Classification. AllowOverrun inherits through chain.
- **Appropriation**: Authorization to spend against a BudgetItem. Typed (Original/Supplement/Reduction/Transfer/Adjustment). Transfer restricted to Draft only. Lifecycle governed. Fund/FiscalYear context via join.
- **Encumbrance**: Reservation of funds against an Appropriation. Typed (Commitment/Obligational/Contractual/Advance/Adjustment). Supports reversal via ReversalOfId. Budget context via join through Appropriation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Build produces zero warnings; all tests pass at 100%.
- **SC-002**: Grep for dropped columns (Level on entity, IsReversed stored, denormalized FKs on Appropriation/Encumbrance, ApprovedById/At + IsActive on Budget) returns zero matches in Domain entities.
- **SC-003**: Availability nets verified to 2 decimal places including Supplement, Reduction, Transfer, and Adjustment types.
- **SC-004**: Blocking control method rejects over-availability with explicit failure message.
- **SC-005**: AllowOverrun null-inherit chain tested across 3 levels (Item -> Budget -> BudgetType).
- **SC-006**: Level and IsReversed projections match computed truth in functional tests.
- **SC-007**: Anonymous requests return 401; wrong permissions return 403 on all endpoints.

## Assumptions

- DocumentSequence prefixes BGT/APR/ENC exist or are created as part of this spec.
- Suppliers table exists (VendorId FK on Encumbrance).
- ApprovalHistory table exists and serves as approval source of truth (Constitution Principle VIII).
- FiscalYears, Accounts, CostCenters tables exist (FK targets).
- Data wipe is accepted — no production data preservation required.
- PostingPipeline integration is out of scope for v1 (future work).
- Frontend is delivered by 5 separate frontend specs — this spec covers backend only.
