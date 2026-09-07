# Tasks: Checks & Monthly Statement (TRE-03)

**Input**: Design documents from `/specs/033-checks-statements/`

**Prerequisites**: plan.md, spec.md, data-model.md, research.md, contracts/api.md, quickstart.md

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for the checks lifecycle feature

- [X] T001 Add `CheckDetailDto` to `src/Application/Revenue/Common/DTOs/ReceiptVoucherDtos.cs`
- [X] T002 Add `ChecksReplace` permission code to `src/Application/Common/Security/PermissionCodes.cs`
- [X] T003 Register `ChecksReplace` policy in `src/Web/DependencyInjection.cs`
- [X] T004 [P] Seed `CheckCleared` posting rule in `src/Infrastructure/Data/Seeds/PostingRuleSeedData.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Add date validation helpers (date >= CheckDate, date <= today) in `src/Application/Revenue/Common/` — shared by Clear and Bounce commands
- [ ] T006 [P] Establish optimistic-concurrency RowVersion pattern for Check entity (set property entry's original value for EF token check) in `src/Application/Revenue/Commands/Checks/`

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Under-Collection Checks List (Priority: P1)

**Goal**: Cashier opens a checks screen showing every check still under collection for a period

**Independent Test**: Create receipt vouchers with checks for a period → open the checks list → every check appears with its current state

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T007 [P] [US1] Contract test for `GET /api/Revenue/Checks` endpoint in `tests/Application.FunctionalTests/Revenue/`
- [X] T008 [P] [US1] Contract test for `GET /api/Revenue/Checks/{id}` endpoint in `tests/Application.FunctionalTests/Revenue/`
- [X] T009 [P] [US1] Integration test: checks list date-range filtering and status filter in `tests/Application.FunctionalTests/Revenue/`

### Implementation for User Story 1

- [X] T010 [P] [US1] Create `GetChecksQuery` in `src/Application/Revenue/Queries/Checks/GetChecks/GetChecksQuery.cs`
- [X] T011 [P] [US1] Create `GetCheckByIdQuery` in `src/Application/Revenue/Queries/Checks/GetCheckById/GetCheckByIdQuery.cs`
- [X] T012 [US1] Add GET list + GET {id} endpoints to `src/Web/Endpoints/Revenue/Checks.cs`

**Checkpoint**: Checks list with filtering and detail view are functional

---

## Phase 4: User Story 2 — Clearing (Priority: P1)

**Goal**: Cashier clears an under-collection check with confirmation and clearing date for revenue recognition

**Independent Test**: Clear an under-collection check with a date → the check becomes Cleared and a posting effect is visible

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T013 [P] [US2] Functional test: clear an UnderCollection check → Cleared + DocumentStatusLog + AccountingEvent in `tests/Application.FunctionalTests/Revenue/`
- [X] T014 [P] [US2] Functional test: reject clear on Bounced check, clearedAt before check date, future clearedAt, cancelled voucher, concurrency conflict in `tests/Application.FunctionalTests/Revenue/`

### Implementation for User Story 2

- [X] T015 [US2] Rebuild `ClearCheckCommand` handler in `src/Application/Revenue/Commands/Checks/ClearCheck/ClearCheckCommand.cs` — fix date validation (D6), cancelled-voucher guard (D7), concurrency pattern (D10), remove any divergence
- [X] T016 [US2] Rebuild `ClearCheckCommand` validator in `src/Application/Revenue/Commands/Checks/ClearCheck/ClearCheckCommandValidator.cs`
- [X] T017 [US2] Fix clear endpoint policy in `src/Web/Endpoints/Revenue/Checks.cs`

**Checkpoint**: Clearing produces status transition, status log, and ledger posting

---

## Phase 5: User Story 3 — Bounce and Replacement (Priority: P1)

**Goal**: Cashier bounces a check (locking source voucher) and replaces it with cash or a new check

**Independent Test**: Bounce a check → replace with chosen method → linked replacement voucher created

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T018 [P] [US3] Functional test: bounce UnderCollection check → Bounced + DocumentStatusLog with reason in `tests/Application.FunctionalTests/Revenue/`
- [X] T019 [P] [US3] Functional test: reject bounce on Cleared check, bouncedAt before check date, future bouncedAt, cancelled voucher, concurrency conflict in `tests/Application.FunctionalTests/Revenue/`
- [X] T020 [P] [US3] Functional test: replace Bounced check with Cash → replacement voucher created (number via sequence service, amount fixed, Draft status, linked via ReplacementVoucherId) in `tests/Application.FunctionalTests/Revenue/`
- [X] T021 [P] [US3] Functional test: replace Bounced check with Check → replacement voucher + new Check row (UnderCollection) created in `tests/Application.FunctionalTests/Revenue/`
- [X] T022 [P] [US3] Functional test: reject replace when ReplacementVoucherId already set (double replace FR-005), reject replace when method=Check without checkDetails, reject replace on non-Bounced check, concurrency conflict in `tests/Application.FunctionalTests/Revenue/`

### Implementation for User Story 3

- [X] T023 [US3] Rebuild `BounceCheckCommand` handler in `src/Application/Revenue/Commands/Checks/BounceCheck/BounceCheckCommand.cs` — remove auto-create voucher (D1), remove DepositSlipId reset (D2), add date validation (D6), cancelled-voucher guard (D7), fix concurrency (D10)
- [X] T024 [US3] Rebuild `BounceCheckCommand` validator in `src/Application/Revenue/Commands/Checks/BounceCheck/BounceCheckCommandValidator.cs`
- [X] T025 [US3] Rebuild `ReplaceCheckCommand` handler in `src/Application/Revenue/Commands/Checks/ReplaceCheck/ReplaceCheckCommand.cs` — create replacement voucher (D4, D11), use IDocumentSequenceService (D3), create new Check row when method=Check, add double-replace guard (FR-005), cancelled-voucher guard, fix concurrency (D10)
- [X] T026 [US3] Rebuild `ReplaceCheckCommand` validator in `src/Application/Revenue/Commands/Checks/ReplaceCheck/ReplaceCheckCommandValidator.cs`
- [X] T027 [US3] Fix replace endpoint policy to `ChecksReplace` in `src/Web/Endpoints/Revenue/Checks.cs`

**Checkpoint**: Bounce + replacement lifecycle fully functional with proper guards

---

## Phase 6: User Story 4 — Monthly Statement Clearings (Priority: P2)

**Goal**: Monthly statement shows the month's check clearings for reconciliation

**Independent Test**: Clear checks in a month → the monthly statement's clearings table lists them

### Tests for User Story 4

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T028 [P] [US4] Functional test: cleared checks appear in monthly statement clearings tab in `tests/Application.FunctionalTests/Revenue/`
- [X] T029 [P] [US4] Functional test: month with no clearings returns empty clearings list, not error in `tests/Application.FunctionalTests/Revenue/`

### Implementation for User Story 4

- [X] T030 [US4] Verify `GetMonthlyStatementQuery` clearings projection handles empty state correctly in `src/Application/Revenue/Queries/Statements/GetMonthlyStatement/GetMonthlyStatementQuery.cs`
- [X] T031 [P] [US4] Add clearings tab UI to monthly statement page in `src/Web/ClientApp/src/features/treasury/statements/`
- [X] T032 [US4] Verify empty clearings renders empty table (not blank screen) in `src/Web/ClientApp/src/features/treasury/statements/`

**Checkpoint**: Statement clearings tab shows cleared checks or empty state

---

## Phase 7: Frontend — Checks List & Action Dialogs (Priority: P1)

**Goal**: Frontend checks screen with list, clear/bounce/replace dialogs

**Independent Test**: Navigate to checks list → filter by date/status → clear/bounce/replace via dialogs → confirm actions

### Implementation for Frontend

- [X] T033 [P] Create checks list page in `src/Web/ClientApp/src/features/treasury/checks/pages/ChecksListPage.tsx`
- [X] T034 [P] Create clear dialog component in `src/Web/ClientApp/src/components/TreasuryChecksClearDialog.tsx`
- [X] T035 [P] Create bounce dialog component in `src/Web/ClientApp/src/components/TreasuryChecksBounceDialog.tsx`
- [X] T036 [P] Create replace dialog component in `src/Web/ClientApp/src/components/TreasuryChecksReplaceDialog.tsx`
- [X] T037 Create Zod schemas for clear/bounce/replace forms in `src/Web/ClientApp/src/features/treasury/checks/shared/schemas.ts`
- [X] T038 Create entity-scoped client wrapper in `src/Web/ClientApp/src/features/treasury/checks/shared/client.ts`
- [X] T039 Create query hooks in `src/Web/ClientApp/src/features/treasury/checks/hooks/`
- [X] T040 Register checks routes in `src/Web/ClientApp/src/routes/`

**Checkpoint**: Frontend checks UI fully functional with RTL Arabic

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T041 Run full 5-project backend test suite: `dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests && dotnet test tests/Infrastructure.IntegrationTests && dotnet test tests/Web.AcceptanceTests`
- [X] T042 Validate quickstart.md scenarios against implementation
- [X] T043 Run frontend lint + build: `cd src/Web/ClientApp && npm run lint && npm run build`
- [X] T044 Regenerate NSwag API client: `cd src/Web/ClientApp && npm run generate-api`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS all user stories
- **Phases 3–6 (User Stories)**: All depend on Phase 2 completion
  - US1, US2, US3 (all P1) can proceed in parallel
  - US4 (P2) depends on US2 (clearing produces data US4 consumes)
- **Phase 7 (Frontend)**: Depends on Phase 1 endpoints being stable; can run in parallel with backend US phases
- **Phase 8 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (Checks List)**: Independent — starts after Foundational
- **US2 (Clearing)**: Independent — starts after Foundational
- **US3 (Bounce + Replace)**: Independent — starts after Foundational
- **US4 (Statement Clearings)**: Depends on US2 (clearings data source)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Validators before handlers
- Handlers before endpoints
- Core implementation before integration

### Parallel Opportunities

- All Phase 1 tasks marked [P] can run in parallel
- All Phase 2 tasks marked [P] can run in parallel
- Once Phase 2 completes, US1 + US2 + US3 can start in parallel
- All tests within a story marked [P] can run in parallel
- All frontend components marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for US1 together:
Task: "Contract test for GET /api/Revenue/Checks endpoint in tests/Application.FunctionalTests/Revenue/"
Task: "Contract test for GET /api/Revenue/Checks/{id} endpoint in tests/Application.FunctionalTests/Revenue/"
Task: "Integration test: checks list filtering in tests/Application.FunctionalTests/Revenue/"

# Launch both queries in parallel:
Task: "Create GetChecksQuery in src/Application/Revenue/Queries/Checks/GetChecks/GetChecksQuery.cs"
Task: "Create GetCheckByIdQuery in src/Application/Revenue/Queries/Checks/GetCheckById/GetCheckByIdQuery.cs"
```

---

## Implementation Strategy

### MVP First (US1 + US2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: US1 (Checks List) — test independently
4. Complete Phase 4: US2 (Clearing) — test independently
5. **STOP and VALIDATE**: Run quickstart scenarios 1 + 2
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 (List) → Test independently → Deploy/Demo
3. US2 (Clear) → Test independently → Deploy/Demo
4. US3 (Bounce + Replace) → Test independently → Deploy/Demo
5. US4 (Statement) → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (List) + Frontend
   - Developer B: US2 (Clear)
   - Developer C: US3 (Bounce + Replace)
3. US4 starts after US2 completes
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Divergences D1–D11 fixed during command rebuilds (Phases 4–5)
