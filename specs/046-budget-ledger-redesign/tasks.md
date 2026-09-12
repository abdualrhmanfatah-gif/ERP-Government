# Tasks: Budget Preparation — BudgetItemAllocations Model

**Input**: Design documents from `/specs/046-budget-ledger-redesign/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: TDD mandatory per Constitution Principle XI. Tests written FIRST, observed failing, then implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Domain entity and EF configuration for BudgetItemAllocation + migration

- [X] T001 [P] Create BudgetItemAllocation entity in src/Domain/Budgeting/Entities/BudgetItemAllocation.cs
- [X] T002 [P] Create BudgetItemAllocation EF configuration in src/Infrastructure/Data/Configurations/BudgetItemAllocationConfiguration.cs (UNIQUE constraint on BudgetId+BudgetItemId, FK Restrict)
- [X] T003 Register BudgetItemAllocation DbSet in src/Infrastructure/Data/ApplicationDbContext.cs
- [X] T004 Add BudgetItemAllocationId column to BudgetTransaction entity in src/Domain/Budgeting/BudgetTransaction.cs (int FK, required)
- [X] T005 Add BudgetItemAllocationId column to PaymentOrder entity in src/Domain/Payments/Entities/PaymentOrder.cs (int? FK, nullable)
- [X] T006 Update BudgetTransactionType enum in src/Domain/Budgeting/Enums/BudgetTransactionType.cs — remove Transfer as createable (keep value=3 in DB)
- [X] T007 Create EF migration: dotnet ef migrations add BudgetItemAllocations --project src/Infrastructure --startup-project src/Web
- [X] T008 Create migration data script: migrate BudgetTransactionLines data to BudgetTransaction records with BudgetItemAllocationId resolved from BudgetItemId

**Checkpoint**: Schema ready, entity registered, migration created

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core services and commands that ALL user stories depend on

- [X] T009 Implement BudgetItemAllocation CRUD commands in src/Application/Budgeting/Commands/BudgetItemAllocations/ (Create, Update, Delete)
- [X] T010 Implement BudgetItemAllocation validators in src/Application/Budgeting/Commands/BudgetItemAllocations/ (UNIQUE check, BudgetItem belongs to Budget, IsActive check, GL account uniqueness per budget/fiscal year)
- [X] T011 Implement BudgetItemAllocation queries in src/Application/Budgeting/Queries/BudgetItemAllocations/ (List, GetById with remaining/available computation)
- [X] T012 Update BudgetAvailabilityService in src/Application/Budgeting/Common/BudgetAvailabilityService.cs — compute ActualExpenditure from JournalEntryLines via AccountId, RemainingAmount, AvailableAmount
- [X] T013 Implement BudgetItemAllocation endpoint group in src/Web/Endpoints/Budgeting/BudgetItemAllocations.cs
- [X] T014 Add permission codes for BudgetItemAllocations in src/Application/Common/Security/PermissionCodes.cs
- [X] T015 Register BudgetItemAllocation policies in src/Web/DependencyInjection.cs

**Checkpoint**: Allocation CRUD, availability computation, and endpoints ready

---

## Phase 3: User Story 1 — Create Budget with Item Allocations (Priority: P1) MVP

**Goal**: Budget officer creates budget, adds allocations, submits, approves (amounts frozen), rejects with notes

**Independent Test**: Create budget with allocations, verify uniqueness, submit, approve (frozen), reject, resubmit

### Implementation for User Story 1

- [X] T016 [US1] Implement SubmitBudgetCommand in src/Application/Budgeting/Commands/Budgets/SubmitBudgetCommand.cs — transition Draft→Submitted, record ApprovalHistory
- [X] T017 [US1] Implement ApproveBudgetCommand in src/Application/Budgeting/Commands/Budgets/ApproveBudgetCommand.cs — transition Submitted→Approved, set ApprovedAmount=ProposedAmount for all allocations atomically, record ApprovalHistory + DocumentStatusLog
- [X] T018 [US1] Implement RejectBudgetCommand in src/Application/Budgeting/Commands/Budgets/RejectBudgetCommand.cs — transition Submitted→Draft with notes, record rejection
- [X] T019 [US1] Implement ActivateBudgetCommand in src/Application/Budgeting/Commands/Budgets/ActivateBudgetCommand.cs — transition Approved→Active, record status
- [X] T020 [US1] Implement CloseBudgetCommand in src/Application/Budgeting/Commands/Budgets/CloseBudgetCommand.cs — close budget, block new operations
- [X] T021 [US1] Implement CancelBudgetCommand in src/Application/Budgeting/Commands/Budgets/CancelBudgetCommand.cs — cancel only if no posted transactions/encumbrances
- [X] T022 [US1] Add Budget lifecycle endpoint actions in src/Web/Endpoints/Budgeting/Budgets.cs — submit/approve/reject/activate/close/cancel
- [X] T023 [US1] Implement Allocation CRUD validation: reject edit/delete when budget not in Draft
- [X] T024 [US1] Implement Allocation deletion guard: reject if linked BudgetTransactions exist

**Checkpoint**: Full budget preparation workflow functional

---

## Phase 4: User Story 3 — Compute Actual Expenditure from Journal Entries (Priority: P1)

**Goal**: ActualExpenditure computed from posted JournalEntryLines via GL account, considering fiscal year, reversals, cancellations

**Independent Test**: Post journal entries against linked account, verify net expenditure, reverse some, cancel some

### Implementation for User Story 3

- [X] T025 [US3] Implement ActualExpenditure computation in src/Application/Budgeting/Common/BudgetAvailabilityService.cs — SUM(Debit)-SUM(Credit) WHERE AccountId=BudgetItem.AccountId AND FiscalYearId AND Posted AND not reversed/cancelled
- [X] T026 [US3] Implement GL account uniqueness validation in src/Application/Budgeting/Commands/BudgetItemAllocations/ — reject if same AccountId linked to multiple items in same budget/fiscal year
- [X] T027 [US3] Handle BudgetItem with no AccountId — return 0 expenditure with note

**Checkpoint**: ActualExpenditure accurate from ledger data

---

## Phase 5: User Story 4 — BudgetTransaction Per Allocation (Priority: P1)

**Goal**: Each BudgetTransaction linked to one BudgetItemAllocation, atomic ApprovedAmount update on post

**Independent Test**: Create transaction per allocation, post, verify atomic ApprovedAmount update, verify negative floor

### Implementation for User Story 4

- [X] T028 [US4] Implement CreateBudgetTransactionCommand in src/Application/Budgeting/Commands/BudgetTransactions/ — validate allocation exists, budget Active, linked
- [X] T029 [US4] Implement PostBudgetTransactionCommand in src/Application/Budgeting/Commands/BudgetTransactions/PostBudgetTransactionCommand.cs — atomic ApprovedAmount update, reject if would go negative
- [X] T030 [US4] Implement SubmitBudgetTransactionCommand, ApproveBudgetTransactionCommand in src/Application/Budgeting/Commands/BudgetTransactions/
- [X] T031 [US4] Implement BudgetTransaction queries in src/Application/Budgeting/Queries/BudgetTransactions/ (List, GetById)
- [X] T032 [US4] Implement BudgetTransaction endpoint group in src/Web/Endpoints/Budgeting/BudgetTransactions.cs
- [X] T033 [US4] Add BudgetTransaction permission codes and register policies

**Checkpoint**: Per-allocation transactions functional with atomic updates

---

## Phase 6: User Story 5 — Reverse Posted Transactions (Priority: P2)

**Goal**: Reversal creates new transaction with inverted direction, original untouched, ApprovedAmount restored

**Independent Test**: Post transaction, reverse, verify restoration, verify no double-reversal

### Implementation for User Story 5

- [X] T034 [US5] Implement ReverseBudgetTransactionCommand in src/Application/Budgeting/Commands/BudgetTransactions/ReverseBudgetTransactionCommand.cs — create Reversal type, invert direction/amount, link via ReversalOfId
- [X] T035 [US5] Validate reversal: only Posted transactions, no double-reversal (ReversalOfId must be null)
- [X] T036 [US5] Add reverse action to BudgetTransaction endpoint

**Checkpoint**: Reversal lifecycle complete

---

## Phase 7: User Story 2 — Link Disbursement Orders to Allocations (Priority: P1)

**Goal**: PaymentOrder linked to BudgetItemAllocation, remaining amount decreases after payment

**Independent Test**: Create payment order linked to allocation, post journal entry, verify remaining decrease

### Implementation for User Story 2

- [X] T037 [US2] Add BudgetItemAllocationId to PaymentOrder create/update commands in src/Application/Payments/Commands/
- [X] T038 [US2] Validate allocation exists, is active, belongs to Active budget on PaymentOrder create
- [X] T039 [US2] Implement allocation remaining/available display on PaymentOrder detail query
- [X] T040 [US2] Add allocation reference to PaymentOrder DTO and endpoint responses

**Checkpoint**: Payment orders linked to allocations

---

## Phase 8: User Story 6 — Budget Preparation Workflow (Priority: P1)

**Goal**: Full lifecycle with ApprovalHistory + DocumentStatusLog, rejection notes, resubmission audit trail

**Independent Test**: Walk through all lifecycle states, verify audit records at each transition

### Implementation for User Story 6

- [X] T041 [US6] Implement IDocumentStatusLogger calls in all Budget lifecycle commands (Submit, Approve, Reject, Activate, Close, Cancel)
- [X] T042 [US6] Implement ApprovalHistory recording in all Budget lifecycle commands
- [X] T043 [US6] Store rejection reason + notes on RejectBudgetCommand, make visible on re-entry to Draft
- [X] T044 [US6] Create new ApprovalHistory entry on resubmission (not reuse previous)

**Checkpoint**: Full audit trail for budget preparation workflow

---

## Phase 9: User Story 7 — Existing Controls Preserved (Priority: P2)

**Goal**: Budget control (None/Warning/Blocking), AllowOverrun inheritance, encumbrance availability preserved

**Independent Test**: Configure Blocking, attempt exceed, verify rejection. Verify encumbrance uses allocation amounts.

### Implementation for User Story 7

- [X] T045 [US7] Update BudgetAvailabilityService to compute AvailableAmount from allocation model (ApprovedAmount - ActualExpenditure - OutstandingEncumbrance)
- [X] T046 [US7] Preserve AllowOverrun inheritance chain: BudgetItem → Budget → BudgetType
- [X] T047 [US7] Preserve ControlMethod evaluation against AvailableAmount (not RemainingAmount)
- [X] T048 [US7] Verify encumbrance availability uses allocation's ApprovedAmount as basis

**Checkpoint**: All existing budget controls working with new model

---

## Phase 10: Transfer Removal

**Goal**: Transfer type removed from createable options, UI does not expose it

- [X] T049 Filter Transfer from BudgetTransactionType in create endpoints and UI
- [X] T050 Ensure existing Transfer rows remain queryable and displayable
- [X] T051 Document Reduction + Supplement pattern for fund movement in spec notes

**Checkpoint**: Transfer no longer createable

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Frontend, documentation, final validation

- [X] T052 [P] Create budget preparation frontend pages in src/Web/ClientApp/src/features/budgeting/budgets/ — list, detail with allocation grid, lifecycle buttons
- [X] T053 [P] Create budget item allocation frontend pages in src/Web/ClientApp/src/features/budgeting/budget-item-allocations/ — create/edit/delete inline
- [X] T054 [P] Create budget transaction frontend pages in src/Web/ClientApp/src/features/budgeting/budget-transactions/ — list, create, lifecycle actions
- [X] T055 [P] Update payment order frontend to show BudgetItemAllocationId reference and remaining amount
- [X] T056 [P] Run frontend lint and build: cd src/Web/ClientApp && npm run lint && npm run build
- [X] T057 [P] Regenerate NSwag client: cd src/Web/ClientApp && npm run generate-api
- [X] T058 Update docs/database-schema.md with BudgetItemAllocations table and modified entities
- [X] T059 Run quickstart.md validation scenarios end-to-end
- [X] T060 Run full backend test suite: dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests && dotnet test tests/Infrastructure.IntegrationTests && dotnet test tests/Web.AcceptanceTests

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2
- **Phase 4 (US3)**: Depends on Phase 2 — can parallel with Phase 3
- **Phase 5 (US4)**: Depends on Phase 2 — can parallel with Phase 3/4
- **Phase 6 (US5)**: Depends on Phase 5 (needs transaction model)
- **Phase 7 (US2)**: Depends on Phase 2 — can parallel with Phase 3-6
- **Phase 8 (US6)**: Depends on Phase 3 (needs lifecycle commands)
- **Phase 9 (US7)**: Depends on Phase 2, Phase 4 (needs availability service)
- **Phase 10 (Transfer)**: Depends on Phase 5
- **Phase 11 (Polish)**: Depends on all user stories

### User Story Dependencies

- **US1 (P1)**: Foundation only — independent
- **US3 (P1)**: Foundation only — independent
- **US4 (P1)**: Foundation only — independent
- **US5 (P2)**: Depends on US4 (needs transaction model)
- **US2 (P1)**: Foundation only — independent
- **US6 (P1)**: Depends on US1 (needs lifecycle commands)
- **US7 (P2)**: Depends on US3 (needs availability service)

### Parallel Opportunities

```bash
# After Phase 2 (Foundational) completes:
# Group A (Allocation + Lifecycle):
Task: "T016-T024 [US1] Budget preparation workflow"
Task: "T041-T044 [US6] Audit trail"

# Group B (Transactions):
Task: "T028-T033 [US4] Per-allocation transactions"
Task: "T034-T036 [US5] Reversal"

# Group C (Expenditure):
Task: "T025-T027 [US3] ActualExpenditure computation"

# Group D (Payments):
Task: "T037-T040 [US2] PaymentOrder linkage"
```

---

## Implementation Strategy

### MVP First (US1 + US3 + US4)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: US1 (budget preparation workflow)
4. Complete Phase 4: US3 (actual expenditure)
5. Complete Phase 5: US4 (per-allocation transactions)
6. **STOP and VALIDATE**: Run quickstart scenarios 1-3
7. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 + US3 + US4 → Test independently → Deploy/Demo (MVP!)
3. Add US5 (reversal) → Test → Deploy/Demo
4. Add US2 (payment linkage) → Test → Deploy/Demo
5. Add US6 + US7 (workflow + controls) → Test → Deploy/Demo
6. Add Transfer removal + Polish → Final release

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- TDD: Write tests FIRST, observe red, implement, observe green
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
