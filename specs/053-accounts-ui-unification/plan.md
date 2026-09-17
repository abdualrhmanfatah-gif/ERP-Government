# Implementation Plan: Accounts Management UI Unification (Phase 2)

**Branch**: `053-accounts-ui-unification` | **Date**: 2026-09-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/053-accounts-ui-unification/spec.md`

## Summary

Migrate the six accounts-management screens (accounts list/create/edit/detail; account groups list/detail) to the approved Phase 1 visual language (spec 052 contracts, recipes, densities, status vocabulary) without backend changes. Concretely: fix the tree grid's status display, undefined token references, loading/error/empty scoping; rebuild the account form submit flow on the shared error contract with a feature-boundary Zod schema; correct the edit page's loading/not-found/error state machine; apply the detail recipe (identity, `StatusBadge`, single primary edit, `Page.onBack`, `MetaItem` toolbar) and build the sub-accounts tab from already-fetched accounts data; align the groups list/detail (FilterSearch, inactive status role, persistent error state, action hierarchy). Record structured review evidence per screen and update the deferred backlog.

## Technical Context

**Language/Version**: TypeScript / React 19 (frontend only; no backend changes)

**Primary Dependencies**: Vite, Tailwind v3, shared components in `src/Web/ClientApp/src/components/ui/`, `src/Web/ClientApp/src/components/Accounting*` domain components, React Hook Form + Zod, TanStack Query, NSwag-generated `AccountsClient` / account-group clients, `handleApiError` from `src/Web/ClientApp/src/shared/api/result-to-ui.ts`

**Storage**: N/A — no schema, API, or payload changes. The sub-accounts view derives from the existing accounts query (`ParentId`, `Level`).

**Testing**: No frontend automated tests (AGENTS.md governance override). Verification = lint/build/design gates plus the structured manual review log (`review-evidence.md`). Backend suites are unchanged and only gate once the working tree compiles.

**Target Platform**: Web SPA — Arabic-first RTL, light + dark modes, responsive to a 320 CSS px equivalent viewport

**Project Type**: Frontend design-language migration inside the existing web application

**Performance Goals**: No regression to existing load/interaction behavior; no new API calls beyond what pages already issue (the detail page may reuse the existing accounts query for sub-accounts).

**Constraints**: Preserve endpoints, payloads, permissions, and business rules (FR-010); keep dialogs for group editing (clarified); no pagination/API work for the accounts list (clarified); no screens outside the six (FR-017).

**Scale/Scope**: 6 screens, 3 feature page folders, 4 shared accounting components touched (`AccountingAccountGrid`, `AccountingAccountForm`, `AccountingAccountDetail`, `AccountingAccountGroupForm`), 1 new schema file, 1 evidence log.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Frontend-only; no backend project or layer touched |
| II. Bounded Contexts | ✅ PASS | Changes stay in `features/accounting` and the shared component library it owns |
| III. Server-Side Business-Rule Integrity | ✅ PASS | No business rule changed; account code uniqueness and group deactivation rules remain server-enforced |
| IV. Financial Integrity | ⬜ N/A | No ledger/posting/money logic changed |
| V. Budget Control | ⬜ N/A | Not touched |
| VI. Data Integrity | ⬜ N/A | No schema, migration, or persisted-data change |
| VII. Authorization | ✅ PASS | Existing permission checks preserved unchanged |
| VIII. Approval Workflows and Audit | ⬜ N/A | Not touched |
| IX. API and Frontend Contract Integrity | ✅ PASS | No endpoint or payload change; optimistic-concurrency tokens keep round-tripping |
| X. UI and Design System Consistency | ✅ PASS | Consumes the Phase 1 foundation; no new tokens or components |
| XI. Testing, Verification, and Evidence | ⚠️ CONDITIONAL | Frontend automated tests excluded by AGENTS.md governance override; evidence recorded per FR-015; backend suites unaffected |
| XII. Controlled Architectural Change | ✅ PASS | No architectural change; shared component public props preserved |
| XIII. Error Handling, Diagnostics, and Recovery | ✅ PASS | Forms adopt the existing `handleApiError` contract; no error-contract change |

**Gate result**: PASS with the documented frontend testing condition. No registered exception is modified.

**Post-Phase 1 re-check**: unchanged — research/data-model/contracts introduce no new dependency, storage, error-contract, or authorization change.

## Project Structure

### Documentation (this feature)

```text
specs/053-accounts-ui-unification/
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
├── features/accounting/
│   ├── pages/
│   │   ├── AccountsListPage.tsx        # UPDATE: no-results vs empty, error scope, grid props
│   │   ├── AccountCreatePage.tsx       # UPDATE: form submit → shared error contract, maxWidth
│   │   ├── AccountEditPage.tsx         # UPDATE: loading/not-found/error state machine + submit
│   │   └── AccountDetailPage.tsx       # UPDATE: detail recipe, toolbar, primary edit, sub-accounts tab
│   ├── account-groups/pages/
│   │   ├── AccountGroupsListPage.tsx   # UPDATE: FilterSearch, status role, error state, primary create
│   │   └── AccountGroupDetailPage.tsx  # UPDATE: onBack, StatusBadge, action hierarchy, empty states
│   └── shared/
│       └── schemas.ts                  # NEW: account + group Zod schemas shared with the forms
├── components/
│   ├── AccountingAccountGrid.tsx       # UPDATE: StatusBadge, token fixes, ErrorState/empty variants
│   ├── AccountingAccountForm.tsx       # UPDATE: schema import, sections, error binding, async submit
│   ├── AccountingAccountDetail.tsx     # UPDATE: token fixes, presentation per detail recipe
│   ├── AccountingAccountGroupForm.tsx  # REVIEW: schema reuse + error binding (RHF+Zod already present)
│   └── AccountingGroupTree.tsx         # REVIEW: verify states/status/tokens (likely no change)
└── shared/api/result-to-ui.ts          # REUSE: handleApiError (no change)

specs/053-accounts-ui-unification/
└── review-evidence.md                  # NEW: structured review log (Phase 1 schema)

docs/ui-patterns.md                     # UPDATE: deferred backlog statuses for this batch
```

**Structure Decision**: The existing feature/shared-component layout is preserved; no folders are moved and no new routes are introduced. The account form component keeps its domain-prefixed location per AGENTS.md; only its internal composition changes.

## Complexity Tracking

> No Constitution violations requiring justification. All changes stay inside the existing frontend boundaries.
