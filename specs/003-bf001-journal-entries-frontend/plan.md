# Implementation Plan: BF-001 Journal Entries Frontend

**Branch**: `003-bf001-journal-entries-frontend` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-bf001-journal-entries-frontend/spec.md`

## Summary

Frontend implementation for BF-001 Journal Entries — complete lifecycle management (list, create, detail, move lines, submit, approve, post, cancel, reverse) using React 19, TypeScript 5.9, React Query, RHF/Zod, shadcn/ui, and Tailwind CSS. Arabic RTL-first. Client-side pagination. Hybrid form approach (RHF header, manual lines).

## Technical Context

**Language/Version**: TypeScript ~5.9, React 19.1.0

**Primary Dependencies**: react-hook-form 7.86, @tanstack/react-query 5.102, @tanstack/react-table 9.2, zod 3.25, shadcn/ui (base-nova), tailwindcss 3.4, lucide-react, sonner, date-fns

**Storage**: N/A (frontend only — consumes backend API)

**Testing**: Vitest 4.1 + React Testing Library + jsdom

**Target Platform**: Web browser (Chrome, Firefox, Safari, Edge)

**Project Type**: web application (SPA frontend)

**Performance Goals**: List load <2 sec for 1,000 entries; Create entry with 5 lines <3 min; Workflow transitions <2 sec

**Constraints**: Arabic-only, RTL-first, client-side pagination (no backend changes), no RBAC backend (stub permissions)

**Scale/Scope**: 3 pages (list, create, detail), ~15 components, 12 API endpoints consumed, 30 functional requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | PASS | Frontend is separate application consuming HTTP contract only |
| II. Bounded Contexts | PASS | Frontend accesses Accounting module via published API |
| III. Server-Side Business-Rule Integrity | PASS | Frontend validates UX only; all business rules enforced server-side |
| IV. Financial Integrity | PASS | Frontend displays balance indicator; server enforces double-entry |
| VII. Authorization | PASS (registered exception DEP-020) | Permission stub — real RBAC out of scope |
| IX. API and Frontend Contract Integrity | PASS | NSwag-generated client conforms to OpenAPI contract |
| X. UI and Design System Consistency | PASS | Uses design tokens, shared UI components, RTL, dark mode |
| XI. Testing | PASS | Vitest + RTL configured; tests required before done |

**No violations requiring justification.**

## Project Structure

### Documentation (this feature)

```text
specs/003-bf001-journal-entries-frontend/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-contracts.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── features/accounting/
│   ├── pages/
│   │   ├── MovesListPage.tsx        # MODIFY — add DataGrid, filters, URL params
│   │   ├── MoveCreatePage.tsx       # MODIFY — refactor to RHF header, keep manual lines
│   │   └── MoveDetailPage.tsx       # MODIFY — minor updates
│   ├── components/
│   │   ├── MovesGrid.tsx            # MODIFY — replace table with DataGrid
│   │   ├── MoveDetail.tsx           # MODIFY — add confirmation dialogs, improve workflow UX
│   │   ├── MoveLinesEditor.tsx      # MODIFY — add BalanceIndicator
│   │   ├── MovesFilters.tsx         # NEW — date range + journal filter bar
│   │   ├── FiscalYearIndicator.tsx  # NEW — auto-detected FY/Period display
│   │   └── BalanceIndicator.tsx     # NEW — running debit/credit balance
│   ├── hooks/
│   │   ├── useMoves.ts             # MODIFY — add pagination support
│   │   ├── useFiscalYearByDate.ts  # NEW — FY auto-detection via React Query
│   │   └── useExchangeRateLookup.ts # NEW — exchange rate auto-fetch via React Query
│   ├── client.ts                   # NO CHANGES — all endpoints covered
│   └── types.ts                    # NO CHANGES — types already defined
├── components/ui/                   # NO CHANGES — shared library
├── shared/                          # NO CHANGES — shared utils
└── layouts/                         # NO CHANGES — navigation already configured
```

**Structure Decision**: Frontend-only changes within existing `features/accounting/` module. No new directories. 3 new components, 2 new hooks. All modifications to existing files.

## Complexity Tracking

> No violations. No complexity tracking needed.
