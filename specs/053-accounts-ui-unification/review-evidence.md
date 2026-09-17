# Visual & Interaction Review Evidence — 053 Accounts Management UI Unification (Phase 2)

**Purpose**: structured review log required by FR-015. One row per interface × condition, per `data-model.md` §5.

**Conditions**: light, dark, RTL, 320-css-px, 400%-zoom, keyboard, grayscale/forced-colors.

**Result values**: `pass` | `fail` | `pending-visual` (static checks done, human visual confirmation required) | `n/a` (reason required).

**Reviewer/date**: each row must identify who confirmed the result and when. `static:*` entries mean verified by code inspection/automated gate, not by eye.

## Accounts list — `/accounting/accounts`

**Static verification (2026-09-14, implementation pass)**: tree grid uses `StatusBadge variant="active"/"inactive"` instead of `Badge`; header uses `text-[var(--color-on-primary)]`; focused row uses `--color-focus-ring` outline; `ErrorState` + outline retry replaces ad-hoc error block; `emptyMessage` prop added for the list page to distinguish empty vs no-results; compact filter controls; `FilterSearch` replaces raw input. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Account create form — `/accounting/accounts/create`

**Static verification (2026-09-14, implementation pass)**: shared schema from `features/accounting/shared/schemas.ts`; three labeled sections (basic / classification / options); submit contract `onSubmit(values) => Promise<unknown>` + `onSuccess`; `handleApiError` with root `Alert` fallback; cancel action (ghost) added; code required; comfortable density. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Account edit form — `/accounting/accounts/:id/edit`

**Static verification (2026-09-14, implementation pass)**: edit mode renders the same shared `AccountForm` with `initialData`; code read-only; loading/not-found/error state machine separated (`Page loading` → `EmptyState` only after resolved miss → `Page error` + retry); submission payload preserves `rowVersion`. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Account detail — `/accounting/accounts/:id`

**Static verification (2026-09-14, implementation pass)**: detail recipe applied — title = account name; `Page.onBack`; toolbar with `StatusBadge` + `MetaItem`s (code/group/level/balance); edit as single primary action; details tab with field grid; sub-accounts tab from `useAccountsList()` with typed `DataGrid`, empty/loading/error states. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Account groups list — `/accounting/account-groups`

**Static verification (2026-09-14, implementation pass)**: `FilterSearch` replaces raw `Input`; `StatusBadge variant="inactive"` for inactive groups; `Page error` + retry instead of inline error text; primary create button with icon pattern; tree/grid modes preserved; server pagination kept. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Account group detail — `/accounting/account-groups/:id`

**Static verification (2026-09-14, implementation pass)**: `Page.onBack` replaces custom back button; `StatusBadge` active/inactive role; edit as primary action; outline/destructive toggle with `ConfirmDialog`; postable capability as icon + text; `EmptyState` for empty children/accounts; loading/error through `Page`. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

---

## Static checks (not a substitute for visual review)

| Check | Command | Result | Notes |
|-------|---------|--------|-------|
| Token generator | `npm run generate-tokens` | pass | no new tokens in this batch |
| Token-reference scan (batch scope) | `npm run design:usage` | pass in scope | 0 unknown refs in the touched accounting files |
| Lint | `npm run lint` | pass | no new problems added |
| Build | `npm run build` | pass | Vite production build |
| DESIGN sync | `npm run design:lint` | pass | no new errors |
| DESIGN diff | `npm run design:check` | pass | no regression |
| Backend regression (convergence) | `dotnet test tests/Domain.UnitTests` | **blocked — pre-existing** | Same blocked state as Phase 1; this feature changes no backend code |
