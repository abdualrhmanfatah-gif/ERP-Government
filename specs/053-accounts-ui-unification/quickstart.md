# Quickstart: Validating the Accounts Management Migration

**Feature**: 053-accounts-ui-unification | **Plan**: [plan.md](./plan.md) | **Evidence log**: create `specs/053-accounts-ui-unification/review-evidence.md` from spec 052 `data-model.md` §6

## Prerequisites

- Same as spec 052 `quickstart.md`: .NET 10 + database available, `dotnet run --project src/AppHost`, frontend dependencies installed under `src/Web/ClientApp`.
- Sign in with the existing development account; the chart of accounts seed data includes groups and accounts with parent/child relations.

## Surfaces under validation

| Surface | Route | Recipe |
|---------|-------|--------|
| Accounts list | `/accounting/accounts` | list (tree-grid variation) |
| Account create | `/accounting/accounts/create` | form |
| Account edit | `/accounting/accounts/:id/edit` | form |
| Account detail (incl. sub-accounts) | `/accounting/accounts/:id` | detail |
| Account groups list | `/accounting/account-groups` | list |
| Group detail | `/accounting/account-groups/:id` | detail |
| Reference | `/__gallery__` (dev only) | component/status/density reference |

## Validation matrix (record every row)

Per screen, run all conditions and record result, issues, resolution, reviewer, and date: **light**, **dark**, **RTL**, **320 CSS px equivalent**, **400% zoom from 1280 CSS px**, **keyboard only**, **grayscale/forced-colors**. Contrast spot-check normal text (≥ 4.5:1), large text and non-text indicators (≥ 3:1) in both modes.

## Scenario walkthrough (real data flow)

1. **Accounts list**: confirm the single primary create action, compact filters, tree expand/collapse and arrow-key navigation; search a term that matches nothing → no-results message (distinct from the empty-dataset message); stop the API → persistent error + retry; confirm every status badge uses the active/inactive roles; confirm no undefined token styling (inspect computed borders/colors).
2. **Account create**: submit empty → field errors with values preserved; submit a duplicate `code` → exactly one notification and the form-level alert bound from the server rejection; cancel returns to the list without saving; confirm comfortable density and the save/cancel hierarchy.
3. **Account edit**: open with a slow/blocked request → loading state (never a premature "not found"); open a bogus id → not-found state with a return action; force an API failure → error state with retry; submit a stale `rowVersion` → conflict surfaces as a form-level error with no duplicate toast.
4. **Account detail**: identity = name, status in the toolbar, edit is the only primary action; details tab presents the account fields; sub-accounts tab lists direct children with working navigation, shows the explicit empty state for a leaf account, and never shows the empty message while loading or after an error.
5. **Groups list**: `FilterSearch` present (no raw input); inactive groups use the `inactive` role; a failed fetch shows the persistent page error with retry; pagination and totals still work; create/edit dialogs validate with bound field errors.
6. **Group detail**: `Page.onBack` replaces the custom back button; edit is primary and the enable/disable toggle is `outline` (activate) or `destructive` (deactivate) with the outcome-labeled confirmation; children/accounts sections use `EmptyState` when empty; the postable capability is not color-only.

## Automated gates (not acceptance evidence)

From `src/Web/ClientApp`:

- `npm run design:usage` — zero unknown `var(--color-*)` references in `features/accounting/**` and `components/Accounting*`.
- `npm run lint`, `npm run build`.
- `npm run generate-tokens` if any token source changes (none expected in this batch).
- `npm run design:lint` / `design:check` for DESIGN.md consistency.
- Backend regression (convergence/pre-merge): the five test projects once the working tree compiles (blocked pre-existing at Phase 1 time — re-check).

## Exit criteria

- All evidence rows filled or carrying an accepted, documented deviation; SC-001–SC-013 evidenced on real data.
- `design:usage` scope clean; no physical-direction utilities in touched files.
- `docs/ui-patterns.md` backlog statuses and spec 052 `scope-inventory.md` updated for this batch (FR-016).
