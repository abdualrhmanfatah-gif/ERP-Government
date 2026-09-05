# Tasks: Rebuild Budgeting Module Backend

**Input**: Design documents from `/specs/013-budgeting-backend-rebuild/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/endpoints.md, quickstart.md

**Tests**: Included per FR-019 — unit tests per command (success + failure), functional tests against real database.

**Organization**: Tasks grouped by user story. US7 (Migration) is foundational and MUST complete first. All other stories depend on US7 completion.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Delete old budgeting code, establish new entity skeleton and enums

- [x] T001 Delete all existing Budgeting domain entities in src/Domain/Budgeting/Entities/
- [x] T002 Delete all existing Budgeting enums in src/Domain/Budgeting/Enums/
- [x] T003 Delete all existing Budgeting commands in src/Application/Budgeting/Commands/
- [x] T004 Delete all existing Budgeting queries in src/Application/Budgeting/Queries/
- [x] T005 Delete all existing Budgeting DTOs in src/Application/Budgeting/Common/
- [x] T006 Delete all existing EF configurations in src/Infrastructure/Data/Configurations/Budgeting/
- [x] T007 Delete all existing seeds in src/Infrastructure/Data/Seeds/
- [x] T008 Delete all existing Web endpoints in src/Web/Endpoints/Budgeting/
- [x] T009 Delete all existing Budgeting unit tests in tests/Application.UnitTests/Budgeting/
- [x] T010 Delete all existing Budgeting functional tests in tests/Application.FunctionalTests/Budgeting/
- [x] T011 [P] Create 8 enums in src/Domain/Budgeting/Enums/: BudgetControlMethod, BudgetStatus, FundType, FundCategory, AppropriationType, AppropriationStatus, EncumbranceType, EncumbranceStatus per data-model.md
- [x] T012 [P] Create 7 domain entities in src/Domain/Budgeting/Entities/: BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance per data-model.md — all extend BaseAuditableEntity, all have RowVersion, all FKs Restrict, no derived columns (Level, IsReversed, snapshot amounts, ApprovedById/At, IsActive on Budget)
- [x] T013 Create IApplicationDbContext interface additions for 7 new DbSets in src/Application/Common/Interfaces/IApplicationDbContext.cs
- [x] T014 Create 7 EF entity configurations in src/Infrastructure/Data/Configurations/Budgeting/ per data-model.md: correct column types (decimal(23,2)), unique constraints (Code, BudgetNumber, AppropriationNumber, EncumbranceNumber, BudgetId+ItemCode), FK indexes (AppropriationId, BudgetId, BudgetItemId, ReversalOfId), RowVersion, Restrict delete behavior

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST complete before ANY user story

- [x] T015 Create EF migration: drop 7 old tables, recreate per new schema, preserve DocumentSequence BGT/APR/ENC rows in src/Infrastructure/Data/Migrations/
- [x] T016 [P] Create BudgetTypeSeedData in src/Infrastructure/Data/Seeds/BudgetTypeSeedData.cs — idempotent (INSERT IF NOT EXISTS)
- [x] T017 [P] Create BudgetClassificationSeedData in src/Infrastructure/Data/Seeds/BudgetClassificationSeedData.cs — idempotent
- [x] T018 [P] Create role-permission seed data for 5 roles (Admin, BudgetOfficer, Approver, ProcurementOfficer, Analyst) per FR-016 in src/Infrastructure/Data/Seeds/
- [x] T019 Create AvailabilityService in src/Application/Budgeting/Common/AvailabilityService.cs — AvailableForAppropriation(BudgetItemId), AvailableForEncumbrance(AppropriationId), AllowOverrun resolution chain, ControlMethod evaluation (None/Warning/Blocking)
- [x] T020 Create Result<T> pattern helpers if not existing in src/Application/Common/Models/Result.cs
- [x] T021 Verify dotnet build passes with zero warnings after all Phase 1+2 changes

**Checkpoint**: Foundation ready — all old code deleted, new entities/enums/configurations/migrations/seeds in place

---

## Phase 3: User Story 1 — Budget Master Data Management (Priority: P1) — MVP

**Goal**: BudgetType, Fund, BudgetClassification CRUD with Level computation and IsActive toggle

**Independent Test**: Create BudgetTypes with distinct ControlMethod, Funds with FundType/FundCategory, multi-level Classifications verifying Level computed via hierarchy traversal

### Tests for User Story 1

- [x] T022 [P] [US1] Unit test: CreateBudgetTypeCommand success + failure (unique Code, validation) in tests/Application.UnitTests/Budgeting/BudgetTypeCommandTests.cs
- [x] T023 [P] [US1] Unit test: UpdateBudgetTypeCommand success + failure in tests/Application.UnitTests/Budgeting/BudgetTypeCommandTests.cs
- [x] T024 [P] [US1] Unit test: ToggleBudgetTypeActiveCommand in tests/Application.UnitTests/Budgeting/BudgetTypeCommandTests.cs
- [x] T025 [P] [US1] Unit test: CreateFundCommand success + failure in tests/Application.UnitTests/Budgeting/FundCommandTests.cs
- [x] T026 [P] [US1] Unit test: UpdateFundCommand success + failure in tests/Application.UnitTests/Budgeting/FundCommandTests.cs
- [x] T027 [P] [US1] Unit test: CreateBudgetClassificationCommand success + failure (unique Code, ParentId validation) in tests/Application.UnitTests/Budgeting/BudgetClassificationCommandTests.cs
- [x] T028 [P] [US1] Unit test: UpdateBudgetClassificationCommand success + failure in tests/Application.UnitTests/Budgeting/BudgetClassificationCommandTests.cs
- [x] T029 [P] [US1] Unit test: ToggleBudgetClassificationActiveCommand in tests/Application.UnitTests/Budgeting/BudgetClassificationCommandTests.cs
- [x] T030 [P] [US1] Functional test: BudgetType CRUD lifecycle in tests/Application.FunctionalTests/Budgeting/BudgetTypeTests.cs
- [x] T031 [P] [US1] Functional test: Fund CRUD lifecycle in tests/Application.FunctionalTests/Budgeting/FundTests.cs
- [x] T032 [P] [US1] Functional test: BudgetClassification CRUD + Level computation (3-level and 5+ level hierarchy) in tests/Application.FunctionalTests/Budgeting/BudgetClassificationTests.cs

### Implementation for User Story 1

- [x] T033 [P] [US1] Create BudgetType commands: Create, Update, ToggleActive in src/Application/Budgeting/Commands/BudgetTypes/
- [x] T034 [P] [US1] Create BudgetType queries: GetById, GetList in src/Application/Budgeting/Queries/BudgetTypes/
- [x] T035 [P] [US1] Create BudgetType DTOs in src/Application/Budgeting/Common/BudgetTypeDto.cs
- [x] T036 [P] [US1] Create Fund commands: Create, Update, ToggleActive in src/Application/Budgeting/Commands/Funds/
- [x] T037 [P] [US1] Create Fund queries: GetById, GetList in src/Application/Budgeting/Queries/Funds/
- [x] T038 [P] [US1] Create Fund DTOs in src/Application/Budgeting/Common/FundDto.cs
- [x] T039 [P] [US1] Create BudgetClassification commands: Create, Update, ToggleActive in src/Application/Budgeting/Commands/BudgetClassifications/
- [x] T040 [P] [US1] Create BudgetClassification queries: GetById, GetList, GetTree (recursive CTE for Level) in src/Application/Budgeting/Queries/BudgetClassifications/
- [x] T041 [P] [US1] Create BudgetClassification DTOs with computed Level in src/Application/Budgeting/Common/BudgetClassificationDto.cs
- [x] T042 [US1] Create BudgetTypes endpoint group in src/Web/Endpoints/Budgeting/BudgetTypes.cs
- [x] T043 [US1] Create Funds endpoint group in src/Web/Endpoints/Budgeting/Funds.cs
- [x] T044 [US1] Create BudgetClassifications endpoint group in src/Web/Endpoints/Budgeting/BudgetClassifications.cs
- [x] T045 [US1] Run unit tests for US1 — verify all pass
- [x] T046 [US1] Run functional tests for US1 — verify all pass

**Checkpoint**: BudgetType, Fund, BudgetClassification fully functional with Level computation

---

## Phase 4: User Story 2 — Budget and BudgetItem Lifecycle with Tree (Priority: P1)

**Goal**: Budget CRUD + FSM (Draft->Submitted->Approved->Active->Suspended/Closed/Cancelled), BudgetItem tree with AllowOverrun inheritance

**Independent Test**: Create Budget in Draft, progress through lifecycle states, build BudgetItem tree, verify AllowOverrun 3-level null-inherit chain

### Tests for User Story 2

- [x] T047 [P] [US2] Unit test: CreateBudgetCommand success + failure in tests/Application.UnitTests/Budgeting/BudgetCommandTests.cs
- [x] T048 [P] [US2] Unit test: SubmitBudgetCommand, ApproveBudgetCommand, ActivateBudgetCommand, SuspendBudgetCommand, CloseBudgetCommand, CancelBudgetCommand success + invalid transition failures in tests/Application.UnitTests/Budgeting/BudgetCommandTests.cs
- [x] T049 [P] [US2] Unit test: CreateBudgetItemCommand success + failure (unique BudgetId+ItemCode) in tests/Application.UnitTests/Budgeting/BudgetItemCommandTests.cs
- [x] T050 [P] [US2] Unit test: UpdateBudgetItemCommand, DeleteBudgetItemCommand, MoveBudgetItemCommand in tests/Application.UnitTests/Budgeting/BudgetItemCommandTests.cs
- [x] T051 [P] [US2] Functional test: Full Budget lifecycle (Draft->Submitted->Approved->Active->Suspended->Active->Closed) with ApprovalHistory recording in tests/Application.FunctionalTests/Budgeting/BudgetLifecycleTests.cs
- [x] T052 [P] [US2] Functional test: Budget cancel from Draft/Submitted/Suspended in tests/Application.FunctionalTests/Budgeting/BudgetLifecycleTests.cs
- [x] T053 [P] [US2] Functional test: BudgetItem tree creation + Level computation + AllowOverrun null-inherit chain (Item null -> Budget null -> BudgetType) in tests/Application.FunctionalTests/Budgeting/BudgetItemTests.cs

### Implementation for User Story 2

- [x] T054 [P] [US2] Create Budget commands: Create, Update in src/Application/Budgeting/Commands/Budgets/
- [x] T055 [US2] Create Budget FSM commands: Submit, Approve, Activate, Suspend, Close, Cancel in src/Application/Budgeting/Commands/Budgets/ — each validates current Status allows transition, records in ApprovalHistory
- [x] T056 [P] [US2] Create Budget queries: GetById, GetList in src/Application/Budgeting/Queries/Budgets/
- [x] T057 [P] [US2] Create Budget DTOs with AllowOverrun effective resolution in src/Application/Budgeting/Common/BudgetDto.cs
- [x] T058 [P] [US2] Create BudgetItem commands: Create, Update, Delete, Move (tree re-parenting) in src/Application/Budgeting/Commands/BudgetItems/
- [x] T059 [P] [US2] Create BudgetItem queries: GetById, GetList, GetTree (recursive CTE for Level) in src/Application/Budgeting/Queries/BudgetItems/
- [x] T060 [P] [US2] Create BudgetItem DTOs with computed Level in src/Application/Budgeting/Common/BudgetItemDto.cs
- [x] T061 [US2] Create Budgets endpoint group with items sub-routes in src/Web/Endpoints/Budgeting/Budgets.cs
- [x] T062 [US2] Run unit tests for US2 — verify all pass
- [x] T063 [US2] Run functional tests for US2 — verify all pass

**Checkpoint**: Budget and BudgetItem fully functional with lifecycle and tree

---

## Phase 5: User Story 3 — Appropriation Management with Computed Availability (Priority: P1)

**Goal**: Appropriation CRUD, lifecycle, availability computation, Transfer Draft-only restriction

**Independent Test**: Create appropriations of different types against same BudgetItem, verify AvailableForAppropriation computed correctly, Blocking rejects over-availability, Warning allows with audit log

### Tests for User Story 3

- [ ] T064 [P] [US3] Unit test: CreateAppropriationCommand success + failure in tests/Application.UnitTests/Budgeting/AppropriationCommandTests.cs
- [ ] T065 [P] [US3] Unit test: Appropriation FSM commands (Submit/Approve/Activate/Suspend/Close/Cancel) success + invalid transition in tests/Application.UnitTests/Budgeting/AppropriationCommandTests.cs
- [ ] T066 [P] [US3] Unit test: ReverseAppropriationCommand creates new Adjustment row with negative Amount in tests/Application.UnitTests/Budgeting/AppropriationCommandTests.cs
- [ ] T067 [P] [US3] Unit test: Transfer type rejected after Draft status in tests/Application.UnitTests/Budgeting/AppropriationCommandTests.cs
- [ ] T068 [P] [US3] Functional test: Availability accuracy — Original+Supplement-Reduction-Adjustment nets to 2 decimals in tests/Application.FunctionalTests/Budgeting/AvailabilityTests.cs
- [ ] T069 [P] [US3] Functional test: Blocking control rejects over-availability with explicit failure in tests/Application.FunctionalTests/Budgeting/AvailabilityTests.cs
- [ ] T070 [P] [US3] Functional test: Warning control allows over-availability with audit log entry in tests/Application.FunctionalTests/Budgeting/AvailabilityTests.cs
- [ ] T071 [P] [US3] Functional test: Appropriation DTO includes Fund/FiscalYear context via Budget join in tests/Application.FunctionalTests/Budgeting/AppropriationTests.cs

### Implementation for User Story 3

- [x] T072 [P] [US3] Create Appropriation commands: Create, Update (Draft only), Delete (Draft only) in src/Application/Budgeting/Commands/Appropriations/
- [x] T073 [US3] Create Appropriation FSM commands: Submit, Approve, Activate, Suspend, Close, Cancel in src/Application/Budgeting/Commands/Appropriations/
- [x] T074 [US3] Create ReverseAppropriationCommand — creates new Appropriation row with type Adjustment, negative Amount, same BudgetItem in src/Application/Budgeting/Commands/Appropriations/
- [x] T075 [US3] Add Transfer Draft-only validation to CreateAppropriation and UpdateAppropriation commands
- [x] T076 [P] [US3] Create Appropriation queries: GetById (with Fund/FiscalYear join projection), GetList in src/Application/Budgeting/Queries/Appropriations/
- [x] T077 [P] [US3] Create Appropriation DTOs with contextual FK projections in src/Application/Budgeting/Common/AppropriationDto.cs
- [x] T078 [US3] Integrate AvailabilityService into Appropriation activation — compute AvailableForAppropriation, evaluate ControlMethod, reject/allow per FR-009
- [x] T079 [US3] Create Appropriations endpoint group in src/Web/Endpoints/Budgeting/Appropriations.cs
- [ ] T080 [US3] Run unit tests for US3 — verify all pass
- [ ] T081 [US3] Run functional tests for US3 — verify all pass

**Checkpoint**: Appropriations fully functional with computed availability

---

## Phase 6: User Story 4 — Encumbrance Lifecycle with Availability Engine (Priority: P1)

**Goal**: Encumbrance CRUD, FSM (Draft through FullyLiquidated), reversal with ReversalOfId, availability check against whole BudgetItem net

**Independent Test**: Create encumbrances under appropriations, verify AvailableForEncumbrance computed correctly, reversal restores availability, Suspended appropriation rejected, closed FiscalYear rejected

### Tests for User Story 4

- [ ] T082 [P] [US4] Unit test: CreateEncumbranceCommand success + failure (availability check, Suspended appropriation) in tests/Application.UnitTests/Budgeting/EncumbranceCommandTests.cs
- [ ] T083 [P] [US4] Unit test: Encumbrance FSM commands (Approve/Activate/Release/Liquidate/Cancel) success + invalid transition in tests/Application.UnitTests/Budgeting/EncumbranceCommandTests.cs
- [ ] T084 [P] [US4] Unit test: ReverseEncumbranceCommand creates new row with ReversalOfId + reason in tests/Application.UnitTests/Budgeting/EncumbranceCommandTests.cs
- [ ] T085 [P] [US4] Functional test: AvailableForEncumbrance = netAppropriated - sum(Active/PartiallyReleased/PartiallyLiquidated where ReversalOfId null) in tests/Application.FunctionalTests/Budgeting/EncumbranceAvailabilityTests.cs
- [ ] T086 [P] [US4] Functional test: Reversal restores availability — reversed encumbrance excluded from sum in tests/Application.FunctionalTests/Budgeting/EncumbranceAvailabilityTests.cs
- [ ] T087 [P] [US4] Functional test: Encumbrance on Suspended appropriation rejected in tests/Application.FunctionalTests/Budgeting/EncumbranceTests.cs
- [ ] T088 [P] [US4] Functional test: Closed FiscalYear (via Budget join) rejects new encumbrance in tests/Application.FunctionalTests/Budgeting/EncumbranceTests.cs

### Implementation for User Story 4

- [ ] T089 [P] [US4] Create Encumbrance commands: Create in src/Application/Budgeting/Commands/Encumbrances/
- [ ] T090 [US4] Create Encumbrance FSM commands: Approve, Activate, Release (PartiallyReleased), Liquidate (PartiallyLiquidated/FullyLiquidated), Cancel in src/Application/Budgeting/Commands/Encumbrances/
- [ ] T091 [US4] Create ReverseEncumbranceCommand — new row with ReversalOfId + ReversalReason in src/Application/Budgeting/Commands/Encumbrances/
- [ ] T092 [US4] Integrate AvailabilityService into Encumbrance create/approve — compute AvailableForEncumbrance via Appropriation->BudgetItem resolution, evaluate ControlMethod
- [ ] T093 [P] [US4] Create Encumbrance queries: GetById (with IsReversed computed, Budget/Fund/FiscalYear context via join), GetList in src/Application/Budgeting/Queries/Encumbrances/
- [ ] T094 [P] [US4] Create GetAvailabilityForEncumbrance query endpoint in src/Application/Budgeting/Queries/Encumbrances/
- [ ] T095 [P] [US4] Create Encumbrance DTOs with computed IsReversed + contextual joins in src/Application/Budgeting/Common/EncumbranceDto.cs
- [ ] T096 [US4] Create Encumbrances endpoint group with availability endpoint in src/Web/Endpoints/Budgeting/Encumbrances.cs
- [ ] T097 [US4] Run unit tests for US4 — verify all pass
- [ ] T098 [US4] Run functional tests for US4 — verify all pass

**Checkpoint**: Encumbrances fully functional with availability engine and reversal

---

## Phase 7: User Story 5 — Derived-Value Projections and Contextual Joins (Priority: P2)

**Goal**: Ensure all derived values (Level, IsReversed, contextual FKs, approval history, creator identity) are correctly projected in DTOs

**Independent Test**: Query any budgeting record and verify all derived fields match computed truth without stored columns

### Tests for User Story 5

- [ ] T099 [P] [US5] Functional test: Classification Level computed correctly at 3+ levels in tests/Application.FunctionalTests/Budgeting/DerivedProjectionTests.cs
- [ ] T100 [P] [US5] Functional test: BudgetItem Level computed correctly in tests/Application.FunctionalTests/Budgeting/DerivedProjectionTests.cs
- [ ] T101 [P] [US5] Functional test: Encumbrance IsReversed = true when reversal row exists, false otherwise in tests/Application.FunctionalTests/Budgeting/DerivedProjectionTests.cs
- [ ] T102 [P] [US5] Functional test: Encumbrance DTO includes Budget, BudgetItem, Fund, FiscalYear context via Appropriation->Budget join in tests/Application.FunctionalTests/Budgeting/DerivedProjectionTests.cs
- [ ] T103 [P] [US5] Functional test: Latest ApprovalHistory decision included in DTO via DocumentType+DocumentId query in tests/Application.FunctionalTests/Budgeting/DerivedProjectionTests.cs
- [ ] T104 [P] [US5] Functional test: Creator identity available from audit CreatedBy in all DTOs in tests/Application.FunctionalTests/Budgeting/DerivedProjectionTests.cs

### Implementation for User Story 5

- [ ] T105 [US5] Audit all DTOs for correct derived field projections — Level via recursive CTE, IsReversed via exists check, contextual FKs via joins, approval via ApprovalHistory query, creator via audit field
- [ ] T106 [US5] Verify all existing queries use correct join paths for derived projections
- [ ] T107 [US5] Run all derived projection functional tests — verify all pass

**Checkpoint**: All derived values correctly projected, no stored columns used

---

## Phase 8: User Story 6 — Permissions and Security (Priority: P2)

**Goal**: Enforce [Authorize(Policy=...)] on all endpoints, verify 401/403 behavior, audit authorization decisions

**Independent Test**: Unauthenticated requests → 401, wrong permission → 403, correct permission → success with audit log entry

### Tests for User Story 6

- [ ] T108 [P] [US6] Functional test: All endpoints return 401 without auth token in tests/Application.FunctionalTests/Budgeting/AuthorizationTests.cs
- [ ] T109 [P] [US6] Functional test: All endpoints return 403 with wrong permission in tests/Application.FunctionalTests/Budgeting/AuthorizationTests.cs
- [ ] T110 [P] [US6] Functional test: Authorization decisions recorded in audit log in tests/Application.FunctionalTests/Budgeting/AuthorizationTests.cs

### Implementation for User Story 6

- [ ] T111 [US6] Add [Authorize(Policy = PermissionCodes.XxxYyy)] to all Budgeting command classes in src/Application/Budgeting/Commands/
- [ ] T112 [US6] Add .RequireAuthorization() to all Budgeting endpoint groups in src/Web/Endpoints/Budgeting/
- [ ] T113 [US6] Verify PermissionCodes constants exist for: Budgets.*, BudgetItems.*, BudgetClassifications.*, BudgetTypes.*, Funds.*, Appropriations.*, Encumbrances.* in src/Application/Common/Security/PermissionCodes.cs
- [ ] T114 [US6] Run authorization functional tests — verify 401/403/audit behavior

**Checkpoint**: All endpoints secured, authorization audited

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, OpenAPI, final validation

- [ ] T115 Update docs/database-schema.md with new Budgeting tables, removed columns documented in docs/database-schema.md
- [ ] T116 Update feature registry with Budget->Appropriation->Encumbrance chain and computed availability in docs/
- [ ] T117 Regenerate OpenAPI spec from updated endpoints in docs/
- [ ] T118 Run dotnet build --no-incremental — verify zero warnings
- [ ] T119 Run all unit tests — verify 100% pass
- [ ] T120 Run all functional tests — verify 100% pass
- [ ] T121 Run quickstart.md validation scenarios — verify all pass
- [ ] T122 Grep for dropped columns (Level on entity, IsReversed stored, denormalized FKs on Appropriation/Encumbrance, ApprovedById/At + IsActive on Budget) — verify zero matches in Domain entities

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — can start after foundation
- **US2 (Phase 4)**: Depends on Phase 2 — can start after foundation (parallel with US1)
- **US3 (Phase 5)**: Depends on Phase 2 + US2 (needs Budget/BudgetItem entities)
- **US4 (Phase 6)**: Depends on Phase 2 + US3 (needs Appropriation entity)
- **US5 (Phase 7)**: Depends on US1+US2+US3+US4 (audits all DTOs)
- **US6 (Phase 8)**: Depends on US1+US2+US3+US4 (needs all endpoints)
- **Polish (Phase 9)**: Depends on all stories complete

### User Story Dependencies

```
Phase 1 (Setup) ──> Phase 2 (Foundation) ──┬──> US1 (Master Data) ──────────> US5 (Projections)
                                            ├──> US2 (Budget Lifecycle) ─────> US5 (Projections)
                                            ├──> US3 (Appropriations) ───────> US5 (Projections)
                                            │         │
                                            │         └──> US4 (Encumbrances) > US5 (Projections)
                                            │
                                            └──> US6 (Permissions) [needs all endpoints]
```

### Parallel Opportunities

- Phase 1: T011, T012 (enums + entities) can run in parallel
- Phase 2: T016, T017, T018 (seeds) can run in parallel
- US1: All unit tests (T022-T029) can run in parallel; all commands (T033-T041) can run in parallel
- US2: All unit tests (T047-T050) can run in parallel
- US3: All unit tests (T064-T067) can run in parallel
- US4: All unit tests (T082-T084) can run in parallel
- US5: All functional tests (T099-T104) can run in parallel
- US6: All functional tests (T108-T110) can run in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (delete old, create new entities/enums)
2. Complete Phase 2: Foundation (migration, seeds, availability service)
3. Complete Phase 3: User Story 1 (BudgetType/Fund/BudgetClassification CRUD + Level)
4. **STOP and VALIDATE**: Test US1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1+2 → Foundation ready (migration wipes data, new schema live)
2. + US1 → Master data functional (MVP!)
3. + US2 → Budget lifecycle + BudgetItem tree
4. + US3 → Appropriations + availability engine
5. + US4 → Encumbrances + reversal
6. + US5 → All derived projections verified
7. + US6 → Security enforced
8. + Polish → Documentation, OpenAPI, final validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable after Phase 2
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Total tasks: 122
