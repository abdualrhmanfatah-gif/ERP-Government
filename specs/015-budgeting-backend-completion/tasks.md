# Tasks: Budgeting Backend Completion

**Input**: Design documents from `/specs/015-budgeting-backend-completion/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-contracts.md, quickstart.md

**Tests**: INCLUDED and MANDATORY — spec FR-27 + Constitution XI make TDD non-negotiable. Test tasks are listed FIRST in every story phase; each must be observed FAILING before its implementation task starts. Tests are never weakened, skipped, or deleted to reach green.

**Organization**: Tasks grouped by user story (US1–US6 from spec.md).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US6)
- Exact file paths included in every description

## Path Conventions

- Backend: `src/Domain`, `src/Application`, `src/Infrastructure`, `src/Web`
- Tests: `tests/Application.UnitTests/Budgeting/`, `tests/Application.FunctionalTests/Budgeting/`
- Frontend: `src/Web/ClientApp/src/`
- Docs: `docs/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Baseline verification before any change

- [X] T001 Verify clean build of the full solution with warnings-as-errors (`dotnet build` at repo root) — record baseline result
- [X] T002 [P] Run existing test suites for a green baseline (`dotnet test tests/Application.UnitTests` and `dotnet test tests/Application.FunctionalTests`) — note any pre-existing failures

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Schema/entity changes, new enum, permission codes, availability summary — required by multiple user stories. NO story work before this phase completes.

- [X] T003 [P] Remove `Budget.IsActive` property from src/Domain/Budgeting/Entities/Budget.cs and its mapping in src/Infrastructure/Data/Configurations/Budgeting/BudgetConfiguration.cs (Status is sole lifecycle state)
- [X] T004 [P] Remove `BudgetItem.OriginalAmount` property from src/Domain/Budgeting/Entities/BudgetItem.cs and its mapping in src/Infrastructure/Data/Configurations/Budgeting/BudgetItemConfiguration.cs (zero-stored-aggregates); keep `Remarks` (confirm config length)
- [X] T005 [P] Add `Appropriation.TargetBudgetItemId int?` to src/Domain/Budgeting/Entities/Appropriation.cs with FK → BudgetItems (Restrict) + index in src/Infrastructure/Data/Configurations/Budgeting/AppropriationConfiguration.cs
- [X] T006 [P] Create BudgetItemMonthlyPlan entity in src/Domain/Budgeting/Entities/BudgetItemMonthlyPlan.cs (BudgetItemId FK Restrict required, Month int 1–12, PlannedAmount decimal(23,2) ≥ 0, BaseAuditableEntity + RowVersion) + config in src/Infrastructure/Data/Configurations/Budgeting/BudgetItemMonthlyPlanConfiguration.cs (UNIQUE (BudgetItemId, Month), explicit precision)
- [X] T007 [P] Add `Suspended = 10` to EncumbranceStatus enum in src/Domain/Budgeting/Enums/EncumbranceStatus.cs (append-only, int storage)
- [X] T008 [P] Add `DbSet<BudgetItemMonthlyPlan> BudgetItemMonthlyPlans` to src/Application/Common/Interfaces/IApplicationDbContext.cs + ApplicationDbContext DbSet + config registration
- [X] T009 [P] Add `PermissionCodes.EncumbrancesUpdate` and `PermissionCodes.EncumbrancesDelete` in src/Application/Common/Security/PermissionCodes.cs (Encumbrances block, lines ~128-136) and register their stub policies in src/Web/DependencyInjection.cs (lines ~159-167)
- [X] T010 Fix all `Budgets.IsActive` writers/readers to Status-only: ActivateBudgetCommand.cs (line ~32 sets true) and grep-replace any remaining `IsActive` usage under src/Application/Budgeting/Commands/Budgets/ and Queries
- [X] T011 Ensure EncumbranceConfiguration (src/Infrastructure/Data/Configurations/Budgeting/EncumbranceConfiguration.cs) declares NO relationship to Suppliers (keep plain VendorId index) so the snapshot realigns
- [X] T012 Create ONE EF migration `CompleteBudgetingSchema` (`dotnet ef migrations add CompleteBudgetingSchema --project src/Infrastructure --startup-project src/Web`) implementing data-model.md change list: drop BudgetItems.OriginalAmount; add BudgetItems.Remarks; drop Budgets.IsActive; length alignment (Funds.FundNumber/LegalAuthority/Description, Budgets.Description, Encumbrances.Description — configs win); add Appropriations.TargetBudgetItemId FK Restrict + index; create BudgetItemMonthlyPlans with UNIQUE; drop FK_Encumbrances_Suppliers_VendorId (keep index). Apply with `dotnet ef database update` and verify zero snapshot drift
- [X] T013 [P] Extend BudgetAvailabilityService (src/Application/Budgeting/Common/BudgetAvailabilityService.cs + interface) with `GetAvailabilitySummaryAsync(budgetItemId)` returning netAppropriated / encumbered / available / effectiveAllowOverrun (reuse existing sums + EvaluateAllowOverrun chain item → budget → type)
- [X] T014 Checkpoint: `dotnet build` clean + all suites green after foundational changes (fix compile fallout of entity changes in Commands/Queries/DTOs touching removed properties)

**Checkpoint**: Foundation ready — user story implementation can begin in priority order.

---

## Phase 3: User Story 1 — Unique Document Numbers (Priority: P1)

**Goal**: BGT/APR numbers system-assigned, immutable, race-free; no user-supplied numbers; typed failures.

**Independent Test**: Parallel creates of the same document kind all succeed with distinct `{prefix}-{D6}` numbers; update never changes a number; concurrent allocation never duplicates (quickstart.md Scenario A).

### Tests for User Story 1 (write FIRST, observe FAIL)

- [X] T015 [P] [US1] Unit tests for typed sequence failures — unknown document type and inactive sequence throw typed exceptions (not "Error: ..." strings) in tests/Application.UnitTests/FinancialSettings/DocumentSequenceServiceTests.cs (new)
- [X] T016 [P] [US1] Functional test: N parallel appropriation creates yield N distinct APR numbers, zero unique-index collisions in tests/Application.FunctionalTests/Budgeting/SequenceConcurrencyTests.cs (new)
- [X] T017 [P] [US1] Unit test: CreateBudgetCommand no longer exposes BudgetNumber; number comes from DocumentSequenceService; validator rejects supplied number in tests/Application.UnitTests/Budgeting/BudgetCommandTests.cs
- [X] T018 [P] [US1] Functional test: number immutability — approve/activate/update leaves BudgetNumber and AppropriationNumber unchanged in tests/Application.FunctionalTests/Budgeting/SequenceImmutabilityTests.cs (new)

### Implementation for User Story 1

- [X] T019 [US1] Rewrite DocumentSequenceService.GenerateNextNumberAsync as single atomic `UPDATE ... SET CurrentNumber += 1 OUTPUT inserted.CurrentNumber` (src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs lines 41-59); add typed exceptions DocumentSequenceNotFoundException / DocumentSequenceInactiveException; remove string-error returns
- [X] T020 [US1] Wire BGT into CreateBudgetCommand (src/Application/Budgeting/Commands/Budgets/CreateBudgetCommand.cs): call service, remove user-supplied BudgetNumber from command + CreateBudgetCommandValidator
- [X] T021 [US1] Wire APR into CreateAppropriationCommand (src/Application/Budgeting/Commands/Appropriations/CreateAppropriationCommand.cs lines 40-49): assign AppropriationNumber via service; catch typed exceptions → Result.Failure
- [X] T022 [US1] Update existing BudgetCommandTests/BudgetLifecycleTests that supply BudgetNumber; make US1 suites green

**Checkpoint**: US1 independently verified — parallel-create test shows 0 duplicates.

---

## Phase 4: User Story 2 — Encumbrance Full Stack (Priority: P1)

**Goal**: Complete encumbrance document lifecycle with availability gate, computed IsReversed, reversal, ApprovalHistory on every transition, endpoints + permissions.

**Independent Test**: Walk Draft→…→Closed/Cancelled/Reversed per FSM (data-model.md); gate honors ControlMethod (Blocking rejects, Warning records override); reversal from Active/Suspended only; availability restored computationally (quickstart.md Scenario B).

### Tests for User Story 2 (write FIRST, observe FAIL)

- [X] T023 [P] [US2] Unit tests: encumbrance FSM guard matrix (legal/illegal transitions per data-model.md; reverse only from Active|Suspended; update/delete Draft-only) in tests/Application.UnitTests/Budgeting/EncumbranceCommandTests.cs (new)
- [X] T024 [P] [US2] Unit tests: create gate decisions via EvaluateControlMethod — None create / Warning create + ApprovalHistory reason / Blocking Result.Failure; plus CreateEncumbranceCommandValidator rules in tests/Application.UnitTests/Budgeting/EncumbranceCommandTests.cs
- [X] T025 [P] [US2] Functional test: full lifecycle with exactly one ApprovalHistory row per transition ("Encumbrance", "from -> to") in tests/Application.FunctionalTests/Budgeting/EncumbranceLifecycleTests.cs (new)
- [X] T026 [P] [US2] Functional test: reversal — new negative row with ReversalOfId, original → Reversed, isReversed computed true on original, availability reduced then restored, reverse-from-invalid rejected, posted fields never mutated in tests/Application.FunctionalTests/Budgeting/EncumbranceLifecycleTests.cs
- [X] T027 [P] [US2] Functional test: authorization — 401 unauthenticated and 403 without permission for every /api/Encumbrances route (incl. EncumbrancesUpdate/Delete) in tests/Application.FunctionalTests/Budgeting/EncumbranceAuthorizationTests.cs (new)

### Implementation for User Story 2

- [X] T028 [US2] Encumbrance DTOs with computed IsReversed (subquery exists row WHERE ReversalOfId = id): src/Application/Budgeting/Queries/Encumbrances/EncumbranceDtos.cs (EncumbranceListItem, EncumbranceDetail per contracts/api-contracts.md §1)
- [X] T029 [US2] GetEncumbrancesListQuery + handler + validator (filters: AppropriationId, Type, Status, date range, reversal-state; paging) in src/Application/Budgeting/Queries/Encumbrances/GetEncumbrancesListQuery.cs
- [X] T030 [US2] GetEncumbranceByIdQuery + handler in src/Application/Budgeting/Queries/Encumbrances/GetEncumbranceByIdQuery.cs
- [X] T031 [US2] CreateEncumbranceCommand + validator + availability gate (controlMethod from owning Budget's BudgetType.ControlMethod; ENC number via DocumentSequenceService; Warning override → ApprovalHistory Reason) in src/Application/Budgeting/Commands/Encumbrances/CreateEncumbranceCommand.cs
- [X] T032 [US2] UpdateEncumbranceCommand + DeleteEncumbranceCommand (Draft-only, RowVersion verified, Result<T>) in src/Application/Budgeting/Commands/Encumbrances/
- [X] T033 [US2] SubmitEncumbranceCommand / ApproveEncumbranceCommand / ActivateEncumbranceCommand (FSM guards, ApprovalHistory each) in src/Application/Budgeting/Commands/Encumbrances/
- [X] T034 [US2] SuspendEncumbranceCommand / CloseEncumbranceCommand / CancelEncumbranceCommand (per FSM: suspend Active→Suspended; close Active|Suspended→Closed; cancel non-terminal→Cancelled) in src/Application/Budgeting/Commands/Encumbrances/
- [X] T035 [US2] ReverseEncumbranceCommand (Active|Suspended only; required reason; new negative-effect row ReversalOfId = original in same SaveChanges; original → Reversed; no mutation of original financial fields) in src/Application/Budgeting/Commands/Encumbrances/ReverseEncumbranceCommand.cs
- [X] T036 [US2] Endpoint group src/Web/Endpoints/Budgeting/Encumbrances.cs → /api/Encumbrances: GET /, GET /{id}, POST /, PUT /{id}, DELETE /{id}, PATCH /{id}/submit|approve|activate|suspend|close|cancel|reverse — all .RequireAuthorization with mapped permission codes, RowVersion on all mutations, problem-details failures
- [X] T037 [US2] Checkpoint: US2 unit + functional + authorization suites green

**Checkpoint**: Encumbrance stack independently functional (US1 numbers already wired into create).

---

## Phase 5: User Story 3 — Transfer Appropriation Pairs (Priority: P2)

**Goal**: Atomic transfer pair (negative source row + positive target row via TargetBudgetItemId), joint lifecycle, hard source-coverage validation.

**Independent Test**: One command → exactly 2 rows, all-or-nothing; transitioning one row transitions both; availability of source item unchanged by active pair; invalid transfers rejected (quickstart.md Scenario C).

### Tests for User Story 3 (write FIRST, observe FAIL)

- [X] T038 [P] [US3] Unit tests: transfer validation — same Budget required, source ≠ target, source net active appropriation ≥ amount (clarified Q2), amount > 0, Transfer stays Draft-only in tests/Application.UnitTests/Budgeting/TransferCommandTests.cs (new)
- [X] T039 [P] [US3] Functional test: pair atomicity (failure → neither row), joint status transitions (submit/approve/activate on one → both), net-zero item availability with active pair, in tests/Application.FunctionalTests/Budgeting/TransferPairTests.cs (new)

### Implementation for User Story 3

- [X] T040 [US3] CreateTransferAppropriationCommand + validator: one SaveChanges creating both rows per data-model.md pair rules (source row negative-effect on source item with TargetBudgetItemId = target; target row positive on target item with TargetBudgetItemId = source); both get APR numbers; in src/Application/Budgeting/Commands/Appropriations/CreateTransferAppropriationCommand.cs
- [X] T041 [US3] Joint lifecycle: extend appropriation lifecycle handlers (Submit/Approve/Activate/Suspend/Close/Cancel in src/Application/Budgeting/Commands/Appropriations/) to detect Effect=Transfer and transition the partner row (pair-match rule in data-model.md) in the same unit of work
- [X] T042 [US3] Endpoint POST /api/Appropriations/transfers in src/Web/Endpoints/Budgeting/Appropriations.cs (AppropriationsCreate policy; returns { sourceId, targetId })
- [X] T043 [US3] Checkpoint: US3 suites green; existing CreateAppropriationCommand still rejects Effect=Transfer

---

## Phase 6: User Story 4 — Availability Query API (Priority: P2)

**Goal**: Computed availability (netAppropriated, encumbered, available) + EffectiveAllowOverrun exposed for BudgetItem and Appropriation.

**Independent Test**: Seeded figures match service exactly; overrun resolution follows item > budget > type precedence; unknown id → 404 (quickstart.md Scenario D).

### Tests for User Story 4 (write FIRST, observe FAIL)

- [X] T044 [P] [US4] Functional test: availability endpoints — exact figures for seeded appropriations/encumbrances, EffectiveAllowOverrun precedence (flip each layer), 404 unknown ids, whole-item semantics via appropriation endpoint, in tests/Application.FunctionalTests/Budgeting/BudgetAvailabilityEndpointTests.cs (new)

### Implementation for User Story 4

- [X] T045 [US4] GetBudgetItemAvailabilityQuery + handler (uses GetAvailabilitySummaryAsync) in src/Application/Budgeting/Queries/BudgetItems/GetBudgetItemAvailabilityQuery.cs
- [X] T046 [US4] GetAppropriationAvailabilityQuery + handler (resolve owning BudgetItem; same whole-item figures) in src/Application/Budgeting/Queries/Appropriations/GetAppropriationAvailabilityQuery.cs
- [X] T047 [US4] Endpoints: new group src/Web/Endpoints/Budgeting/BudgetItems.cs → GET /api/BudgetItems/{id}/availability; add GET /api/Appropriations/{id}/availability to src/Web/Endpoints/Budgeting/Appropriations.cs (View permissions)
- [X] T048 [US4] Checkpoint: US4 suite green; figures identical from both endpoints

---

## Phase 7: User Story 5 — Budget Head Fields + Monthly Plans (Priority: P2)

**Goal**: Budget head fields accepted/persisted (TotalAmount no longer 0); monthly plans GET/PUT batch-12, informational only.

**Independent Test**: Head fields round-trip; monthly plan PUT replaces idempotently; validation rejects month ∉ 1–12 / negative / duplicates (quickstart.md Scenario E).

### Tests for User Story 5 (write FIRST, observe FAIL)

- [X] T049 [P] [US5] Unit tests: budget head mapping — create/update accept TotalAmount, AllowOverrun?, EffectiveFrom, EffectiveTo?, Description; persist exactly as entered; validator rules in tests/Application.UnitTests/Budgeting/BudgetCommandTests.cs
- [X] T050 [P] [US5] Unit tests: monthly plan validation — month 1–12, PlannedAmount ≥ 0, no duplicate months, batch ≤ 12, in tests/Application.UnitTests/Budgeting/MonthlyPlanCommandTests.cs (new)
- [X] T051 [P] [US5] Functional test: PUT monthly plan replaces idempotently; GET returns current plan; UNIQUE (BudgetItemId, Month) enforced in tests/Application.FunctionalTests/Budgeting/MonthlyPlanTests.cs (new)

### Implementation for User Story 5

- [X] T052 [US5] CreateBudgetCommand/UpdateBudgetCommand accept + persist TotalAmount, AllowOverrun (bool?), EffectiveFrom, EffectiveTo (nullable), Description; update validators in src/Application/Budgeting/Commands/Budgets/
- [X] T053 [US5] Update request DTOs in src/Web/Endpoints/Budgeting/Budgets.cs (add head fields to POST/PUT request records; BudgetNumber stays absent from requests)
- [X] T054 [US5] SaveBudgetItemMonthlyPlanCommand (batch ≤12, replace-all in one SaveChanges, validations) + GetBudgetItemMonthlyPlanQuery in src/Application/Budgeting/Commands/BudgetItems/ + Queries/BudgetItems/
- [X] T055 [US5] Endpoints: GET /api/BudgetItems/{id}/monthly-plan + PUT /api/BudgetItems/{id}/monthly-plan in src/Web/Endpoints/Budgeting/BudgetItems.cs
- [X] T056 [US5] Checkpoint: US5 suites green; monthly plans provably unread by any workflow (grep + no enforcement paths)

---

## Phase 8: Polish, Cleanup & Cross-Cutting (US6 remainder)

**Purpose**: Appropriation edge-case test gaps (spec 013 US3), frontend cleanup, docs, final validation.

- [X] T057 [P] [US6] Functional tests: Appropriation edge-case gaps — activate gate Blocking/Warning/None, reduction overflowing availability rejected, concurrency conflict surfaced distinctly, in tests/Application.FunctionalTests/Budgeting/AppropriationEdgeCaseTests.cs (new)
- [X] T058 [P] [US6] Frontend: run `npm run generate-api` in src/Web/ClientApp to regenerate web-api-client.ts; rewrite src/Web/ClientApp/src/features/budgeting/hooks/useEncumbrances.ts (remove liquidatePartial/liquidateFull + useReleaseEncumbrance) and src/Web/ClientApp/src/features/budgeting/shared/client.ts wrappers to consume the generated EncumbrancesClient per contracts/api-contracts.md §1
- [X] T059 [P] [US6] Remove 'Liquidation' ('التصفية') dropdown option from src/Web/ClientApp/src/features/financial/document-sequences/components/DocumentSequenceForm.tsx (lines ~16-17)
- [X] T060 [P] [US6] Extend docs/database-schema.md with "Budgeting tables" section (first section, per data-model.md: 7 tables + BudgetItemMonthlyPlans, key constraints/indexes)
- [X] T061 [P] [US6] Update docs: docs/feature-architecture-map-v1.0.md FM-004 Budget Liquidations → REMOVED; docs/final-business-feature-registry.md BF-002/BF-003 notes (encumbrances live, transfers, monthly plan)
- [X] T062 [P] [US6] Mark specs/013-budgeting-backend-rebuild/spec.md Status line: "Superseded by specs/015-budgeting-backend-completion"
- [X] T063 [US6] Run quickstart.md Scenarios A–F end-to-end; full solution build + all suites green (no skipped/weakened tests)
- [X] T064 [US6] Final contract review: OpenAPI v1.json shows all new operations with stable names; problem-details semantics; RowVersion round-trip on every mutation; `npm run build` (prebuild regen) passes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: immediate
- **Foundational (Phase 2)**: after Phase 1 — BLOCKS all stories (schema + enum + permissions + availability summary)
- **US1 (Phase 3)**: after Phase 2 — sequence rewrite blocks encumbrance create number assignment
- **US2 (Phase 4)**: after US1 (create needs ENC wiring pattern + atomic service); biggest phase
- **US3 (Phase 5)**: after Phase 2 (needs TargetBudgetItemId); independent of US2 (may run in parallel)
- **US4 (Phase 6)**: after Phase 2 (summary method); independent of US1–US3
- **US5 (Phase 7)**: after Phase 2; US5 budget-head part independent; monthly-plan endpoints share BudgetItems.cs group with US4 endpoint (sequence T047 before T055)
- **Polish/US6 (Phase 8)**: after all stories; T057 (appropriation edge cases) can start after Phase 2

### User Story Dependencies

- **US1**: Foundational only. **US2**: + US1 (ENC number assignment). **US3**: Foundational only. **US4**: Foundational only. **US5**: Foundational only (T055 after T047 for shared file). **US6**: all stories (docs/quickstart) but T057–T062 parallelizable early.

### Within Each Story

Tests → observed FAILING → implementation → checkpoint green. Never implement before its test fails.

### Parallel Opportunities

- Phase 2: T003–T009, T011, T013 all [P] (different files)
- Story phases: all test tasks [P] within a story; stories US3/US4/US5 can proceed in parallel once Foundational done (US2 after US1)
- Phase 8: T057–T062 all [P]

---

## Parallel Example: Foundational

```text
Task: "Remove Budget.IsActive (T003)"
Task: "Remove BudgetItem.OriginalAmount (T004)"
Task: "Add Appropriation.TargetBudgetItemId (T005)"
Task: "Create BudgetItemMonthlyPlan entity (T006)"
```

## Parallel Example: User Story 2 tests

```text
Task: "FSM guard matrix unit tests (T023)"
Task: "Gate decision unit tests (T024)"
Task: "Lifecycle functional test (T025)"
Task: "Reversal functional test (T026)"
Task: "Authorization functional test (T027)"
```

---

## Implementation Strategy

### MVP First (US1 + US2 = P1)

1. Phase 1 Setup → Phase 2 Foundational
2. Phase 3 US1 (fixes the active runtime bug) → validate
3. Phase 4 US2 (encumbrance stack) → validate independently
4. STOP and demo: budgeting chain Budget→Appropriation→Encumbrance works with unique numbers and gated commitments

### Incremental Delivery

- +US3 (transfers) → +US4 (availability API) → +US5 (head fields + monthly plans) → Phase 8 (cleanup/docs/tests)
- Each increment keeps full suite green (Constitution XI: refactor only on green)

### Notes

- Commit after each task or logical group
- One migration only (T012) — all schema changes land there; later stories must NOT add migrations
- ApprovalHistory shape frozen (research.md D13) — no Action enum
- Avoid: cross-story edits to the same file in parallel (T047/T055 share BudgetItems.cs — sequence them)
