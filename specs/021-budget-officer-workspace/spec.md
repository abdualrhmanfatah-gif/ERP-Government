# Feature Specification: Budget Officer Workspace — Full Budget Lifecycle UI

**Feature Branch**: `021-budget-officer-workspace`

**Created**: 2026-09-06

**Status**: Draft

**Input**: User description: "Budget officer workspace for the full budget lifecycle. Today the backend supports budgets, budget items, appropriations (including transfers), encumbrances, and informational monthly plans — but the UI only covers reference data (types, funds, classifications). Build the working screens. US1 (P1): A budget officer creates a budget draft with its item tree, edits item details, and submits it for approval; approval then activation follow the document lifecycle. US2 (P1): On an active budget item, the officer records appropriations (original/supplementary/reduction) and transfers between items, with the target item selectable. US3 (P1): The officer records encumbrances against items and sees the availability indicator (blocking/warning/none) computed live before saving. US4 (P2): An informational monthly plan editor per budget item (12 months, no enforcement). US5 (P2): Budget execution drill-down: item → its appropriations → its encumbrances, with totals per level. Edge cases: transfer to a frozen/closed item; cancelling an encumbrance with a reversal; reductions exceeding balance. Entities: none new — UI over existing backend."

## Clarifications

### Session 2026-09-06

- Q: Should the monthly plan (US4) persist to a backend endpoint or be purely client-side until an endpoint is added? → A: Client-side state only (localStorage or in-memory) for now; no backend endpoint. The editor is informational — no enforcement. When a backend endpoint is added later, this UI will wire to it.
- Q: Should the availability indicator (US3) fetch availability data as the user types amounts in the encumbrance form, or only on explicit action? → A: Live fetch — availability re-fetches whenever the encumbrance amount or appropriation selection changes, so the officer always sees current state before saving.
- Q: For the budget execution drill-down (US5), should it be a dedicated page or an expandable panel within the budget detail view? → A: Expandable drill-down panels within the budget detail page — click a row to expand its appropriations, click an appropriation to expand its encumbrances.
- Q: Should the budget officer who creates and submits a budget also be allowed to approve it, or must approval come from a different user? → A: Same user can approve their own submission. The approval step is a lifecycle gate, not a separation-of-duties checkpoint in this UI.
- Q: How should the budget list page be accessed — as a dedicated navigation item in the sidebar, or nested under an existing "Budgeting" menu group? → A: Nested under the existing Budgeting nav group, alongside Funds, Types, and Classifications.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Budget Draft & Item Tree (Priority: P1)

As a budget officer, I create a new budget draft for a fiscal year, populate it with a tree of budget items (categories, sub-categories, line items), edit item details (name, classification, account, cost center), and submit the draft for approval. The approval and subsequent activation follow the document lifecycle (Draft → Submitted → Approved → Active).

**Why this priority**: Budget creation is the entry point for the entire lifecycle. Without a budget draft, no appropriations or encumbrances can exist. This is the foundation for all other stories.

**Independent Test**: Can be fully tested by creating a budget, adding/editing/removing items in the tree, submitting for approval, approving, and activating — verifying status transitions and tree integrity at each step. No appropriation or encumbrance needed.

**Acceptance Scenarios**:

1. **Given** the officer navigates to the budgeting section, **When** they click "New Budget", **Then** a form collects: BudgetName, BudgetType, FiscalYear, Fund, EffectiveFrom, EffectiveTo (optional), Description, and TotalAmount.
2. **Given** a budget is created in Draft status, **When** the budget detail page loads, **Then** the BudgetItemTree is displayed with expand/collapse, inline editing, add-child, and delete capabilities (only in Draft).
3. **Given** a budget in Draft with 3 top-level items and 2 children under the first, **When** the tree is rendered, **Then** each node shows ItemCode, ItemName, computed Level, and IsActive status; the tree can be re-ordered via drag or manual sort.
4. **Given** the officer clicks "Submit" on a Draft budget, **When** the budget transitions to Submitted, **Then** the transition is recorded in ApprovalHistory and the UI disables editing.
5. **Given** a Submitted budget, **When** the officer (or any user with approve permission) approves it, **Then** Status becomes Approved and the budget can be activated.
6. **Given** an Approved budget, **When** the officer activates it, **Then** Status becomes Active and appropriations can be created against its items.
7. **Given** a budget in any terminal status (Closed/Cancelled), **When** the officer views it, **Then** all action buttons are disabled and the status badge reflects the terminal state.

---

### User Story 2 — Appropriations & Transfers (Priority: P1)

As a budget officer, on an Active budget item I record appropriations (Original, Supplement, Reduction) and transfers between items. Transfers require selecting a target item from the same budget. All appropriations start in Draft and follow the lifecycle (Draft → PendingApproval → Approved → Active). Availability is shown before creating each appropriation.

**Why this priority**: Appropriations are the authorization to spend. Without them, encumbrances cannot exist. Transfers between items are essential for fund reallocation.

**Independent Test**: Can be fully tested by creating appropriations of each type against an active budget item, verifying amounts against computed availability, and confirming lifecycle transitions. Transfer target item selection verified independently.

**Acceptance Scenarios**:

1. **Given** an Active budget with items, **When** the officer selects an item and clicks "New Appropriation", **Then** the form presents: AppropriationType dropdown (Original/Supplement/Reduction/Transfer), Amount, DocumentType, DocumentId, and for Transfer type a TargetBudgetItem selector showing available items.
2. **Given** an appropriation form with type Transfer, **When** the officer selects the type, **Then** a searchable dropdown lists budget items from the same budget (excluding the current item and any items in Closed/Cancelled status).
3. **Given** an appropriation in Draft, **When** the officer submits it, **Then** it transitions to PendingApproval and the availability indicator updates to reflect the pending amount.
4. **Given** an Active budget item with Original appropriation of 100,000, **When** the officer creates a Supplement of 20,000, **Then** the availability indicator shows net appropriated of 120,000 before the supplement is saved.
5. **Given** an Active budget item with net appropriated 100,000, **When** the officer attempts a Reduction of 120,000, **Then** the system displays a blocking error (if ControlMethod is Blocking) or a warning (if Warning) before saving.
6. **Given** a transfer to a budget item in Suspended status, **When** the officer selects it as target, **Then** the system displays an error: "Cannot transfer to a suspended item".
7. **Given** a transfer to a budget item in Closed status, **When** the officer selects it as target, **Then** the system displays an error: "Cannot transfer to a closed item".

---

### User Story 3 — Encumbrances with Live Availability (Priority: P1)

As a budget officer, I record encumbrances against items and see the availability indicator (blocking/warning/none) computed live before saving. The indicator updates as I change the encumbrance amount or appropriation selection.

**Why this priority**: Encumbrances reserve budget before payment. The live availability check prevents over-commitment and is the core budget-control enforcement point visible to the user.

**Independent Test**: Can be fully tested by creating encumbrances against known appropriations, verifying that the availability indicator reflects correct net amounts (appropriated - encumbered) in real time, and confirming that blocking/warning behavior matches the budget's ControlMethod.

**Acceptance Scenarios**:

1. **Given** an Active appropriation on a budget item with net appropriated 50,000 and existing encumbrances of 30,000, **When** the officer opens the encumbrance form and enters 15,000, **Then** the availability indicator shows 5,000 (green — within budget).
2. **Given** the same budget item, **When** the officer enters 25,000, **Then** the indicator shows -5,000 (red for Blocking, amber for Warning).
3. **Given** an encumbrance form, **When** the officer changes the appropriation selection, **Then** the availability re-fetches for the new appropriation's budget item.
4. **Given** an Active encumbrance, **When** the officer reverses it (selects "Reverse" action), **Then** the form requires a ReversalReason and creates a new encumbrance row with ReversalOfId pointing to the original; the original shows as reversed.
5. **Given** an encumbrance in Draft, **When** the officer submits it, **Then** it transitions to PendingApproval and the availability indicator updates to include this pending encumbrance.
6. **Given** a budget item with ControlMethod None, **When** the officer creates an encumbrance exceeding availability, **Then** no warning or blocking occurs — the encumbrance proceeds without indicator feedback.

---

### User Story 4 — Monthly Plan Editor (Priority: P2)

As a budget officer, I edit an informational monthly plan per budget item — 12 monthly columns (January–December) — to distribute the budget across months for planning purposes. No enforcement is applied; the plan is advisory only.

**Why this priority**: Monthly distribution planning is useful for cash-flow forecasting but does not affect budget control. It is independent of appropriations and encumbrances.

**Independent Test**: Can be fully tested by entering monthly amounts for a budget item, verifying they persist across page reloads (localStorage), and confirming that the 12-month total is displayed. No backend validation needed.

**Acceptance Scenarios**:

1. **Given** a budget item selected in the detail view, **When** the officer clicks "Monthly Plan", **Then** a 12-column editor appears with the item's total amount as a reference and input fields for each month (January–December).
2. **Given** the monthly plan editor, **When** the officer enters amounts for 6 months and saves, **Then** the plan is stored locally and the total of all 12 months is displayed alongside the budget item's appropriated total.
3. **Given** a monthly plan with amounts that do not sum to the item's total, **When** the plan is viewed, **Then** the UI shows a visual indicator (info badge, not error) noting the variance.
4. **Given** a monthly plan saved locally, **When** the officer navigates away and returns to the same item, **Then** the plan is restored from local storage.
5. **Given** a monthly plan for one item, **When** the officer exports or copies it, **Then** the plan is available as tab-separated text for pasting into a spreadsheet.

---

### User Story 5 — Budget Execution Drill-Down (Priority: P2)

As a budget officer, I drill down from a budget summary to its items, from an item to its appropriations, and from an appropriation to its encumbrances, with totals computed at each level.

**Why this priority**: Execution visibility helps officers understand where funds are allocated and consumed. It is a read-only analytical view layered on top of the data created in US1–US3.

**Independent Test**: Can be fully tested by creating a budget with items, appropriations, and encumbrances, then verifying that the drill-down view shows correct totals at each level and that navigation between levels works.

**Acceptance Scenarios**:

1. **Given** a budget in Active status with 5 items, **When** the budget detail page loads, **Then** a summary panel shows: Total Appropriated, Total Encumbered, Total Available, computed as Appropriated − Encumbered.
2. **Given** the budget detail page, **When** the officer clicks an item row, **Then** the item expands to show its appropriations list with columns: Number, Type, Amount, Status, Date.
3. **Given** an expanded item showing appropriations, **When** the officer clicks an appropriation row, **Then** the appropriation expands to show its encumbrances list with columns: Number, Type, Amount, Status, Date, Vendor.
4. **Given** the drill-down view, **When** totals are computed at each level, **Then** the item total equals the sum of its active appropriations, and the appropriation total equals the sum of its active encumbrances.
5. **Given** a budget with appropriations across multiple items, **When** the execution view is loaded, **Then** a top-level summary table shows each item with its appropriation total, encumbrance total, and available balance.

---

### Edge Cases

- **Transfer to a Suspended item**: Reject with explicit error message before saving the transfer.
- **Transfer to a closed item**: Reject with explicit error message before saving the transfer.
- **Cancelling an encumbrance with a reversal**: Create a new encumbrance row with ReversalOfId and ReversalReason; original encumbrance shows IsReversed=true; availability is restored.
- **Reductions exceeding balance**: Blocking mode rejects with explicit failure; Warning mode logs and allows.
- **Concurrent modification**: If the RowVersion changes between fetch and save, surface a concurrency conflict error and prompt the officer to refresh.
- **Budget in terminal status**: All action buttons (create, edit, delete, submit, approve, activate) are disabled.
- **Empty budget item tree**: Show a call-to-action to add the first item.
- **Availability fetch failure**: Show a degraded indicator ("—") rather than blocking the form.
- **Monthly plan variance**: Show informational badge when 12-month total does not match appropriated total; do not block saving.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001 (Budget List & Create)**: System MUST display a list of budgets with filters (status, fiscal year, fund) and a "New Budget" button that opens a creation form. All fields from the BudgetDto MUST be collected. The form MUST validate required fields before submission. The budget list MUST be accessible from the Budgeting navigation group alongside Funds, Types, and Classifications.
- **FR-002 (Budget Detail & Lifecycle Actions)**: System MUST display a budget detail page with status badge, metadata summary, lifecycle action buttons (Submit, Approve, Activate, Suspend, Close, Cancel) conditional on current status, and the item tree panel.
- **FR-003 (BudgetItem Tree)**: System MUST render the budget item tree with expand/collapse, inline editing, add-child, delete (Draft only), and drag-to-reorder (Draft only). Each node MUST show ItemCode, ItemName, computed Level, and IsActive.
- **FR-004 (BudgetItem Edit)**: System MUST provide a form to edit BudgetItem fields: ItemCode, ItemName, AccountId, FundId, CostCenterId, BudgetClassificationId, AllowOverrun. Changes MUST send RowVersion for concurrency.
- **FR-005 (Appropriation Create & Edit)**: System MUST provide a form to create/edit appropriations with AppropriationType, Amount, DocumentType, DocumentId. For Transfer type, a target item selector MUST be shown. Appropriation editing is allowed only in Draft status.
- **FR-006 (Appropriation Lifecycle)**: System MUST display lifecycle buttons (Submit, Approve, Activate, Suspend, Close, Cancel) conditional on appropriation status. Transitions MUST follow the FSM: Draft → PendingApproval → Approved → Active.
- **FR-007 (Transfer Target Selector)**: When AppropriationType is Transfer, System MUST display a searchable dropdown of budget items from the same budget, excluding the source item and any items in Closed/Cancelled status.
- **FR-008 (Encumbrance Create & Edit)**: System MUST provide a form to create/edit encumbrances with EncumbranceType, AppropriationId, Amount, EncumbranceDate, VendorId, Description. The form MUST display the live availability indicator.
- **FR-009 (Encumbrance Lifecycle)**: System MUST display lifecycle buttons (Submit, Approve, Activate, Release, Liquidate, Cancel, Reverse) conditional on encumbrance status. Reversal MUST require a ReversalReason.
- **FR-010 (Availability Indicator)**: System MUST display a live availability indicator (green/amber/red badge) on both appropriation and encumbrance forms. The indicator MUST re-fetch whenever the amount or appropriation selection changes. Tone MUST match BudgetControlMethod: None=green, Warning=amber, Blocking=red.
- **FR-011 (Monthly Plan Editor)**: System MUST provide a 12-column monthly plan editor per budget item. Plans MUST persist in local storage. A variance indicator MUST show when the 12-month total does not match the item's appropriated total.
- **FR-012 (Budget Execution Drill-Down)**: System MUST display a drill-down view: budget summary → items → appropriations → encumbrances. Totals MUST be computed at each level. Navigation MUST be expandable (click row to expand children).
- **FR-013 (RTL & Accessibility)**: All screens MUST render correctly in Arabic-first RTL layout. All interactive elements MUST be keyboard-navigable. All indicators MUST have ARIA labels. Focus management MUST follow WAI-ARIA patterns.
- **FR-014 (API Integration)**: System MUST use the existing API clients in `shared/client.ts`. NSwag-generated clients MUST be regenerated via `npm run generate-api` after any endpoint changes. Hand-written clients in `shared/client.ts` MUST mirror the OpenAPI contract exactly.
- **FR-015 (Error Handling)**: System MUST display ProblemDetails errors inline on forms. Concurrency conflicts MUST show a distinct "data changed by another user" message. Network errors MUST show a toast notification.
- **FR-016 (Loading & Empty States)**: System MUST show skeleton loaders during data fetch. List pages MUST show a message with call-to-action when empty. Tree views MUST show an "add first item" prompt when empty.

### Key Entities

- **Budget**: Top-level container. Links to BudgetType, FiscalYear, Fund. Has lifecycle (Draft → Active → Closed/Cancelled). Budget items form a tree within it.
- **BudgetItem**: Line item within a Budget. Self-referencing parent-child tree. Links to Account, Fund, CostCenter, Classification. Availability computed at this level.
- **Appropriation**: Authorization to spend against a BudgetItem. Typed (Original/Supplement/Reduction/Transfer). Lifecycle governed. Fund/FiscalYear context via join through Budget.
- **Encumbrance**: Reservation of funds against an Appropriation. Typed. Supports reversal. Budget context via join through Appropriation.
- **MonthlyPlan (client-side)**: 12-month distribution of a BudgetItem's budget. No backend entity. Stored in local storage.

### Field Contract

#### Budget

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `budgetNumber` | string | No (issued at creation) | System-generated |
| `name` | string | Yes (create + edit) | Required |
| `fiscalYearId` | number | Yes (create) | Open years only |
| `fundId` | number | Yes (create) | Fund picker |
| `typeId` | number | Yes (create) | Budget type picker |
| `status` | enum | No (lifecycle) | Draft/Submitted/Approved/Active/Suspended/Closed/Cancelled |
| `audit` | object | No | createdAt/By, modifiedAt/By |

#### BudgetItem

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `itemCode` | string | Yes (create + edit) | Required |
| `itemName` | string | Yes (create + edit) | Required |
| `parentId` | number? | Yes (move) | Tree hierarchy |
| `accountId` | number | Yes (edit) | Account picker |
| `fundId` | number | Yes (edit) | Fund picker |
| `costCenterId` | number? | Yes (edit) | Cost center picker |
| `budgetClassificationId` | number? | Yes (edit) | Classification picker |
| `allowOverrun` | boolean | Yes (edit) | Overrun flag |
| `totalAppropriated` | decimal | No (computed) | Server total |
| `totalEncumbered` | decimal | No (computed) | Server total |
| `totalAvailable` | decimal | No (computed) | Appropriated − Encumbered |

#### Appropriation

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `number` | string | No (auto) | System-generated |
| `itemId` | number | Yes (create) | Budget item picker |
| `type` | enum | Yes (create) | Original/Supplementary/Reduction/Transfer |
| `amount` | decimal | Yes (create + edit, Draft only) | Required |
| `status` | enum | No (lifecycle) | Draft/PendingApproval/Approved/Active/Reversed |
| `targetBudgetItemId` | number? | Yes (Transfer only) | Target item picker |
| `documentType` | string | Yes (create) | Optional |
| `documentId` | number? | Yes (create) | Optional |

#### Encumbrance

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `number` | string | No (auto) | System-generated |
| `itemId` | number | Yes (create) | Budget item picker |
| `vendorPartyId` | number | Yes (create) | Vendor picker (inactive excluded) |
| `amount` | decimal | Yes (create + edit, Draft only) | Required |
| `encumbranceDate` | Date | Yes (create) | Required |
| `status` | enum | No (lifecycle) | Draft/PendingApproval/Approved/Active/Reversed |
| `reversalOfId` | number? | No | Link to original (set at reverse) |
| `reversalReason` | string? | No (set at reverse) | Mandatory on reverse |

#### Availability (computed, read-only)

| Field | Source | Notes |
|-------|--------|-------|
| `appropriated` | Sum of active appropriation amounts | Per budget item |
| `encumbered` | Sum of active encumbrance amounts | Per budget item |
| `available` | appropriated − encumbered | Per budget item |
| `controlMethod` | BudgetControlMethod | None/Warning/Blocking |
| `tone` | Derived from controlMethod + available | Green/Amber/Red |

### Permissions

- **FR-PERM-001**: `Budgets.View` — View list + detail
- **FR-PERM-002**: `Budgets.Create` — Create budget
- **FR-PERM-003**: `Budgets.Update` — Edit budget
- **FR-PERM-004**: `Budgets.Submit` — Submit Draft→Submitted
- **FR-PERM-005**: `Budgets.Approve` — Approve Submitted→Approved
- **FR-PERM-006**: `Budgets.Activate` — Activate Approved→Active
- **FR-PERM-007**: `Budgets.Suspend` — Suspend Active→Suspended
- **FR-PERM-008**: `Budgets.Close` — Close→Closed
- **FR-PERM-009**: `Budgets.Cancel` — Cancel→Cancelled
- **FR-PERM-010**: `BudgetItems.View` — View items
- **FR-PERM-011**: `BudgetItems.Create` — Create item
- **FR-PERM-012**: `BudgetItems.Update` — Edit item
- **FR-PERM-013**: `BudgetItems.Delete` — Delete item
- **FR-PERM-014**: `BudgetItems.Move` — Move item in tree
- **FR-PERM-015**: `Appropriations.View` — View appropriations
- **FR-PERM-016**: `Appropriations.Create` — Create appropriation
- **FR-PERM-017**: `Appropriations.Update` — Edit appropriation (Draft only)
- **FR-PERM-018**: `Appropriations.Submit` — Submit
- **FR-PERM-019**: `Appropriations.Approve` — Approve
- **FR-PERM-020**: `Appropriations.Activate` — Activate
- **FR-PERM-021**: `Appropriations.Suspend` — Suspend
- **FR-PERM-022**: `Appropriations.Close` — Close
- **FR-PERM-023**: `Appropriations.Cancel` — Cancel
- **FR-PERM-024**: `Appropriations.Reverse` — Reverse
- **FR-PERM-025**: `Encumbrances.View` — View encumbrances
- **FR-PERM-026**: `Encumbrances.Create` — Create encumbrance
- **FR-PERM-027**: `Encumbrances.Update` — Edit encumbrance (Draft only)
- **FR-PERM-028**: `Encumbrances.Submit` — Submit
- **FR-PERM-029**: `Encumbrances.Approve` — Approve
- **FR-PERM-030**: `Encumbrances.Activate` — Activate
- **FR-PERM-031**: `Encumbrances.Release` — Release
- **FR-PERM-032**: `Encumbrances.Cancel` — Cancel
- **FR-PERM-033**: `Encumbrances.Reverse` — Reverse

### Lifecycle Transitions (PATCH-style)

- Budget: Draft→Submitted→Approved→Active→Suspended/Closed/Cancelled
- Appropriation: Draft→PendingApproval→Approved→Active→Reversed
- Encumbrance: Draft→PendingApproval→Approved→Active→Reversed

### UI States Required

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| Budgets List | Skeleton | "لا توجد موازنات" | Toast | Guard | N/A | DataGrid |
| Budget Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + item tree + actions |
| Budget Create | N/A | N/A | Toast | Guard | N/A | Form |
| Appropriations List | Skeleton | "لا توجد تحويلات" | Toast | Guard | N/A | DataGrid |
| Encumbrances List | Skeleton | "لا توجد التزامات" | Toast | Guard | N/A | DataGrid |

### Tests Expected

| ID | Page | Test |
|----|------|------|
| T-021-001 | Budgets List | Renders with status badges |
| T-021-002 | Budget Detail | Item tree renders |
| T-021-003 | Budget Create | Form validates |
| T-021-004 | Budget Detail | Lifecycle buttons per status |
| T-021-005 | BudgetItem Tree | Add/edit/delete in Draft |
| T-021-006 | BudgetItem Tree | Move item (no cycle) |
| T-021-007 | Appropriations | Create Original/Supplement/Reduction/Transfer |
| T-021-008 | Appropriations | Transfer target selector excludes source |
| T-021-009 | Encumbrances | Create with availability indicator |
| T-021-010 | Encumbrances | Availability blocks when exceeding |
| T-021-011 | Encumbrances | Reverse requires reason |
| T-021-012 | MonthlyPlan | 12-month editor persists |
| T-021-013 | MonthlyPlan | Variance indicator |
| T-021-014 | Drill-Down | Totals at each level |
| T-021-015 | Availability | Green/Amber/Red per controlMethod |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A budget officer can complete a full budget cycle (create draft → add items → submit → approve → activate) entirely from the UI in under 10 minutes.
- **SC-002**: Appropriations of all types (Original, Supplement, Reduction, Transfer) can be created, submitted, and activated against active budget items within 3 minutes per appropriation.
- **SC-003**: Encumbrances can be created with live availability feedback; the availability indicator updates within 1 second of amount change.
- **SC-004**: Transfers between items correctly update availability on both source and target items without page reload.
- **SC-005**: Monthly plans persist across page reloads (local storage) and show variance indicators when totals mismatch.
- **SC-006**: Budget execution drill-down shows correct totals at each level (item, appropriation, encumbrance) within 2 seconds.
- **SC-007**: All screens render correctly in RTL with Arabic text; no layout breakage.
- **SC-008**: All interactive elements are keyboard-navigable and screen-reader accessible.
- **SC-009**: Blocking control method prevents saving encumbrances that exceed availability; Warning mode allows with visual feedback.
- **SC-010**: Concurrency conflicts are handled gracefully — user is prompted to refresh without data loss.

## Assumptions

- The backend API endpoints for Budgets, BudgetItems, Appropriations, Encumbrances, and their lifecycle actions already exist and are functional (per specs/013-budgeting-backend-rebuild and superseding specs).
- The existing frontend components (AvailabilityIndicator, BudgetItemTree, LifecycleActions, ApprovalHistoryPanel) in `components/` can be reused or extended.
- The existing API clients in `shared/client.ts` and generated clients cover all needed endpoints.
- Local storage is sufficient for client-side monthly plan persistence; no backend endpoint is required.
- No new database entities are needed — this is purely a UI feature spec.
- The RBAC permission model (per AGENTS.md) is already wired and will gate UI actions via permission checks. The same user may perform create, submit, and approve actions — approval is a lifecycle gate, not a role-restricted separation-of-duties checkpoint.
- The design tokens and RTL layout system are already established and must be followed (Principle X).
- Arabic labels for all new UI strings must be provided; English is secondary.

## Out of Scope

- **Reports and dashboards**: Budget execution reports are covered by spec 020-erp-read-only-reporting. This spec builds only the transactional workspace screens.
- **Backend changes**: No new endpoints, entities, or migrations. This spec is UI-only over existing backend.
- **Payment processing**: The payment stage of the budget chain (Budget → Appropriation → Encumbrance → Payment) is not part of this workspace.
- **Admin screens**: Budget type, fund, and classification management already exist and are not rebuilt here.
- **Mobile/responsive**: Desktop-first; mobile layout is a separate concern.
