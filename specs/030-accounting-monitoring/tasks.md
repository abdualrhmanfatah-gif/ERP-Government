# Tasks: ACC-05 — مراقبة المحاسبة (Accounting Monitoring)

**Input**: Design documents from `/specs/030-accounting-monitoring/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: TDD approach — tests written first per Constitution Principle XI.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Enum extensions and shared types that all user stories depend on

- [x] T001 [P] Extend EventStatus enum with Processing=1, Failed=4 values in src/Domain/Accounting/Enums/EventStatus.cs
- [x] T002 [P] Create AccountSource enum (FixedAccount=0, FromEventDimension=1) in src/Domain/Accounting/Enums/AccountSource.cs
- [x] T003 [P] Create AmountSource enum (FixedAmount=2, EventAmount=3) in src/Domain/Accounting/Enums/AmountSource.cs
- [x] T004 [P] Create AccountBalanceDto in src/Application/Accounting/Common/AccountBalanceDto.cs with 6 financial columns + metadata
- [x] T005 [P] Create ReconciliationResultDto and ReconciliationDiscrepancyDto in src/Application/Accounting/Common/AccountBalanceDto.cs
- [x] T006 [P] Create PostingRuleDto and PostingRuleLineDto in src/Application/Accounting/Common/AccountingDtos.cs
- [x] T007 [P] Create PeriodCloseRequest record (fiscalYearId, fiscalPeriodId) in src/Application/Accounting/Common/

**Checkpoint**: Shared types ready — user story implementation can begin

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core query and service interfaces that multiple stories depend on

- [x] T008 [P] Create IAccountBalanceService interface in src/Application/Accounting/Common/ with GetBalancesAsync, ReconcileAsync, RebuildAsync methods
- [x] T009 [US1] Implement GetAccountBalancesQuery + Handler in src/Application/Accounting/Queries/AccountBalances/GetAccountBalances/ with fiscal year/period/account/currency filters
- [x] T010 [US1] Add AccountBalanceDto mapping profile in src/Application/Accounting/Common/AccountBalanceDto.cs (AutoMapper)

**Checkpoint**: Foundation ready — individual user stories can proceed

---

## Phase 3: User Story 1 — الأرصدة وإغلاق الفترة (Priority: P1) — MVP

**Goal**: Display account balances with 6 columns + finalize/unfinalize period

**Independent Test**: Close period → isFinalized=true; reopen → isFinalized=false; reject close if pending events

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T011 [P] [US1] Unit test for GetAccountBalancesQuery in tests/Application.UnitTests/Accounting/GetAccountBalancesQueryTests.cs
- [x] T012 [P] [US1] Unit test for FinalizePeriodCommand in tests/Application.UnitTests/Accounting/FinalizePeriodCommandTests.cs
- [x] T013 [P] [US1] Unit test for UnfinalizePeriodCommand in tests/Application.UnitTests/Accounting/UnfinalizePeriodCommandTests.cs

### Implementation for User Story 1

- [x] T014 [US1] Implement FinalizePeriodCommand + Handler + Validator in src/Application/Accounting/Commands/AccountBalances/Finalize/FinalizePeriodCommand.cs — guard: reject if pending events exist
- [x] T015 [US1] Implement UnfinalizePeriodCommand + Handler + Validator in src/Application/Accounting/Commands/AccountBalances/Unfinalize/UnfinalizePeriodCommand.cs
- [x] T016 [US1] Implement AccountingBalances endpoint group in src/Web/Endpoints/Accounting/AccountingBalances.cs — GET /, POST /finalize, POST /unfinalize
- [x] T017 [US1] Functional test for period close/open in tests/Application.FunctionalTests/Accounting/PeriodCloseTests.cs

**Checkpoint**: User Story 1 fully functional — balances display + finalize/unfinalize

---

## Phase 4: User Story 2 — التوافق وإعادة البناء (Priority: P2)

**Goal**: Reconciliation check (materialized vs calculated) and balance rebuild from journal lines

**Independent Test**: Reconcile → discrepancy table per currency or "balanced"; Rebuild → recalculate from journal entry lines

### Tests for User Story 2

- [x] T018 [P] [US2] Unit test for ReconcileAccountBalancesQuery in tests/Application.UnitTests/Accounting/ReconcileAccountBalancesQueryTests.cs
- [x] T019 [P] [US2] Unit test for RebuildAccountBalancesCommand in tests/Application.UnitTests/Accounting/RebuildAccountBalancesCommandTests.cs

### Implementation for User Story 2

- [x] T020 [US2] Implement ReconcileAccountBalancesQuery + Handler in src/Application/Accounting/Queries/AccountBalances/Reconcile/ — compare materialized vs calculated from JournalEntryLines
- [x] T021 [US2] Implement RebuildAccountBalancesCommand + Handler in src/Application/Accounting/Commands/AccountBalances/Rebuild/RebuildAccountBalancesCommand.cs — recalculate from journal lines, guard: period not finalized
- [x] T022 [US2] Add reconcile + rebuild routes to AccountingBalances endpoint in src/Web/Endpoints/Accounting/AccountingBalances.cs
- [x] T023 [US2] Functional test for reconcile/rebuild in tests/Application.FunctionalTests/Accounting/ReconcileRebuildTests.cs

**Checkpoint**: User Story 2 complete — reconciliation detects discrepancies, rebuild corrects them

---

## Phase 5: User Story 3 — طابور الأحداث (Priority: P2)

**Goal**: Read-only event queue showing pending/failed events with error details

**Independent Test**: Failed event shows errorMessage + retryCount + journalEntryId link

### Tests for User Story 3

- [x] T024 [P] [US3] Unit test for GetPendingEventsQuery in tests/Application.UnitTests/Accounting/GetPendingEventsQueryTests.cs

### Implementation for User Story 3

- [x] T025 [US3] Implement GetPendingEventsQuery + Handler in src/Application/Accounting/Queries/AccountingEvents/GetPendingEvents/ with status/eventType filters and pagination
- [x] T026 [US3] Implement AccountingEvents endpoint group (read-only) in src/Web/Endpoints/Accounting/AccountingEvents.cs — GET /pending only
- [x] T027 [US3] Functional test for event queue in tests/Application.FunctionalTests/Accounting/EventQueueTests.cs

**Checkpoint**: User Story 3 complete — event queue displays all events with error details

---

## Phase 6: User Story 4 — قواعد الترحيل (Priority: P2)

**Goal**: Full CRUD for posting rules with nested lines (header + lines atomic save)

**Independent Test**: Create rule with lines → saved complete; delete unused rule → success; delete used rule → rejected

### Tests for User Story 4

- [x] T028 [P] [US4] Unit test for CreatePostingRuleCommand in tests/Application.UnitTests/Accounting/CreatePostingRuleCommandTests.cs
- [x] T029 [P] [US4] Unit test for UpdatePostingRuleCommand in tests/Application.UnitTests/Accounting/UpdatePostingRuleCommandTests.cs
- [x] T030 [P] [US4] Unit test for DeletePostingRuleCommand in tests/Application.UnitTests/Accounting/DeletePostingRuleCommandTests.cs

### Implementation for User Story 4

- [x] T031 [US4] Implement CreatePostingRuleCommand + Handler + Validator in src/Application/Accounting/Commands/PostingRules/Create/CreatePostingRuleCommand.cs — validate at least one line, FixedAccount requires FixedAccountId
- [x] T032 [US4] Implement UpdatePostingRuleCommand + Handler + Validator in src/Application/Accounting/Commands/PostingRules/Update/UpdatePostingRuleCommand.cs — lines replaced atomically
- [x] T033 [US4] Implement DeletePostingRuleCommand + Handler in src/Application/Accounting/Commands/PostingRules/Delete/DeletePostingRuleCommand.cs — guard: reject if referenced by Processing/Posted events
- [x] T034 [US4] Implement PostingRules endpoint group in src/Web/Endpoints/Accounting/PostingRules.cs — GET /, POST /, PUT /{id}, DELETE /{id}
- [x] T035 [US4] Functional test for posting rules CRUD in tests/Application.FunctionalTests/Accounting/PostingRulesTests.cs

**Checkpoint**: User Story 4 complete — posting rules fully manageable with nested lines

---

## Phase 7: Frontend — accounting-monitoring feature

**Purpose**: React UI for all 4 user stories

- [x] T036 [P] Create feature folder structure at src/Web/ClientApp/src/features/accounting-monitoring/ with pages/, components/, hooks/, shared/
- [x] T037 [P] Create shared/types.ts with AccountBalanceDto, ReconciliationResultDto, AccountingEventDto, PostingRuleDto, PostingRuleLineDto types
- [x] T038 [P] Create shared/client.ts with NSwag-generated client wrappers for AccountingBalances, AccountingEvents, PostingRules
- [x] T039 [P] [US1] Create AccountBalancesListPage in src/Web/ClientApp/src/features/accounting-monitoring/pages/ — 6-column table, filters, finalize/unfinalize buttons
- [x] T040 [P] [US1] Create useAccountBalances hook in src/Web/ClientApp/src/features/accounting-monitoring/hooks/
- [x] T041 [P] [US2] Create ReconciliationPage in src/Web/ClientApp/src/features/accounting-monitoring/pages/ — reconcile button, discrepancy table, rebuild button
- [x] T042 [P] [US2] Create useReconciliation hook in src/Web/ClientApp/src/features/accounting-monitoring/hooks/
- [x] T043 [P] [US3] Create EventsQueuePage in src/Web/ClientApp/src/features/accounting-monitoring/pages/ — read-only table with status/error filters
- [x] T044 [P] [US3] Create usePendingEvents hook in src/Web/ClientApp/src/features/accounting-monitoring/hooks/
- [x] T045 [P] [US4] Create PostingRulesListPage in src/Web/ClientApp/src/features/accounting-monitoring/pages/ — rules list with nested lines
- [x] T046 [P] [US4] Create PostingRuleFormPage in src/Web/ClientApp/src/features/accounting-monitoring/pages/ — create/edit form with dynamic lines
- [x] T047 [P] [US4] Create usePostingRules hook in src/Web/ClientApp/src/features/accounting-monitoring/hooks/
- [x] T048 Register accounting-monitoring routes in src/Web/ClientApp/src/app/routes.tsx — /accounting/balances, /accounting/events, /accounting/posting-rules
- [x] T049 [P] Unit tests for accounting-monitoring pages in src/Web/ClientApp/src/features/accounting-monitoring/__tests__/

**Checkpoint**: Frontend complete — all 4 user stories accessible via UI

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Integration, documentation, final validation

- [x] T050 Run EF migration for EventStatus enum change: dotnet ef migrations add ExtendEventStatus --project src/Infrastructure --startup-project src/Web
- [x] T051 Run full backend test suite: dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests
- [x] T052 Run frontend lint + test + build: cd src/Web/ClientApp && npm run lint && npm run test && npm run build
- [x] T053 Run quickstart.md validation scenarios end-to-end
- [x] T054 Update docs/database-schema.md with EventStatus enum changes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 (enum extensions, DTOs)
- **Phase 3 (US1)**: Depends on Phase 2 (GetAccountBalancesQuery)
- **Phase 4 (US2)**: Depends on Phase 2 (IAccountBalanceService) — can parallel with US1
- **Phase 5 (US3)**: Depends on Phase 1 (EventStatus extension) — can parallel with US1/US2
- **Phase 6 (US4)**: Depends on Phase 1 (AccountSource/AmountSource enums) — can parallel with US1/US2/US3
- **Phase 7 (Frontend)**: Depends on backend endpoints from Phases 3-6
- **Phase 8 (Polish)**: Depends on all previous phases

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US2 (P2)**: Can start after Phase 2 — may use US1 query but independently testable
- **US3 (P2)**: Can start after Phase 1 — fully independent
- **US4 (P2)**: Can start after Phase 1 — fully independent

### Parallel Opportunities

```
Phase 1: T001-T003 parallel (enum files), T004-T006 parallel (DTO files), T007 independent
Phase 2: T008 parallel with T009+T010
Phase 3: T011-T013 parallel (tests), T014-T015 sequential (commands depend on query), T016 depends on T014+T015
Phase 4: T018-T019 parallel (tests), T020-T021 sequential, T022 depends on T020+T021
Phase 5: T024 independent, T025-T026 sequential
Phase 6: T028-T030 parallel (tests), T031-T033 parallel (independent commands), T034 depends on T031-T033
Phase 7: T036-T038 parallel (shared), T039-T047 parallel (pages/hooks per story), T048 depends on pages, T049 depends on pages
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (enums + DTOs)
2. Complete Phase 2: Foundational (query + service interface)
3. Complete Phase 3: User Story 1 (balances + finalize/unfinalize)
4. **STOP and VALIDATE**: Test balances display + period close/open independently
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1+2 → Foundation ready
2. + Phase 3 (US1) → MVP: balances + finalize ✓
3. + Phase 4 (US2) → Reconciliation + rebuild ✓
4. + Phase 5 (US3) → Event queue ✓
5. + Phase 6 (US4) → Posting rules CRUD ✓
6. + Phase 7 → Frontend for all stories ✓
7. + Phase 8 → Polish + validation ✓

### Parallel Team Strategy

With multiple developers:
1. Team completes Phase 1+2 together
2. Once Phase 2 is done:
   - Developer A: US1 (Phase 3)
   - Developer B: US3 (Phase 5) + US4 (Phase 6)
   - Developer C: US2 (Phase 4)
3. All backend done → Frontend (Phase 7) in parallel
4. Phase 8 together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests written FIRST (TDD) per Constitution Principle XI
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
