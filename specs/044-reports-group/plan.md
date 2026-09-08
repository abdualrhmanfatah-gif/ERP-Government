# Implementation Plan: Reports Group (RPT-01..06)

**Branch**: `044-reports-group` | **Date**: 2026-09-08 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/044-reports-group/spec.md`

## Summary

6 government ERP reports (budget execution, revenue collections, disbursement register, availability snapshot, trial balance, financial statements) with shared requirements (RC-1..RC-7). **Backend is fully implemented** — all 5 Reporting endpoints, legacy Reports.cs, all DTOs, handlers, and permissions exist. **Only RPT-01 has a frontend**. This plan covers frontend implementation for RPT-02 through RPT-06, following the established RPT-01 pattern.

## Technical Context

**Language/Version**: TypeScript 5.x, React 19, Vite

**Primary Dependencies**: TanStack Query, React Router v7, Zustand, React Hook Form + Zod, Tailwind v3 + shadcn, @tanstack/react-table, date-fns, sonner

**Storage**: N/A (read-only reports — no new entities)

**Testing**: No frontend test suite (governance decision — see AGENTS.md Frontend Architecture)

**Target Platform**: Web (SPA), Arabic RTL, dark mode

**Project Type**: Web application (frontend only — backend complete)

**Performance Goals**: P1 reports <3s load, RPT-02 <2s filters, GL pages independent

**Constraints**: Arabic-only hardcoded strings, RTL logical CSS, no mock data, no cross-feature imports, shadcn primitives in `src/components/`

**Scale/Scope**: 5 new report pages + 5 filter components + 5 detail components + route/nav updates

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Layered Architecture | ✅ PASS | Backend complete; frontend is delivery adapter only |
| II. Bounded Contexts | ✅ PASS | Reports are read-only queries across modules |
| III. Server-Side Business Rules | ✅ PASS | All financial amounts server-computed (RC-1); frontend displays only |
| IV. Financial Integrity | ✅ PASS | No mutations; amounts from DTOs |
| V. Budget Control | ✅ PASS | Reports are read-only; available = appropriated - encumbered - paid (server) |
| VI. Data Integrity | ✅ PASS | No schema changes; read-only |
| VII. Authorization | ✅ PASS | All endpoints have permissions; frontend checks are UX-only |
| VIII. Approval Workflows | ✅ PASS | No approvals in read-only reports |
| IX. API Contract Integrity | ✅ PASS | NSwag-generated clients; OpenAPI is SSOT |
| X. UI and Design System | ✅ PASS | Tokens from design system; RTL; shadcn primitives |
| XI. Testing | ⚠️ NOTE | Frontend has no test suite (governance decision); backend tests remain |
| XII. Controlled Change | ✅ PASS | No architectural changes |

**No violations. All gates pass.**

## Project Structure

### Documentation (this feature)

```text
specs/044-reports-group/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (DTOs already exist — reference only)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API contracts already exist — reference only)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── features/reporting/
│   ├── budget-execution-report/          # EXISTS — RPT-01 (reference exemplar)
│   │   ├── pages/BudgetExecutionReportPage.tsx
│   │   ├── hooks/useBudgetExecutionReport.ts
│   │   └── shared/ (types.ts, schemas.ts, client.ts)
│   ├── revenue-collections-report/       # NEW — RPT-02
│   │   ├── pages/RevenueCollectionsReportPage.tsx
│   │   ├── hooks/useRevenueCollectionsReport.ts
│   │   └── shared/ (types.ts, schemas.ts)
│   ├── disbursement-register-report/    # NEW — RPT-03
│   │   ├── pages/DisbursementRegisterReportPage.tsx
│   │   ├── hooks/useDisbursementRegisterReport.ts
│   │   └── shared/ (types.ts, schemas.ts)
│   ├── availability-snapshot-report/    # NEW — RPT-04
│   │   ├── pages/AvailabilitySnapshotReportPage.tsx
│   │   ├── hooks/useAvailabilitySnapshotReport.ts
│   │   └── shared/ (types.ts, schemas.ts)
│   ├── trial-balance-report/            # NEW — RPT-05
│   │   ├── pages/TrialBalanceReportPage.tsx
│   │   ├── hooks/useTrialBalanceReport.ts
│   │   └── shared/ (types.ts, schemas.ts)
│   └── financial-statements/            # NEW — RPT-06
│       ├── pages/ (BalanceSheetPage, IncomeStatementPage, GeneralLedgerPage, CashFlowPage, TrialBalanceLegacyPage)
│       ├── hooks/ (useBalanceSheet, useIncomeStatement, useGeneralLedger, useCashFlow, useTrialBalanceLegacy)
│       └── shared/ (types.ts, schemas.ts)
├── components/
│   ├── ReportingBudgetExecutionFilters.tsx     # EXISTS
│   ├── ReportingBudgetExecutionDetail.tsx      # EXISTS
│   ├── ReportingRevenueCollectionsFilters.tsx  # NEW
│   ├── ReportingRevenueCollectionsDetail.tsx   # NEW
│   ├── ReportingDisbursementRegisterFilters.tsx # NEW
│   ├── ReportingDisbursementRegisterDetail.tsx  # NEW
│   ├── ReportingAvailabilitySnapshotFilters.tsx # NEW
│   ├── ReportingAvailabilitySnapshotDetail.tsx  # NEW
│   ├── ReportingTrialBalanceFilters.tsx         # NEW
│   ├── ReportingTrialBalanceDetail.tsx          # NEW
│   ├── ReportingFinancialStatementsFilters.tsx  # NEW
│   └── ReportingFinancialStatementsDetail.tsx   # NEW
├── shared/api/query-keys.ts               # EXTEND — add keys for RPT-02..06
├── app/routes.tsx                          # EXTEND — add routes for RPT-02..06
└── layouts/navigation.ts                   # EXTEND — add nav items for RPT-02..06
```

**Structure Decision**: Follow the established RPT-01 pattern. Each report gets its own feature subfolder under `features/reporting/`. Filter and detail components are feature-scoped with domain prefix in `src/components/`. No new shared infrastructure needed — `ReportFilterDto`, `ReportResultMapper`, and `IReportAuditLogger` already exist.

## Complexity Tracking

No Constitution violations. No complexity tracking needed.

## Phase 0: Research

**No research needed.** All technical unknowns are resolved:
- Backend is fully implemented (all endpoints, handlers, DTOs, permissions)
- Frontend pattern is established (RPT-01 exemplar)
- NSwag-generated clients exist for all 6 reports
- No new entities, no schema changes, no new API contracts

**Decision**: Skip Phase 0 research. Proceed directly to Phase 1 design.

## Phase 1: Design & Contracts

### Data Model

No new entities. All DTOs already exist in the backend. The frontend consumes them via NSwag-generated clients. Reference existing DTOs:

| Report | DTO Location |
|---|---|
| RPT-01 Budget Execution | `src/Application/Reporting/BudgetExecution/` |
| RPT-02 Revenue Collections | `src/Application/Reporting/RevenueCollections/` |
| RPT-03 Disbursement Register | `src/Application/Reporting/DisbursementRegister/` |
| RPT-04 Availability Snapshot | `src/Application/Reporting/AvailabilitySnapshot/` |
| RPT-05 Trial Balance | `src/Application/Reporting/TrialBalance/` |
| RPT-06 Financial Statements | `src/Application/Accounting/Reports/` (legacy) |

### Interface Contracts

No new API contracts. All endpoints exist and are published via OpenAPI. Frontend uses NSwag-generated clients.

### Quickstart Validation

See `quickstart.md` for end-to-end validation scenarios.
