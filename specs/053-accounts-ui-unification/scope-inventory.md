# Phase 2 Scope Inventory — 053 Accounts Management UI Unification

Per `data-model.md` §6. Created 2026-09-14 (baseline).

## Baseline gates

| Gate | Result | Notes |
|------|--------|-------|
| `npm run lint` | 141 problems (118 errors, 23 warnings) | baseline from Phase 1; no new problems added by this batch |
| `npm run build` | pass | Vite production build |
| `npm run design:usage` (batch scope) | 0 unknown refs in touched files | `--color-border-container` refs exist in the 3 AccountingAccount* components (T005) |

## Resolved inconsistencies (Phase 2 batch 1)

| # | Inconsistency | File(s) | Resolution |
|---|---------------|---------|------------|
| 1 | `Badge` used for lifecycle state in accounts tree grid and groups detail/list | `AccountingAccountGrid.tsx`, `AccountGroupDetailPage.tsx`, `AccountGroupsListPage.tsx` | `StatusBadge variant="active"/"inactive"` |
| 2 | Undefined `--color-border-container` refs in accounting components | `AccountingAccountGrid.tsx`, `AccountingAccountForm.tsx`, `AccountingAccountDetail.tsx` | Renamed to `--color-container-border` |
| 3 | Accounts grid error block ad-hoc (text + destructive button) | `AccountingAccountGrid.tsx` | `ErrorState` + outline retry via `onRetry` prop |
| 4 | Account form inline Zod schema, no root error binding, no cancel | `AccountingAccountForm.tsx` | Shared schema in `features/accounting/shared/schemas.ts`; `handleApiError` + root `Alert`; cancel ghost button |
| 5 | Account create/edit pages toast-only server errors | `AccountCreatePage.tsx`, `AccountEditPage.tsx` | Form-level error binding through `handleApiError`; single notification owner |
| 6 | Account edit page shows "not found" while loading | `AccountEditPage.tsx` | Loading/not-found/error state machine separated; `EmptyState` only after resolved miss |
| 7 | Account detail uses outline edit button and custom back button | `AccountDetailPage.tsx` | Detail recipe: `Page.onBack`, single primary edit action, `StatusBadge` in toolbar |
| 8 | Sub-accounts tab placeholder ("قريبًا") | `AccountDetailPage.tsx` | Real sub-accounts view from `useAccountsList()` with typed `DataGrid` |
| 9 | Groups list uses raw `Input` for search, inline error text | `AccountGroupsListPage.tsx` | `FilterSearch`; `Page error` + retry |
| 10 | Groups list uses `StatusBadge variant="closed"` for inactive | `AccountGroupsListPage.tsx`, `AccountGroupDetailPage.tsx` | `StatusBadge variant="inactive"` |
| 11 | Group detail has custom back button and inconsistent action hierarchy | `AccountGroupDetailPage.tsx` | `Page.onBack`; edit primary + outline/destructive toggle with `ConfirmDialog` |
| 12 | Group form has "(20)/(200)" label hints, double-spaced label, no loading state, no root error binding | `AccountingAccountGroupForm.tsx` | Canonical labels; `loading` prop on save button; `handleApiError` + root `Alert` |
| 13 | Group detail children/accounts empty states are plain `<p>` not `EmptyState` | `AccountGroupDetailPage.tsx` | `EmptyState` component for empty sections |
| 14 | Group detail postable capability is color-only (`Badge`) | `AccountGroupDetailPage.tsx` | Icon + text (no color-only meaning) |

## Deferred inconsistencies (out of Phase 2 batch scope)

| # | Inconsistency | Evidence | Reason deferred |
|---|---------------|----------|-----------------|
| 1 | Non-accounting `--color-border-*` refs outside this batch | token scan: 33 refs outside `components/ui/` | Pages outside validated scope |
| 2 | Remaining `docs/ui-patterns.md` backlog items not touching these 6 screens | backlog table | Only items blocking validated pages are in scope |
| 3 | Pre-existing ESLint errors (118) across the app | `npm run lint` baseline | Not caused by this feature |
| 4 | Sub-accounts tree expansion (recursive children) | FR-019 clarification | Explicitly out of scope for this batch |
| 5 | Deeper accounting-group tree in the groups list | FR-019 clarification | Group tree exists; deeper expansion deferred |

## Behavior changes register

| Change | Justification | Risk |
|--------|---------------|------|
| Cancel action added to account create/edit form | Form recipe requires a secondary cancel (FR-006) | None — payloads and submission unchanged |
| Accounts list differentiates empty vs no-results messages | List recipe state rule (FR-005) | None — data behavior unchanged |
| Account detail edit becomes the single primary action | Clarified answer for FR-006/SC-002 | None — navigation unchanged |
| Sub-accounts tab built from existing data | Clarified answer for FR-019/SC-013 | None — no new endpoints |
| Inactive status renders with `inactive` base role instead of `closed` | Status vocabulary conformance (FR-002) | None — purely presentational |
| Edit page no longer shows "not found" while loading | State-machine defect fix required by FR-005 | None — correct UX behavior |
| Group form labels drop "(20)/(200)" count hints | Canonical field pattern (FR-003) | None — limits unchanged server-side |
