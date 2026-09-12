# Implementation Plan: Purchase Requests UI Completion

**Branch**: `048-purchase-requests-ui` | **Date**: 2026-09-11 | **Spec**: [spec.md](./spec.md)

**Input**: Frontend-only completion of Purchase Requests feature in Procurement module

## Summary

Complete the Purchase Requests UI with create page, edit page, catalog comboboxes for Item/Unit selection, confirmation dialogs for lifecycle actions, and error handling. All work is frontend-only — no backend changes.

## Technical Context

**Language/Version**: TypeScript 5.9, React 19, Vite

**Primary Dependencies**: React Hook Form + Zod (forms), TanStack Query (server state), shadcn (UI), react-router v7 (routing), date-fns (dates)

**Storage**: None (frontend only, API-backed via `shared/api`)

**Testing**: No automated tests (governance decision — frontend has no test suite)

**Target Platform**: Modern browsers, Arabic RTL SPA

**Project Type**: Web application (frontend feature module)

**Performance Goals**: SC-002: list page loads within 3s, SC-003: form validations complete within 200ms

**Constraints**: Arabic only, RTL only, no physical CSS properties, no cross-feature imports

**Scale/Scope**: 5 pages (list, detail, create, edit, dashboard), ~15 new files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| V. Service/Use-Case Independence | ✅ | Feature-scoped hooks, no cross-feature imports |
| VI. Vertical Slice Architecture | ✅ | Feature folder: `features/procurement/purchase-requests/` |
| VIII. Single Responsibility | ✅ | One page per route, hooks per operation |
| X. Avoid Premature Abstractions | ✅ | Direct API calls via existing `api` utility |
| XII. Domain Experts & Code | ✅ | Arabic UI text hardcoded, no i18n framework needed |

No violations.

## Project Structure

```text
src/Web/ClientApp/src/
├── features/procurement/purchase-requests/
│   ├── shared/
│   │   ├── types.ts                    # Existing — interfaces
│   │   ├── schemas.ts                  # NEW — Zod schemas for create/edit forms
│   │   └── catalog-hooks.ts            # NEW — useItemsList, useUnitsList hooks
│   ├── hooks/
│   │   └── usePurchaseRequests.ts      # Existing — all PR hooks
│   └── pages/
│       ├── PurchaseRequestsListPage.tsx  # Existing — list page
│       ├── PurchaseRequestDetailPage.tsx # Existing — detail page
│       ├── PurchaseRequestCreatePage.tsx # NEW — create form page
│       └── PurchaseRequestEditPage.tsx   # NEW — edit form page
├── components/
│   └── ProcurementPurchaseRequest*.tsx   # NEW — shared form components if needed
├── app/routes.tsx                      # Existing — needs /create and /:id/edit routes added
└── shared/api/
    └── index.ts                        # Existing — api utility
```

## Complexity Tracking

No constitution violations — no complexity justification needed.
