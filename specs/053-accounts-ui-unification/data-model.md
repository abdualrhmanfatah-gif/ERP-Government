# Phase 1 Data Model: Accounts Management UI Unification (Phase 2)

**Date**: 2026-09-14 | **Plan**: [plan.md](./plan.md) | **Spec**: [spec.md](./spec.md)

No database or API data model changes. The entities below are the conceptual structures this batch must satisfy; concrete per-screen rules live in `contracts/screen-contract.md`.

## 1. MigratedScreen

| Field | Description | Validation |
|-------|-------------|------------|
| `id` | `accounts-list` \| `account-create` \| `account-edit` \| `account-detail` \| `groups-list` \| `group-detail` | Exactly the six in-scope screens (FR-017) |
| `path` | Real route (`/accounting/accounts`, `/accounting/accounts/create`, `/accounting/accounts/:id/edit`, `/accounting/accounts/:id`, `/accounting/account-groups`, `/accounting/account-groups/:id`) | Matches `routes.tsx` |
| `recipe` | `list` \| `form` \| `detail` (spec 052 `contracts/page-recipes.md`) | Every screen maps to one recipe |
| `components` | Feature pages + shared `Accounting*` components touched | Preserves public props for other consumers |
| `primaryAction` | The single primary action per context | Edit on both detail screens (clarified); create on both lists |
| `statusRoles` | Lifecycle states rendered and their base role | `active` / `inactive` via `StatusBadge` |
| `states` | loading / empty / no-results / not-found / error handling owners | All distinguishable (FR-005) |
| `evidenceRef` | Rows in `review-evidence.md` | One row per condition |

Screen map:

| Screen | Recipe | Primary action | Notes |
|--------|--------|----------------|-------|
| Accounts list | list (tree-grid variation) | إنشاء حساب (`primary sm`) | Client-side search; no pagination; empty vs no-results by page |
| Account create | form | حفظ (`primary`) | Cancel added per recipe; code required |
| Account edit | form | حفظ (`primary`) | Code read-only; loading/not-found/error separated |
| Account detail | detail | تعديل (`primary`) | Toolbar: status + meta; tabs: details + sub-accounts |
| Groups list | list | إنشاء مجموعة (`primary sm`) | `FilterSearch`; tree or grid mode; server pagination kept |
| Group detail | detail | تعديل (`primary`) | `Page.onBack`; toggle `outline`/`destructive` + confirm |

## 2. AccountFormSchema (feature boundary)

Source of truth: `src/Web/ClientApp/src/features/accounting/shared/schemas.ts` (new).

| Field | Type | Rule |
|-------|------|------|
| `code` | string | required; read-only when editing |
| `name` | string | required |
| `description` | string? | optional |
| `accountGroupId` | number | required (≥ 1) |
| `parentId` | number? | optional (null = root) |
| `normalBalance` | number (0 debit / 1 credit) | required |
| `isPostable` | boolean | default true |
| `isReconcilable` | boolean | default false |
| `currencyId` | number? | optional |

Mapping to `CreateAccountCommand`/update payload stays unchanged (FR-010): empty strings/ids normalize to `undefined`/`null` exactly as today.

## 3. AccountGroupFormSchema (feature boundary, moved from the component)

| Field | Type | Rule |
|-------|------|------|
| `code` | string | required, max 20; read-only when editing |
| `name` | string | required, max 200 |
| `type` | enum Asset/Liability/Equity/Revenue/Expense | required |
| `normalBalance` | enum Debit/Credit | required; must match `type` (cross-field rule kept) |
| `description` | string? | optional, max 500 |
| `parentId` | number? | optional (null = root); excludes self |

## 4. SubAccountsView

| Field | Description | Validation |
|-------|-------------|------------|
| `sourceQuery` | Existing `useAccountsList()` (fetch-all) | No new endpoint or query key |
| `derivation` | `accounts.filter(a => a.parentId === account.id)`, sorted by `code` | Direct children only (clarified) |
| `columns` | code, name, group, state | Typed rows; identifiers LTR-isolated |
| `states` | loading (from query), `EmptyState "لا توجد حسابات فرعية"`, error (persistent + retry) | Failure never renders the empty message (SC-013) |
| `navigation` | Row click / view action → `/accounting/accounts/:childId` | Same pattern as other lists |

## 5. ReviewEvidenceRecord

Identical to spec 052 `data-model.md` §6: one row per interface × condition (light, dark, RTL, 320-css-px, 400%-zoom, keyboard, grayscale/forced-colors) with result, issue, resolution, reviewer, date. Committed as `specs/053-accounts-ui-unification/review-evidence.md`.

## 6. BatchScopeInventory

Extends spec 052 `scope-inventory.md`:

| Field | Description | Validation |
|-------|-------------|------------|
| `resolved` | Inconsistencies fixed in this batch (undefined token refs, status misuse, error/empty scoping, form error binding, action hierarchy, edit-page state machine, group filter control, sub-accounts view) | Each with evidence reference |
| `deferred` | Out-of-scope findings from these screens (e.g., deeper sub-account tree expansion, non-accounting token refs) | Recorded, not silently fixed |
| `behaviorChanges` | Explicit list: cancel action added to account form; nothing else | Each justified by a requirement |

## Relationships

- A **MigratedScreen** consumes the Phase 1 recipes and selects **AccountFormSchema**/**AccountGroupFormSchema** or **SubAccountsView** accordingly; the **BatchScopeInventory** references resolved/deferred items; the **ReviewEvidenceRecord** proves each screen against SC-001–SC-011.
