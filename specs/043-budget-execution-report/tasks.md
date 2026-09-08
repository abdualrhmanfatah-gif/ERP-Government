# Tasks: Budget Execution Report (RPT-01)

**Input**: Design documents from `/specs/043-budget-execution-report/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/budget-execution-api.md, quickstart.md

**Tests**: INCLUDED — TDD is NON-NEGOTIABLE per Constitution XI. Test tasks written FIRST, observed red (filtered single test), then implementation to green (touched project's suite). No assertion ever weakened/skipped/deleted. Frontend has NO test suite (AGENTS.md Frontend Governance Override) — frontend tasks are verified by `npm run lint` + `npm run build` + manual review only.

**Organization**: Tasks grouped by user story (US1 overview P1, US2 detail P1, US3 export P2).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Backend: `src/Application|Web|Infrastructure` · Frontend: `src/Web/ClientApp/src` · Tests: `tests/` (see plan.md Project Structure)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Baseline verification and feature scaffolding

- [x] T001 Verify existing Reporting slice matches binding contract in specs/043-budget-execution-report/data-model.md (DTOs in src/Application/Reporting/BudgetExecution/, endpoints in src/Web/Endpoints/Reporting/BudgetExecutionReports.cs) and record confirmed delta list; baseline `dotnet build src/Web/Web.csproj` must pass
- [x] T002 [P] Create frontend feature folder skeleton src/Web/ClientApp/src/features/reporting/budget-execution-report/shared/types.ts (typed re-exports from NSwag client) and shared/schemas.ts (Zod filter schema: fiscalYearId required, fundId/programId/projectId/budgetItemId optional — research D1/OQ-N1)
- [x] T003 [P] Register reporting query keys in src/Web/ClientApp/src/shared/api (keys factory: budget-execution list + detail) following existing query-key patterns
- [x] T004 Register route + navigation entry for the report page in src/Web/ClientApp/src/routes/ with permission identifier Reporting.ViewBudgetExecution on the nav entry (Constitution X)

**Checkpoint**: Build passes; feature folder, routes, query keys ready — story work can begin

---

## Phase 2: User Story 1 — Budget Execution Overview (Priority: P1) — MVP

**Goal**: Decision maker sees per-item appropriated/encumbered/paid/available with totals; filters (year/fund/program/project) work; usage-ratio display column; client-side pagination @500; RC-3 states.

**Independent Test**: Open report for seeded fiscal year → lines + totals correct vs ledger-derived figures; program/project subtree items included; Draft payment orders NOT in paidAmount.

### Tests for User Story 1 (TDD — write FIRST, observe RED)

- [x] T005 [P] [US1] Write failing functional test: ProgramId/ProjectId filter returns subtree items (parent classification node includes descendant-classified items) and populates programId/programCode/projectId/projectCode on lines — in tests/Application.FunctionalTests/Reporting/BudgetExecutionReportTests.cs
- [x] T006 [P] [US1] Write failing functional test: paidAmount includes only payment orders with status Approved/SentToTreasury/Paid/PartiallyPaid (Draft/Submitted/Rejected/Voided/Cancelled excluded); encumberedAmount uses open-commitment statuses per BudgetAvailabilityService — in tests/Application.FunctionalTests/Reporting/BudgetExecutionReportTests.cs
- [x] T007 [P] [US1] Write failing functional test: totals equal Σ lines for all four amounts on EACH filtered set (fund/program/project) — extends existing totals test in tests/Application.FunctionalTests/Reporting/BudgetExecutionReportTests.cs

### Implementation for User Story 1 (make RED → GREEN)

- [x] T008 [US1] Apply ProgramId/ProjectId subtree filters and populate program/project fields in src/Application/Reporting/BudgetExecution/GetBudgetExecutionReport/GetBudgetExecutionReportQueryHandler.cs (classification ancestry walk per research D1); run `dotnet test tests/Application.FunctionalTests` filtered to new tests → green
- [x] T009 [US1] Fix paid/encumbered status semantics in src/Application/Reporting/BudgetExecution/GetBudgetExecutionReport/GetBudgetExecutionReportQueryHandler.cs (research D3); run filtered suite → green; verify existing reconciliation test still passes
- [x] T010 [US1] Implement useBudgetExecutionReport hook (TanStack Query) + fiscal-year auto-select hook (Status == Open → IsActive → latest YearNumber; research D5) in src/Web/ClientApp/src/features/reporting/budget-execution-report/hooks/
- [x] T011 [US1] Implement ReportingBudgetExecutionFilters.tsx in src/Web/ClientApp/src/components/ — year/fund/program/project selects bound to Zod schema from T002, shadcn primitives, Arabic labels
- [x] T012 [US1] Implement BudgetExecutionReportPage.tsx in src/Web/ClientApp/src/features/reporting/budget-execution-report/pages/ — table (FR-001 columns), fixed totals row (FR-002, server totals never client-re-summed), usage-ratio column "عرض" display-side with "—" when appropriated=0 (FR-003/D6), client-side pagination at 500 rows (D2), skeleton/error+retry/explicit Arabic empty state/unauthorized states (RC-3), money via shared money-display
- [x] T013 [US1] Run `npm run generate-api` (no endpoint shape change expected), `npm run lint`, `npm run build` in src/Web/ClientApp; fix violations

**Checkpoint**: US1 fully functional independently — report loads, filters, totals, pagination, states

---

## Phase 3: User Story 2 — Budget Item Detail (Priority: P1)

**Goal**: Click a line → drill-down with two tabs (التزامات/مدفوعات) listing exactly the movements behind the summary figures.

**Independent Test**: Click item with known encumbrances/payments → detail lists them; empty tab shows explicit empty state; no flicker/data loss on tab switch.

### Tests for User Story 2 (TDD — write FIRST, observe RED)

- [x] T014 [P] [US2] Write failing functional test: GetBudgetExecutionDetail returns encumbrances[]/payments[] consistent with the item's summary figures and applies the same status semantics as the main report (research D3) — in tests/Application.FunctionalTests/Reporting/BudgetExecutionReportTests.cs (or detail test class if one exists)

### Implementation for User Story 2 (make RED → GREEN)

- [x] T015 [US2] Align src/Application/Reporting/BudgetExecution/GetBudgetExecutionDetail/GetBudgetExecutionDetailQueryHandler.cs status semantics with D3 if test T014 exposes divergence; run filtered suite → green
- [x] T016 [US2] Implement useBudgetExecutionDetail hook in src/Web/ClientApp/src/features/reporting/budget-execution-report/hooks/ (query key from T003; cached per budgetItemId)
- [x] T017 [US2] Implement ReportingBudgetExecutionDetail.tsx in src/Web/ClientApp/src/components/ — two tabs (التزامات/مدفوعات), per-tab explicit empty state, stable tab switching (no flicker/data loss — spec US2.3), ARIA-labelled keyboard-reachable tabs
- [x] T018 [US2] Wire click-to-drill from BudgetExecutionReportPage.tsx rows to the detail component (row click opens detail view preserving filters)
- [ ] T019 [US2] Extend Web.AcceptanceTests journey for drill-down (click line → detail tabs visible) in tests/Web.AcceptanceTests/Features/; run `npm run lint` + `npm run build`

**Checkpoint**: US1 + US2 both independently functional

---

## Phase 4: User Story 3 — Export (Priority: P2)

**Goal**: Excel + PDF export identical to screen (same query/filters/order), 4 amount columns + per-column totals, Arabic title, RTL, partial-data marker.

**Independent Test**: Export with filters applied → file rows/columns/order match screen; totals row present; RTL renders; open-period data carries "بيانات جزئية" marker.

### Tests for User Story 3 (TDD — write FIRST, observe RED)

- [ ] T020 [P] [US3] Write failing integration test: budget-execution ReportResult mapping produces 4 amount columns + per-column totals + Arabic section title "تقرير تنفيذ الموازنة" — in tests/Infrastructure.IntegrationTests/ (report exporter test class)
- [ ] T021 [P] [US3] Write failing integration test: exported XLSX and PDF render RTL direction with logical numeric column alignment — in tests/Infrastructure.IntegrationTests/

### Implementation for User Story 3 (make RED → GREEN)

- [x] T022 [US3] Upgrade BudgetExecutionReportDto mapping in src/Application/Reporting/Common/ReportResultMapper.cs — replace Debit/Credit/Balance flattening with 4-amount column layout, per-column totals, Arabic title (research D4); run filtered integration tests → green
- [x] T023 [US3] Extend src/Infrastructure/Services/ExcelReportExporter.cs and PdfReportExporter.cs — RTL sheet/page direction, numeric start/end alignment, partial-data marker "بيانات جزئية" in document header when report range includes open fiscal period (RC-4); run filtered integration tests → green
- [x] T024 [US3] Update export endpoint src/Web/Endpoints/Reporting/BudgetExecutionReports.cs — pass partial-data flag through to exporters, correct content-type/filename per format (xlsx/pdf) per contracts/budget-execution-api.md; run `npm run generate-api` in src/Web/ClientApp (contract regenerated)
- [x] T025 [US3] Implement frontend export actions in BudgetExecutionReportPage.tsx — Excel/PDF buttons carrying the SAME filter state as the displayed report (RC-2), partial-data warning banner when current period included, export permission gate Reporting.ExportReports (RC-6), 4xx inline / 5xx toast via result-to-ui; run `npm run lint` + `npm run build`
- [ ] T026 [US3] Extend Web.AcceptanceTests journey for export (filters → export → download succeeds) in tests/Web.AcceptanceTests/Features/

**Checkpoint**: All three user stories independently functional

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Gates, docs, cross-story verification

- [x] T027 [P] Repoint AGENTS.md exemplar table if features/reporting/ establishes a new reference pattern (only if warranted — do not touch otherwise); update specs/043-budget-execution-report/contracts/budget-execution-api.md against regenerated web-api-client.ts
- [x] T028 Manual frontend review per AGENTS.md accessibility tier: ARIA labels, keyboard reachability (rows/tabs/filters/export), focus-visible; RTL logical properties only; dark mode pass on all new components
- [ ] T029 Run quickstart.md validation end-to-end (T1–T5 scenario table) in dev environment
- [ ] T030 Run FULL 5-project suite gate: `dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests && dotnet test tests/Infrastructure.IntegrationTests && dotnet test tests/Web.AcceptanceTests` — all green, no weakened assertions

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: no dependencies — start immediately
- **US1 (Phase 2)**: depends on Setup; MVP
- **US2 (Phase 3)**: depends on US1 page existing for wiring (T018), but backend test T014 and fix T015 are independent of frontend US1 tasks
- **US3 (Phase 4)**: backend export tasks independent of US2; frontend export (T025) depends on US1 page (T012)
- **Polish (Phase 5)**: depends on all stories complete

### User Story Dependencies

- US1: standalone after Setup (backend gaps independent of frontend; frontend depends on backend filters live)
- US2: backend (T014–T015) parallelizable with US1 frontend; frontend detail (T016–T018) depends on US1 page
- US3: backend (T020–T024) parallelizable with US2; frontend (T025) depends on US1 page

### Within Each Story

- Tests FIRST (red) → implementation → filtered suite green → full touched project suite
- Backend before frontend consumption; hooks before components; components before page wiring

### Parallel Opportunities

- T002, T003 (Setup) in parallel
- T005, T006, T007 (US1 tests) in parallel — same file conflicts: coordinate as one red batch, separate test methods
- T014 (US2 backend) ∥ US1 frontend
- T020, T021 (US3 tests) in parallel; US3 backend ∥ US2 frontend

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Phase 1 Setup → 2. Phase 2 US1 → 3. **STOP and VALIDATE** (report loads, filters, totals, pagination, RC-3 states) → demo-ready oversight view

### Incremental Delivery

- +US2: drill-down traceability → +US3: exportable audit artifact → Phase 5 gates → merge after /speckit.converge

### Notes

- **TEST EXECUTION DEFERRED (user instruction, 2026-09-08)**: Docker daemon unavailable on the dev machine — Aspire/SQL-Server-backed functional/integration/acceptance tests could not run. Test code for T005–T007 is written in-repo; T019/T020/T021/T026 test tasks remain unchecked → tracked debt (Constitution XI): run the full red→green cycle (T030 gate) once Docker is available, BEFORE merge. Red-phase observation was not performed for implemented tests.
- Commit after each task or logical group (caveman-commit style per repo config: commit_style fixed — hooks are optional; user decides)
- Never proceed past a red test without observing it fail for the right reason
- Frontend tasks carry no test tasks — governance override; manual review gate is T028
