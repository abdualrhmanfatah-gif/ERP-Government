# Implementation Plan: Unified Party Registry & Shared Document Panels

**Branch**: `022-unified-party-registry` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/022-unified-party-registry/spec.md`

## Summary

Build the party management UI (list + detail pages) and three reusable document panels (approvals timeline, status log, attachments with mandatory gate) that plug into any existing or future document screen. No new backend entities — all work is frontend over existing backend services.

## Technical Context

**Language/Version**: TypeScript 5.9, React 19

**Primary Dependencies**: Tanstack Query 5, react-hook-form 7, zod 3, shadcn/ui (Tailwind CSS 3), react-router-dom 7, Vitest 4

**Storage**: N/A (frontend only — backend persists via EF Core + SQL Server)

**Testing**: Vitest 4 + @testing-library/react + jsdom + jest-dom/vitest

**Target Platform**: Desktop web (Chromium, Firefox, Safari) — RTL Arabic layout

**Project Type**: Web application (React SPA with Vite 8)

**Performance Goals**: Party search < 1s for 10k records; related documents < 2s for 100 items; duplicate tax check < 2s on blur

**Constraints**: Desktop-first; Arabic-primary UI; no mobile-responsive requirement; no new backend entities

**Scale/Scope**: 2 new pages (PartiesListPage, PartyDetailPage), 3 new shared components (ApprovalsPanel, StatusLogPanel, AttachmentsPanel), 2 feature folders (features/parties/, features/documents/)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

No constitution.md found. Skipping constitution gate check.

Relevant AGENTS.md principles applied:
- Minimal APIs only — no controllers (backend already follows this)
- Every handler returns Result<T> (frontend handles Result wrapper in API client)
- [Authorize(Policy = PermissionCodes.X)] on every endpoint (backend has placeholder policies — UI hides buttons per spec clarification)
- Frontend: React 19 + TypeScript + Vite in src/Web/ClientApp
- API clients: NSwag-generated + manual fetch wrappers in features/<domain>/shared/client.ts
- Tests: Vitest + @testing-library/react, co-located in features/<domain>/__tests__/

## Project Structure

### Documentation (this feature)

```text
specs/022-unified-party-registry/
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
├── features/
│   ├── parties/                              # NEW — party management
│   │   ├── __tests__/
│   │   │   ├── PartiesListPage.test.tsx      # filter/search/toggle tests
│   │   │   ├── PartyDetailPage.test.tsx      # detail + related docs tests
│   │   │   └── DuplicateTaxWarning.test.tsx  # duplicate tax number UX
│   │   ├── shared/
│   │   │   ├── client.ts                     # partiesClient (manual fetch wrappers)
│   │   │   ├── types.ts                      # DTOs, enums, label maps, filter types
│   │   │   └── index.ts                      # barrel export
│   │   ├── hooks/
│   │   │   └── useParties.ts                 # react-query hooks wrapping partiesClient
│   │   └── pages/
│   │       ├── PartiesListPage.tsx           # list with search, filter, toggle active
│   │       └── PartyDetailPage.tsx           # profile + related documents tabs
│   │
│   └── documents/                            # NEW — shared document panels (no routes)
│       ├── __tests__/
│       │   ├── ApprovalsPanel.test.tsx       # renders fixture data, pending states
│       │   ├── StatusLogPanel.test.tsx       # renders fixture data, empty states
│       │   └── AttachmentsPanel.test.tsx     # upload/delete, gate badge states
│       ├── components/
│       │   ├── ApprovalsPanel.tsx            # reusable approvals timeline
│       │   ├── StatusLogPanel.tsx            # reusable status history log
│       │   └── AttachmentsPanel.tsx          # upload/delete + gate badge indicator
│       └── shared/
│           ├── client.ts                     # documentsClient (generic document endpoints)
│           ├── types.ts                      # shared document DTOs
│           └── index.ts                      # barrel export
│
├── app/
│   └── routes.tsx                            # ADD: /parties route entries
```

**Structure Decision**: Two feature folders. `features/parties/` owns party-specific pages, hooks, and API client. `features/documents/` owns the three reusable panels with no routes — imported by parties and future document screens (024, 025, 021). This follows the existing pattern where `features/budgeting/components/` holds cross-cutting panels like `ApprovalHistoryPanel.tsx`.

## Complexity Tracking

> No constitution violations — no constitution exists. No complexity justifications needed.
