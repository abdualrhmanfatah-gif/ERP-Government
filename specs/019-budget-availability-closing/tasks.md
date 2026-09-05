# Tasks: Financial Control Layer — Multi-Dimension Budget Availability, Year-End Operations, and Final Account

**Input**: Design documents from `/specs/019-budget-availability-closing/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: TDD mandatory per Constitution Principle XI. Test tasks are included and must be executed first (red → green → refactor).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Entity definitions, enums, DbContext registration, permission codes

- [x] T001 [P] Create YearClosingRunStatus enum in src/Domain/Budgeting/Enums/YearClosingRunStatus.cs
- [x] T002 [P] Create FinalAccountStatus enum in src/Domain/Budgeting/Enums/FinalAccountStatus.cs
- [x] T003 [P] Create YearClosingRunType enum in src/Domain/Budgeting/Enums/YearClosingRunType.cs
- [x] T004 [P] Create YearClosingRun entity in src/Domain/Budgeting/Entities/YearClosingRun.cs
- [x] T005 [P] Create FinalAccount entity in src/Domain/Budgeting/Entities/FinalAccount.cs
- [x] T006 [P] Create FinalAccountLine entity in src/Domain/Budgeting/Entities/FinalAccountLine.cs
- [x] T007 Register DbSets in src/Application/Common/Interfaces/IApplicationDbContext.cs (YearClosingRuns, FinalAccounts, FinalAccountLines)
- [x] T008 Register DbSets in src/Infrastructure/Data/ApplicationDbContext.cs
- [x] T009 [P] Create YearClosingRun EF configuration in src/Infrastructure/Data/Configurations/YearClosingRunConfiguration.cs (unique index on FiscalYearId WHERE Status = Completed)
- [x] T010 [P] Create FinalAccount EF configuration in src/Infrastructure/Data/Configurations/FinalAccountConfiguration.cs (unique index on FiscalYearId)
- [x] T011 [P] Create FinalAccountLine EF configuration in src/Infrastructure/Data/Configurations/FinalAccountLineConfiguration.cs (unique index on FinalAccountId + Dimension + DimensionId)
- [x] T012 Add EF migration for YearClosingRun, FinalAccount, FinalAccountLine entities
- [x] T013 Add PermissionCodes for FinancialControl in src/Application/Common/Security/PermissionCodes.cs (FinancialControlLapseYear, FinancialControlApproveFinalAccount)
- [x] T014 Register authorization policies in src/Web/DependencyInjection.cs for new permission codes

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared DTOs, availability service extension, and query infrastructure that all user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T015 [P] Create AvailabilityBreakdownDto in src/Application/Budgeting/Common/AvailabilityBreakdownDto.cs
- [x] T016 [P] Create AvailabilityBreakdownTotalDto in src/Application/Budgeting/Common/AvailabilityBreakdownTotalDto.cs
- [x] T017 Extend IBudgetAvailabilityService with GetAvailabilityBreakdownAsync method in src/Application/Budgeting/Common/BudgetAvailabilityService.cs
- [x] T018 Implement GetAvailabilityBreakdownAsync in BudgetAvailabilityService — group by fund/program/project/budget item, filter by fiscal year, sum appropriations/encumbrances/payments per dimension in src/Application/Budgeting/Common/BudgetAvailabilityService.cs
- [x] T019 [P] Create GetAvailabilityBreakdownQuery in src/Application/Budgeting/Queries/FinancialControl/GetAvailabilityBreakdown/GetAvailabilityBreakdownQuery.cs

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Multi-Dimension Budget Availability Check (Priority: P1) 🎯 MVP

**Goal**: Controller selects any budget line and gets a single breakdown showing appropriations, open encumbrances, and executed payments per dimension: fund, program, project, and budget item for the selected fiscal year.

**Independent Test**: Set up appropriations across multiple funds/programs/projects; create encumbrances and payments; query availability and verify breakdown matches expected values per dimension; verify totals sum correctly.

### Tests for User Story 1 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T020 [P] [US1] Unit test: GetAvailabilityBreakdown returns correct breakdown per fund/program/project/item — in tests/Application.UnitTests/Budgeting/AvailabilityBreakdownTests.cs
- [x] T021 [P] [US1] Unit test: GetAvailabilityBreakdown computes available = appropriation - encumbrances - payments — in tests/Application.UnitTests/Budgeting/AvailabilityBreakdownTests.cs
- [x] T022 [P] [US1] Unit test: GetAvailabilityBreakdown returns empty for non-existent budget item — in tests/Application.UnitTests/Budgeting/AvailabilityBreakdownTests.cs
- [x] T023 [P] [US1] Unit test: GetAvailabilityBreakdown shows negative availability when encumbrances exceed appropriations on one dimension — in tests/Application.UnitTests/Budgeting/AvailabilityBreakdownTests.cs
- [x] T024 [P] [US1] Unit test: GetAvailabilityBreakdown scoped to single fiscal year — in tests/Application.UnitTests/Budgeting/AvailabilityBreakdownTests.cs

### Implementation for User Story 1

- [x] T025 [US1] Implement GetAvailabilityBreakdownQuery handler in src/Application/Budgeting/Queries/FinancialControl/GetAvailabilityBreakdown/GetAvailabilityBreakdownQuery.cs (depends on T015-T018)
- [x] T026 [US1] Implement Availability endpoint group (GET /api/Availability/{budgetItemId}) in src/Web/Endpoints/FinancialControl/Availability.cs (depends on T025, T019)

**Checkpoint**: Multi-dimensional availability query fully functional — returns breakdown by all dimensions

---

## Phase 4: User Story 2 — Year-End Lapse Run (Priority: P2)

**Goal**: At fiscal year close, unspent appropriations and open encumbrances lapse; payments against lapsed items blocked; lapse recorded as append-only log. Reopening allowed before final account issuance, blocked if payments exist against lapsed items.

**Independent Test**: Run lapse for fiscal year — amounts nullified, YearClosingRun created. Attempt payment against lapsed item — blocked. Attempt duplicate lapse — rejected. Reopen year — amounts restored. Reopen with payments against lapsed items — blocked.

### Tests for User Story 2 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T027 [P] [US2] Unit test: LapseFiscalYear nullifies appropriations and marks encumbrances lapsed — in tests/Application.UnitTests/Budgeting/LapseFiscalYearTests.cs
- [x] T028 [P] [US2] Unit test: LapseFiscalYear creates YearClosingRun append-only record — in tests/Application.UnitTests/Budgeting/LapseFiscalYearTests.cs
- [x] T029 [P] [US2] Unit test: LapseFiscalYear idempotent — duplicate run rejected — in tests/Application.UnitTests/Budgeting/LapseFiscalYearTests.cs
- [x] T030 [P] [US2] Unit test: Payment against lapsed item blocked with clear error — in tests/Application.UnitTests/Budgeting/LapseFiscalYearTests.cs
- [x] T031 [P] [US2] Unit test: ReopenFiscalYear restores amounts when no payments against lapsed items — in tests/Application.UnitTests/Budgeting/ReopenFiscalYearTests.cs
- [x] T032 [P] [US2] Unit test: ReopenFiscalYear blocked when payments exist against lapsed items — in tests/Application.UnitTests/Budgeting/ReopenFiscalYearTests.cs
- [x] T033 [P] [US2] Unit test: ReopenFiscalYear blocked when final account issued — in tests/Application.UnitTests/Budgeting/ReopenFiscalYearTests.cs
- [x] T034 [P] [US2] Unit test: Year-boundary document splitting — encumbrance split proportionally — in tests/Application.UnitTests/Budgeting/LapseFiscalYearTests.cs

### Implementation for User Story 2

- [x] T035 [US2] Implement LapseFiscalYearCommand + Handler + Validator in src/Application/Budgeting/Commands/FinancialControl/LapseFiscalYear/LapseFiscalYearCommand.cs (depends on T001-T014)
- [x] T036 [US2] Implement payment blocking guard — check FiscalYear lapsed status before processing payments in src/Application/Budgeting/Common/ (depends on T035)
- [x] T037 [US2] Implement ReopenFiscalYearCommand + Handler + Validator in src/Application/Budgeting/Commands/FinancialControl/ReopenFiscalYear/ReopenFiscalYearCommand.cs (depends on T035)
- [x] T038 [US2] Implement YearClosing endpoint group (POST /api/YearClosing/Lapse, POST /api/YearClosing/Reopen) in src/Web/Endpoints/FinancialControl/YearClosing.cs (depends on T035, T037)
- [x] T039 [US2] Implement GetYearClosingRunsQuery in src/Application/Budgeting/Queries/FinancialControl/GetYearClosingRuns/GetYearClosingRunsQuery.cs (depends on T004)

**Checkpoint**: Year-end closing fully functional — lapse, reopen, payment blocking working

---

## Phase 5: User Story 3 — Final Account Generation (Priority: P2)

**Goal**: After closing, system produces final account: closing entries (balanced journal entries), final balances per fund-program, budget-versus-actual comparison. Final account immutability on issuance.

**Independent Test**: Generate final account for closed year — closing entries created, final balances correct, budget-vs-actual matches. Approve final account — status Issued, immutable. Attempt regenerate — rejected.

### Tests for User Story 3 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T040 [P] [US3] Unit test: GenerateFinalAccount creates FinalAccount with Draft status — in tests/Application.UnitTests/Budgeting/GenerateFinalAccountTests.cs
- [x] T041 [P] [US3] Unit test: GenerateFinalAccount creates closing entries (balanced journal entries) — in tests/Application.UnitTests/Budgeting/GenerateFinalAccountTests.cs
- [x] T042 [P] [US3] Unit test: GenerateFinalAccount creates FinalAccountLines with correct budget-vs-actual per fund/program — in tests/Application.UnitTests/Budgeting/GenerateFinalAccountTests.cs
- [x] T043 [P] [US3] Unit test: GenerateFinalAccount rejected when year not lapsed — in tests/Application.UnitTests/Budgeting/GenerateFinalAccountTests.cs
- [x] T044 [P] [US3] Unit test: IssueFinalAccount transitions to Issued, makes immutable — in tests/Application.UnitTests/Budgeting/GenerateFinalAccountTests.cs
- [x] T045 [P] [US3] Unit test: IssueFinalAccount prevents reopening of fiscal year — in tests/Application.UnitTests/Budgeting/GenerateFinalAccountTests.cs

### Implementation for User Story 3

- [x] T046 [US3] Implement GenerateFinalAccountCommand + Handler + Validator in src/Application/Budgeting/Commands/FinancialControl/GenerateFinalAccount/GenerateFinalAccountCommand.cs (depends on T035, T004-T006)
- [x] T047 [US3] Implement closing entry generation — AccountingEvent for revenue/expense accounts via posting pipeline in src/Application/Budgeting/Commands/FinancialControl/GenerateFinalAccount/ (depends on T046)
- [x] T048 [US3] Implement FinalAccountLine materialization — snapshot budget-vs-actual per dimension at generation time in src/Application/Budgeting/Commands/FinancialControl/GenerateFinalAccount/ (depends on T046)
- [x] T049 [US3] Implement IssueFinalAccountCommand + Handler in src/Application/Budgeting/Commands/FinancialControl/IssueFinalAccount/IssueFinalAccountCommand.cs (depends on T005)
- [x] T050 [US3] Implement GetFinalAccountQuery in src/Application/Budgeting/Queries/FinancialControl/GetFinalAccount/GetFinalAccountQuery.cs (depends on T005, T006)
- [x] T051 [US3] Implement FinalAccounts endpoint group (POST generate, POST issue, GET by id) in src/Web/Endpoints/FinancialControl/FinalAccounts.cs (depends on T046, T049, T050)

**Checkpoint**: Final account generation fully functional — closing entries, balances, budget-vs-actual, immutability

---

## Phase 6: User Story 4 — Constitution Amendment (Priority: P3)

**Goal**: Amend Principle V to cover full chain (appropriations → encumbrances → payments), version bump to 1.3.0. Last task of this feature.

**Independent Test**: Verify constitution updated with new version, amended principle text, and version history.

### Tests for User Story 4 (TDD) ⚠️

> No TDD needed — documentation-only task. Verification is manual inspection.

### Implementation for User Story 4

- [x] T052 [US4] Update .specify/memory/constitution.md — bump version 1.2.0 → 1.3.0, update Sync Impact Report, amend Principle V to cover full chain (appropriations → encumbrances → payments), record amendment date and rationale

**Checkpoint**: Constitution reflects implemented availability principle

---

## Phase 7: User Story 5 — Yearly Collection and Disbursement Statements (Priority: P3)

**Goal**: Generate collection and disbursement statements by fund/program with subtotals and grand totals. Statement totals reconcile with final account figures.

**Independent Test**: Generate statements for fiscal year — correct totals per fund/program. Verify totals match final account. Generate for unclosed year — warning included.

### Tests for User Story 5 (TDD) ⚠️

> **Write these tests FIRST, ensure they FAIL before implementation**

- [x] T053 [P] [US5] Unit test: GetCollectionStatement returns collections grouped by fund/program with subtotals — in tests/Application.UnitTests/Budgeting/CollectionStatementTests.cs
- [x] T054 [P] [US5] Unit test: GetDisbursementStatement returns disbursements grouped by fund/program with subtotals — in tests/Application.UnitTests/Budgeting/DisbursementStatementTests.cs
- [x] T055 [P] [US5] Unit test: Statement totals reconcile with final account figures — in tests/Application.UnitTests/Budgeting/CollectionStatementTests.cs

### Implementation for User Story 5

- [x] T056 [US5] Implement GetCollectionStatementQuery in src/Application/Budgeting/Queries/FinancialControl/GetCollectionStatement/GetCollectionStatementQuery.cs
- [x] T057 [US5] Implement GetDisbursementStatementQuery in src/Application/Budgeting/Queries/FinancialControl/GetDisbursementStatement/GetDisbursementStatementQuery.cs
- [x] T058 [US5] Add collection/disbursement statement routes to YearClosing or new Statements endpoint group in src/Web/Endpoints/FinancialControl/

**Checkpoint**: Yearly statements fully functional — totals reconcile with final account

---

## Phase 8: Functional Tests & Integration

**Purpose**: End-to-end functional tests covering the full financial control lifecycle

- [x] T059 Functional test: Multi-dimension availability query — correct breakdown per dimension — in tests/Application.FunctionalTests/Budgeting/AvailabilityBreakdownTests.cs
- [x] T060 Functional test: Full year-end lifecycle — lapse → payment blocked → reopen → final account generate → issue — in tests/Application.FunctionalTests/Budgeting/YearClosingLifecycleTests.cs
- [x] T061 Functional test: Lapse idempotency — duplicate run rejected — in tests/Application.FunctionalTests/Budgeting/YearClosingLifecycleTests.cs
- [x] T062 Functional test: Reopen blocked by payments against lapsed items — in tests/Application.FunctionalTests/Budgeting/YearClosingLifecycleTests.cs
- [x] T063 Functional test: Final account closing entries are balanced journal entries — in tests/Application.FunctionalTests/Budgeting/FinalAccountGenerationTests.cs
- [x] T064 Functional test: Budget-vs-actual comparison matches appropriation and payment data — in tests/Application.FunctionalTests/Budgeting/FinalAccountGenerationTests.cs
- [x] T065 Functional test: Year-boundary document splitting — encumbrance split proportionally — in tests/Application.FunctionalTests/Budgeting/YearClosingLifecycleTests.cs

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Validation, cleanup, and documentation

- [x] T066 Run quickstart.md validation scenarios end-to-end
- [x] T067 Run full test suite: `dotnet test tests/Application.UnitTests` + `dotnet test tests/Application.FunctionalTests`
- [x] T068 Run backend build: `dotnet build src/Web/Web.csproj` — zero warnings
- [x] T069 Update docs/database-schema.md with new tables (YearClosingRun, FinalAccount, FinalAccountLine)
- [x] T070 Run NSwag regeneration: `npm run generate-api` in src/Web/ClientApp

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2
- **US2 (Phase 4)**: Depends on Phase 2
- **US3 (Phase 5)**: Depends on Phase 2 + T035 (US2 LapseFiscalYear — final account requires lapsed year)
- **US4 (Phase 6)**: Depends on all previous stories (last task)
- **US5 (Phase 7)**: Depends on Phase 2
- **Functional Tests (Phase 8)**: Depends on Phases 3-7
- **Polish (Phase 9)**: Depends on all phases

### User Story Dependencies

- **US1 (Availability)**: Independent after Foundational. Foundation for all other stories.
- **US2 (Lapse)**: Independent after Foundational. No dependency on US1 beyond shared availability service.
- **US3 (Final Account)**: Depends on US2 — requires lapsed fiscal year before generation.
- **US4 (Constitution)**: Depends on all stories — last task, documents implemented reality.
- **US5 (Statements)**: Independent after Foundational. Can run in parallel with US2/US3.

### Parallel Opportunities

- T001-T006: All enum/entity creation in parallel
- T009-T011: All EF configurations in parallel
- T015-T016: DTOs in parallel
- T020-T024: All US1 tests in parallel
- T027-T034: All US2 tests in parallel
- T040-T045: All US3 tests in parallel
- T053-T055: All US5 tests in parallel
- US1, US2, and US5 can all start in parallel after Phase 2
- US3 starts after US2 (needs lapsed year)
- US4 is last (after all stories)

---

## Parallel Example: User Story 1

```
# Write all US1 tests first (parallel):
Task T020: Unit test — breakdown per dimension
Task T021: Unit test — available = appropriation - encumbrances - payments
Task T022: Unit test — empty for non-existent item
Task T023: Unit test — negative availability shown
Task T024: Unit test — scoped to fiscal year

# Then implement (sequential):
Task T025: GetAvailabilityBreakdownQuery handler
Task T026: Availability endpoint group
```

---

## Parallel Example: User Stories 1, 2, and 5

```
# After Phase 2 completes, these can all start in parallel:
US1 (Phase 3): Availability breakdown query
US2 (Phase 4): Lapse run + reopen
US5 (Phase 7): Collection/disbursement statements

# US3 (Phase 5) waits for US2 T035 (lapse command exists)
# US4 (Phase 6) is last — after all stories complete
```

---

## Implementation Strategy

### MVP First (US1 — P1)

1. Complete Phase 1: Setup (entities, enums, DbContext, permissions)
2. Complete Phase 2: Foundational (availability service extension, DTOs, queries)
3. Complete Phase 3: US1 — Multi-Dimension Availability
4. **STOP and VALIDATE**: Availability query working with breakdown by all dimensions
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Test independently → Deploy/Demo (MVP!)
3. Add US2 → Test independently → Deploy/Demo
4. Add US3 → Test independently → Deploy/Demo (requires US2)
5. Add US5 → Test independently → Deploy/Demo
6. Add US4 → Constitution updated → Deploy/Demo (final)
7. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- TDD mandatory: tests must be written and observed failing before implementation (Constitution Principle XI)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- YearClosingRunStatus: Completed, Reversed
- FinalAccountStatus: Draft, Issued
- YearClosingRunType: Lapse, Reopen
- FinalAccountLine Dimension: Fund, Program, Project, Item
- Constitution amendment (US4) is the LAST task — documents implemented reality
