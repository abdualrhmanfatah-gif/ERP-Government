# Implementation Plan: Journal Entry Lifecycle Screens

**Branch**: `025-journal-entry-lifecycle` | **Date**: 2026-09-06 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/025-journal-entry-lifecycle/spec.md`

## Summary

Rebuild the journal entry lifecycle screens (list, create, detail) to serve as the accountant's complete working surface for the six-state machine (Draft → Submitted → Approved → Posted → Reversed / Cancelled). Replaces stale MovesListPage/MoveCreatePage/MoveDetailPage with modern patterns matching the budgeting/financial-settings features. Frontend-only work — no backend changes. Uses POST-style lifecycle actions (not PATCH) via NSwag client methods.

## Technical Context

**Language/Version**: TypeScript 5.x, React 19, Vite

**Primary Dependencies**: TanStack Query (useQuery/useMutation), react-router-dom, lucide-react icons, Tailwind CSS with design tokens, NSwag-generated API client (JournalEntriesClient)

**Storage**: None (frontend consumes existing backend APIs)

**Testing**: Vitest + @testing-library/react + @testing-library/jest-dom/vitest

**Target Platform**: Web (SPA), Arabic-first RTL, dark mode support

**Project Type**: Web application frontend (journal entry lifecycle UI over existing backend)

**Performance Goals**: Standard SPA — instant page transitions, <1s data loads, live balance totals update within 500ms

**Constraints**: Must use shared UI component library; no per-feature duplicate primitives; all monetary values through MoneyDisplay; RTL layout throughout; POST-style lifecycle actions (NOT PATCH)

**Scale/Scope**: 3 pages (list/detail/create), 1 dialog (reverse), ~6 hooks, ~8 components, ~10 tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Frontend is separate application consuming HTTP contract only. No backend references. |
| II. Bounded Contexts | ✅ PASS | Frontend feature folder is self-contained under `features/accounting/`. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | All business rules enforced server-side. Frontend mirrors state machine for UI gating only. |
| IV. Financial Integrity | ✅ PASS | Balance checks are UI convenience; server is authoritative. Reversal creates counter-entry server-side. |
| V. Budget Control | ✅ PASS | Not applicable — journal entries are accounting, not budget control. |
| VI. Data Integrity | ✅ PASS | No schema changes. Frontend reads/writes via API contracts. RowVersion sent on every action. |
| VII. Authorization | ⚠️ REGISTERED EXCEPTION | Frontend permission stubs (exception #4). Use `usePermission` hook with server as authority. |
| VIII. Approval Workflows | ✅ PASS | Approval uses existing backend approval pipeline. ApprovalsPanel displays history. |
| IX. API and Frontend Contract Integrity | ✅ PASS | NSwag-generated clients from OpenAPI spec. Contract-conformant modules. |
| X. UI and Design System Consistency | ✅ PASS | All design from tokens. Shared component library. RTL throughout. MoneyDisplay for monetary values. |
| XI. Testing, Verification, and Evidence | ✅ PASS | TDD mandatory. Frontend tests in Vitest. |
| XII. Controlled Architectural Change | ✅ PASS | No architectural changes — UI replacement only. |

**Gate Result**: PASS. One registered exception (#4 — frontend permission stubs) applies; no new violations.

## Project Structure

### Documentation (this feature)

```text
specs/025-journal-entry-lifecycle/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/features/accounting/
├── types.ts                          # DTOs, enums, commands (EXTEND — add missing fields)
├── shared/
│   └── client.ts                     # Hand-written fetch wrappers (NEW — wrap NSwag clients)
│
├── pages/
│   ├── JournalEntriesListPage.tsx    # REBUILD — status filter chips, pagination, system badge
│   ├── JournalEntryCreatePage.tsx    # REBUILD — header form + line editor + balance footer
│   └── JournalEntryDetailPage.tsx    # REBUILD — header, lines, lifecycle actions, approvals, status log
│
├── components/
│   ├── JournalEntriesGrid.tsx        # REBUILD — modern table with filters, status badges, system badge
│   ├── JournalEntryDetail.tsx        # REBUILD — full detail with lifecycle action bar
│   ├── JournalEntryHeaderForm.tsx    # REBUILD — react-hook-form + zod header creation
│   ├── JournalEntryLinesEditor.tsx   # REBUILD — line editing with XOR validation
│   ├── EntryLifecycleActions.tsx     # NEW — POST-style lifecycle action bar (NOT LifecycleActions)
│   ├── BalanceIndicator.tsx          # KEEP — debit/credit balance check
│   ├── DimensionPickers.tsx          # KEEP — fund/project/budget-item/encumbrance/PO pickers
│   ├── FiscalYearIndicator.tsx       # KEEP — shows fiscal year + period for a date
│   ├── ReverseDialog.tsx             # REBUILD — mandatory reason + counter-line preview
│   └── StatusBadge.tsx               # KEEP — entry-status-specific badge
│
├── hooks/
│   ├── useJournalEntries.ts          # REBUILD — list, get, create, update, submit, approve, post, reverse, cancel
│   ├── useJournalEntryLines.ts       # REBUILD — create, update, remove lines
│   └── useJournalsList.ts            # KEEP — query active journals
│
└── __tests__/
    ├── JournalEntriesListPage.test.tsx    # NEW — filter chips, search, system badge
    ├── JournalEntryCreatePage.test.tsx    # NEW — form, line editor, balance, submit
    ├── JournalEntryDetailPage.test.tsx    # NEW — lifecycle actions per state, reversal linkage
    ├── EntryLifecycleActions.test.tsx     # NEW — action rendering per state machine
    ├── ReverseDialog.test.tsx             # NEW — reason required, counter-line preview
    └── JournalEntryLinesEditor.test.tsx   # EXISTING — update with XOR validation tests
```

**Structure Decision**: Rebuild within existing `features/accounting/` feature folder. Replace stale journal entry pages/components while keeping account-related pages untouched. Reuse ApprovalsPanel and StatusLogPanel from documents feature. Create new EntryLifecycleActions component (POST-style, not budgeting LifecycleActions).

## Complexity Tracking

No violations. No complexity tracking needed.
