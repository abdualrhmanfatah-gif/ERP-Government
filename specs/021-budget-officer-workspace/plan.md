# Implementation Plan: Budget Officer Workspace — Full Budget Lifecycle UI

**Branch**: `021-budget-officer-workspace` | **Date**: 2026-09-06 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/021-budget-officer-workspace/spec.md`

## Summary

Build the transactional workspace screens for budget officers: budget CRUD with item tree, appropriation management (including transfers), encumbrance recording with live availability indicator, informational monthly plan editor, and budget execution drill-down. UI-only — no backend changes. All backend endpoints already exist.

## Technical Context

**Language/Version**: TypeScript 5.x, React 19, Vite

**Primary Dependencies**: TanStack Query (React Query), React Router, NSwag-generated clients (`web-api-client.ts`), Vitest + @testing-library/react

**Storage**: LocalStorage for monthly plans (client-side only); all other data via REST API

**Testing**: Vitest (unit/component), follow `BudgetItemTree.test.tsx` pattern

**Target Platform**: Web desktop (Arabic-first RTL)

**Project Type**: Web application (frontend module within existing ERP)

**Performance Goals**: Availability indicator updates within 1 second; drill-down loads within 2 seconds

**Constraints**: No backend changes; must use existing API clients; Arabic RTL everywhere; design tokens from DESIGN.md are SSOT

**Scale/Scope**: ~12 new pages/components, 5 user stories, 16 functional requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | Frontend only — no layer violations. UI is a delivery adapter consuming HTTP contract. |
| II. Bounded Contexts | ✅ PASS | Budgeting module owns its UI. No cross-module UI imports expected. |
| III. Server-Side Rules | ✅ PASS | All business rules enforced server-side. Frontend displays availability indicator as read-only feedback; no enforcement in UI code. |
| IV. Financial Integrity | ✅ PASS | No financial calculations in UI — all amounts come from API. Monthly plan is informational only. |
| V. Budget Control | ✅ PASS | Availability indicator reads from server-computed values. No client-side budget control logic. |
| VI. Data Integrity | ✅ PASS | RowVersion concurrency tokens round-tripped via forms. No schema changes. |
| VII. Authorization | ✅ PASS | All actions gated via `usePermission` + `BUDGET_PERMISSIONS` codes. Policies are placeholder but still declared. |
| VIII. Approval Workflows | ✅ PASS | `ApprovalHistoryPanel` renders unconditionally. Lifecycle transitions delegated to server. |
| IX. API Contract | ✅ PASS | Uses NSwag-generated clients. No hand-written fetch for new endpoints. |
| X. UI/Design System | ✅ PASS | Design tokens from DESIGN.md. Arabic-first RTL. Logical properties. Shared UI components. |
| XI. Testing | ✅ PASS | Vitest tests required per user story. TDD mandatory. |
| XII. Controlled Change | ✅ PASS | No architectural changes — UI-only feature. |

**Gate result**: PASS — no violations.

## Project Structure

### Documentation (this feature)

```text
specs/021-budget-officer-workspace/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-contracts.md
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── app/
│   └── routes.tsx                          # ADD: budget list + detail + create + edit routes
├── layouts/
│   └── navigation.ts                       # ADD: "الموازنة" group → Budgets nav item
├── shared/
│   └── constants/
│       └── permissions.ts                  # EXISTS: BUDGET_PERMISSIONS (no changes needed)
├── features/budgeting/
│   ├── budgets/                            # NEW folder
│   │   ├── pages/
│   │   │   ├── BudgetsListPage.tsx         # US1: list with filters
│   │   │   ├── BudgetDetailPage.tsx        # US1+US5: detail + lifecycle + drill-down
│   │   │   └── BudgetCreatePage.tsx        # US1: create form
│   │   └── index.ts
│   ├── budgets-items/                      # NEW folder
│   │   ├── pages/
│   │   │   └── BudgetItemEditPage.tsx      # US1: item edit form
│   │   └── index.ts
│   ├── appropriations/                     # NEW folder
│   │   ├── pages/
│   │   │   ├── AppropriationsListPage.tsx  # US2: list by item
│   │   │   └── AppropriationCreatePage.tsx # US2: create/edit form
│   │   └── index.ts
│   ├── encumbrances/                       # NEW folder
│   │   ├── pages/
│   │   │   ├── EncumbrancesListPage.tsx    # US3: list by appropriation
│   │   │   └── EncumbranceCreatePage.tsx   # US3: create form with live indicator
│   │   └── index.ts
│   ├── monthly-plan/                       # NEW folder
│   │   ├── components/
│   │   │   └── MonthlyPlanEditor.tsx       # US4: 12-column editor
│   │   └── index.ts
│   ├── execution/                          # NEW folder
│   │   ├── components/
│   │   │   └── ExecutionDrillDown.tsx      # US5: expandable drill-down
│   │   └── index.ts
│   ├── components/                         # EXISTS: reuse
│   │   ├── AvailabilityIndicator.tsx       # US3: existing, reuse as-is
│   │   ├── BudgetItemTree.tsx              # US1: existing, reuse as-is
│   │   ├── LifecycleActions.tsx            # US1/US2/US3: existing, reuse as-is
│   │   └── ApprovalHistoryPanel.tsx        # US1: existing, reuse as-is
│   ├── hooks/                              # EXISTS: extend
│   │   ├── useBudgets.ts                   # EXISTS: add useBudgetItemsTree, useCreateBudgetItem
│   │   ├── useBudgetItems.ts               # NEW: item-specific hooks
│   │   ├── useAppropriations.ts            # EXISTS: reuse as-is
│   │   ├── useEncumbrances.ts              # EXISTS: reuse as-is
│   │   └── useMonthlyPlan.ts               # NEW: localStorage hook for monthly plans
│   └── shared/                             # EXISTS: reuse
│       ├── client.ts                       # EXISTS: extend with budget items + monthly plan keys
│       └── types.ts                        # EXISTS: reuse as-is
└── __tests__/                              # NEW tests per story
    └── budgeting/
        ├── BudgetsListPage.test.tsx
        ├── BudgetDetailPage.test.tsx
        ├── BudgetCreatePage.test.tsx
        ├── AppropriationCreatePage.test.tsx
        ├── EncumbranceCreatePage.test.tsx
        ├── MonthlyPlanEditor.test.tsx
        └── ExecutionDrillDown.test.tsx
```

**Structure Decision**: Follows existing `features/budgeting/` convention. New screens organized by entity in subfolders (`budgets/`, `appropriations/`, `encumbrances/`, `monthly-plan/`, `execution/`). Shared components stay in `components/`. Hooks stay in `hooks/`. No new UI primitives — use existing `components/ui/*`.

## Complexity Tracking

No violations — all constitution principles pass.

## Shared FE Conventions (Binding)

- **Exemplar**: `src/Web/ClientApp/src/features/budgeting/` — feature/<domain>/<entity>/{pages,hooks,index} + shared/ + components/ + __tests__/
- **API**: NSwag classes from `web-api-client.ts` (already generated — never regenerate). Wrap per feature in `shared/client.ts`.
- **UI kit**: `components/ui/*` (Button, Badge, AlertDialog, ApprovalTimeline, AuditTimeline, calendar...) + ButtonBar. No new primitives.
- **Shared hooks**: `useAuth`, `usePermission`, `usePagination`, `useDebounce`; formatters; financial validators; patch-fetch for PATCH.
- **Routes**: Register in `app/routes.tsx` (Arabic label + `protected: true`) + nav group in `layouts/navigation.ts`.
- **Arabic RTL**: Read DESIGN.md + design-system tokens BEFORE styling; `tokens.ts` is SSOT.
- **Permission gates**: `usePermission` + codes in `shared/constants/permissions.ts`.
- **Lifecycle actions**: Use existing `LifecycleActions` component; approval history via `ApprovalHistoryPanel` (unconditional).
