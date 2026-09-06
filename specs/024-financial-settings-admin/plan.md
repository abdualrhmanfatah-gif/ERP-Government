# Implementation Plan: Financial Settings Administration UI

**Branch**: `024-financial-settings-admin` | **Date**: 2026-09-06 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/024-financial-settings-admin/spec.md`

## Summary

Build the admin UI for Financial Settings over the fully-built backend. Five sub-sections: Fiscal Years + Periods (P1), Document Sequences (P1), Currencies (P2), Exchange Rates (P2), Closing Entries (P2). Frontend-only work — no backend changes. Follows existing budgeting feature folder pattern with pages/, hooks/, shared/, components/.

## Technical Context

**Language/Version**: TypeScript 5.x, React 19, Vite

**Primary Dependencies**: TanStack Query (useQuery/useMutation), react-router-dom, lucide-react icons, Tailwind CSS with design tokens, NSwag-generated API client

**Storage**: None (frontend consumes existing backend APIs)

**Testing**: Vitest + @testing-library/react + @testing-library/jest-dom/vitest

**Target Platform**: Web (SPA), Arabic-first RTL, dark mode support

**Project Type**: Web application frontend (admin UI over existing backend)

**Performance Goals**: Standard SPA — instant page transitions, <1s data loads

**Constraints**: Must use shared UI component library; no per-feature duplicate primitives; all monetary values through MoneyDisplay; RTL layout throughout

**Scale/Scope**: 5 admin screens (list/detail/create for each sub-section), ~15 pages total, ~8 hooks, ~6 shared type/client files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Frontend is separate application consuming HTTP contract only. No backend references. |
| II. Bounded Contexts | ✅ PASS | Frontend feature folder is self-contained under `features/financial-settings/`. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | Frontend is delivery adapter only. All business rules enforced server-side. |
| IV. Financial Integrity | ✅ PASS | No financial calculations in frontend. Backend is source of truth. |
| V. Budget Control | ✅ PASS | Not applicable — this feature is admin UI, not budget control. |
| VI. Data Integrity | ✅ PASS | No schema changes. Frontend reads/writes via API contracts. |
| VII. Authorization | ⚠️ REGISTERED EXCEPTION | Frontend permission stubs (exception #4). Use `usePermission` hook with server as authority. |
| VIII. Approval Workflows | ✅ PASS | Closing entry approval uses existing backend approval pipeline. |
| IX. API and Frontend Contract Integrity | ✅ PASS | NSwag-generated clients from OpenAPI spec. Contract-conformant modules. |
| X. UI and Design System Consistency | ✅ PASS | All design from tokens. Shared component library. RTL throughout. MoneyDisplay for monetary values. |
| XI. Testing, Verification, and Evidence | ✅ PASS | TDD mandatory. Frontend tests in Vitest. |
| XII. Controlled Architectural Change | ✅ PASS | No architectural changes — UI-only addition. |

**Gate Result**: PASS. One registered exception (#4 — frontend permission stubs) applies; no new violations.

## Project Structure

### Documentation (this feature)

```text
specs/024-financial-settings-admin/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API contract references)
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/features/financial-settings/
├── shared/
│   ├── types.ts                    # DTOs, enums, Arabic label maps, filter types
│   ├── client.ts                   # Hand-written fetch wrapper with cache key factory
│   └── index.ts                    # Barrel re-export
├── hooks/
│   ├── useFiscalYears.ts           # TanStack Query hooks for fiscal year CRUD + lifecycle
│   ├── useFiscalPeriods.ts         # TanStack Query hooks for period CRUD + lock/unlock
│   ├── useDocumentSequences.ts     # TanStack Query hooks for sequence CRUD
│   ├── useCurrencies.ts            # TanStack Query hooks for currency CRUD + activate/deactivate
│   ├── useExchangeRates.ts         # TanStack Query hooks for rate CRUD + lookup + activate/deactivate
│   └── useClosingEntries.ts        # TanStack Query hooks for closing entry generate/approve/reverse
├── components/
│   ├── FiscalYearStatusBadge.tsx   # Status badge for Draft/Open/SoftClosed/HardClosed
│   ├── PeriodLockIndicator.tsx     # Lock status indicator for periods
│   ├── ClosingEntryLines.tsx       # Reviewable closing entry line table
│   └── ExchangeRateLookup.tsx      # Effective rate resolution display
├── fiscal-years/
│   ├── pages/
│   │   ├── FiscalYearsListPage.tsx
│   │   ├── FiscalYearDetailPage.tsx
│   │   └── FiscalYearCreatePage.tsx
├── document-sequences/
│   ├── pages/
│   │   └── DocumentSequencesListPage.tsx
├── currencies/
│   ├── pages/
│   │   ├── CurrenciesListPage.tsx
│   │   └── CurrencyCreatePage.tsx
├── exchange-rates/
│   ├── pages/
│   │   ├── ExchangeRatesListPage.tsx
│   │   └── ExchangeRateCreatePage.tsx
├── closing-entries/
│   ├── pages/
│   │   ├── ClosingEntriesListPage.tsx
│   │   └── ClosingEntryDetailPage.tsx
└── __tests__/
    ├── FiscalYearsListPage.test.tsx
    ├── FiscalYearDetailPage.test.tsx
    ├── FiscalYearCreatePage.test.tsx
    ├── DocumentSequencesListPage.test.tsx
    ├── CurrenciesListPage.test.tsx
    ├── CurrencyCreatePage.test.tsx
    ├── ExchangeRatesListPage.test.tsx
    ├── ExchangeRateCreatePage.test.tsx
    ├── ClosingEntriesListPage.test.tsx
    ├── ClosingEntryDetailPage.test.tsx
    ├── FiscalYearStatusBadge.test.tsx
    ├── PeriodLockIndicator.test.tsx
    ├── ClosingEntryLines.test.tsx
    └── ExchangeRateLookup.test.tsx
```

**Structure Decision**: Single frontend feature folder `features/financial-settings/` following the established budgeting pattern. Five sub-section folders for each admin area. Shared types/client at feature root. Components for reusable UI elements. Tests alongside pages.

## Post-Design Constitution Re-Check

**Gate Result (post-Phase 1)**: PASS. No new violations introduced by design.

- **I. Layered Architectural Integrity**: ✅ Frontend consumes HTTP contract only. No backend references.
- **III. Server-Side Business-Rule Integrity**: ✅ All business rules enforced server-side. Frontend is delivery adapter.
- **IV. Financial Integrity**: ✅ No financial calculations in frontend. Closing entry lines are display-only.
- **VII. Authorization**: ⚠️ Registered exception #4 still applies (frontend permission stubs). All actions declare required permission codes for future enforcement.
- **IX. API and Frontend Contract Integrity**: ✅ Hand-written clients mirror published OpenAPI contract exactly.
- **X. UI and Design System Consistency**: ✅ All design from tokens. Shared component library. RTL throughout.
- **XI. Testing, Verification, and Evidence**: ✅ TDD mandatory. Frontend tests in Vitest.

## Complexity Tracking

No violations. No complexity tracking needed.
