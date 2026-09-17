# Contract: Accounts Management Screens (Phase 2)

**Feature**: 053-accounts-ui-unification | **Consumes**: spec 052 contracts (`design-token`, `component`, `status-semantics`, `page-recipes`)

## 1. Per-screen contract

| Screen | Recipe slots | Action rule | Status rule | State rule |
|--------|--------------|-------------|-------------|------------|
| Accounts list | Page → title + description → primary create → FilterBar (search, group, active, postable) → tree grid → (no pagination) | Exactly one primary: إنشاء حساب (`primary sm` + icon) | `StatusBadge variant="active"/"inactive"` per row | loading = grid-level; empty = `"لا توجد حسابات"`; filtered-empty = `"لا توجد نتائج مطابقة لمعايير البحث"`; error = `ErrorState` + outline retry |
| Account create | Page (`maxWidth sm`) → Card → sections (basic / classification / options) → actions | حفظ primary; إلغاء ghost | n/a | field errors bound; root `Alert` for untargeted server failures; values preserved; submit disables actions |
| Account edit | Same as create; code read-only | حفظ primary; إلغاء ghost | n/a | `Page loading` while fetching; `Page error` + retry on failure; not-found `EmptyState` only after a resolved miss |
| Account detail | Page (`maxWidth lg`) → title (name) + `Page.onBack` → toolbar (`StatusBadge` + code/group/level/balance meta) → tabs | تعديل primary (single) | `StatusBadge active/inactive` in toolbar | loading/error through `Page`; each tab owns its own empty/error state |
| Groups list | Page → title + description → primary create → FilterBar (`FilterSearch`, type, active) → tree or typed grid → pagination + total | Exactly one primary: إنشاء مجموعة (`primary sm` + icon); row actions ghost/icon | `StatusBadge variant="active"/"inactive"` | `Page error` + retry instead of inline error text; empty vs no-results distinguished; loading at grid scope |
| Group detail | Page → title (`code — name`) + `Page.onBack` → actions → basic-info Card → children Card → accounts Card | تعديل primary; toggle outline when activating / destructive when deactivating (both with `ConfirmDialog`) | `StatusBadge active/inactive`; postable capability rendered as icon + text, never color-only | loading/error through `Page`; children/accounts empty states use `EmptyState` |

## 2. Sub-accounts contract

- **Source**: existing accounts query (`useAccountsList`), no new endpoint/query key.
- **Derivation**: direct children of the current account (`parentId === account.id`), sorted by `code`, inactive children included with their status badge.
- **Presentation**: typed `DataGrid` rows (code, name, group, state) + navigation to the child detail; compact density.
- **States**: loading while the query runs; `EmptyState "لا توجد حسابات فرعية"` only for a resolved empty result; persistent error + retry on failure.
- **Out of scope**: recursive tree expansion, balances, or posting information in this view.

## 3. Form contract

- Schemas live at `features/accounting/shared/schemas.ts`; components import them (no inline schemas).
- Submit signature: `onSubmit(values) => Promise<unknown>` with optional `onSuccess(result)`; the form catches rejections and calls `handleApiError(err, setError)`.
- Untargeted server failures (including duplicate account code) render in the form-level `Alert` bound to `errors.root`; no message-text classification.
- One notification owner per failed action; pages never also toast the same failure.
- Account-group dialogs keep their `open/onOpenChange/initial/onSubmit/isPending` props; validation and error handling align inside.

## 4. Behavior-change register

| Change | Justification |
|--------|---------------|
| Cancel action added to the account create/edit form | Form recipe requires a secondary cancel (FR-006); payloads and submission behavior unchanged |
| Accounts list no-results message differentiates from empty | List recipe state rule (FR-005); no data behavior change |
| Account detail edit becomes the single primary action | Clarified answer for FR-006/SC-002; navigation unchanged |
| Sub-accounts tab built (direct children from existing data) | Clarified answer for FR-019/SC-013; no new endpoints or business rules |
| Inactive status renders with the `inactive` base role instead of `closed`/`success` | Status vocabulary conformance (FR-002); purely presentational |
| Edit page no longer shows "not found" while loading or after a failed load | State-machine defect fix required by FR-005 |
| Account group form labels drop the "(20)/(200)" count hints and the double-spaced label text | Canonical field pattern / label consistency (FR-003); no validation change (limits stay) |

## 5. Forbidden patterns in this batch

1. `Badge` for lifecycle state; `closed` used for inactive.
2. Undefined `var(--color-*)` references or hard-coded colors in the touched files.
3. Physical direction utilities in touched files.
4. Toast-only server errors on the forms (field or root binding required).
5. New tokens, new shared components, new routes, or API changes.
6. Restyling or migrating screens outside the six (journal entries, journals, templates, recurring entries).
