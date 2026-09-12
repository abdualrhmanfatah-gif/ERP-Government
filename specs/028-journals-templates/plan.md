# Implementation Plan: ACC-03 — Journals & Templates Management

**Branch**: `028-journals-templates` | **Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/028-journals-templates/spec.md`

## Summary

Frontend-only feature: admin screens for managing Journals and Journal Entry Templates in the accounting module. Backend CRUD endpoints, domain entities, enums, and commands/queries are fully built and operational. The frontend must provide list/create/edit screens for both entities, respecting the binding data contract from web-api-client.ts. The feature integrates with spec 023 (journal entry creation pickers) by exposing active-only filtered lists.

## Technical Context

**Language/Version**: TypeScript 5.x / React 19 / Vite

**Primary Dependencies**: React 19, React Router, TanStack Query, Lucide icons, existing UI components (PageHeader, FilterBar, FilterSelect, Button, DataGrid patterns)

**Storage**: N/A (backend API consumed via NSwag-generated web-api-client.ts)

**Testing**: Vitest + React Testing Library (existing patterns in `features/accounting/__tests__/`)

**Target Platform**: Web (SPA)

**Project Type**: Web application (frontend module within existing ERP)

**Performance Goals**: Standard CRUD — list pages load < 2s, form submissions < 1s

**Constraints**: Arabic-first RTL layout, dark mode via design tokens, binding data contract from web-api-client.ts (no field renaming)

**Scale/Scope**: ~6 pages + template detail lines section, ~4 hooks + `useTemplateLines.ts`, ~2 shared components + `TemplateLinesTable.tsx` + `TemplateLineEditor.tsx`

**Amend US4 (lines)**: Backend lines CRUD (Commands/Queries/Endpoints mirroring `JournalEntryLines`) + `JournalEntryTemplateDto.Lines` + totals + frontend read/edit tables + TDD. Exemplar bindings: `CreateJournalEntryLineCommand.cs`, `useJournalEntryLines.ts:5`, `JournalEntryDetailPage.tsx:172`, `JournalEntryCreatePage.tsx:214`. No migration (table `JournalEntryTemplateLines` exists); `npm run generate-api` required after endpoints.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

No constitution.md file exists. Proceeding under standard project conventions from AGENTS.md.

**Gates evaluated**:
- ✅ Minimal APIs only (no controllers) — backend already uses IEndpointGroup pattern
- ✅ Every handler returns Result<T> — backend already does this
- ✅ Permission codes from PermissionCodes.cs — backend uses `Accounting.Journals.*` and `Accounting.Templates.*`
- ✅ No inline approval columns — no lifecycle state machine needed (isActive toggle only)
- ✅ Arabic-first RTL — spec requires it
- ✅ No new backend entities — frontend-only feature

## Project Structure

### Documentation (this feature)

```text
specs/028-journals-templates/
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
src/Web/ClientApp/src/features/accounting/
├── journals/
│   ├── pages/
│   │   ├── JournalsListPage.tsx
│   │   ├── JournalCreatePage.tsx
│   │   └── JournalEditPage.tsx
│   └── components/
│       ├── JournalGrid.tsx
│       └── JournalForm.tsx
├── templates/
│   ├── pages/
│   │   ├── TemplatesListPage.tsx
│   │   ├── TemplateCreatePage.tsx
│   │   └── TemplateEditPage.tsx
│   └── components/
│       ├── TemplateGrid.tsx
│       └── TemplateForm.tsx
├── hooks/
│   ├── useJournalsList.ts      # Already exists — extend with type filter
│   ├── useJournalById.ts       # New
│   ├── useCreateJournal.ts     # New
│   ├── useUpdateJournal.ts     # New
│   ├── useTemplatesList.ts     # New
│   ├── useTemplateById.ts      # New
│   ├── useCreateTemplate.ts    # New
│   └── useUpdateTemplate.ts    # New
└── shared/
    └── client.ts               # Extend with JournalsClient + TemplatesClient instances
```

**Structure Decision**: Feature follows existing accounting module pattern — `journals/` and `templates/` subdirectories under `features/accounting/`, mirroring `account-groups/` and root-level pages pattern.

## Complexity Tracking

No constitution violations — no complexity tracking needed.
