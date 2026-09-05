# Implementation Plan: Budget Classifications Tree #2 of 5

**Branch**: `012-budget-classifications-tree` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/012-budget-classifications-tree/spec.md`

## Summary

Build a hierarchical tree view page for Budget Classifications with expand/collapse, level badges, search (auto-expand ancestors), create/edit dialog with parent tree-select and cycle prevention, toggle active state, and route + sidebar activation. Consumes shared types/client/query keys from spec #1.

## Technical Context

**Language/Version**: TypeScript 5.9 (strict mode, ES2020 target, bundler module resolution)

**Primary Dependencies**: React 19.1, TanStack Query 5.102, react-router-dom 7.6, Tailwind CSS 3.4 (class-based dark mode), @base-ui/react 1.7, sonner 2.0 (toasts), zod 3.25

**Storage**: None (frontend consumes REST API only)

**Testing**: Vitest 4.1, @testing-library/react 16.3, jsdom 29.1

**Target Platform**: Web SPA (Vite 8.0 dev server with /api proxy to backend)

**Project Type**: Web application (React frontend, separate .NET backend)

**Performance Goals**: Tree renders 500+ nodes without jank; search filter response under 50ms; mutation feedback under 300ms

**Constraints**: Arabic-first RTL; design tokens only (zero hardcoded colors/fonts); dark mode mandatory; permission gating UI-only (server is authority per Constitution Principle VII)

**Scale/Scope**: ~3 new files in classifications/, route entry update, sidebar group update

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | PASS | Frontend consumes HTTP contract only; no backend references |
| II. Bounded Contexts | PASS | Budgeting feature self-contained under features/budgeting/ |
| III. Server-Side Business Rules | PASS | Permission checks are UX-only; server is sole authority |
| IV. Financial Integrity | N/A | No financial calculations in this spec |
| V. Budget Control | N/A | Classifications are reference data; no budget logic |
| VI. Data Integrity | PASS | RowVersion round-tripped on all mutations |
| VII. Authorization | PASS | Permission gating on every page/action (FR-002.6, FR-003.1, FR-004.2) |
| VIII. Approval Workflows | N/A | No approval flows in this spec |
| IX. API Contract Integrity | PASS | Shared client from spec #1 mirrors backend contract |
| X. UI and Design System | PASS | Design tokens only; RTL Arabic-first; shared UI components |
| XI. Testing | PASS | Vitest + Testing Library available |
| XII. Controlled Change | PASS | Builds on spec #1 shared infra; clean additions only |

**No violations detected.**

## Project Structure

### Documentation (this feature)

```text
specs/012-budget-classifications-tree/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── features/budgeting/
│   ├── shared/
│   │   ├── types.ts              # Existing (spec #1) — BudgetClassificationDto, BudgetClassificationTreeDto
│   │   ├── client.ts             # Existing (spec #1) — budgetClassificationsClient
│   │   └── index.ts              # Existing (spec #1)
│   └── classifications/
│       ├── hooks/
│       │   └── useClassifications.ts    # NEW — query/mutation hooks
│       └── pages/
│           └── ClassificationsListPage.tsx  # NEW — tree view page
├── app/routes.tsx                # UPDATE — add route
└── layouts/navigation.ts         # UPDATE — add sidebar link
```

**Structure Decision**: Single project (frontend-only feature). New files under `features/budgeting/classifications/`. Routes and sidebar updated in existing files.
