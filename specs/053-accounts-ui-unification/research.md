# Phase 0 Research: Accounts Management UI Unification (Phase 2)

**Date**: 2026-09-14 | **Plan**: [plan.md](./plan.md) | **Spec**: [spec.md](./spec.md)

Decisions are grounded in the current code; file references are evidence.

## 1. The accounts tree grid stays a justified list variation

- **Evidence**: `src/Web/ClientApp/src/components/AccountingAccountGrid.tsx` is a 349-line custom tree grid with WAI-ARIA `role="tree"`/`treeitem`, roving focus, expand/collapse, and Home/End/arrow keyboard navigation. It presents the chart-of-accounts hierarchy that a flat `DataGrid` cannot express.
- **Decision**: Keep the custom tree grid and standardize its shell only: status display, token references, loading/error/empty states, header colors, and focus styling. The `DataGrid` recipe remains the default for flat lists (account groups).
- **Rationale**: Replacing it with `DataGrid` would rebuild working keyboard accessibility with no user benefit; keeping it as-is leaves the exact inconsistencies this batch targets.
- **Alternatives considered**: (a) migrate to `DataGrid` with expandable rows — rejected: rebuild risk, loses tree semantics; (b) leave untouched — rejected: `Badge` status, undefined token vars, custom error UI, hard-coded white header.

## 2. Token and status fixes in the accounting components

- **Evidence**: `AccountingAccountGrid.tsx`, `AccountingAccountForm.tsx`, `AccountingAccountDetail.tsx` reference `--color-border-container` (never generated; the valid variable is `--color-container-border`). The grid renders active state with `Badge variant="success"` and a hard-coded `text-white` header, and uses `--color-secondary` for its focus outline.
- **Decision**: Rename to `--color-container-border`; replace status rendering with `StatusBadge variant="active"/"inactive"`; use `--color-on-primary` for the header; use `--color-focus-ring` for the focused row.
- **Rationale**: Phase 1 contract (`docs/ui-patterns.md`, `contracts/status-semantics.md`) and FR-002/FR-009.
- **Alternatives considered**: alias variables in the generator — rejected in Phase 1 as codifying drift.

## 3. Grid state scoping (loading / empty / no-results / error)

- **Evidence**: The grid returns `<Loading />` (grid-level, acceptable), an ad-hoc red error block with a `destructive` retry button, and a single `EmptyState message="لا توجد حسابات"` for both empty data and filter no-results.
- **Decision**: Keep grid-level loading; render `ErrorState` with an `outline` retry for failures; accept a page-computed `emptyMessage` prop (additive) so the page distinguishes "لا توجد حسابات" from "لا توجد نتائج مطابقة لمعايير البحث" (search is client-side, so the page owns the derivation).
- **Rationale**: FR-005 and the Phase 1 list recipe state rules.
- **Alternatives considered**: moving filter state into the grid — rejected: changes the component's contract and duplicates the page's filter logic.

## 4. Account form: shared schema + page-agnostic submit with shared error binding

- **Evidence**: `AccountingAccountForm.tsx` already uses RHF + Zod but declares its schema inline; its `onSubmit: (data) => void` leaves error surfacing to the pages, which show only toasts (`AccountCreatePage`, `AccountEditPage`) and cannot bind server field errors. The server returns duplicate-code failures without a field target (`CreateAccountCommand` → `Result.Failure(["Account code already exists."])`).
- **Decision**: Move the schema to `features/accounting/shared/schemas.ts`; change the form submit contract to `onSubmit: (cmd) => Promise<unknown>` + optional `onSuccess`, with the form itself calling `handleApiError(err, setError)` and rendering `errors.root` as a form-level `Alert`; group fields into labeled sections; add `onCancel` (ghost) so the form has primary save + secondary cancel per the recipe; keep the code field read-only on edit.
- **Rationale**: FR-003/FR-004, `docs/error-handling.md` (one notification owner, field-bound errors, form-level fallback), and the Phase 1 form recipe. Server duplicate-code failures without a target land in the visible root fallback by design.
- **Alternatives considered**: (a) pages keep `try/catch` — rejected: cannot bind field errors without exposing `setError`; (b) ref-based `setError` exposure — rejected: less explicit than the Phase 1 `PartyForm` pattern; (c) map duplicate-code text to the `code` field — rejected: classification must not match message text (Principle XIII).
- **Justified behavior addition**: the create/edit pages gain a cancel action per the approved form recipe (FR-006); submission payloads are unchanged.

## 5. Edit page state machine (loading vs not-found vs error)

- **Evidence**: `AccountEditPage` renders `EmptyState "الحساب غير موجود"` whenever `account` is falsy — including while loading and after a failed request — and ignores the query error.
- **Decision**: Use `isLoading` → `Page loading`; `error` → `Page error` + `refetch`; only a resolved empty result shows the not-found `EmptyState` with a return action.
- **Rationale**: FR-005; a not-found flash on slow or failed loads is a user-visible defect.
- **Alternatives considered**: redirect on not-found — rejected: hides the failure cause and changes navigation behavior.

## 6. Account detail per the detail recipe (with the sub-accounts view)

- **Evidence**: `AccountDetailPage` has an empty-title loading/error path, a secondary (`outline`) edit action, no status in the header, and a placeholder sub-accounts tab; `AccountDto` already carries `parentId` and `level`, and the accounts query already fetches all accounts.
- **Decision**: Apply the detail recipe: identity = account name, code in the toolbar; `MetaItem` toolbar with `StatusBadge` (active/inactive), group, level, normal balance; edit as the single primary action; `Page.onBack` to the list; details tab uses the refreshed `AccountingAccountDetail`; the sub-accounts tab lists direct children derived client-side from `useAccountsList()` (`parentId === account.id`, sorted by code) in a typed `DataGrid` with navigation, its own `EmptyState` ("لا توجد حسابات فرعية"), and loading/error states from that query.
- **Rationale**: FR-006/FR-019, clarified answers, and the detail recipe. No API change: the accounts query already returns all accounts with `parentId`/`level`.
- **Alternatives considered**: (a) new backend query for children — rejected: out of scope, unnecessary; (b) full subtree expansion — deferred by clarification.

## 7. Groups list: shared filter control and real error state

- **Evidence**: `AccountGroupsListPage` embeds a raw `Input` in `FilterBar`, renders fetch failure as a plain `<p>خطأ في التحميل</p>` with no retry, uses `StatusBadge variant="closed"` for inactive groups, and its create button relies on the deprecated `default` variant.
- **Decision**: Use `FilterSearch` (compact density); pass `error`/`onRetry` to `Page` for a persistent retry state; map inactive to `StatusBadge variant="inactive"`; make the create button explicitly `variant="primary" size="sm"` with the icon pattern.
- **Rationale**: FR-001/FR-002/FR-005; keeps tree/grid modes and pagination untouched.
- **Alternatives considered**: keep the raw input — rejected: the batch's whole purpose is recipe conformance.

## 8. Group detail: actions, status, and empty states

- **Evidence**: `AccountGroupDetailPage` has a custom back button beside edit/toggle, `StatusBadge variant="closed"` for inactive, a `Badge` for the postable capability, ad-hoc `<p>` empty states for child groups/accounts, a `default`-variant toggle button, and an `if (!data)` loading path that ignores errors.
- **Decision**: Move back navigation to `Page.onBack`; make edit the explicit `primary` action; render the toggle as `outline` when activating and `destructive` when deactivating (with `ConfirmDialog`); map inactive to the `inactive` role; render the postable capability as an icon + text (Check/X) rather than a color badge; use `StatusBadge`/`EmptyState` consistently; handle loading/error through `Page` and `EmptyState`.
- **Rationale**: FR-002/FR-005/FR-006 and the Phase 1 detail recipe; keeps the clarified "edit = primary on detail" decision.
- **Alternatives considered**: keep dual `outline` back/edit — rejected: two competing secondary actions next to the primary edit.

## 9. Group form dialogs stay (clarified) with shared error binding

- **Evidence**: `AccountingAccountGroupForm.tsx` already uses RHF + Zod and is rendered by both list and detail pages through `open/onOpenChange`.
- **Decision**: Keep the dialog surface and props; ensure canonical field composition and route server errors through `handleApiError` where the dialog is used, preserving the current `onSubmit` contract for existing callers.
- **Rationale**: Clarified answer + FR-003; minimal contract churn.
- **Alternatives considered**: convert to pages — rejected by clarification.

## 10. Evidence, backlog, and scope guardrails

- **Decision**: Create `specs/053-accounts-ui-unification/review-evidence.md` with the Phase 1 schema for the six screens × seven conditions; update `docs/ui-patterns.md` backlog rows resolved by this batch and append a Phase-2-batch note to spec 052's deferred inventory; run `npm run design:usage` scoped to the accounting pages/components and require zero unknown references.
- **Rationale**: FR-015/FR-016; without a recorded artifact the migration is unverifiable, and the backlog must not silently rot.
- **Non-decisions**: no journal/template/recurring screens; no endpoint, DTO, permission, or business-rule changes; no new tokens or shared components; no pagination or server-side search for the accounts list.
