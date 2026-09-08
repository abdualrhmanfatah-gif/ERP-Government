# Implementation Plan: Budget Execution Report (RPT-01)

**Branch**: `043-budget-execution-report` | **Date**: 2026-09-08 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/043-budget-execution-report/spec.md`

## Summary

Budget execution report for decision makers: one line per budget item showing appropriated / encumbered / paid / available, with totals, drill-down detail (encumbrances + payments tabs), and Excel + PDF export matching the screen. The backend reporting slice (DTOs, queries, endpoints, exporters) already exists from spec 020 read-only reporting groundwork — this feature **completes and hardens it to the RPT-01 contract** (program/project filters + dimensions, display-side pagination, correct paid-status semantics, export fidelity) and **builds the frontend feature** (Arabic RTL list page + detail drill-down with two tabs).

## Technical Context

**Language/Version**: C# 13 / .NET 10 (backend); TypeScript / React 19 + Vite (frontend)

**Primary Dependencies**: EF Core + SQL Server, MediatR, FluentValidation, minimal APIs, NSwag-generated API client, TanStack Query, React Hook Form + Zod, Tailwind v3 + shadcn primitives, date-fns

**Storage**: SQL Server (read-only access — no schema changes, no migrations)

**Testing**: xUnit + NUnit + Shouldly (Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests). Frontend: NO test suite by governance decision (AGENTS.md Frontend Governance Override) — ESLint + dependency-cruiser only.

**Target Platform**: Web (server-rendered API + SPA), Arabic-only RTL UI

**Project Type**: Web application (minimal API backend + React SPA)

**Performance Goals**: Full fiscal year report loads < 3 seconds (SC-001)

**Constraints**: Strictly read-only (no mutations, no new transactional tables — FR-007); contract fields binding verbatim (BudgetExecutionReportDto family); usage ratio computed display-side only (FR-003); money decimal(23,2); totals over ALL filtered rows regardless of pagination (clarified 2026-09-08)

**Scale/Scope**: One report page + one detail drill-down; backend deltas in 1 query handler + 1 export mapper; pagination threshold 500 rows (client-side — see research.md D2)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Layered integrity | PASS | Query handler in Application reads via IApplicationDbContext; endpoint delegates to use case; no business rules in endpoint (export endpoint orchestrates only) |
| II. Bounded contexts | PASS | Cross-module reads via shared persistence abstraction only (Budgeting + Payments + FinancialSettings read); no writes to other modules; read-only feature |
| III. Server-side rules | PASS | All financial amounts server-computed in handler; frontend computes usage ratio only (FR-003 explicitly display-side, non-financial derived ratio) |
| IV. Financial integrity | PASS | Read-only; no postings; per-line invariant appropriated = encumbered + paid + available asserted by existing reconciliation test (SC-004) |
| V. Budget control | PASS | Report only; no expenditure; availability math mirrors BudgetAvailabilityService status conventions (see research.md D3) |
| VI. Data integrity | PASS | No schema changes; no migrations; decimal precision unchanged |
| VII. Authorization | PASS | View + export permissions separate (RC-6): endpoint `.RequireAuthorization(ReportingViewBudgetExecution / ReportingExportReports)` + query-level `[Authorize]`. NOTE: policies remain open placeholders (registered exception 1, DEP-020) — use-case-layer `[Authorize]` attribute is the effective control |
| VIII. Approval/audit immutability | PASS | No approval flows in a read-only report |
| IX. API contract integrity | PASS | Binding DTO contract from spec matches existing DTOs; endpoint changes (program/project filter application) are additive — OpenAPI regenerated via `npm run generate-api` after implementation |
| X. UI/design system | PASS | Arabic-only RTL; logical properties; shadcn primitives; money via shared money-display; design tokens from tokens.ts |
| XI. Testing & TDD | PASS (backend) / **EXCEPTION (frontend)** | Backend: TDD mandatory, red→green per cycle, full 5-project suite at gates. Frontend: no automated tests per AGENTS.md Frontend Governance Override — logged as tracked deviation pending constitution amendment (see Complexity Tracking) |
| XII. Controlled change | PASS | No layer/module/boundary changes; frontend `features/reporting/` is a new feature folder within existing structure |

**Post-design re-check**: PASS. No new violations introduced by Phase 1 artifacts.

## Project Structure

### Documentation (this feature)

```text
specs/043-budget-execution-report/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (endpoint + export contracts)
│   └── budget-execution-api.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
src/
├── Application/Reporting/BudgetExecution/
│   ├── GetBudgetExecutionReport/        # EXISTING — deltas: apply ProgramId/ProjectId filters, populate ProgramCode/ProjectCode, status semantics
│   └── GetBudgetExecutionDetail/        # EXISTING — verify vs contract, tabs data
├── Application/Reporting/Common/
│   └── ReportResultMapper.cs            # EXISTING — delta: 4-column budget-execution mapping, totals, Arabic titles, partial-data marker
├── Web/Endpoints/Reporting/
│   └── BudgetExecutionReports.cs        # EXISTING — deltas: partial-data warning flag, export filename/headers
├── Infrastructure/Services/
│   ├── ExcelReportExporter.cs           # EXISTING — delta: RTL + numeric column alignment
│   └── PdfReportExporter.cs             # EXISTING — delta: RTL + numeric column alignment
└── Web/ClientApp/src/
    ├── features/reporting/              # NEW feature folder (domain: reporting)
    │   └── budget-execution-report/
    │       ├── pages/                   # BudgetExecutionReportPage, (detail as drawer/route per tasks)
    │       ├── hooks/                   # useBudgetExecutionReport, useBudgetExecutionDetail
    │       └── shared/                  # client.ts, types.ts, schemas.ts
    ├── components/                      # feature-scoped: ReportingBudgetExecutionFilters.tsx, ReportingBudgetExecutionDetail.tsx
    └── shared/api/                      # query keys + result-to-ui reuse

tests/
├── Application.FunctionalTests/Reporting/BudgetExecutionReportTests.cs   # EXTEND (T1, T2, filters, program/project)
├── Infrastructure.IntegrationTests/                                      # EXTEND — exporters (RTL, totals, columns)
└── Web.AcceptanceTests/Features/                                         # EXTEND — T3 detail tabs, T5 empty state journeys
```

**Structure Decision**: Extend the existing Reporting slice (spec 020 groundwork) rather than creating parallel structures. Frontend gets a NEW `features/reporting/` domain folder — reporting is cross-cutting (budget + treasury + payments sources) and must not nest under `features/budgeting/`. Follows AGENTS.md layer map (`features/<domain>/<entity>/pages|hooks|shared`). Feature-scoped components live in `src/components/` with `Reporting` prefix — no `components/` inside `features/`.

## Complexity Tracking

> Filled only for justified deviations.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Constitution XI vs frontend governance | Constitution XI requires frontend tests; AGENTS.md Frontend Governance Override prohibits them (no Vitest/RTL/Playwright/axe) | The override is an explicit AGENTS.md convention pending constitution amendment; adding a frontend test runner mid-feature would violate AGENTS.md "Don't" list and dependency-cruiser CI setup. Tracked: frontend verification is manual review (ARIA, keyboard, focus-visible) + `npm run lint` + `npm run build` gates |
