# Visual & Interaction Review Evidence — 057 Assets Management UI Unification (Phase 3)

**Purpose**: structured review log required by FR-015. One row per interface × condition, per `data-model.md` §5.

**Conditions**: light, dark, RTL, 320-css-px, 400%-zoom, keyboard, grayscale/forced-colors.

**Result values**: `pass` | `fail` | `pending-visual` (static checks done, human visual confirmation required) | `n/a` (reason required).

**Reviewer/date**: each row must identify who confirmed the result and when. `static:*` entries mean verified by code inspection/automated gate, not by eye.

## Assets list — `/assets`

**Static verification (2026-09-15, implementation pass)**: `Page` recipe (title/description + one primary create `sm` + icon); `FilterBar` with `FilterSearch` + status `FilterSelect` (duplicate `Active` option removed); typed `DataGrid` with row activation, `StatusBadge` via the base-role map, `MoneyDisplay`, `dir="ltr"` identifiers; server pagination mapped through the shared grid/pagination with API totals (next-page heuristic removed); dataset-empty vs no-results messages; normalized error + `refetch`; palette classes and `text-right` removed. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Asset create form — `/assets/create`

**Static verification (2026-09-15, implementation pass)**: `Page` (`maxWidth md`, `onBack`); shared `Input`/`Select`/`Textarea` on the canonical field pattern; `Card` sections (basic / financial / identification / acquisition); `createAssetSchema` validation; promise submit + `handleApiError` with root `Alert`; cancel (ghost) + primary save with submit loading; no toasts. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Asset edit form — `/assets/:id/edit`

**Static verification (2026-09-15, implementation pass)**: loading/not-found/error state machine separated (`Page loading` → `Page error` + `refetch` → `EmptyState` only after a resolved miss); `updateAssetSchema` for edit mode with the concurrency token seeded from the loaded record; payload preserves `rowVersion`; cancel returns to detail; custom breadcrumb/header and palette classes removed. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Asset detail — `/assets/:id`

**Static verification (2026-09-15, implementation pass)**: detail recipe applied — title = asset name, `Page.onBack`, toolbar with `StatusBadge` (base-role map) + meta (group, purchase date, original value via `MoneyDisplay`); edit is the single primary action; deactivate is destructive, status-gated as before, and confirmed via the shared `ConfirmDialog` with `handleLifecycleError` on failure; read-only `AssetForm` renders no action bar and no operative submit; states through `Page`; bespoke modal and palette classes removed. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Asset groups list — `/assets/asset-groups`

**Static verification (2026-09-15, implementation pass)**: active filter wired to `FilterSelect` (no dead state); `StatusBadge` active/inactive via the status helper in tree and grid views (category stays metadata `Badge`); grid view is a typed `DataGrid`; the tree view is a `Card` with a primary-colored header row (same colors as `DataGrid`), fixed code/status/action columns, logical indentation, RTL-correct chevrons (down = expanded, left = collapsed), always-visible row actions, and row separators; unreachable double-skeleton loading path removed (page-level loading); `EmptyState` dataset-empty vs no-results; normalized error + `refetch`; view switcher uses shared `Button` variants with `aria-pressed`; lifecycle toggle failures use `handleLifecycleError`. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Asset group create — `/assets/asset-groups/create`

**Static verification (2026-09-15, implementation pass; redesign 2026-09-15)**: `Page` (`maxWidth lg`, `onBack`); basic-info `Card` with fields in one adaptive content-proportional row; `Tabs` for معلمات الأصول / الحسابات المحاسبية (icons) with the canonical controls and `Switch` inside; root `Alert` + `handleApiError`; required markers; invalid submit switches to the tab holding the first error; cancel (ghost) + primary save with loading; corrupted string corrected. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Asset group edit — `/assets/asset-groups/:id/edit`

**Static verification (2026-09-15, implementation pass; redesign 2026-09-15)**: loading/not-found/error state machine separated; header shows code + `StatusBadge`; basic-info `Card` with one adaptive field row; `Tabs` for معلمات الأصول / الحسابات المحاسبية (icons); root `Alert` + `handleApiError`; invalid submit switches to the tab holding the first error; `rowVersion` preserved in the update payload; corrupted string corrected; cancel returns to detail. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Asset group detail — `/assets/asset-groups/:id`

**Static verification (2026-09-15, implementation pass; redesign 2026-09-15)**: `Page.onBack`; header `StatusBadge` active/inactive; edit is primary; activate is `outline` / deactivate is `destructive`, both confirmed via `ConfirmDialog` with pending state and `handleLifecycleError`; normalized query error + retry; not-found `EmptyState`; corrupted string corrected. **Redesign**: status and category shown in the page header under the name; المعلومات الأساسية rendered as a read-only `Card` with its fields in one adaptive flex row (content-proportional widths: narrow code, flexible name/parent/description, compact `h-[var(--density-compact-control-height)]` value boxes with truncation + hover title); sections moved into `Tabs` below the form (المجموعات الفرعية with count / معلمات الإهلاك / الحسابات المحاسبية / سجل التدقيق) with icons and the same adaptive row presentation for field-based tabs; سجل التدقيق renders through `DataGrid` (الإجراء / المستخدم / التاريخ); قابل للإهلاك moved into the depreciation tab and labels now match the form labels with units (العمر الإنتاجي (سنوات) / نسبة القيمة التخريدية (%) / معدل الإهلاك (%)) while read-only values stay plain. The sub-groups tab lists direct children from the existing list query (`parentId` filter, no new endpoint) as a typed `DataGrid` with navigation, loading/error/empty states, and lifecycle badges. Human visual confirmation below is still required.

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
| Token-reference scan (batch scope) | `npm run design:usage` | pass in scope | 0 unknown refs in `features/assets` (30 pre-existing refs remain outside scope) |
| Palette scan | `rg "bg-(blue\|gray\|red\|slate\|green)-…"` | pass | 0 matches in `features/assets` |
| Direction scan | `rg "text-right\|text-left\|ml-\|mr-\|pl-\|pr-\|left-\|right-"` | pass | 0 matches in `features/assets` |
| Corrupted-text scan | `rg "الت识别ية\|التخليضية"` | pass | 0 matches in `features/assets` |
| Lint (scoped) | `npx eslint src/features/assets` | pass | 0 problems |
| Switch state styling | `rg "\[data-(checked\|unchecked\|disabled)\]" build/assets/*.css` | pass | selectors present after the v3-variant fix (was absent — invisible track) |
| Lint (repo) | `npm run lint` | pass | 132 problems (111 errors, 21 warnings) — improved from the 141 baseline; no new problems |
| Build | `npm run build` | pass | Vite production build (~6s) |
| DESIGN sync | `npm run design:lint` | pass | 0 errors (23 warnings, 1 info — unchanged tier) |
| DESIGN diff | `npm run design:check` | pass | no regression |
| Backend regression (convergence) | `dotnet test tests/Domain.UnitTests` | **blocked — pre-existing** | Same blocked state as Phase 1/2 (unrelated in-flight backend work); this feature changes no backend code |
