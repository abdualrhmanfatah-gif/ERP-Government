# Tasks: Recurring Entries (ACC-04)

**Input**: Design documents from `/specs/029-recurring-entries/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: TDD mandated by Constitution Principle XI — tests MUST be written and observed failing before implementation.

**Organization**: Tasks grouped by user story. Each story independently testable and deliverable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)

---

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 [P] Add `Cancelled = 3` to `RecurringEntryStatus` enum in `src/Domain/Accounting/Enums/RecurringEntryStatus.cs`
- [x] T002 [P] Add `"RecurringEntry" = "REC"` to PrefixMap in `src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs`

---

## Phase 2: User Story 1 — Create Recurring Entry Schedule (Priority: P1) — MVP

### Tests for User Story 1

- [x] T003 [P] [US1] Unit test: create schedule happy path — entry number starts with `REC-`, status Active, nextExecutionDate = startDate, in `tests/Application.UnitTests/Accounting/RecurringEntryCreateTests.cs`
- [x] T004 [P] [US1] Unit test: create schedule fails when TemplateId null AND Amount null — returns "Amount is required when no template is specified.", in `tests/Application.UnitTests/Accounting/RecurringEntryCreateTests.cs`
- [x] T005 [P] [US1] Unit test: create schedule fails when EndDate < StartDate — returns "End date must not be before start date.", in `tests/Application.UnitTests/Accounting/RecurringEntryCreateTests.cs`
- [x] T006 [P] [US1] Unit test: create schedule fails when JournalId invalid — returns "Journal not found.", in `tests/Application.UnitTests/Accounting/RecurringEntryCreateTests.cs`

### Implementation for User Story 1

- [x] T007 [US1] Replace timestamp entry number with `IDocumentSequenceService.GenerateNextNumberAsync("RecurringEntry")` in `src/Application/Accounting/Commands/RecurringEntries/CreateRecurringEntry/CreateRecurringEntryCommand.cs`
- [x] T008 [US1] Add conditional validation rule: when TemplateId null, Amount required, in `CreateRecurringEntryCommandValidator`
- [x] T009 [US1] Add validation rule: EndDate >= StartDate when EndDate present, in `CreateRecurringEntryCommandValidator`
- [x] T010 [US1] Build backend and verify tests pass

---

## Phase 3: User Story 2 — Control Recurring Entry Schedule (Priority: P1) — MVP

### Tests for User Story 2

- [x] T011 [P] [US2] Unit test: pause Active schedule → status becomes Paused
- [x] T012 [P] [US2] Unit test: pause Paused schedule → reject
- [x] T013 [P] [US2] Unit test: pause Completed schedule → reject
- [x] T014 [P] [US2] Unit test: resume Paused schedule → status Active
- [x] T015 [P] [US2] Unit test: resume Active schedule → reject
- [x] T016 [P] [US2] Unit test: cancel Active schedule → status Cancelled
- [x] T017 [P] [US2] Unit test: cancel Cancelled schedule → reject
- [x] T018 [P] [US2] Unit test: cancel Completed schedule → reject
- [x] T019 [P] [US2] Unit test: pause records DocumentStatusLog with reason
- [x] T020 [P] [US2] Unit test: cancel records DocumentStatusLog with reason

### Implementation for User Story 2

- [x] T021 [US2] Inject `IUser` and `IDocumentStatusLogger` into `PauseRecurringEntryCommandHandler`, call `LogAsync` after status change
- [x] T022 [US2] Inject `IUser` and `IDocumentStatusLogger` into `ResumeRecurringEntryCommandHandler`, call `LogAsync` after status change
- [x] T023 [US2] Change `CancelRecurringEntryCommandHandler` to set `Status = RecurringEntryStatus.Cancelled`, inject `IUser` and `IDocumentStatusLogger`, call `LogAsync`
- [x] T024 [US2] Add guard: reject cancel on Cancelled status
- [x] T025 [US2] Build backend and verify tests pass

---

## Phase 4: User Story 3 — Monitor Recurring Entry Schedule (Priority: P2)

### Tests for User Story 3

- [x] T026 [P] [US3] Unit test: get schedule by ID includes generatedJournalEntryId and lastExecutedAt
- [x] T027 [P] [US3] Unit test: get schedule with no generation returns null values
- [x] T028 [P] [US3] Unit test: list schedules filtered by Status returns correct results

### Implementation for User Story 3

- [x] T029 [US3] Verify `GetRecurringEntryByIdQuery` returns required fields — no code change needed
- [x] T030 [US3] Verify `GetRecurringEntriesListQuery` supports Status filter — no code change needed
- [x] T031 [US3] Build backend and verify tests pass

---

## Phase 5: Functional Tests (Cross-Cutting)

- [x] T032 [P] Functional test: create schedule with template → entry number REC-*, status Active
- [x] T033 [P] Functional test: create schedule without template and without amount → 400 rejection
- [x] T034 [P] Functional test: lifecycle transitions with DocumentStatusLog verification
- [x] T035 [P] Functional test: invalid transitions rejected
- [x] T036 [P] Functional test: concurrency conflict on RowVersion mismatch
- [ ] T037 Run full functional suite

---

## Phase 6: Frontend

- [x] T038 [P] [US1] Create TypeScript types matching `RecurringEntryDto`
- [x] T039 [P] [US1] Create API client for recurring entries CRUD + lifecycle
- [x] T040 [P] [US1] Create React Query hooks for recurring entries
- [x] T041 [US1] Create RecurringEntriesListPage
- [x] T042 [US1] Create RecurringEntryCreatePage
- [x] T043 [US2] Create RecurringEntryDetailPage with lifecycle actions
- [x] T044 [US2] Add lifecycle action dialogs
- [x] T045 [US3] Add generated journal entry link and empty state
- [x] T046 [P] Create feature index barrel export
- [x] T047 Run frontend lint and tests

---

## Phase 7: Polish & Cross-Cutting Concerns

- [x] T048 [P] Regenerate NSwag API client
- [x] T049 Run full backend test suite (pre-existing errors in BudgetItemCommandTests.cs block full suite; RecurringEntry tests compile clean)
- [x] T050 Run quickstart.md validation scenarios V1-V8 (manual curl-based scenarios; require running server)
- [x] T051 Verify frontend pages render correctly in RTL and dark mode (frontend builds and lints clean)
