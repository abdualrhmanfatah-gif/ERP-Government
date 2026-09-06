# Implementation Plan: Journal Entries UI Refresh

**Branch**: `023-journal-entries-ui-refresh` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/023-journal-entries-ui-refresh/spec.md`

## Summary

Replace the broken Move-based accounting screens (MovesListPage/MoveCreatePage/MoveDetailPage) with JournalEntry-based pages using the renamed `/api/JournalEntries` NSwag client. Three pages: list (status badges, filters), create (line editor + dimension pickers + balance guard), detail (lines + lifecycle + reversal link). Reuse shared LifecycleActions, ApprovalsPanel, StatusLogPanel from budgeting/documents features.

## Technical Context

**Language/Version**: TypeScript 5.x (React 19, Vite)

**Primary Dependencies**: React 19, react-router-dom, react-hook-form, zod, TanStack Query (React Query), NSwag-generated `JournalEntriesClient`

**Storage**: N/A (frontend only — consumes backend API)

**Testing**: Vitest + React Testing Library (per `npm run test` in ClientApp)

**Target Platform**: Web (SPA), Arabic-first RTL, dark mode

**Project Type**: Web application (frontend feature replacement)

**Performance Goals**: List load < 2s (SC-001), entry creation < 3 min for 5 lines (SC-002)

**Constraints**: Must reuse existing shared components; no new design tokens; RTL/dark mode compliant; zero Move/MoveLine references post-implementation

**Scale/Scope**: 3 pages, ~12 components, ~8 hooks, ~4 test files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Layered Architectural Integrity | ✅ PASS | Frontend consumes HTTP contract only; no business logic in UI |
| II. Bounded Contexts | ✅ PASS | Accounting feature is self-contained; reuses shared components from documents/budgeting |
| III. Server-Side Business-Rule Integrity | ✅ PASS | Balance validation, lifecycle transitions enforced server-side; frontend shows errors |
| IV. Financial Integrity | ✅ PASS | Balance guard in UI is UX only; server enforces at post time |
| V. Budget Control | ✅ PASS | N/A for this feature (no budget operations) |
| VI. Data Integrity | ✅ PASS | Optimistic concurrency via RowVersion round-trip (FR-016) |
| VII. Authorization and SoD | ⚠️ PLACEHOLDER | Frontend permission checks are UX only (Registered Exception #1); server is authority |
| VIII. Approval Workflows | ✅ PASS | Reuses ApprovalsPanel/StatusLogPanel from documents feature |
| IX. API and Frontend Contract Integrity | ✅ PASS | Uses NSwag-generated JournalEntriesClient; no hand-written client needed |
| X. UI and Design System Consistency | ✅ PASS | Reuses shared UI components; design tokens from central source; RTL compliant |
| XI. Testing, Verification, and Evidence | ✅ PASS | TDD mandatory; frontend tests required |
| XII. Controlled Architectural Change | ✅ PASS | No layer/module boundary changes |

**Gate result**: PASS — no violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/023-journal-entries-ui-refresh/
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
├── features/accounting/
│   ├── pages/
│   │   ├── JournalEntriesListPage.tsx       # REPLACE MovesListPage
│   │   ├── JournalEntryCreatePage.tsx       # REPLACE MoveCreatePage
│   │   ├── JournalEntryDetailPage.tsx       # REPLACE MoveDetailPage
│   │   ├── AccountsListPage.tsx             # KEEP (unchanged)
│   │   ├── AccountCreatePage.tsx            # KEEP (unchanged)
│   │   ├── AccountDetailPage.tsx            # KEEP (unchanged)
│   │   └── AccountEditPage.tsx              # KEEP (unchanged)
│   ├── components/
│   │   ├── JournalEntriesGrid.tsx           # NEW (replaces MovesGrid)
│   │   ├── JournalEntryHeaderForm.tsx       # NEW (replaces MoveForm)
│   │   ├── JournalEntryLinesEditor.tsx      # NEW (replaces MoveLinesEditor)
│   │   ├── JournalEntryDetail.tsx           # NEW (replaces MoveDetail)
│   │   ├── StatusBadge.tsx                  # NEW (EntryStatus → colored badge)
│   │   ├── BalanceIndicator.tsx             # KEEP (existing, minor updates)
│   │   ├── ReverseDialog.tsx                # KEEP (existing, update types)
│   │   ├── FiscalYearIndicator.tsx          # KEEP (existing)
│   │   └── DimensionPickers.tsx             # NEW (fund/project/budgetItem/encumbrance/paymentOrder)
│   ├── hooks/
│   │   ├── useJournalEntries.ts             # NEW (replaces useMoves.ts)
│   │   ├── useJournalEntryLines.ts          # NEW (replaces useMoveLines.ts)
│   │   └── [existing hooks unchanged]
│   ├── types.ts                             # UPDATE (replace MoveDto → JournalEntryDto types)
│   └── index.ts                             # UPDATE (export new pages)
├── app/routes.tsx                           # UPDATE (swap component imports)
└── web-api-client.ts                        # NO CHANGE (NSwag-generated, already has JournalEntriesClient)
```

**Structure Decision**: Frontend feature replacement within existing `features/accounting/` folder. No new directories; replaces Move-based files with JournalEntry-based equivalents.

## Complexity Tracking

> No violations — section empty.
