# Implementation Plan: Assets Management UI Unification (Phase 3)

**Branch**: `057-assets-ui-unification` | **Date**: 2026-09-15 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/057-assets-ui-unification/spec.md`

## Summary

Migrate the eight assets-management screens (assets list/create/edit/detail; asset groups list/create/edit/detail) to the approved Phase 1 visual language (spec 052 contracts, recipes, densities, status vocabulary) without backend changes, following the spec 053 accounts batch as the precedent. Concretely: replace palette/ad-hoc styling with shared components and semantic tokens; render lifecycle state through `StatusBadge` with a feature-boundary base-role map and fallback; rebuild forms on the canonical field pattern with shared controls, correct per-mode schemas, `handleApiError` + root `Alert`, and preserved values; apply the detail recipe (`Page.onBack`, toolbar status, single primary edit, confirmed deactivate) and remove the read-only no-op submit; give lists typed grids (assets list keeps server-side pagination via the shared grid/pagination; the groups tree stays as a justified variation) with distinct loading/empty/no-results/error states; make every visible control functional (dead filter wired, duplicate option removed); fix navigation permission naming and corrupted Arabic strings; record structured review evidence and update the deferred backlog.

## Technical Context

**Language/Version**: TypeScript / React 19 (frontend only; no backend changes)

**Primary Dependencies**: Vite, Tailwind v3, shared components in `src/Web/ClientApp/src/components/ui/`, React Hook Form + Zod, TanStack Query, existing asset hooks (`useAssets`, `useAssetGroups`) and routes, `handleApiError`/`handleLifecycleError` from `src/Web/ClientApp/src/shared/api/result-to-ui.ts`, `getQueryErrorMessage` from `src/Web/ClientApp/src/shared/api/query-error.ts`

**Storage**: N/A — no schema, API, or payload changes; endpoints and concurrency-token round-trips stay as built by specs 054/055

**Testing**: No frontend automated tests (AGENTS.md governance override). Verification = lint/build/design gates plus the structured manual review log (`review-evidence.md`). Backend suites are unchanged and only gate once the working tree compiles.

**Target Platform**: Web SPA — Arabic-first RTL, light + dark modes, responsive to a 320 CSS px equivalent viewport

**Project Type**: Frontend design-language migration inside the existing web application

**Performance Goals**: No regression to existing load/interaction behavior; no new API calls beyond what the screens already issue (same queries/mutations)

**Constraints**: Preserve endpoints, payloads (including `rowVersion`), permissions, routes, and business rules (FR-010); keep create/edit as dedicated routes; keep the assets list's server-side paging and the groups list's tree/grid modes; no new tokens/components/routes; no other asset sub-features

**Scale/Scope**: 8 screens, 2 feature folders (`features/assets/assets`, `features/assets/asset-groups`), 2 shared form components touched, 2 schema modules, 2 shared hooks modules reviewed, 1 evidence log, 1 scope inventory

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Frontend-only; no backend project or layer touched |
| II. Bounded Contexts | ✅ PASS | Changes stay in `features/assets` and the shared UI library it consumes |
| III. Server-Side Business-Rule Integrity | ✅ PASS | No business rule changed; asset lifecycle/group deactivation rules remain server-enforced |
| IV. Financial Integrity | ⬜ N/A | No ledger/posting/money logic changed |
| V. Budget Control | ⬜ N/A | Not touched |
| VI. Data Integrity | ⬜ N/A | No schema, migration, or persisted-data change |
| VII. Authorization | ✅ PASS | Route guards preserved unchanged; only the navigation permission identifier is corrected to match the guard (recorded); no new in-page gating |
| VIII. Approval Workflows and Audit | ⬜ N/A | Not touched |
| IX. API and Frontend Contract Integrity | ✅ PASS | No endpoint or payload change; optimistic-concurrency tokens keep round-tripping |
| X. UI and Design System Consistency | ✅ PASS | This feature consumes the Phase 1 foundation; palette styling replaced by tokens/shared components; logical RTL properties only |
| XI. Testing, Verification, and Evidence | ⚠️ CONDITIONAL | Frontend automated tests excluded by AGENTS.md governance override; evidence recorded per FR-015; backend suites unaffected |
| XII. Controlled Architectural Change | ✅ PASS | No architectural change; shared component public props preserved (additive only where needed) |
| XIII. Error Handling, Diagnostics, and Recovery | ✅ PASS | Forms adopt `handleApiError` + root `Alert`; toggles use `handleLifecycleError`; queries use normalized `getQueryErrorMessage` + `refetch`; one notification owner per failure; no error-contract change |

**Gate result**: PASS with the documented frontend testing condition. No registered exception is modified.

**Constitution XIII detail** (per plan template): affected failure paths — asset create/update/deactivate, group create/update/activate/deactivate, and all eight screens' query paths. Classifications/contracts are unchanged (shared normalized representation). Presentation/recovery ownership: forms own field/root binding; pages own query error states with retry; toggles own their notification. Success compatibility: payload shapes and success navigation unchanged. Verification evidence: per-screen error rows in `review-evidence.md` plus FR-009 checks. No new decision record required; the frontend testing condition remains covered by the AGENTS.md override and the constitution's registered exception framework.

**Post-Phase 1 re-check**: unchanged — research/data-model/contracts introduce no new dependency, storage, error-contract, or authorization change.

## Project Structure

### Documentation (this feature)

```text
specs/057-assets-ui-unification/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── screen-contract.md
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── features/assets/
│   ├── assets/
│   │   ├── pages/
│   │   │   ├── AssetsListPage.tsx       # UPDATE: Page recipe, FilterBar, DataGrid + server pagination, states
│   │   │   ├── AssetCreatePage.tsx      # UPDATE: Page recipe, promise submit, error contract
│   │   │   ├── AssetEditPage.tsx        # UPDATE: Page recipe, loading/not-found/error machine, rowVersion
│   │   │   └── AssetDetailPage.tsx      # UPDATE: detail recipe, read-only form, primary edit, ConfirmDialog deactivate
│   │   ├── components/
│   │   │   └── AssetForm.tsx            # UPDATE: shared controls, Card sections, per-mode schema, read-only mode, error binding
│   │   └── shared/
│   │       ├── schemas.ts               # REUSE (create/update already exist; fix import usage)
│   │       └── types.ts                 # UPDATE: add feature-boundary status base-role map + labels
│   └── asset-groups/
│       ├── pages/
│       │   ├── AssetGroupsListPage.tsx  # UPDATE: functional active filter, StatusBadge, DataGrid grid view, states
│       │   ├── AssetGroupCreatePage.tsx # UPDATE: Alert + handleApiError, Card sections, page recipe polish
│       │   ├── AssetGroupEditPage.tsx   # UPDATE: state machine, Alert + handleApiError, rowVersion
│       │   └── AssetGroupDetailPage.tsx # UPDATE: primary edit, outline/destructive toggles, StatusBadge, empty states
│       └── shared/
│           └── schemas.ts               # REUSE (create/update already correct per mode)
├── components/ui/                       # REUSE: Page, DataGrid, Pagination, StatusBadge, Alert, EmptyState, ErrorState, ConfirmDialog, Button/Input/Select/Textarea
├── shared/api/result-to-ui.ts           # REUSE: handleApiError / handleLifecycleError
├── shared/api/query-error.ts            # REUSE: getQueryErrorMessage
├── app/routes.tsx                       # REUSE: asset routes unchanged
└── layouts/navigation.ts                # UPDATE: /assets permission identifier

specs/057-assets-ui-unification/
└── review-evidence.md                   # NEW: structured review log (Phase 1 schema)

docs/ui-patterns.md                      # UPDATE: deferred backlog statuses for this batch
```

**Structure Decision**: The existing feature layout is preserved; no folders are moved and no routes are introduced. Shared UI components are consumed as-is (any necessary prop is additive and justified in tasks). The assets feature keeps its two sub-modules (`assets`, `asset-groups`) and their hooks; schema modules stay at each sub-module's feature boundary.

## Complexity Tracking

> No Constitution violations requiring justification. All changes stay inside the existing frontend boundaries.
