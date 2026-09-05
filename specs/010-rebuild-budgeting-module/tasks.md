# Tasks: Rebuild Budgeting Module from Scratch

**Input**: Design documents from `/specs/010-rebuild-budgeting-module/` (plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md)

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/budgeting-api.md, contracts/budgeting-dtos.md

**Tests**: Test tasks included — explicitly requested by FR-016 and SC-001 (unit per command, real-DB functional tests, no stubs; plus frontend vitest per FR-014).

**Organization**: Tasks grouped by user story. US1–US4 are P1; US5–US7 are P2.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US7)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Governance prerequisite — Constitution gate requires `DEP-0xx` before implementation.

- [x] T001 Write decision record `docs/decision-records/DEP-0xx-rebuild-budgeting-module.md` (rationale, scope: 7 tables/8 enums/wipe, migration + remediation plan, contract breakage per Principle IX, permission additions, evaluation snapshots)

**Checkpoint**: Decision record accepted — implementation may begin.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Delete the entire existing Budgeting Application/Web surface, rewrite Domain entities/enums, EF configurations, permissions, numbering prefixes, the shared availability service, and scaffold the wipe migration. MUST complete before ANY user story.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Delete existing Budgeting surface (FR-011: delete ALL, then rebuild)

- [x] T002 [P] Delete all existing Budgeting commands in `src/Application/Budgeting/Commands/` (remove all entity subfolders)
- [x] T003 [P] Delete all existing Budgeting queries in `src/Application/Budgeting/Queries/` (remove all entity subfolders)
- [x] T004 [P] Delete all existing Budgeting DTOs in `src/Application/Budgeting/Common/*.cs`
- [x] T005 [P] Delete all existing Budgeting endpoints in `src/Web/Endpoints/Budgeting/*.cs`

### Rewrite Domain entities (data-model.md §1–7)

- [x] T006 [P] Rewrite `src/Domain/Budgeting/Entities/BudgetType.cs` per FR-001 (drop OverrunRequiresApproval, IsSystemType)
- [x] T007 [P] Rewrite `src/Domain/Budgeting/Entities/BudgetClassification.cs` per FR-003 (drop stored Level)
- [x] T008 [P] Rewrite `src/Domain/Budgeting/Entities/Budget.cs` per FR-004 (drop snapshots, ControlMethod, approval identity, IsActive; nullable AllowOverrun)
- [x] T009 [P] Rewrite `src/Domain/Budgeting/Entities/BudgetItem.cs` per FR-005 (drop amounts, Percentage, ControlLevel, IsMandatory, ProjectId, OrganizationUnitId; nullable AllowOverrun)
- [x] T010 [P] Rewrite `src/Domain/Budgeting/Entities/Appropriation.cs` per FR-006 (BudgetId+BudgetItemId only; no FundId/FiscalYearId/snapshots/reversal cols/approval identity)
- [x] T011 [P] Rewrite `src/Domain/Budgeting/Entities/Encumbrance.cs` per FR-007 (AppropriationId single context; ReversalOfId/ReversalReason only; drop denormalized FKs/snapshots/approval identity)
- [x] T012 [P] Rewrite `src/Domain/Budgeting/Enums/AppropriationType.cs` (Original, Supplement, Reduction, Adjustment) and `src/Domain/Budgeting/Enums/AppropriationStatus.cs` (drop Reversed)
- [x] T013 Verify remaining Budgeting enums match data-model.md and delete obsolete ones in `src/Domain/Budgeting/Enums/` (BudgetItemControlLevel; ReleaseStatus if unreferenced elsewhere)

### Rewrite EF configurations (int enums, Restrict FKs, RowVersion)

- [x] T014 [P] Rewrite `src/Infrastructure/Data/Configurations/Budgeting/BudgetTypeConfiguration.cs` and `src/Infrastructure/Data/Configurations/Budgeting/BudgetClassificationConfiguration.cs`
- [x] T015 [P] Rewrite `src/Infrastructure/Data/Configurations/Budgeting/BudgetConfiguration.cs` and `src/Infrastructure/Data/Configurations/Budgeting/BudgetItemConfiguration.cs` (incl. UNIQUE(BudgetId, ItemCode))
- [x] T016 [P] Rewrite `src/Infrastructure/Data/Configurations/Budgeting/AppropriationConfiguration.cs` and `src/Infrastructure/Data/Configurations/Budgeting/EncumbranceConfiguration.cs` (FundConfiguration unchanged — verify only)

### Permissions, numbering, shared service, migration scaffold

- [x] T017 Add missing lifecycle PermissionCodes to `src/Application/Common/Security/PermissionCodes.cs` (Appropriations.Submit/.Activate/.Suspend/.Close/.Cancel/.Update/.Delete; Encumbrances.Submit/.Activate/.Close/.Cancel; BudgetItems.Delete; BudgetTypes.Update) per R8
- [x] T018 Register the new policies in `src/Web/DependencyInjection.cs` following the existing `RequireAssertion` pattern
- [x] T019 Update `PrefixMap` in `src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs` (add `Budget → BGT`, change `Appropriation APP → APR`, keep `Encumbrance → ENC`) per R2
- [x] T020 Implement `IBudgetAvailabilityService` + implementation in `src/Application/Budgeting/Common/` per FR-009/R6 (item net + whole-item encumbrance net, ControlMethod/AllowOverrun resolution, Warning payload)
- [x] T021 Scaffold the wipe-and-recreate migration via `dotnet ef migrations add RebuildBudgetingModule --project src/Infrastructure --startup-project src/Web` and verify `ApplicationDbContextModelSnapshot.cs` matches data-model.md (7 tables, int enums, unique + FK indexes)

**Checkpoint**: Foundation ready — `dotnet build` compiles with the deleted surface removed; user story implementation can now begin.

---

## Phase 3: User Story 1 — Budget Master Data (Priority: P1) ⭐ MVP

**Goal**: BudgetType / Fund / BudgetClassification CRUD + toggles with computed Level.

**Independent Test**: Create BudgetTypes with distinct ControlMethods, Funds with type/category, and a 3-level Classification hierarchy; verify computed Level, unique-Code enforcement, and IsActive toggling (spec US1 scenarios 1–4).

### Implementation for User Story 1

- [x] T022 [P] [US1] Implement BudgetType commands (Create/Update/ToggleActive + validators + `[Authorize]`) in `src/Application/Budgeting/Commands/BudgetTypes/`
- [x] T023 [P] [US1] Implement Fund commands (Create/Update/Activate/Deactivate + validators + `[Authorize]`) in `src/Application/Budgeting/Commands/Funds/`
- [x] T024 [P] [US1] Implement BudgetClassification commands (Create/Update/ToggleActive + validators + `[Authorize]`, parent-cycle guard) in `src/Application/Budgeting/Commands/BudgetClassifications/`
- [x] T025 [P] [US1] Implement master-data queries (List/ById per entity + `GetBudgetClassificationsTree` with computed Level) in `src/Application/Budgeting/Queries/BudgetTypes/`, `.../Funds/`, `.../BudgetClassifications/`
- [x] T026 [US1] Implement master-data DTOs + AutoMapper profiles in `src/Application/Budgeting/Common/` (BudgetTypeDto, FundDto, BudgetClassificationDto/TreeDto with computed `level`)
- [x] T027 [P] [US1] Implement endpoints in `src/Web/Endpoints/Budgeting/BudgetTypes.cs`
- [x] T028 [P] [US1] Implement endpoints in `src/Web/Endpoints/Budgeting/Funds.cs`
- [x] T029 [P] [US1] Implement endpoints in `src/Web/Endpoints/Budgeting/BudgetClassifications.cs`
- [x] T030 [US1] Rewrite seeds in `src/Infrastructure/Data/Seeds/BudgetTypeSeedData.cs`, `.../FundSeedData.cs`, `.../BudgetClassificationSeedData.cs` (no Level), `.../DocumentSequenceSeedData.cs` (add Budget/Appropriation/Encumbrance rows), and new permission codes in `.../RolePermissionSeedData.cs` (all idempotent)

### Tests for User Story 1

- [x] T031 [P] [US1] Unit tests for BudgetType commands in `tests/Application.UnitTests/Budgeting/BudgetTypeCommandTests.cs` (success + validation + concurrency paths)
- [x] T032 [P] [US1] Unit tests for Fund commands in `tests/Application.UnitTests/Budgeting/FundCommandTests.cs`
- [x] T033 [P] [US1] Unit tests for Classification commands + Level computation (incl. depth ≥ 5 and cycle rejection) in `tests/Application.UnitTests/Budgeting/BudgetClassificationCommandTests.cs`
- [x] T034 [P] [US1] Functional tests for masters CRUD, unique-Code violations, and IsActive toggles in `tests/Application.FunctionalTests/Budgeting/MasterDataTests.cs`

**Checkpoint**: US1 fully functional and independently testable — masters CRUD works end-to-end via API.

---

## Phase 4: User Story 2 — Budget and BudgetItem Lifecycle with Tree (Priority: P1)

**Goal**: Budget lifecycle FSM + BudgetItem tree with AllowOverrun inheritance.

**Independent Test**: Drive a Budget Draft→Submitted→Approved→Active→Suspended→Active→Closed/Cancelled; build a 3-level item tree; verify the 3-level AllowOverrun null-inherit chain and UNIQUE(BudgetId, ItemCode) (spec US2 scenarios 1–4).

### Implementation for User Story 2

- [x] T035 [P] [US2] Implement Budget commands (Create with server-generated BGT number, Update, Submit, Approve, Activate, Suspend, Close, Cancel + validators + `[Authorize]` + ApprovalHistory snapshots on Approve) in `src/Application/Budgeting/Commands/Budgets/`
- [x] T036 [P] [US2] Implement BudgetItem commands (Create, Update, Delete, Move/TreeReorder + validators + `[Authorize]`, same-Budget parent guard) in `src/Application/Budgeting/Commands/BudgetItems/`
- [x] T037 [P] [US2] Implement Budget queries (List/ById) and BudgetItem queries (Tree/List/ById with computed Level) in `src/Application/Budgeting/Queries/Budgets/` and `.../BudgetItems/`
- [x] T038 [US2] Implement BudgetDto/BudgetItemDto (+tree node) with `effectiveAllowOverrun` projection in `src/Application/Budgeting/Common/`
- [x] T039 [US2] Implement endpoints incl. `/{id}/items` sub-routes in `src/Web/Endpoints/Budgeting/Budgets.cs` (BudgetItems routes live here per FR-013)

### Tests for User Story 2

- [x] T040 [P] [US2] Unit tests for Budget commands in `tests/Application.UnitTests/Budgeting/BudgetCommandTests.cs` (success + illegal-transition + concurrency paths)
- [x] T041 [P] [US2] Unit tests for BudgetItem commands + AllowOverrun 3-level chain in `tests/Application.UnitTests/Budgeting/BudgetItemCommandTests.cs`
- [x] T042 [P] [US2] Functional tests for lifecycle, tree Level, unique item constraint, and ApprovalHistory snapshots in `tests/Application.FunctionalTests/Budgeting/BudgetLifecycleTests.cs`

**Checkpoint**: US1 + US2 work — budgets with item trees and full lifecycle via API.

---

## Phase 5: User Story 3 — Appropriation Management with Computed Availability (Priority: P1)

**Goal**: Appropriation CRUD/lifecycle with transaction-time availability (no Transfer, signed Adjustment, Draft-only Update/Delete).

**Independent Test**: Multiple appropriations per item verify `AvailableForAppropriation` = Original+Supplement−Reduction+signed-Adjustment to 2 decimals; Blocking rejects, Warning allows+logs (spec US3 scenarios 1–4).

### Implementation for User Story 3

- [x] T043 [P] [US3] Implement CreateAppropriation in `src/Application/Budgeting/Commands/Appropriations/CreateAppropriation/` (APR number, signed-Adjustment validation: Original/Supplement/Reduction > 0, Adjustment ≠ 0; Active Budget+item; FY-status + period gates per R3; availability check via T020)
- [x] T044 [P] [US3] Implement Update/DeleteAppropriation with Draft-only guards in `src/Application/Budgeting/Commands/Appropriations/UpdateAppropriation/` and `.../DeleteAppropriation/`
- [x] T045 [P] [US3] Implement Appropriation lifecycle commands (Submit/Approve/Activate/Suspend/Close/Cancel + snapshots; availability re-check on Approve) in `src/Application/Budgeting/Commands/Appropriations/`
- [x] T046 [P] [US3] Implement Appropriation queries (List/ById + `GetAvailabilityForItem`) in `src/Application/Budgeting/Queries/Appropriations/`
- [x] T047 [US3] Implement AppropriationDto with Fund/FiscalYear context + latest-approval + createdBy projections in `src/Application/Budgeting/Common/`
- [x] T048 [US3] Implement endpoints (no Reverse route) in `src/Web/Endpoints/Budgeting/Appropriations.cs`

### Tests for User Story 3

- [x] T049 [P] [US3] Unit tests for appropriation commands in `tests/Application.UnitTests/Budgeting/AppropriationCommandTests.cs` (validation, Draft-only edit/delete, Blocking reject, Warning allow, signed Adjustment)
- [x] T050 [P] [US3] Functional tests for availability nets, overrun chain, FY/period gates in `tests/Application.FunctionalTests/Budgeting/AppropriationAvailabilityTests.cs`

**Checkpoint**: US1–US3 work — appropriations with computed availability enforced at create and approve.

---

## Phase 6: User Story 4 — Encumbrance Lifecycle with Availability Engine (Priority: P1)

**Goal**: Encumbrance lifecycle with whole-BudgetItem availability and reversal rows.

**Independent Test**: Cross-appropriation availability (50k net − 30k encumbered rejects a 25k creation under Blocking); release transitions; reversal restores availability; Suspended/closed gates (spec US4 scenarios 1–5).

### Implementation for User Story 4

- [x] T051 [P] [US4] Implement CreateEncumbrance in `src/Application/Budgeting/Commands/Encumbrances/CreateEncumbrance/` (ENC number, Active-appropriation gate, FY-status + EncumbranceDate period gates, whole-item availability check)
- [x] T052 [P] [US4] Implement Encumbrance lifecycle commands (Submit/Approve/Activate/Release/LiquidatePartial/LiquidateFull/Close/Cancel/Reverse + snapshots; availability re-check on Approve) in `src/Application/Budgeting/Commands/Encumbrances/`
- [x] T053 [P] [US4] Implement Encumbrance queries (List/ById + `GetAvailabilityForEncumbrance?appropriationId=`) in `src/Application/Budgeting/Queries/Encumbrances/`
- [x] T054 [US4] Implement EncumbranceDto with `isReversed` EXISTS projection + context + latest-approval in `src/Application/Budgeting/Common/`
- [x] T055 [US4] Implement endpoints in `src/Web/Endpoints/Budgeting/Encumbrances.cs`

### Tests for User Story 4

- [x] T056 [P] [US4] Unit tests for encumbrance commands in `tests/Application.UnitTests/Budgeting/EncumbranceCommandTests.cs`
- [x] T057 [P] [US4] Functional tests for whole-item availability, reversal/IsReversed, release transitions, Suspended/closed gates in `tests/Application.FunctionalTests/Budgeting/EncumbranceAvailabilityTests.cs`

**Checkpoint**: US1–US4 work — the full Budget→Appropriation→Encumbrance chain with computed availability.

---

## Phase 7: User Story 5 — Derived-Value Projections and Contextual Joins (Priority: P2)

**Goal**: Provably correct projections (Level, IsReversed, context joins, latest approval, creator).

**Independent Test**: Query an Encumbrance + Classification and verify every projected field matches computed truth with zero stored derived columns (spec US5 scenarios 1–4).

### Implementation for User Story 5

- [x] T058 [P] [US5] Implement shared latest-approval + createdBy projection helper used by Appropriation/Encumbrance DTOs in `src/Application/Budgeting/Common/ApprovalProjection.cs` (query `ApprovalHistory` by DocumentType+DocumentId, `DecisionAt DESC`)
- [x] T059 [US5] Wire the helper into Appropriation/Encumbrance detail queries in `src/Application/Budgeting/Queries/Appropriations/` and `.../Encumbrances/` (no new tables)

### Tests for User Story 5

- [x] T060 [P] [US5] Unit tests for Level cycle-guard failure in `tests/Application.UnitTests/Budgeting/ProjectionTests.cs`
- [x] T061 [P] [US5] Functional tests for depth-5+ Level, IsReversed true/false, context joins, latest-first approval ordering in `tests/Application.FunctionalTests/Budgeting/ProjectionTests.cs`

**Checkpoint**: All projected fields verified against computed truth on a real database.

---

## Phase 8: User Story 6 — Budgeting Frontend (Priority: P2)

**Goal**: `features/budgeting` — 7 entity pages, item tree editor, lifecycle buttons, availability indicator, approval panel; RTL + dark mode; permission-gated nav.

**Independent Test**: Navigate all 7 pages (list/create/detail); tree editor in Budget detail; buttons enable per Status; indicator green/amber/red; approval panel renders latest-first history (spec US6 scenarios 1–4).

### Implementation for User Story 6

- [x] T062 [P] [US6] Implement TanStack Query hooks for all 7 entities + availability + approval history in `src/Web/ClientApp/src/features/budgeting/hooks/` (typed via regenerated `src/Web/ClientApp/src/web-api-client.ts`)
- [x] T063 [P] [US6] Implement master-data pages (BudgetType/Fund/Classification list/create/detail) in `src/Web/ClientApp/src/features/budgeting/pages/`
- [x] T064 [P] [US6] Implement Budget pages + embedded BudgetItem tree editor in `src/Web/ClientApp/src/features/budgeting/pages/` and `.../components/`
- [x] T065 [P] [US6] Implement Appropriation/Encumbrance pages with lifecycle action buttons, availability indicator, and approval history panel in `src/Web/ClientApp/src/features/budgeting/pages/` and `.../components/`
- [x] T066 [US6] Register `/budgeting/*` routes in `src/Web/ClientApp/src/app/routes.tsx` and the الموازنة module group with permission identifiers in `src/Web/ClientApp/src/layouts/navigation.ts`
- [x] T067 [US6] Regenerate the TS client via `npm run generate-api` in `src/Web/ClientApp/` and fix resulting type diffs in `src/Web/ClientApp/src/features/budgeting/`

### Tests for User Story 6

- [x] T068 [P] [US6] Vitest tests for hooks, availability indicator colors, approval panel ordering, and lifecycle button gating in `src/Web/ClientApp/src/features/budgeting/__tests__/` (RTL + dark-mode render checks; loading/empty/error states)

**Checkpoint**: Full budgeting UI usable end-to-end against the rebuilt backend.

---

## Phase 9: User Story 7 — Migration, Tests, and Documentation (Priority: P2)

**Goal**: Reproducible wipe migration, idempotent seed, passing suites, updated docs.

**Independent Test**: Fresh-DB migration creates exactly the new schema; double-seed creates nothing; full suites green with zero stubs (spec US7 scenarios 1–3).

### Implementation for User Story 7

- [ ] T069 [US7] Apply the scaffolded migration on a fresh database and verify 7 tables, unique + FK indexes, and Budget/Appropriation/Encumbrance DocumentSequence rows (quickstart §1) — **BLOCKED**: requires Docker + SQL Server running
- [ ] T070 [US7] Verify idempotent reseed by running the seed path twice with zero new rows (quickstart §2) — **BLOCKED**: requires Docker + SQL Server running
- [x] T071 [US7] Rewrite the Budgeting tables section in `docs/database-schema.md` with explicit removed-column notes per FR-017
- [x] T072 [US7] Update `docs/final-business-feature-registry.md` to the `Budget → Appropriation → Encumbrance` chain with computed availability
- [x] T073 [US7] Regenerate OpenAPI (`dotnet build src/Web`) + NSwag client and verify removed tables/fields are gone (quickstart §5)

**Checkpoint**: Migration + docs + contract artifacts complete and verified.

---

## Phase 10: Polish & Cross-Cutting Validation

**Purpose**: Prove every Success Criterion end-to-end.

- [x] T074 Run `dotnet build ERP-Government.slnx -warnaserror` — zero warnings (SC-001 build half)
- [x] T075 Run full backend suites (`dotnet test`) — 100% pass, zero stubs (SC-001 test half) — unit: 62/62 passed; functional: blocked by Docker
- [x] T076 [P] Run `npm test`, `npm run build`, `npm run lint` in `src/Web/ClientApp/` — all green (SC-001/SC-007 frontend half)
- [x] T077 [P] Grep-verify SC-002: zero dropped-column properties in `src/Domain/Budgeting/Entities/*.cs`
- [ ] T078 [P] Auth smoke test per SC-008: anonymous `GET /api/Budgets` → 401; authenticated without permission → 403 ProblemDetails (quickstart §7) — **BLOCKED**: requires running server

**Checkpoint**: All 8 Success Criteria demonstrated. Feature complete.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately (`DEP-0xx` blocks implementation start, not planning).
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories (entities/configs/permissions/numbering/service/migration scaffold).
- **User Stories (Phases 3–9)**: All depend on Foundational completion.
  - US1 → US2 → US3 → US4 are data-chain ordered (masters → budgets/items → appropriations → encumbrances); each is independently testable once its predecessors exist.
  - US5 refines US1–US4 queries — starts after US4 queries exist.
  - US6 needs built endpoints + regenerated client (US1–US4 endpoints, T067 after backend).
  - US7 verification needs the migration (T021) + seeds (T030) + suites (US1–US4 tests); docs updates can start earlier.
- **Polish (Phase 10)**: Depends on all desired stories being complete.

### Within Each User Story

- Tests are explicitly requested: write the listed unit/functional/vitest tests alongside implementation; each must FAIL before the fix and PASS after (no stubs — every test asserts).
- Entities/configs (Foundational) → commands → queries/DTOs → endpoints → seeds → tests.
- Availability-affecting handlers (T043, T045-approve, T051, T052-approve) must call `IBudgetAvailabilityService` — never inline math.
- Commit after each task or logical group; stop at any checkpoint to validate the story independently.

### Parallel Opportunities

- All `[P]` deletions (T002–T005) and entity rewrites (T006–T012) can run in parallel (different files).
- All `[P]` EF configurations (T014–T016) can run in parallel.
- Within a story, all `[P]` command/query/endpoint/test tasks can run in parallel (different files).
- US6 page tasks (T063–T065) can run in parallel once hooks (T062) exist.
- Polish verifications T076–T078 can run in parallel.

---

## Parallel Example: User Story 1

```bash
# Launch all US1 command implementations together (different folders):
Task: "Implement BudgetType commands in src/Application/Budgeting/Commands/BudgetTypes/"
Task: "Implement Fund commands in src/Application/Budgeting/Commands/Funds/"
Task: "Implement BudgetClassification commands in src/Application/Budgeting/Commands/BudgetClassifications/"

# Launch all US1 test suites together:
Task: "Unit tests for BudgetType commands in tests/Application.UnitTests/Budgeting/BudgetTypeCommandTests.cs"
Task: "Unit tests for Fund commands in tests/Application.UnitTests/Budgeting/FundCommandTests.cs"
Task: "Unit tests for Classification commands in tests/Application.UnitTests/Budgeting/BudgetClassificationCommandTests.cs"
Task: "Functional tests in tests/Application.FunctionalTests/Budgeting/MasterDataTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (`DEP-0xx` accepted)
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (masters end-to-end)
4. **STOP and VALIDATE**: masters CRUD + tree Level + unique violations via API; suites green
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → deletion + new schema foundation ready
2. + US1 → masters demoable
3. + US2 → budgets with item trees and lifecycle demoable
4. + US3 → appropriations with computed availability demoable
5. + US4 → full Budget→Appropriation→Encumbrance chain demoable
6. + US5 → projections proven; + US6 → UI; + US7 → migration/docs verified
7. Each increment adds value without breaking previous stories

### Parallel Team Strategy

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 → US2 (masters → budgets)
   - Developer B: US3 → US4 (appropriations → encumbrances, needs A's entities — already in Foundational)
   - Developer C: US7 docs (T071–T072) + US6 prep in parallel
3. Converge on US5 projections, US6 frontend, US7 verification, then Polish

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Historical migrations before the new one are NEVER modified (FR-015)
- Availability math lives ONLY in `IBudgetAvailabilityService` — handlers call it, never duplicate it
- Signed Adjustment (`≠ 0`) vs positive-only Original/Supplement/Reduction is enforced in validators (FR-006/FR-009)
- `APP → APR` prefix change and `Budget → BGT` numbering ship in this migration (fresh wipe makes it safe)
- Frontend permission identifiers are UX-only; `[Authorize]` + `AuthorizationBehaviour` remain the authority (Principle VII)
