# Feature Specification: Rebuild Budgeting Module from Scratch

**Feature Branch**: `010-rebuild-budgeting-module`

**Created**: 2026-09-04

**Status**: Draft

**Input**: User description: "Rebuild Budgeting module from scratch - 7 tables (BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance), enum statuses, zero derived/duplicate columns, computed availability, full backend + frontend rebuild"

## Clarifications

### Session 2026-09-04

- Q: How should an Appropriation reversal be represented if the table no longer has IsReversed, ReversalOfId, or ReversalReason columns? → A: New Appropriation row with reversal type (Adjustment/Reversal) — reversal is another typed amount that nets in availability, no reversal columns needed.
- Q: When an Appropriation has AppropriationType Transfer, how do you determine whether it adds to or subtracts from the BudgetItem's available amount? → A: Do not use Transfer; appropriation changes are handled through Adjustment, and records may be edited or deleted according to system rules. No TransferIn/TransferOut/Reversal. This narrows the earlier reversal-model answer: Adjustment only, no Reversal alternative for Appropriation.
- Q: When an encumbrance points to one Appropriation row but later Adjustment rows change the same budget line, which total should the encumbrance availability check use? → A: Whole BudgetItem net total — sum all Active Original/Supplement/Reduction/signed-Adjustment rows for the same BudgetItem.
- Q: In which Appropriation lifecycle states should Update and Delete be allowed? → A: Draft only for Update and Delete; after submit, use Adjustment or Cancel.
- Q: For an Appropriation row with type Adjustment, should Amount be allowed to be negative so the same type can increase or decrease availability? → A: Signed Adjustment Amount — positive adds availability, negative subtracts it.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Budget Master Data (BudgetType, Fund, BudgetClassification) (Priority: P1)

A budget administrator defines the master data that governs the entire budget cycle: budget types (with control method and overrun policy), funds, and hierarchical budget classifications.

**Why this priority**: Master data is prerequisite for all downstream operations. Budgets reference BudgetType/FiscalYear/Fund; BudgetItems reference Classifications. Without masters, no budget can be created.

**Independent Test**: Can be fully tested by creating BudgetTypes with distinct ControlMethod values, Funds with FundType/FundCategory, and multi-level Classifications and verifying Level is computed via hierarchy traversal and IsActive toggling works. All three entities are independently CRUD-verifiable.

**Acceptance Scenarios**:

1. **Given** an administrator creates a BudgetType with Code "CAPEX", Name "Capital", ControlMethod Blocking and AllowOverrun false, **When** the record is saved, **Then** it is retrievable with those values and a RowVersion for concurrency.
2. **Given** an administrator creates a BudgetClassification hierarchy Parent "1000" → Child "1100" → Grandchild "1110", **When** the hierarchy is queried, **Then** the DTO for "1110" reports Level 3 (computed, not stored) and parent linkage is correct; deep hierarchy (5+ levels) computes without overflow.
3. **Given** a BudgetClassification with existing BudgetItems, **When** an update violates unique Code, **Then** the system returns an explicit result failure and no duplicate is created.
4. **Given** a Fund with FiscalYearId referencing an existing FiscalYear, **When** the Fund is created, **Then** it stores the legal authority, default revenue account, and IsActive state.

---

### User Story 2 - Budget and BudgetItem Lifecycle with Tree (Priority: P1)

A budget officer creates a Budget for a fiscal year, defines its line items as a tree (e.g., Projects → CostCenters → Accounts), and progresses the Budget through its lifecycle states.

**Why this priority**: Budgets and BudgetItems are the allocation containers for all appropriations. Every appropriation must reference a valid Budget+BudgetItem. The lifecycle governs when appropriations may be created.

**Independent Test**: Can be fully tested by creating a Budget in Draft, submitting, approving, activating, suspending/reactivating, and closing/cancelling, and by building a BudgetItem tree with AllowOverrun inheritance (Item null → Budget null → BudgetType concrete). No appropriation or encumbrance is needed to verify the lifecycle and tree.

**Acceptance Scenarios**:

1. **Given** a Budget in Draft with TotalAmount 1,000,000 and BudgetType AllowOverrun false, **When** the officer submits it, **Then** Status becomes Submitted and the transition is recorded in ApprovalHistory with an evaluation snapshot.
2. **Given** a Budget in Submitted, **When** an approver approves it, **Then** Status becomes Approved; only Approved budgets can be Activated.
3. **Given** a BudgetItem with BudgetId 1, ItemCode "ITM-001", and AllowOverrun null, Budget.AllowOverrun null, BudgetType.AllowOverrun false, **When** the effective overrun policy is queried, **Then** the system resolves to false (three-level null-inherit chain).
4. **Given** a Budget in Active with child BudgetItems forming a 3-level tree, **When** the tree is queried, **Then** each node reports its computed Level and the unique constraint (BudgetId, ItemCode) is enforced.

---

### User Story 3 - Appropriation Management with Computed Availability (Priority: P1)

A budget officer creates appropriations (Original, Supplement, Reduction, Adjustment) against a BudgetItem, and the system computes availability for that item at transaction time. There is no Transfer type; post-submission appropriation changes use Adjustment or Cancel, while Update and Delete are allowed only while the row is Draft.

**Why this priority**: Appropriations are the authorization to spend. Availability determines whether new appropriations are allowed and must be computed from live data (Principle III/V), not stored snapshots.

**Independent Test**: Can be fully tested by creating multiple appropriations against the same BudgetItem with different types and verifying `AvailableForAppropriation(BudgetItemId)` equals sum(Active Original/Supplement) − sum(Active Reduction) + sum(Active Adjustment signed Amount: positive Adjustments add availability, negative Adjustments subtract it) to two decimals, and that Blocking control rejects over-availability while Warning allows with a logged warning.

**Acceptance Scenarios**:

1. **Given** a BudgetItem with existing Original 100,000 and Supplement 20,000 appropriations (both Active), **When** a user attempts a Reduction of 130,000, **Then** the system rejects with an explicit failure "exceeds net available" (Blocking mode) computed at transaction time.
2. **Given** a Budget whose BudgetType has ControlMethod Warning, **When** an appropriation exceeds available amount, **Then** the operation succeeds but a warning is logged and surfaced in the availability indicator (amber).
3. **Given** an appropriation in Draft, **When** it is submitted and approved, **Then** Status transitions Draft→PendingApproval→Approved→Active are each guarded server-side and approval decisions are recorded in ApprovalHistory.
4. **Given** an appropriation with BudgetId and BudgetItemId, **When** its DTO is queried, **Then** Fund and FiscalYear context is projected via join through Budget (no denormalized FundId/FiscalYearId stored on Appropriation).

---

### User Story 4 - Encumbrance Lifecycle with Availability Engine (Priority: P1)

A procurement or budget officer creates encumbrances against an appropriation, progresses them through PartiallyReleased/PartiallyLiquidated states, and reverses them. The appropriationId supplied at creation is used to resolve the BudgetItem; the system enforces availability against the whole BudgetItem net total via the BudgetType control method.

**Why this priority**: Encumbrances reserve budget before payment. Incorrect availability computation leads to over-commitment. This is the core budget-control enforcement point (Principle V).

**Independent Test**: Can be fully tested by creating encumbrances under appropriations for a BudgetItem with known net appropriated amount, verifying `AvailableForEncumbrance(AppropriationId)` resolves the BudgetItem from the supplied AppropriationId and equals the whole-BudgetItem net minus the sum of Active/PartiallyReleased/PartiallyLiquidated encumbrances for that BudgetItem where ReversalOfId is null, and that Blocking rejects while Warning allows.

**Acceptance Scenarios**:

1. **Given** a BudgetItem with net appropriated 50,000 and an existing Active encumbrance of 30,000 under one of its appropriations (ReversalOfId null), **When** a new encumbrance of 25,000 is created against another appropriation for the same BudgetItem and the budget's effective BudgetType ControlMethod is Blocking, **Then** the creation is rejected with an explicit availability failure.
2. **Given** an Active encumbrance, **When** it is partially released, **Then** Status transitions Active→PartiallyReleased and the released amount is reflected in subsequent availability computations.
3. **Given** an encumbrance with ReversalOfId pointing to another encumbrance, **When** the original encumbrance's DTO is queried, **Then** IsReversed projects as true via existence check (never stored) and availability is restored by excluding the reversed amount.
4. **Given** an encumbrance referencing a Suspended appropriation, **When** creation is attempted, **Then** the system rejects with "appropriation not active".
5. **Given** a closed FiscalYear (joined via Appropriation→Budget→FiscalYear), **When** a new appropriation or encumbrance is attempted for that fiscal period, **Then** the system rejects.

---

### User Story 5 - Derived-Value Projections and Contextual Joins (Priority: P2)

A budget analyst views any budgeting record and sees contextual information that is never stored but computed via joins and history queries.

**Why this priority**: The rebuild principle forbids stored derived values. All contextual data must be projected correctly for the system to be usable; until projections work, detail pages are incomplete.

**Independent Test**: Can be fully tested by querying an Encumbrance and a BudgetClassification and verifying that Level, IsReversed, Fund/FiscalYear/BudgetItem context, latest ApprovalHistory decision, and creator identity are all present and match computed truth without any of those columns being stored on the entity table.

**Acceptance Scenarios**:

1. **Given** a BudgetClassification hierarchy 5 levels deep, **When** any node's DTO is retrieved, **Then** its Level equals its depth counted via recursive parent traversal.
2. **Given** an Encumbrance that has a reversal row with ReversalOfId pointing to it, **When** the encumbrance is queried, **Then** the DTO field IsReversed is true; if no reversal row exists, IsReversed is false.
3. **Given** an Appropriation and an Encumbrance, **When** their DTOs are queried, **Then** Budget, BudgetItem, Fund, and FiscalYear identifiers are present via join projection through Appropriation→Budget (no denormalized FK columns on the tables).
4. **Given** an appropriation that has been approved and later suspended, **When** its detail is viewed, **Then** the approval history panel shows the latest ApprovalHistory decision ordered by DecisionAt DESC and the creator identity from audit CreatedBy.

---

### User Story 6 - Budgeting Frontend (Priority: P2)

A budget user navigates the Budgeting section to list, create, view, and act on budgets, items, appropriations, encumbrances, classifications, funds, and budget types, in an Arabic-first right-to-left interface.

**Why this priority**: The backend rebuild is unusable without a matching frontend. The new frontend replaces the old Budgeting UI entirely and surfaces lifecycle actions, availability indicators, and approval history.

**Independent Test**: Can be fully tested by navigating to each of the 7 entity pages, verifying list/create/detail flows, using the BudgetItem tree editor inside the Budget detail, triggering lifecycle buttons that are enabled only per current Status, observing the availability indicator color (green/amber/red) and the approval history panel.

**Acceptance Scenarios**:

1. **Given** a user with Budgets.View permission, **When** they open the Budget list page, **Then** budgets are listed with status badges and the page renders correctly in RTL and dark mode.
2. **Given** a Budget in Draft viewed in detail, **When** the detail page loads, **Then** lifecycle action buttons for Submit/Cancel are enabled while Activate/Suspend are disabled; a user without Budgets.Approve sees no Approve button.
3. **Given** an encumbrance creation form for an appropriation with low availability and ControlMethod Blocking, **When** the availability indicator is displayed, **Then** it shows red and blocks submission; with ControlMethod Warning it shows amber and allows submission with a warning.
4. **Given** any detail page with an approval history, **When** the history panel is opened, **Then** it renders decisions from ApprovalHistory projected via DocumentType+DocumentId in descending DecisionAt order.

---

### User Story 7 - Migration, Tests, and Documentation (Priority: P2)

A platform engineer runs the fresh migration on an empty database, and a QA engineer validates lifecycle, availability, and projection correctness via automated tests.

**Why this priority**: The wipe-and-rebuild is only shippable when the migration is reproducible, the seed is idempotent, and tests prove business invariants without stub tests.

**Independent Test**: Can be fully tested by applying the single migration to a fresh database, verifying 7 tables exist with correct indexes and no dropped columns, confirming DocumentSequence BGT/APR/ENC rows exist, running reseed idempotently, and executing the full unit + functional test suite which covers lifecycle gates, availability nets, blocking/warning, null-inherit chain, Level/IsReversed, concurrency, and ApprovalHistory snapshots.

**Acceptance Scenarios**:

1. **Given** a fresh database, **When** the migration is applied, **Then** the 7 Budgeting tables exist per the new schema, unique indexes on Code/Number columns exist, FK indexes on AppropriationId/BudgetId/BudgetItemId exist, and DocumentSequence rows for BGT/APR/ENC exist.
2. **Given** the migration applied twice or the seed executed twice, **When** the second execution completes, **Then** no duplicate rows are created (idempotent seed per Principle VI).
3. **Given** the full test suite is executed, **When** results are collected, **Then** there are zero stub tests (every test has at least one assertion that can fail) and coverage includes lifecycle transitions, availability accuracy to two decimals, Blocking rejection, Warning allow+log, 3-level AllowOverrun inheritance, deep Level, IsReversed existence, RowVersion concurrency conflicts, and ApprovalHistory snapshot capture.

---

### Edge Cases

- Reduction amount exceeding net available for a BudgetItem: system rejects with an explicit failure message including the computed available amount.
- Encumbrance creation referencing a Suspended or Closed appropriation: rejected; only Active appropriations (and Draft/PendingApproval for draft encumbrances that will be approved after appropriation becomes Active) are checked via current status at transaction time.
- AllowOverrun is null at both BudgetItem and Budget levels: the effective value resolves to BudgetType.AllowOverrun (the only non-nullable root); no default assumption of true/false.
- No Transfer/TransferIn/TransferOut appropriation types and no Appropriation reversal operation: post-submission appropriation changes use Adjustment rows or Cancel, while Update and Delete are allowed only while the row is Draft; availability for each BudgetItem reflects only its own Active Original/Supplement/Reduction/signed-Adjustment rows.
- Encumbrance reversal: a new encumbrance row is inserted with ReversalOfId pointing to the original and ReversalReason populated; the original's IsReversed projection becomes true; availability is restored because the reversal row is excluded (ReversalOfId != null) and the original's reversal is detected via existence.
- FiscalYear closed (FiscalYear.Status closed or period locked, joined via Budget): new appropriations and encumbrances for that Budget are rejected.
- Deep BudgetClassification or BudgetItem hierarchy (5+ levels): Level is computed via recursive CTE or iterative traversal without stack overflow and matches expected depth.
- ApprovalHistory missing for a document after wipe: no legacy rows exist post-wipe; fresh documents have history only after their first lifecycle transition.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001 (BudgetType)**: System MUST provide a BudgetType entity with Code (string 50 UNIQUE), Name (string 200), Description (string 500 nullable), ControlMethod (enum None=0, Warning=1, Blocking=2), AllowOverrun (bool NOT NULL default false), IsActive, RowVersion, and BaseAuditableEntity audit fields. System MUST NOT store OverrunRequiresApproval or IsSystemType on BudgetType.
- **FR-002 (Fund)**: System MUST retain Fund with FundNumber UNIQUE, FundName, FundType (General/Special/Project), FundCategory (Operating/Capital), FiscalYearId int nullable FK→FiscalYears Restrict, LegalAuthority, Description nullable, DefaultRevenueDebitAccountId int nullable FK→Accounts Restrict, IsActive, RowVersion, audit. Fund schema MUST NOT be altered beyond preserving the existing definition.
- **FR-003 (BudgetClassification)**: System MUST provide BudgetClassification with Id PK, Code UNIQUE, Name, ParentId int nullable FK→self Restrict, IsActive, RowVersion, audit. System MUST NOT store a Level column; the DTO field Level MUST be computed at query time via recursive hierarchy traversal and exposed as an int.
- **FR-004 (Budget)**: System MUST provide Budget with BudgetNumber UNIQUE (BGT sequence), BudgetName, BudgetTypeId FK→BudgetTypes Restrict, FiscalYearId FK→FiscalYears Restrict, FundId FK→Funds Restrict, TotalAmount decimal(23,2), Status (BudgetStatus: Draft, Submitted, Approved, Active, Suspended, Closed, Cancelled), AllowOverrun bool nullable (null means inherit from BudgetType.AllowOverrun), EffectiveFrom DateOnly, EffectiveTo DateOnly nullable, Description nullable, RowVersion, audit. System MUST NOT store any snapshot amount columns (AppropriatedAmount, EncumberedAmount, etc.), ControlMethod, OverrunRequiresApproval, ApprovedById/ApprovedAt (ApprovalHistory is the source per Principle VIII), or IsActive (Status values Cancelled/Closed render inactive in UI).
- **FR-005 (BudgetItem)**: System MUST provide BudgetItem with ItemCode, ItemName, BudgetId FK→Budgets Restrict, ParentId int nullable FK→self Restrict, AccountId int nullable FK→Accounts Restrict, FundId int nullable FK→Funds Restrict, CostCenterId int nullable FK→CostCenters Restrict, BudgetClassificationId int nullable FK→BudgetClassifications Restrict, AllowOverrun bool nullable (null means inherit Budget.AllowOverrun ?? BudgetType.AllowOverrun), IsActive, RowVersion, audit, and UNIQUE constraint (BudgetId, ItemCode). System MUST NOT store any amount columns, Percentage, ControlLevel, IsMandatory, ProjectId, OrganizationUnitId, or a Level column (computed like Classification).
- **FR-006 (Appropriation)**: System MUST provide Appropriation with AppropriationNumber UNIQUE (APR sequence), BudgetId FK→Budgets Restrict, BudgetItemId FK→BudgetItems Restrict, AppropriationType (Original, Supplement, Reduction, Adjustment), DocumentType string 50, DocumentId int, Amount decimal(23,2) (signed for Adjustment: positive adds availability and negative subtracts it; positive for Original/Supplement/Reduction), Status (AppropriationStatus: Draft, PendingApproval, Approved, Active, Suspended, Closed, Cancelled), RowVersion, audit. System MUST NOT store FundId, FiscalYearId (joined via Budget), CreatedById (audit CreatedBy is the source), ApprovedById/ApprovedAt (ApprovalHistory), any snapshot amount columns, Transfer/TransferIn/TransferOut types, Reversed status, or IsReversed/ReversalOfId/ReversalReason. Post-submission financial corrections MUST use Adjustment rows or Cancel; Update and Delete are allowed only while the row is Draft. DTO MUST expose Fund/FiscalYear/BudgetItem context via join projection through Budget.
- **FR-007 (Encumbrance)**: System MUST provide Encumbrance with EncumbranceNumber UNIQUE (ENC sequence), EncumbranceType (Commitment, Obligational, Contractual, Advance, Adjustment), AppropriationId FK→Appropriations Restrict (single source of budget context), VendorId int nullable FK→Suppliers Restrict, PurchaseOrderId int nullable FK→PurchaseOrders Restrict, DocumentType string 50, DocumentId int, Description nullable, EncumbranceDate DateOnly, Amount decimal(23,2), Status (EncumbranceStatus: Draft, PendingApproval, Approved, Active, PartiallyReleased, PartiallyLiquidated, FullyLiquidated, Closed, Cancelled, Reversed), ReversalOfId int nullable FK→self Restrict (set only on the reversal row, pointing to the original), ReversalReason string 500 nullable (stored only on the reversal row), RowVersion, audit. System MUST NOT store FundId, FiscalYearId, BudgetId, BudgetItemId (all joined via AppropriationId→Budget), IsReversed (computed: exists row where ReversalOfId = this.Id), CreatedById, or ApprovedById/ApprovedAt. DTO MUST expose Budget/BudgetItem/Fund/FiscalYear context via join projection.
- **FR-008 (Derived Value Projections)**: System MUST expose all derived values exclusively via query projections and DTO computed fields, never as stored columns. This includes: BudgetClassification Level and BudgetItem Level via recursive hierarchy, Encumbrance IsReversed via existence check (`EXISTS (SELECT 1 FROM Encumbrances r WHERE r.ReversalOfId = e.Id)`), Encumbrance contextual Fund/FiscalYear/Budget/BudgetItem via joins through Appropriation→Budget→BudgetItem, latest ApprovalHistory decision per document via query `WHERE DocumentType = @type AND DocumentId = @id ORDER BY DecisionAt DESC`, and creator identity via audit CreatedBy field.
- **FR-009 (Availability Engine)**: System MUST compute availability at transaction time from current data (Principle III): `AvailableForAppropriation(BudgetItemId)` = sum(Amount of Active Appropriation rows for the BudgetItem with AppropriationType Original or Supplement) − sum(Amount of Active rows with type Reduction) + sum(Amount of Active rows with type Adjustment, using Amount sign: positive Adjustments add availability and negative Adjustments subtract availability). `AvailableForEncumbrance(AppropriationId)` = whole-BudgetItem net for the BudgetItem resolved from the supplied AppropriationId − sum(Encumbrance.Amount for encumbrances whose Appropriation belongs to the same BudgetItem, with Status in Active/PartiallyReleased/PartiallyLiquidated and ReversalOfId null). The supplied appropriationId is only the anchor used to resolve BudgetItemId; the encumbrance total is not limited to that single AppropriationId. System MUST resolve ControlMethod from BudgetType.ControlMethod and AllowOverrun from the three-level chain BudgetItem.AllowOverrun ?? Budget.AllowOverrun ?? BudgetType.AllowOverrun. When ControlMethod is None the check is skipped; Warning logs and allows with an explicit warning; Blocking rejects with an explicit failure result. Fund/FiscalYear filtering for cross-item checks MUST be done via joins through Appropriation→Budget, not denormalized columns.
- **FR-010 (Lifecycles)**: System MUST enforce Budget, Appropriation, and Encumbrance finite-state machines server-side with guarded transitions and explicit result failures for illegal transitions. Budget: Draft→Submitted→Approved→Active→Suspended↔Active→Closed; Cancelled from Draft/Submitted/Suspended. Appropriation: Draft→PendingApproval→Approved→Active→Suspended/Closed/Cancelled; there is no Reversed state or reversal operation. Post-submission corrections use new Adjustment rows or Cancel (per FR-006); Update and Delete are allowed only while the row is Draft. Encumbrance: Draft→PendingApproval→Approved→Active→PartiallyReleased→PartiallyLiquidated→FullyLiquidated→Closed; Cancelled from Draft/PendingApproval; Reversed from Active/PartiallyReleased/PartiallyLiquidated via a new reversal row plus ReversalOfId and ReversalReason (the only entity that retains reversal linkage). Every transition that requires approval MUST record a decision in ApprovalHistory with an evaluation snapshot including actor, time, decision, and rule snapshot (Principle VIII). Time-windowed controls such as FiscalYear closure MUST be evaluated at transaction time.
- **FR-011 (Application Rebuild)**: System MUST delete ALL existing Budgeting Application commands, queries, and DTOs and rebuild them as: Budgets (Create, Update, Submit, Approve, Activate, Suspend, Close, Cancel + Get/List/Tree queries), BudgetItems (Create, Update, Delete, Move/TreeReorder + GetTree/List queries), Appropriations (Create, Update, Delete, Submit, Approve, Activate, Suspend, Close, Cancel + Get/List + Availability queries; Update/Delete are allowed only while Draft and there is no Reverse command), Encumbrances (Create, Submit, Approve, Activate, Release, LiquidatePartial, LiquidateFull, Close, Cancel, Reverse + Get/List + Availability query + computed availability check on Create and Approve), and BudgetTypes/Funds/BudgetClassifications (CRUD + IsActive toggle). Every command MUST have FluentValidation, an `[Authorize(Policy = PermissionCodes.X)]` attribute, RowVersion concurrency verification, and availability checks evaluated server-side where applicable.
- **FR-012 (Permissions and Seeding)**: System MUST reuse existing PermissionCodes (Budgets.*, Appropriations.*, Encumbrances.*, Funds.*, BudgetClassifications.*, BudgetTypes.*) with a clean, idempotent seed (Principle VI). Every Web endpoint AND its backing use case MUST declare the required named permission; anonymous access MUST be denied; every authorization decision (grant and denial with reason) MUST be recorded in the security audit log (Principle VII).
- **FR-013 (Web Endpoints)**: System MUST expose REST endpoints under `/api/BudgetTypes`, `/api/Funds`, `/api/BudgetClassifications`, `/api/Budgets` (including `/{id}/items` sub-routes for BudgetItems), `/api/Appropriations`, `/api/Encumbrances`, and `GET /api/Encumbrances/availability?appropriationId={id}` for computed availability. All endpoints MUST use the uniform ProblemDetails error contract, carry a stable operationId for client generation, and round-trip RowVersion. The OpenAPI document MUST be regenerated so that removed fields/tables disappear and new contracts appear.
- **FR-014 (Frontend Rebuild)**: System MUST rebuild the `features/budgeting` frontend with 7 entity pages (BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance) each with list/create/detail routes, a BudgetItem tree editor embedded in the Budget detail page, lifecycle action buttons whose enabled state is derived from current Status, an availability indicator (green/amber/red computed from ControlMethod and availability engine result), and an approval history panel that renders the projected ApprovalHistory for the viewed document. Data fetching MUST use TanStack Query hooks; UI MUST be Arabic-first RTL, use only design tokens, support dark mode, provide keyboard navigation, and show loading/empty/error states for every query. Routes and sidebar entries MUST carry their required permission identifiers.
- **FR-015 (Migration)**: System MUST ship a single EF Core migration that drops the 7 existing Budgeting tables (wiping all rows), recreates them per the new schema in FR-001 through FR-007, creates unique indexes on Code/Number columns (BudgetType.Code, Fund.FundNumber, BudgetClassification.Code, Budget.BudgetNumber, AppropriationNumber, EncumbranceNumber, and (BudgetId, ItemCode) for BudgetItem), creates FK indexes on join-path columns (AppropriationId on Encumbrance; BudgetId and BudgetItemId on Appropriation; BudgetId on BudgetItem), preserves (or inserts if missing) DocumentSequence rows for document types BGT/APR/ENC, and reseeds reference data idempotently. The Down migration MUST restore the prior schema (data loss is accepted per the wipe decision). Historical migrations before this one MUST remain untouched.
- **FR-016 (Tests)**: System MUST ship executable tests without stubs (Principle XI): one unit test class per command covering its success path and each principal failure path (validation, lifecycle guard, availability rejection, concurrency); functional tests executing against a real database with per-test state reset that verify lifecycle transitions, availability net arithmetic to two decimals including Supplement/Reduction/signed Adjustment, Blocking rejection vs Warning allow+log, the three-level AllowOverrun null-inheritance chain, Level computation on a hierarchy of depth ≥ 5, IsReversed computed via existence, RowVersion concurrency conflicts, and ApprovalHistory snapshot capture.
- **FR-017 (Docs)**: System MUST update `docs/database-schema.md` to rewrite the Budgeting tables section per the new column sets (explicitly noting each removed column that previously existed), update the feature registry to reflect the simplified chain `Budget → Appropriation → Encumbrance` with computed availability, and include a Constitution compliance table in the implementation plan that shows Principle VI compliance via "no denormalized derived data".

### Key Entities

- **BudgetType**: Defines the control regime for a class of budgets. Key attributes: Code, Name, Description, ControlMethod (None/Warning/Blocking), AllowOverrun (bool, root of inheritance chain), IsActive. Relationships: referenced by Budget.
- **Fund**: A financial fund (General/Special/Project) with category Operating/Capital, optionally scoped to a FiscalYear and linked to a default revenue account. Unchanged from current schema; referenced by Budget.
- **BudgetClassification**: Hierarchical classification code (e.g., economic/functional). Attributes: Code, Name, ParentId (self-reference). Level is computed, not stored.
- **Budget**: An approved budget for one FiscalYear and one Fund. Attributes: BudgetNumber (BGT), BudgetName, BudgetTypeId, FiscalYearId, FundId, TotalAmount, Status (BudgetStatus), AllowOverrun nullable, EffectiveFrom/To, Description. No stored derived amounts or approval identity.
- **BudgetItem**: A line item within a Budget, forming a tree. Attributes: ItemCode, ItemName, BudgetId, ParentId (self), AccountId, FundId, CostCenterId, BudgetClassificationId, AllowOverrun nullable, IsActive. Level is computed. UNIQUE(BudgetId, ItemCode).
- **Appropriation**: An authorization to use a BudgetItem's budget. Attributes: AppropriationNumber (APR), BudgetId, BudgetItemId, AppropriationType (Original, Supplement, Reduction, Adjustment), DocumentType/DocumentId, Amount (signed for Adjustment; positive for Original/Supplement/Reduction), Status (AppropriationStatus, without Reversed). No denormalized Fund/FiscalYear, no snapshot amounts, no Transfer/TransferIn/TransferOut types, and no reversal columns/operation. Post-submission changes use Adjustment rows or Cancel; Update/Delete are allowed only while Draft. DTO projects contextual fields via joins.
- **Encumbrance**: A reservation against an Appropriation. Attributes: EncumbranceNumber (ENC), AppropriationId (single source of budget context), EncumbranceType, VendorId, PurchaseOrderId, DocumentType/DocumentId, Description, EncumbranceDate, Amount, Status (EncumbranceStatus), ReversalOfId nullable (self, on reversal row only), ReversalReason nullable (on reversal row only). IsReversed, and Budget/Fund/FiscalYear context are computed projections. Budget context is obtained via Appropriation→Budget join.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer running `dotnet build` with warnings-as-errors and `dotnet test` observes zero build warnings and 100% of tests passing.
- **SC-002**: A search for any dropped column name as a stored property on its former owning entity (Level stored on BudgetClassification or BudgetItem, IsReversed stored on Encumbrance, FundId/FiscalYearId/BudgetId/BudgetItemId stored on Encumbrance, FundId/FiscalYearId/CreatedById/ApprovedById/ApprovedAt stored on Appropriation, ApprovedById/ApprovedAt/IsActive stored on Budget) returns zero matches in `src/Domain/Budgeting/Entities/*.cs`.
- **SC-003**: For any BudgetItem, a tester executing the availability net `Original + Supplement - Reduction + signed Adjustment` across multiple appropriations observes the computed `AvailableForAppropriation` matching the expected arithmetic to two decimal places.
- **SC-004**: When BudgetType.ControlMethod is Blocking and a creation or approval would exceed computed availability, the operation fails with an explicit, user-visible failure message and no row is created/transitioned.
- **SC-005**: A tester configures BudgetType.AllowOverrun = false, Budget.AllowOverrun = null, BudgetItem.AllowOverrun = null and observes overruns are blocked; then sets BudgetItem.AllowOverrun = true and observes they are allowed, proving the three-level null-inheritance chain (BudgetItem → Budget → BudgetType) is correctly resolved at transaction time.
- **SC-006**: A hierarchy of depth 5+ shows each node's Level in the DTO equal to its true depth, and an encumbrance with a reversal row shows IsReversed true while one without a reversal row shows false, in both cases confirmed by functional tests against a real database.
- **SC-007**: A user opening each of the 7 budgeting pages in both light and dark mode with the interface set to RTL observes correct layout (logical start/end, not physical left/right), design-token-only styling, and for appropriations/encumbrances sees the availability indicator in the correct color and an approval history panel that renders committed ApprovalHistory decisions.
- **SC-008**: An unauthenticated HTTP request to any new budgeting endpoint receives 401, and an authenticated request from a principal lacking the required permission receives 403 with a ProblemDetails body; no budgeting endpoint is anonymously accessible.

## Assumptions

- DocumentSequence rows with DocumentType BGT (Budget), APR (Appropriation), and ENC (Encumbrance) either already exist or are created idempotently by this feature's migration/seed; if missing, the feature creates them.
- The Suppliers table (and thus a valid VendorId) exists as a prerequisite for Encumbrance.VendorId; similar external FK targets (FiscalYears, Accounts, CostCenters, PurchaseOrders, Currencies) exist.
- ApprovalHistory is the existing table that records workflow and approval decisions per Constitution Principle VIII; its DocumentType+DocumentId composite and DecisionAt timestamp are available for projection queries; no new approval storage is introduced here.
- All existing Budgeting rows are wiped by the migration; no data migration or preservation of historical budgeting data is required and data loss is accepted per product decision.
- Integration of budget availability checks into the PostingPipeline (rejecting Moves that would cause overspend) is explicitly out of scope for v1 and will be addressed by a future initiative.
- Frontend route guards and sidebar entries correctly enforce permission identifiers at the UI layer, but server-side `[Authorize]` on endpoints and use cases remains the sole authority per Principle VII (frontend checks are UX only).
- Monetary amounts use decimal(23,2) and date fields use DateOnly per existing project conventions; exchange-rate and base-currency logic is handled elsewhere and is not recomputed within the availability engine.
