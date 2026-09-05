# Tasks: Accounting Core Refactor — JournalEntry Rename, Analytic Dimensions, PaymentOrder Aggregate Strip

**Input**: Design documents from `/specs/014-accounting-core-refactor/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-contracts.md, quickstart.md

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Exact file paths included in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: New enums, base entity renames, EF migration — foundational for all user stories

- [X] T001 [P] Create `EntryStatus` enum in `src/Domain/Accounting/Enums/EntryStatus.cs` with values Draft=0, Posted=1, Reversed=2
- [X] T002 [P] Create `EventCategory` enum in `src/Domain/Accounting/Enums/EventCategory.cs` with values Revenue=0, Expenditure=1, Transfer=2, Adjustment=3, Other=4
- [X] T003 [P] Create `EventType` enum in `src/Domain/Accounting/Enums/EventType.cs` with values ReceiptCollection=0, DepositClearing=1, PaymentExecution=2, Reversal=3
- [X] T004 [P] Rename `AccountingEventStatus` enum to `EventStatus` in `src/Domain/Accounting/Enums/EventStatus.cs` — values Pending=0, Posted=1, Reversed=2 (replaces Processing/Completed/Failed)
- [X] T005 Rename `Move.cs` to `JournalEntry.cs` in `src/Domain/Accounting/Entities/` — rename class, rename `ReversalOfMoveId` to `ReversalOfId`, rename navigation `ReversalOfMove` to `ReversalOf`, change `EntryStatus` from `string` to `EntryStatus` enum, rename `MoveId` FK references to `JournalEntryId`
- [X] T006 Rename `MoveLine.cs` to `JournalEntryLine.cs` in `src/Domain/Accounting/Entities/` — rename class, rename `MoveId` to `JournalEntryId`, rename navigation `Move` to `JournalEntry`
- [X] T007 [P] Add `InKind = 5` to `PaymentMethod` enum and renumber `Other` to 6 in `src/Domain/Payments/Enums/PaymentMethod.cs`
- [X] T008 Rename `MoveConfiguration.cs` to `JournalEntryConfiguration.cs` in `src/Infrastructure/Data/Configurations/Accounting/` — update entity type, table name to `JournalEntries`, rename FK columns
- [X] T009 Rename `MoveLineConfiguration.cs` to `JournalEntryLineConfiguration.cs` in `src/Infrastructure/Data/Configurations/Accounting/` — update entity type, table name to `JournalEntryLines`, rename FK column
- [X] T010 Update `AccountingEventConfiguration.cs` in `src/Infrastructure/Data/Configurations/Accounting/` — rename `SourceTable` to `SourceDocumentType`, `SourceId` to `SourceDocumentId`, add `JournalEntryId` FK, change `EventType` to enum, add unique constraint on (EventType, SourceDocumentType, SourceDocumentId)
- [X] T011 Global rename sweep — find and replace all references to `Move`/`MoveLine`/`MoveId`/`ReversalOfMoveId` across `src/Application/`, `src/Infrastructure/`, `src/Web/`, `tests/` using word-boundary matching
- [X] T012 Update `docs/database-schema.md` with renamed tables (`JournalEntries`, `JournalEntryLines`), new enums, and changed columns

**Checkpoint**: All entity renames complete, enums created, zero stale Move/MoveLine references — user story implementation can begin

---

## Phase 2: User Story 1 — JournalEntry CRUD with Consistent Naming (Priority: P1) — MVP

**Goal**: Accountants create, view, edit, post, and reverse journal entries using "JournalEntry" naming throughout

**Independent Test**: Create a journal entry, verify all screens/APIs/routes use "JournalEntry" naming, confirm status dropdown enforces Draft/Posted/Reversed only, verify posted entries are immutable, verify reversal creates linked entry

### Implementation for User Story 1

- [X] T013 [US1] Update `EntryStatus` string-to-enum EF conversion in `src/Infrastructure/Data/Configurations/Accounting/JournalEntryConfiguration.cs` — add `HasConversion<string>()` or value converter for migration compatibility
- [X] T014 [US1] Create EF migration `src/Infrastructure/Migrations/[timestamp]_AccountingCoreRefactor.cs` — RenameTable Move→JournalEntries, RenameTable MoveLines→JournalEntryLines, RenameColumn MoveId→JournalEntryId, data migration for EntryStatus string→enum, add CHECK constraint on Debit/Credit XOR
- [X] T015 [US1] Update `JournalEntryController` (or equivalent) in `src/Web/` — route becomes `/api/journal-entries`, actions use JournalEntry DTOs
- [X] T016 [US1] Update all MediatR handlers/commands/queries in `src/Application/` referencing Move/MoveLine to use JournalEntry/JournalEntryLine
- [X] T017 [US1] Update FluentValidation validators in `src/Application/` — rename Move validators to JournalEntry validators, add EntryStatus enum validation
- [X] T018 [US1] Update DTOs in `src/Application/` — rename Move DTOs to JournalEntry DTOs, EntryStatus as enum string in API responses
- [X] T019 [US1] Implement optimistic concurrency check in posting handler — catch `DbUpdateConcurrencyException`, return 409 Conflict (FR-021)
- [X] T020 [US1] Update ClientApp types and API clients for JournalEntry naming (if frontend exists in solution)

**Checkpoint**: JournalEntry CRUD fully functional with consistent naming, EntryStatus as enum, concurrency control on post

---

## Phase 3: User Story 2 — Analytic Dimensions on JournalEntryLines (Priority: P2)

**Goal**: Accountants tag journal entry lines with Fund, Project, BudgetItem, Encumbrance, PaymentOrder dimensions

**Independent Test**: Create journal entry lines with various dimension combinations, verify stored and returned in queries, verify availability engine can filter by dimensions

### Implementation for User Story 2

- [X] T021 [P] [US2] Add five nullable FK properties to `JournalEntryLine.cs`: FundId, ProjectId, BudgetItemId, EncumbranceId, PaymentOrderId — with navigation properties
- [X] T022 [P] [US2] Add FluentValidation rules for analytic dimensions (nullable FKs, optional) in `src/Application/` validators
- [X] T023 [US2] Update `JournalEntryLineConfiguration.cs` in `src/Infrastructure/Data/Configurations/Accounting/` — add FK configurations for five dimension columns, all nullable
- [X] T024 [US2] Create EF migration `src/Infrastructure/Migrations/[timestamp]_AddAnalyticDimensions.cs` — add five nullable FK columns to JournalEntryLines
- [X] T025 [US2] Update JournalEntryLine DTOs in `src/Application/` to include dimension fields in responses
- [X] T026 [US2] Update JournalEntry query handlers to include dimension navigation properties in projections

**Checkpoint**: Analytic dimensions stored, queryable, and filterable for cross-dimensional reporting

---

## Phase 4: User Story 3 — PaymentOrder without Stored Aggregates (Priority: P3)

**Goal**: PaymentOrder strips stored aggregates and inline approval columns; totals computed on demand

**Independent Test**: Create payment order with lines and deductions, call totals endpoint, verify computed values match expectations

### Implementation for User Story 3

- [X] T027 [US3] Drop stored aggregate fields from `PaymentOrder.cs` in `src/Domain/Payments/Entities/` — remove AmountNet, BaseAmountNet, TotalDeductionAmount, TotalNetAmount, TotalPaidAmount, TotalRemainingAmount, IsFullyPaid
- [X] T028 [US3] Drop inline approval columns from `PaymentOrder.cs` — remove ApprovedById, ApprovedAt, RejectedById, RejectedAt, CancelledById, CancelledAt, VoidedById, VoidedAt
- [X] T029 [US3] Rename `MoveId` to `JournalEntryId` in `PaymentOrder.cs`
- [X] T030 [US3] Drop computed fields from `PaymentOrderLine.cs` in `src/Domain/Payments/Entities/` — remove BaseAmount, AllocatedAmount, RemainingAmount, NetAmount
- [X] T031 [US3] Drop `BaseAmount` from `PaymentOrderDeduction.cs` in `src/Domain/Payments/Entities/`
- [X] T032 [US3] Update `PaymentOrderConfiguration.cs` in `src/Infrastructure/Data/Configurations/Payments/` — remove dropped columns, rename MoveId→JournalEntryId
- [X] T033 [US3] Update `PaymentOrderLineConfiguration.cs` in `src/Infrastructure/Data/Configurations/Payments/` — remove dropped columns
- [X] T034 [US3] Update `PaymentOrderDeductionConfiguration.cs` in `src/Infrastructure/Data/Configurations/Payments/` — remove BaseAmount
- [X] T035 [US3] Create EF migration `src/Infrastructure/Migrations/[timestamp]_StripPaymentOrderAggregates.cs` — drop columns, rename FK
- [X] T036 [US3] Implement computed totals service in `src/Application/` — net = AmountGross − deductions, paid = sum completed payments, remaining = net − paid
- [X] T037 [US3] Create `GET /api/payment-orders/{id}/totals` endpoint in `src/Web/` — returns computed net/paid/remaining/deductions
- [X] T038 [US3] Update PaymentOrder DTOs in `src/Application/` — remove aggregate fields, add computed totals endpoint response DTO
- [X] T039 [US3] Update all PaymentOrder query/command handlers to work without stored aggregates

**Checkpoint**: PaymentOrder stores only entered fields; totals always computed on demand

---

## Phase 5: User Story 4 — AccountingEvent Extended with Journal Link (Priority: P4)

**Goal**: AccountingEvent gains JournalEntryId, structured enums, and unique constraint preventing double-posting

**Independent Test**: Post an AccountingEvent, verify JournalEntryId is set, confirm duplicate posting is rejected

### Implementation for User Story 4

- [X] T040 [US4] Extend `AccountingEvent.cs` in `src/Domain/Accounting/Entities/` — add `JournalEntryId` (nullable FK), `EventCategory` (enum), change `EventType` from string to `EventType` enum, rename `Status` property type to `EventStatus`
- [X] T041 [US4] Rename `SourceTable` to `SourceDocumentType` and `SourceId` to `SourceDocumentId` in `AccountingEvent.cs`
- [X] T042 [US4] Update `AccountingEventConfiguration.cs` — add unique index on (EventType, SourceDocumentType, SourceDocumentId) where Status=Posted, configure new enum conversions
- [X] T043 [US4] Create EF migration `src/Infrastructure/Migrations/[timestamp]_ExtendAccountingEvent.cs` — add JournalEntryId FK, rename columns, add unique constraint, data migration for EventType string→enum
- [X] T044 [US4] Update AccountingEvent DTOs in `src/Application/` — include JournalEntryId, EventCategory, EventType (enum), Status (EventStatus)
- [X] T045 [US4] Update AccountingEvent handlers in `src/Application/` — set JournalEntryId on post, enforce unique constraint (catch duplicate → 409), update Status to Reversed when linked JournalEntry is reversed
- [X] T046 [US4] Update AccountingEvent controller/routes in `src/Web/` — extended contract with enums + JournalEntryId

**Checkpoint**: AccountingEvent fully linked to journal entries with structured enums and duplicate prevention

---

## Phase 6: User Story 5 — Unified PaymentMethod Enum (Priority: P5)

**Goal**: PaymentMethod enum includes InKind; all consumers use single unified enum

**Independent Test**: Create payments with each method value (including InKind), verify accepted and stored correctly

### Implementation for User Story 5

- [X] T047 [US5] Verify InKind addition in `src/Domain/Payments/Enums/PaymentMethod.cs` (done in T007 — confirm no stale references to old integer values)
- [X] T048 [US5] Update any PaymentMethod switch statements or mappings in `src/Application/` and `src/Web/` to handle InKind
- [X] T049 [US5] Create EF migration `src/Infrastructure/Migrations/[timestamp]_ExtendPaymentMethod.cs` if data migration needed for renumbered Other values (check existing data first)

**Checkpoint**: All 7 payment method values accepted system-wide

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, validation, cleanup

- [X] T050 Final search for any remaining "Move"/"MoveLine" references — zero stale references allowed (SC-001)
- [X] T051 Run `dotnet build` — verify zero errors
- [X] T052 Run `dotnet test` — verify all existing tests pass (SC-002)
- [X] T053 Run quickstart.md validation scenarios V1–V10 (code-level pass; runtime validation pending Docker/DB)
- [X] T054 Update `docs/database-schema.md` — final schema with all changes
- [X] T055 Code cleanup — remove any leftover TODO comments from old Move naming

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — starts immediately. Creates all new enums and renames base entities. BLOCKS all user stories.
- **Phase 2 (US1 — JournalEntry CRUD)**: Depends on Phase 1 complete — renames all references, implements EntryStatus enum, concurrency control
- **Phase 3 (US2 — Analytic Dimensions)**: Depends on Phase 1 complete — can start in parallel with Phase 2 if team capacity allows
- **Phase 4 (US3 — PaymentOrder Strip)**: Depends on Phase 1 complete — can start in parallel with Phase 2/3 if team capacity allows
- **Phase 5 (US4 — AccountingEvent Extension)**: Depends on Phase 1 complete — can start in parallel with Phase 2/3/4
- **Phase 6 (US5 — PaymentMethod InKind)**: Depends on Phase 1 complete (T007) — minimal work, can run anytime after Phase 1
- **Phase 7 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1 — JournalEntry CRUD)**: Foundation for all other stories — must complete first
- **US2 (P2 — Analytic Dimensions)**: Independent after Phase 1 — can parallel with US1
- **US3 (P3 — PaymentOrder Strip)**: Independent after Phase 1 — can parallel with US1/US2
- **US4 (P4 — AccountingEvent Extension)**: Independent after Phase 1 — can parallel with US1/US2/US3
- **US5 (P5 — PaymentMethod InKind)**: Independent after Phase 1 (T007) — trivial, can run anytime

### Parallel Opportunities

```
Phase 1 (sequential — rename is atomic):
  T001, T002, T003, T004, T007 ─── all [P] parallel
  T005, T006 ─── sequential (entity renames)
  T008, T009, T010 ─── after T005/T006
  T011 ─── after T008-T010 (global sweep)
  T012 ─── after T011

Phase 2–6 (parallel after Phase 1):
  US1 (T013-T020) ─── can run with US2/US3/US4/US5
  US2 (T021-T026) ─── can run with US1/US3/US4/US5
  US3 (T027-T039) ─── can run with US1/US2/US4/US5
  US4 (T040-T046) ─── can run with US1/US2/US3/US5
  US5 (T047-T049) ─── can run with any

Within each story:
  [P] tasks can run in parallel
  Models before services, services before endpoints
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (rename + enums + migration)
2. Complete Phase 2: User Story 1 (JournalEntry CRUD + naming + concurrency)
3. **STOP and VALIDATE**: Test JournalEntry CRUD independently
4. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 → Foundation ready
2. Phase 2 (US1) → JournalEntry CRUD works → Deploy (MVP)
3. Phase 3 (US2) → Analytic dimensions added → Deploy
4. Phase 4 (US3) → PaymentOrder stripped → Deploy
5. Phase 5 (US4) → AccountingEvent extended → Deploy
6. Phase 6 (US5) → PaymentMethod InKind → Deploy
7. Phase 7 → Polish & validation

### Parallel Team Strategy

With multiple developers after Phase 1:
- Developer A: US1 (JournalEntry CRUD) — highest priority
- Developer B: US3 (PaymentOrder Strip) — independent
- Developer C: US4 (AccountingEvent Extension) — independent
- Developer D: US2 (Analytic Dimensions) — independent
- Developer E: US5 (PaymentMethod InKind) — quick win
