# Feature Specification: Accounting Core Refactor — JournalEntry Rename, Analytic Dimensions, PaymentOrder Aggregate Strip

**Feature Branch**: `014-accounting-core-refactor`

**Created**: 2026-09-05

**Status**: Draft

**Input**: User description: "Rename Move → JournalEntry and MoveLine → JournalEntryLine across the entire solution, add analytic dimensions to entry lines, convert EntryStatus to an enum, extend AccountingEvent into a first-class link to the journal, and strip ALL stored aggregates + inline approval columns from the Payments domain (computed at query time per constitution rule 23). Executes AFTER Command 1 (ApprovalHistory extension + Party exist)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — JournalEntry CRUD with Consistent Naming (Priority: P1)

Accountants create, view, edit, and post journal entries in the accounting system. All references use the terminology "JournalEntry" and "JournalEntryLine" instead of the previous "Move"/"MoveLine" naming. Entry status is a structured value (Draft, Posted, Reversed) rather than free text.

**Why this priority**: The rename is the foundation for all other changes. Without consistent naming, no other feature can build correctly on top of it.

**Independent Test**: Can be fully tested by creating a journal entry, verifying all screens/APIs/routes use "JournalEntry" naming, and confirming status dropdown enforces Draft/Posted/Reversed only.

**Acceptance Scenarios**:

1. **Given** an accountant navigates to the journal entries list, **When** the page loads, **Then** all labels, columns, filters, and API routes display "JournalEntry" (not "Move")
2. **Given** a new journal entry is created with status "Draft", **When** the accountant posts it, **Then** the status changes to "Posted" and the entry becomes immutable (no further edits allowed except via reversal)
3. **Given** a posted journal entry, **When** the accountant attempts to reverse it, **Then** a new reversal entry is created linked to the original, and the original's status changes to "Reversed"
4. **Given** a journal entry line, **When** both Debit and Credit are set to positive values, **Then** the system rejects the line with a clear validation error
5. **Given** a Draft journal entry loaded by User A, **When** User B modifies and posts the same entry before User A submits, **Then** User A's post is rejected with a concurrency conflict error and must refresh before retrying

---

### User Story 2 — Analytic Dimensions on JournalEntryLines (Priority: P2)

Accountants tag each journal entry line with analytic dimensions: Fund, Project, BudgetItem, Encumbrance, and PaymentOrder. These dimensions enable cross-dimensional reporting and availability calculations.

**Why this priority**: Analytic dimensions unlock cross-dimensional reporting and are required by the availability engine (Command 0). Without them, reporting by fund/project is impossible.

**Independent Test**: Can be tested by creating journal entry lines with various dimension combinations and verifying they are stored and returned in queries.

**Acceptance Scenarios**:

1. **Given** a journal entry line, **When** the accountant assigns a Fund and Project, **Then** both dimensions are stored and returned when querying the line
2. **Given** a journal entry line, **When** no dimensions are assigned, **Then** all dimension fields are null and the line saves successfully
3. **Given** a posted journal entry, **When** querying its lines, **Then** all assigned analytic dimensions are visible in the response
4. **Given** the availability engine, **When** it calculates expended amounts, **Then** it can filter/group by Fund, Project, BudgetItem, Encumbrance, or PaymentOrder dimensions

---

### User Story 3 — PaymentOrder without Stored Aggregates (Priority: P3) ✅ VERIFIED

> **Note**: PaymentOrder aggregate/approval strip was implemented in specs/016-unified-party-document. This story verifies the strip is complete and adds the computed totals endpoint.

Payment orders no longer store computed totals (AmountNet, TotalPaidAmount, TotalRemainingAmount, IsFullyPaid, etc.) or inline approval columns (ApprovedById, RejectedById, etc.). Totals are computed on demand from lines, deductions, and completed payments. Approval status comes exclusively from ApprovalHistory.

**Why this priority**: Eliminates data consistency risks from stale stored aggregates and consolidates approval tracking into a single source of truth.

**Independent Test**: Can be tested by creating a payment order with lines and deductions, then calling the totals endpoint to verify computed values match expectations.

**Acceptance Scenarios**:

1. **Given** a payment order with AmountGross of 10,000 and deductions totaling 1,500, **When** the totals endpoint is called, **Then** net = 8,500, paid = sum of completed payments, remaining = net − paid
2. **Given** a payment order has no stored aggregate fields (AmountNet, TotalPaidAmount, etc.), **When** the entity is loaded, **Then** only AmountGross, DeductionAmount, and entered fields are present
3. **Given** a payment order, **When** approval status is queried, **Then** it reflects the latest ApprovalHistory record (not inline columns)
4. **Given** a payment order line, **When** it is saved, **Then** only Amount and TaxAmount are stored (BaseAmount, AllocatedAmount, RemainingAmount, NetAmount are computed)
5. **Given** a payment order deduction, **When** it is saved, **Then** only Amount and DeductionPercent are stored (BaseAmount is computed)

---

### User Story 4 — AccountingEvent Extended with Journal Link (Priority: P4)

AccountingEvent records gain a direct link to the journal entry they produced, plus structured enums for EventCategory, EventType, and Status. A unique constraint prevents double-posting the same source operation.

**Why this priority**: Provides full traceability from business events to accounting entries and prevents duplicate posting bugs.

**Independent Test**: Can be tested by posting an AccountingEvent, verifying the JournalEntryId is set, and confirming a second posting attempt for the same EventType + source document is rejected.

**Acceptance Scenarios**:

1. **Given** an AccountingEvent is posted, **When** the posting completes, **Then** JournalEntryId is populated with the created journal entry's identifier
2. **Given** an AccountingEvent with EventType "ReceiptCollection" for source document X, **When** a second posting attempt is made for the same EventType + document X, **Then** the system rejects it as a duplicate
3. **Given** an AccountingEvent, **When** it is created, **Then** EventCategory (Revenue/Expenditure/Transfer/Adjustment/Other), EventType (e.g., ReceiptCollection/DepositClearing/PaymentExecution/Reversal), and Status (Pending/Posted/Reversed) are all set
4. **Given** an AccountingEvent with Status "Posted", **When** its linked JournalEntry is reversed, **Then** the event status changes to "Reversed"

---

### User Story 5 — Unified PaymentMethod Enum (Priority: P5)

The PaymentMethod enum includes all payment types in a single definition: Cash, BankTransfer, Check, CreditCard, WireTransfer, InKind, and Other. All consumers reference this single enum.

**Why this priority**: Eliminates duplicate or inconsistent payment method definitions across the system.

**Independent Test**: Can be tested by creating payments with each method value and verifying they are accepted and stored correctly.

**Acceptance Scenarios**:

1. **Given** a payment with method "InKind", **When** the payment is saved, **Then** the method is stored as the InKind enum value
2. **Given** any payment method from the unified set (Cash/BankTransfer/Check/CreditCard/WireTransfer/InKind/Other), **When** a payment is created with that method, **Then** it is accepted without error
3. **Given** a payment method value outside the enum set, **When** a payment is submitted, **Then** the system rejects it with a validation error

---

### Edge Cases

- What happens when a JournalEntry in "Draft" status is partially modified while another user attempts to post it simultaneously? → Resolved: Optimistic concurrency control rejects the post with a conflict error; the posting user must refresh and retry (FR-021)
- How does the system handle migration of existing string-based EntryStatus values to the new enum?
- What happens when a PaymentOrder's computed net is negative (deductions exceed gross)?
- How does the system handle an AccountingEvent where the source document has been deleted before posting?
- What happens when a journal entry line is created with analytic dimensions referencing non-existent Fund/Project/BudgetItem records?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST rename all "Move" references to "JournalEntry" and "MoveLine" to "JournalEntryLine" across the entire solution (entities, repositories, services, controllers, DTOs, validators, API routes, client app, reports)
- **FR-002**: System MUST provide CRUD operations on JournalEntry via `/api/journal-entries` including post and reverse actions with filters
- **FR-003**: System MUST enforce that each JournalEntryLine has exactly one of Debit or Credit greater than zero (XOR constraint) via database CHECK constraint and application-level validation
- **FR-004**: System MUST convert EntryStatus from free text to an enum with values: Draft, Posted, Reversed — with migration of existing data
- **FR-005**: System MUST make Posted JournalEntries immutable — corrections only via reversal entries using the ReversalOfId pattern
- **FR-021**: System MUST use optimistic concurrency control on JournalEntry posting — reject the post if the entry was modified since the user last loaded it, requiring a refresh before retrying
- **FR-006**: System MUST support five analytic dimension FKs on JournalEntryLine: FundId, ProjectId, BudgetItemId, EncumbranceId, PaymentOrderId — all nullable
- **FR-007**: System MUST expose PaymentOrder computed totals (net, paid, remaining, deductions) via a computed endpoint — values are never stored
- **FR-008**: System MUST drop stored aggregate fields from PaymentOrder: AmountNet, BaseAmountNet, TotalDeductionAmount, TotalNetAmount, TotalPaidAmount, TotalRemainingAmount, IsFullyPaid
- **FR-009**: System MUST drop inline approval columns from PaymentOrder: ApprovedById, ApprovedAt, RejectedById, RejectedAt, CancelledById, CancelledAt, VoidedById, VoidedAt — approval status comes from ApprovalHistory only
- **FR-010**: System MUST drop computed fields from PaymentOrderLine: BaseAmount, AllocatedAmount, RemainingAmount, NetAmount (Amount and TaxAmount remain as entered inputs)
- **FR-011**: System MUST drop BaseAmount from PaymentOrderDeduction (Amount and DeductionPercent remain as entered inputs)
- **FR-012**: System MUST add JournalEntryId (nullable FK) to AccountingEvent — set when the event posts
- **FR-013**: System MUST add EventCategory enum to AccountingEvent: Revenue, Expenditure, Transfer, Adjustment, Other
- **FR-014**: System MUST add EventType enum to AccountingEvent as operation-level discriminator with values: ReceiptCollection, DepositClearing, PaymentExecution, Reversal, Other, PurchaseOrderApproved, GoodsReceiptNoteApproved, BankReconciliationPosted, RevenueReceiptPosted, JournalEntryPosted, DepreciationPosted, PaymentOrderExecuted
- **FR-015**: System MUST add Status enum (EventStatus: Pending, Posted, Reversed) to AccountingEvent
- **FR-016**: System MUST enforce a unique constraint on AccountingEvent: one posted event per (EventType, SourceDocumentType, SourceDocumentId) — a three-column composite key preventing double-posting of the same source operation across any entity type
- **FR-017**: System MUST add "InKind" to the PaymentMethod enum, producing the final set: Cash, BankTransfer, Check, CreditCard, WireTransfer, InKind, Other
- **FR-018**: System MUST update `docs/database-schema.md` to reflect all schema changes
- **FR-019**: System MUST preserve all existing functionality during the rename — zero stale references to "Move" or "MoveLine" may remain
- **FR-020**: System MUST use Entity Framework migration for all database schema changes

### Key Entities

- **JournalEntry** (renamed from Move): Represents an accounting journal entry with status (Draft/Posted/Reversed), linked to an optional AccountingEvent, and supporting reversal via ReversalOfId
- **JournalEntryLine** (renamed from MoveLine): A single debit or credit line within a journal entry, carrying analytic dimensions (Fund, Project, BudgetItem, Encumbrance, PaymentOrder) and the XOR constraint on Debit/Credit
- **AccountingEvent** (extended): A business event (e.g., receipt, deposit, payment execution) linked to the journal entry it produced, with structured categories/types/status and a three-column unique constraint (EventType, SourceDocumentType, SourceDocumentId) preventing duplicate posting
- **PaymentOrder** (stripped): A payment planning document with entered amounts (AmountGross, DeductionAmount) but no stored computed aggregates — totals are computed on demand
- **PaymentOrderLine** (stripped): A line item on a payment order with entered Amount and TaxAmount only
- **PaymentOrderDeduction** (stripped): A deduction on a payment order with entered Amount and DeductionPercent only
- **PaymentMethod** (enum): Unified payment method with values: Cash, BankTransfer, Check, CreditCard, WireTransfer, InKind, Other
- **EntryStatus** (enum): Journal entry status with values: Draft, Posted, Reversed
- **EventCategory** (enum): Accounting event category — Revenue, Expenditure, Transfer, Adjustment, Other
- **EventType** (enum): Accounting event operation discriminator — ReceiptCollection, DepositClearing, PaymentExecution, Reversal, etc.
- **EventStatus** (enum): Accounting event status — Pending, Posted, Reversed

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero stale "Move" or "MoveLine" references remain in the entire solution after the rename
- **SC-002**: All existing JournalEntry (formerly Move) functionality works identically after the rename — no regressions in CRUD, posting, or reversal flows
- **SC-003**: PaymentOrder computed totals are always accurate when queried — no stored values drift from actual calculations
- **SC-004**: Double-posting of the same AccountingEvent (same EventType + source document) is blocked by the unique constraint
- **SC-005**: All analytic dimensions on JournalEntryLines are queryable and filterable for cross-dimensional reporting
- **SC-006**: EntryStatus values are strictly constrained to Draft/Posted/Reversed — no invalid status values exist in the database
- **SC-007**: Approval status for PaymentOrders is sourced exclusively from ApprovalHistory — no inline approval columns remain

## Assumptions

- Command 1 (ApprovalHistory extension + Party exist) has been completed and deployed before this feature executes
- The existing Move/MoveLine tables already have data that must be migrated (not a greenfield creation)
- The availability engine (Command 0 service) already consumes Expended from posted JournalEntryLines and requires no interface changes for analytic dimension support
- The ReversalOfId pattern on JournalEntry does not yet exist and must be added
- All existing API consumers (ClientApp, reports engine) will be updated in-place — no parallel old/new API versioning
- The PaymentMethod enum is currently defined without InKind and must be extended
- The "source document" for AccountingEvent's unique constraint is identified by three columns: EventType (enum), SourceDocumentType (string/enum identifying the entity type), and SourceDocumentId (identifier of the triggering entity)

## Clarifications

### Session 2026-09-05

- Q: How should the "source document id" for AccountingEvent's duplicate-posting prevention be identified? → A: Three-column composite: EventType + SourceDocumentType + SourceDocumentId
- Q: How should the system handle concurrent modification when a Draft JournalEntry is being edited while another user posts it? → A: Optimistic concurrency with row-version check — reject the post if modified since last load
