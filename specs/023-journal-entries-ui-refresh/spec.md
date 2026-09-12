# Feature Specification: Journal Entries UI Refresh

**Feature Branch**: `023-journal-entries-ui-refresh`

**Created**: 2026-09-06

**Status**: Draft

**Input**: User description: "Journal entries screen refresh. The accounting core renamed moves to journal entries with an entry-status lifecycle and analytic dimensions on every line (fund, project, budget item, encumbrance, payment order). The current UI still shows the old move screens and is broken against the renamed API."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Browse Journal Entries with Status and Filters (Priority: P1)

An accountant opens the journal entries list and sees entries with status badges (Draft, Submitted, Approved, Posted, Reversed, Cancelled). The accountant filters by period, fund, and status to locate specific entries quickly.

**Why this priority**: The list view is the entry point for all journal entry operations. Without a working list screen that reflects the renamed API and new statuses, accountants cannot access any accounting workflow.

**Independent Test**: Can be fully tested by loading the journal entries list, verifying status badges render correctly, and confirming filters return correct results for each filter combination.

**Acceptance Scenarios**:

1. **Given** an accountant navigates to the journal entries list, **When** the page loads, **Then** all entries display with correct status badges (Draft/Submitted/Approved/Posted/Reversed/Cancelled) and the list uses the renamed JournalEntry API endpoints
2. **Given** the journal entries list is loaded, **When** the accountant filters by period, **Then** only entries within the selected fiscal period are displayed
3. **Given** the journal entries list is loaded, **When** the accountant filters by fund, **Then** only entries containing lines with the selected fund dimension are displayed
4. **Given** the journal entries list is loaded, **When** the accountant filters by status, **Then** only entries with the selected status are displayed
5. **Given** the journal entries list is loaded, **When** the accountant applies multiple filters simultaneously, **Then** results satisfy all filter criteria (AND logic)

---

### User Story 2 — Create Journal Entry with Balanced Lines and Dimension Pickers (Priority: P1)

An accountant creates a new journal entry via a dedicated creation page. The form presents lines with account, debit XOR credit, amount, description, and analytic dimension pickers (fund, project, budget item, encumbrance, payment order). The system enforces that total debits equal total credits before the entry can be submitted.

**Why this priority**: Entry creation is the core data-entry workflow. Without it, accountants cannot record any transactions. Balance validation is a financial integrity requirement.

**Independent Test**: Can be tested by creating an entry with balanced lines and verifying submission succeeds, then creating an entry with unbalanced lines and verifying submission is blocked.

**Acceptance Scenarios**:

1. **Given** an accountant creates a new journal entry, **When** adding a line, **Then** the line presents fields for account, debit or credit (XOR), amount, description, and optional analytic dimension pickers (fund, project, budget item, encumbrance, payment order)
2. **Given** an accountant adds a line with both debit and credit values, **When** saving the line, **Then** the system rejects the line with a validation error indicating exactly one of debit or credit must be set
3. **Given** an accountant adds multiple lines where total debits equal total credits, **When** submitting the entry, **Then** the entry is saved with status Draft and the balance is confirmed
4. **Given** an accountant adds multiple lines where total debits do not equal total credits, **When** submitting the entry, **Then** the system blocks submission with a clear error showing the imbalance amount
5. **Given** an accountant creates a line with no analytic dimensions assigned, **When** saving the line, **Then** the line saves successfully (dimensions are optional per line)
6. **Given** an accountant creates a line with one or more analytic dimensions, **When** saving the line, **Then** the selected dimensions are stored and displayed on the line

---

### User Story 3 — Lifecycle Actions with Approval and Status Panels (Priority: P1)

An accountant performs lifecycle actions on journal entries: submit, approve, post, reverse, and cancel. The entry progresses through the status lifecycle with shared approval and status panels showing the history of each transition.

**Why this priority**: Lifecycle actions are the mechanism by which entries move from draft to posted. Without them, entries cannot reach the general ledger.

**Independent Test**: Can be tested by performing each lifecycle action and verifying the status transitions correctly and the approval history panel records each decision.

**Acceptance Scenarios**:

1. **Given** a Draft journal entry, **When** the accountant submits it, **Then** the status changes to Submitted and the submission is recorded in the status history panel
2. **Given** a Submitted journal entry, **When** an authorized approver approves it, **Then** the status changes to Approved and the approval decision is recorded in the approval history panel
3. **Given** an Approved journal entry, **When** the accountant posts it, **Then** the status changes to Posted, the entry becomes immutable, and the posting is recorded in the status history panel
4. **Given** a Posted journal entry, **When** the accountant reverses it, **Then** a new reversal entry is created linked to the original, the original status changes to Reversed, and the reversal is recorded in the status history panel
5. **Given** a Draft or Submitted journal entry, **When** the accountant cancels it, **Then** the status changes to Cancelled and the cancellation is recorded in the status history panel
6. **Given** any journal entry, **When** viewing the entry, **Then** the status history panel displays the complete lifecycle trail with actor, decision, timestamp, and reason for each transition

---

### User Story 4 — Reversed Entry Links (Priority: P2)

An accountant viewing a reversed journal entry can see a link to its reversal entry, and viewing the reversal entry shows a link back to the original entry.

**Why this priority**: Traceability between original and reversal entries is important for audit but not blocking core workflows.

**Independent Test**: Can be tested by reversing an entry and verifying both the original and reversal entries display bidirectional links.

**Acceptance Scenarios**:

1. **Given** a Posted journal entry that has been reversed, **When** the accountant views the original entry, **Then** a visible link references the reversal entry
2. **Given** a reversal journal entry, **When** the accountant views the reversal, **Then** a visible link references the original entry
3. **Given** a reversal entry linked to an original, **When** the accountant clicks the link to the original, **Then** the original entry opens in detail view

---

### Edge Cases

- What happens when an accountant attempts to submit a journal entry with zero lines? → The system blocks submission with a validation error requiring at least one line
- What happens when an accountant attempts to post a Draft entry directly (skipping submit/approve)? → The system blocks the action; only Approved entries may be posted
- What happens when an accountant reverses a Draft or Submitted entry? → The system allows reversal of any non-Cancelled entry; reversal creates a new entry with status Draft
- What happens when two accountants attempt to approve the same Submitted entry simultaneously? → Optimistic concurrency control rejects the second approval with a conflict error
- What happens when analytic dimensions reference records that no longer exist? → The system displays the stored dimension IDs with a "record not found" indicator; editing the line prompts re-selection
- What happens when the journal entries list has no entries matching the current filters? → The system displays an empty state with a message indicating no results match the filters

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display the journal entries list using the renamed JournalEntry API endpoints (no references to the old Move/MoveLine naming), sorted newest first (entry number descending) by default, with columns: entry number, date, status badge, total debit, total credit, and fund summary
- **FR-002**: System MUST show status badges for each entry with distinct visual treatment for Draft, Submitted, Approved, Posted, Reversed, and Cancelled statuses
- **FR-003**: System MUST provide filters for period, fund, and status on the journal entries list, with AND logic when multiple filters are active
- **FR-004**: System MUST provide a journal entry creation form on a dedicated page with line-level fields: account, debit (XOR credit), amount, description, and analytic dimension pickers
- **FR-004a**: System MUST route the creation form via a dedicated URL path (e.g., `/journal-entries/new`) accessible from the list view
- **FR-005**: System MUST enforce debit XOR credit at the line level — a line cannot have both debit and credit set, and must have exactly one
- **FR-006**: System MUST compute and display running totals for debits and credits while adding lines, and block submission when totals do not balance
- **FR-007**: System MUST support five analytic dimension pickers on each line: fund, project, budget item, encumbrance, payment order — all optional
- **FR-008**: System MUST provide lifecycle action buttons (submit, approve, post, reverse, cancel) contextual to the entry's current status and the user's permissions
- **FR-009**: System MUST display a status history panel showing the complete lifecycle trail for each entry with actor, decision, timestamp, and reason
- **FR-010**: System MUST display an approval history panel for entries in Submitted or Approved status showing approval rule evaluations
- **FR-011**: System MUST enforce that Posted entries are immutable — no edit or delete actions available, only reverse
- **FR-012**: System MUST display bidirectional links between reversed entries and their reversal entries
- **FR-013**: System MUST handle the case where analytic dimension reference records no longer exist by showing a "record not found" indicator
- **FR-014**: System MUST display an empty state message when journal entries list has no results matching current filters
- **FR-015**: System MUST enforce that only Draft entries can be edited, and only Approved entries can be posted
- **FR-016**: System MUST use optimistic concurrency control on all journal entry mutations — reject updates if the entry was modified since last load
- **FR-017**: System MUST enforce the following valid status transitions: Draft → Submitted, Submitted → Approved, Approved → Posted, Posted → Reversed; Cancel is permitted only from Draft or Submitted status; Reverse is permitted from any status except Cancelled
- **FR-018**: System MUST display lifecycle action buttons contextual to valid transitions — only actions permitted from the current status are shown
- **FR-019**: System MUST display a journal entry detail view containing: header (entry number, date, status badge, total debits, total credits), lines table (account, debit/credit, amount, description, analytic dimensions), status history panel, approval history panel, lifecycle action buttons, and a reversal link when applicable

### Permissions

- **FR-PERM-001**: `Accounting.JournalEntries.Read` — View list + detail; required for all journal entry pages
- **FR-PERM-002**: `Accounting.JournalEntries.Create` — Create new journal entry; controls "New Entry" button visibility
- **FR-PERM-003**: `Accounting.JournalEntries.UpdateLines` — Add/edit/remove lines on Draft entries; controls line editor visibility
- **FR-PERM-004**: `Accounting.JournalEntries.Submit` — Submit Draft→Submitted; controls Submit button
- **FR-PERM-005**: `Accounting.JournalEntries.Approve` — Approve Submitted→Approved; controls Approve button
- **FR-PERM-006**: `Accounting.JournalEntries.Post` — Post Approved→Posted; controls Post button
- **FR-PERM-007**: `Accounting.JournalEntries.Reverse` — Reverse Posted→Reversed; controls Reverse button + dialog
- **FR-PERM-008**: `Accounting.JournalEntries.Cancel` — Cancel Draft/Submitted→Cancelled; controls Cancel button

### UI States Required

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| List | Skeleton rows | "لا توجد قيود" message | Error toast | Permission guard hides page | N/A | DataGrid with status chips |
| Create | N/A | N/A | Toast on submit error | Permission guard hides "New Entry" | N/A | Form with header + line editor + balance indicator |
| Detail | Spinner + "جاري تحميل القيد..." | N/A | Error card + back link | Permission guard hides actions | "القيد غير موجودة" + back link | Header card + lines table + actions + status log + approvals |

### Tests Expected

| ID | Page | Test Description |
|----|------|-----------------|
| T-023-001 | List | Renders entries with correct status badges |
| T-023-002 | List | Status filter returns matching entries |
| T-023-003 | List | Number search filters by entryNumber/ref |
| T-023-004 | List | Empty state shows message when no results |
| T-023-005 | List | Zero Move/MoveLine references in DOM |
| T-023-006 | Create | Form renders header fields and line editor |
| T-023-007 | Create | Debit XOR credit validation on lines |
| T-023-008 | Create | Balanced lines enable submit |
| T-023-009 | Create | Unbalanced lines block submit with error |
| T-023-010 | Create | Dimension pickers load reference data |
| T-023-011 | Detail | Header shows entry number/date/status/totals |
| T-023-012 | Detail | Lines table renders with dimensions |
| T-023-013 | Detail | Lifecycle buttons appear based on status |
| T-023-014 | Detail | Posted entries show no edit/delete |
| T-023-015 | Detail | Status log panel shows transitions |
| T-023-016 | Detail | Reversed entry shows link to reversal |
| T-023-017 | Detail | Reversal entry shows link to original |
| T-023-018 | Shared | StatusBadge renders correct color per status |

### Key Entities

- **JournalEntry**: An accounting journal entry with status lifecycle (Draft → Submitted → Approved → Posted → Reversed, with Cancel from Draft/Submitted), linked to optional reversal entries, carrying analytic dimensions context
- **JournalEntryLine**: A single debit or credit line within a journal entry, carrying analytic dimension references (fund, project, budget item, encumbrance, payment order) and the XOR constraint on debit/credit
- **EntryStatus**: Lifecycle status enum — Draft, Submitted, Approved, Posted, Reversed, Cancelled
- **ApprovalHistory**: Record of each lifecycle decision with actor, decision, timestamp, reason, and rule evaluation snapshot
- **DocumentStatusLog**: Append-only log of every status transition for audit trail

### Field Contract

#### JournalEntry Header Fields

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `entryNumber` | string | No (issued at creation) | System-generated, displayed in list + detail |
| `ref` | string? | Yes (create + edit) | Optional reference |
| `documentDate` | Date | Yes (create + edit) | Required |
| `postingDate` | Date? | No (set at post) | Read-only |
| `entryType` | string | Yes (create) | Standard/Reversing/Adjusting |
| `journalId` | number? | Yes (create) | Journal picker |
| `periodId` | number | No (auto-resolved) | Resolved from documentDate |
| `fiscalYearId` | number | No (auto-resolved) | Resolved from documentDate |
| `narration` | string? | Yes (create + edit) | Optional description |
| `sourceEventId` | number? | No | Source badge + link (read-only) |
| `isSystemGenerated` | boolean | No | Read-only lock badge |
| `rowVersion` | string | No (optimistic concurrency) | Conflict detection |
| `reversalOfId` | number? | No | Bidirectional reversal link |
| `reversalReason` | string? | No (set at reverse) | Required on reverse |
| `postedById` / `postedByName` | number? / string? | No | Audit (set at post) |
| `postedAt` | Date? | No | Audit (set at post) |
| `cancelledById` / `cancelledByName` | number? / string? | No | Audit (set at cancel) |
| `cancelledAt` | Date? | No | Audit (set at cancel) |

#### JournalEntryLine Fields

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `sequence` | number | Yes (auto-order) | Line sequence number |
| `accountId` | number | Yes (required) | Account picker |
| `description` | string? | Yes | Line description |
| `currencyId` | number | Yes | Currency picker |
| `exchangeRate` | number | Yes | Exchange rate |
| `debit` | number | Yes (XOR credit) | Debit amount |
| `credit` | number | Yes (XOR debit) | Credit amount |
| `costCenterId` | number? | Yes (optional) | Dimension picker |
| `fundId` | number? | Yes (optional) | Dimension picker |
| `projectId` | number? | Yes (optional) | Dimension picker |
| `budgetItemId` | number? | Yes (optional) | Dimension picker |
| `encumbranceId` | number? | Yes (optional) | Dimension picker |
| `paymentOrderId` | number? | Yes (optional) | Dimension picker |
| `id` (lineId) | long | No (server-assigned) | Line identifier for update/remove |

#### Line Constraints

- **Debit XOR Credit**: A line MUST have exactly one of debit or credit set. Both-zero invalid. Both-set invalid. Exact client messages: "يجب أن يكون السطر مدين أو دائن فقط" (both set) / "يجب إدخال مبلغ مدين أو دائن" (both zero).
- **Line Operations (Draft only)**: create line, update line, remove line (by long lineId). Lines are immutable after submit.

#### List View Columns

Entry number, documentDate, status badge (StatusBadge component), journalName, totalDebit, totalCredit. Sorted newest first (entryNumber descending).

#### List Filters

status chips (Draft/Submitted/Approved/Posted/Reversed/Cancelled/All), number search (by entryNumber/ref), period filter, fund filter. AND logic when multiple active.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Accountants can load the journal entries list and see all entries with correct status badges within 2 seconds of page load
- **SC-002**: Accountants can create a journal entry with balanced lines and submit it in under 3 minutes for a typical 5-line entry
- **SC-003**: Every Posted journal entry shows balanced totals (total debits equal total credits) with no exceptions
- **SC-004**: All analytic dimensions on journal entry lines are visible and correctly displayed in both list and detail views
- **SC-005**: Zero references to the old Move/MoveLine naming remain in the UI — all screens use JournalEntry terminology
- **SC-006**: Every lifecycle transition is recorded in the status history panel with actor, decision, timestamp, and reason
- **SC-007**: Reversed entries display bidirectional links to their reversal entries with 100% accuracy
- **SC-008**: Accountants stop using the broken old screens — all journal entry workflows route through the new UI

## Clarifications

### Session 2026-09-06

- Q: What are the valid status transitions for a journal entry? → A: Draft → Submitted → Approved → Posted → Reversed. Cancel from Draft or Submitted only. Reverse from any non-Cancelled status.
- Q: How should the journal entry creation form be presented? → A: Separate full page (dedicated route like `/journal-entries/new`).
- Q: What should the default sort order be for the journal entries list, and which columns must appear? → A: Newest first (date or entry number descending) — columns: entry number, date, status badge, total debit, total credit, fund summary.
- Q: What must appear on the journal entry detail view? → A: Header (entry number, date, status, totals), lines table (account, debit/credit, amount, description, dimensions), status history panel, approval history panel, lifecycle action buttons, reversal link if applicable.

## Assumptions

- The backend JournalEntry API endpoints (from spec 014) are functional and return correctly named data
- The existing approval workflow infrastructure (ApprovalHistory, IApprovalService) is operational
- Analytic dimension reference data (funds, projects, budget items, encumbrances, payment orders) exists and is accessible via the UI pickers
- The shared UI component library provides status badge, filter panel, and approval history components that can be reused
- Fiscal periods are already configured in the system for period filtering
- User permissions for journal entry lifecycle actions are defined in the RBAC model
