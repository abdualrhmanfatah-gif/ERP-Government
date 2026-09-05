# Implementation Plan: Budgeting Frontend Foundation #1 of 5

**Branch**: `011-budgeting-frontend-foundation` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/011-budgeting-frontend-foundation/spec.md`

## Summary

Build the shared frontend infrastructure for the budgeting module (types, client, AvailabilityIndicator) and the first two reference-data CRUD pages (BudgetTypes, Funds). The shared layer defines all DTOs, enums, and Arabic label maps for the 7 budgeting entities, a typed API client with cache key factories, and a reusable availability indicator component. BudgetTypes and Funds get full list/create/edit/toggle/detail flows using dialog-based edits, client-side filtering, and permission gating. Routes and sidebar "الموازة" group are registered with progressive activation for specs #2-#5.

## Technical Context

**Language/Version**: TypeScript 5.9 (strict mode, ES2020 target, bundler module resolution)

**Primary Dependencies**: React 19.1, TanStack Query 5.102, TanStack Table 9.2, react-router-dom 7.6, Tailwind CSS 3.4 (class-based dark mode), @base-ui/react 1.7, sonner 2.0 (toasts), zod 3.25

**Storage**: None (frontend consumes REST API only)

**Testing**: Vitest 4.1, @testing-library/react 16.3, jsdom 29.1

**Target Platform**: Web SPA (Vite 8.0 dev server with /api proxy to backend)

**Project Type**: Web application (React frontend, separate .NET backend)

**Performance Goals**: Reference data lists under 500ms load; client-side filter response under 50ms; mutation feedback under 300ms

**Constraints**: Arabic-first RTL; design tokens only (zero hardcoded colors/fonts); dark mode mandatory; permission gating UI-only (server is authority per Constitution Principle VII)

**Scale/Scope**: ~5 new files in shared/, ~8 new files in budget-types/, ~10 new files in funds/, 3 route entries, 1 sidebar group update

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | PASS | Frontend is a separate application consuming HTTP contract only; no backend references |
| II. Bounded Contexts | PASS | Budgeting frontend is self-contained under features/budgeting/; shared types are the contract boundary |
| III. Server-Side Business Rules | PASS | Frontend permission checks are UX-only (FR-011); server is sole authority |
| IV. Financial Integrity | N/A | No financial calculations in this spec; availability is fetched from backend |
| V. Budget Control | N/A | Availability indicator reads backend-computed values; no client-side budget logic |
| VI. Data Integrity | PASS | RowVersion round-tripped on all mutations (assumption); no local state mutation |
| VII. Authorization | PASS | Permission gating on every page (FR-011); stub implementation acceptable per assumption; 401/403 expected from backend |
| VIII. Approval Workflows | N/A | No approval flows in this spec |
| IX. API Contract Integrity | PASS | Hand-written client mirrors backend OpenAPI contract (assumption); ProblemDetails error surfacing (FR-002) |
| X. UI and Design System | PASS | Design tokens only (FR-007); RTL Arabic-first (FR-007); shared UI component library (FR-004/005); MoneyDisplay for amounts (FR-003); permission identifiers on navigation (FR-006) |
| XI. Testing | PASS | Vitest + Testing Library available; spec defines testable acceptance scenarios |
| XII. Controlled Change | PASS | No constitutional violations; clean slate rebuild under existing module registry |

**No violations detected.** No complexity tracking needed.

## Project Structure

### Documentation (this feature)

```text
specs/011-budgeting-frontend-foundation/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (frontend DTO shapes)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API client contract)
│   └── frontend-api-contract.md
└── tasks.md             # Phase 2 output (/speckit.tasks - NOT created here)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── features/budgeting/
│   ├── shared/
│   │   ├── types.ts              # FR-001: All DTOs, enums, Arabic label maps
│   │   ├── client.ts             # FR-002: Typed API client + cache key factory
│   │   ├── index.ts              # Barrel exports
│   │   └── components/
│   │       └── AvailabilityIndicator.tsx  # FR-003
│   ├── budget-types/
│   │   ├── pages/
│   │   │   └── BudgetTypesListPage.tsx    # FR-004
│   │   ├── hooks/
│   │   │   └── useBudgetTypes.ts          # Query + mutation hooks
│   │   └── index.ts
│   ├── funds/
│   │   ├── pages/
│   │   │   ├── FundsListPage.tsx          # FR-005
│   │   │   └── FundDetailPage.tsx         # FR-005 detail view
│   │   ├── hooks/
│   │   │   └── useFunds.ts               # Query + mutation hooks
│   │   └── index.ts
│   └── index.ts                  # Feature barrel
├── app/routes.tsx                # FR-006: 3 new route entries
├── layouts/navigation.ts         # FR-006: Sidebar "الموازنة" group update
└── shared/constants/permissions.ts  # Already has BUDGET_PERMISSIONS
```

**Structure Decision**: Follows existing feature module anatomy (types.ts, client.ts, hooks/, pages/, components/). Shared types live under features/budgeting/shared/ as the single source of truth for all 5 specs. No new directories outside the established pattern.

## Complexity Tracking

> No Constitution Check violations. No complexity tracking needed.
