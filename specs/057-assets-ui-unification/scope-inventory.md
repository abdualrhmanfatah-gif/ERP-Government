# Phase 3 Scope Inventory — 057 Assets Management UI Unification

Per `data-model.md` §6. Created 2026-09-15.

## Baseline gates (batch start)

| Gate | Result | Notes |
|------|--------|-------|
| `npm run lint` | 141 problems (118 errors, 23 warnings) | inherited baseline from spec 053 |
| `npm run build` | pass | Vite production build |
| `npm run design:usage` | 30 unknown refs repo-wide, 0 in `features/assets` | pre-existing refs outside scope |
| Palette/direction scans | introduced by this batch | no prior baseline (assets code was uncommitted/new) |

## Resolved inconsistencies (Phase 3 batch)

| # | Inconsistency | File(s) | Resolution |
|---|---------------|---------|------------|
| 1 | Lifecycle status rendered as `Badge variant="success/secondary"` | `AssetGroupsListPage.tsx`, `AssetGroupDetailPage.tsx` | `StatusBadge` via the feature base-role map (`getActiveBadge`) |
| 2 | Asset status rendered as a raw blue `<span>` | `AssetsListPage.tsx` | `StatusBadge` via `getAssetStatusBadge` (map + fallback) |
| 3 | No status→base-role mapping | `features/assets/shared/status.ts` (new) | Map + unknown-value fallback added |
| 4 | No loading/empty/error states on the assets list; no error state at all | `AssetsListPage.tsx` | `Page`/grid loading, `EmptyState` dataset vs no-results, normalized error + retry |
| 5 | Plain-text empty states on the groups list; unreachable double-skeleton path | `AssetGroupsListPage.tsx` | `EmptyState` variants; page-level loading only |
| 6 | Forms surfaced no server errors (asset) or ad-hoc `serverError` divs (groups) | `AssetForm.tsx`, `AssetGroupCreatePage.tsx`, `AssetGroupEditPage.tsx` | `handleApiError` + root `Alert` bound to `errors.root` |
| 7 | Native inputs/selects/textareas in the asset form | `AssetForm.tsx` | Shared `Input`/`Select`/`Textarea` on the canonical field pattern |
| 8 | Edit mode validated with the create schema; `updateAssetSchema` unused | `AssetForm.tsx` | Per-mode schemas; edit seeds `rowVersion` from the loaded record |
| 9 | Read-only asset detail offered an operative no-op save | `AssetDetailPage.tsx`, `AssetForm.tsx` | Read-only mode renders no action bar/submit; edit is the page's single primary action |
| 10 | Bespoke inline deactivate modal | `AssetDetailPage.tsx` | Shared `ConfirmDialog` + `handleLifecycleError` |
| 11 | Custom back buttons/breadcrumbs and raw action buttons | `AssetDetailPage.tsx`, `AssetEditPage.tsx` | `Page.onBack`, shared `Button` variants, primary edit |
| 12 | Hard-coded palette classes (`bg-blue-*`, `bg-gray-*`, `bg-red-*`, `ring-blue-*`) | Assets pages + form | Replaced with shared components/semantic tokens; palette scan returns zero |
| 13 | Physical `text-right` utilities | `AssetsListPage.tsx`, `AssetGroupsListPage.tsx` | Removed (logical properties / shared grid); direction scan returns zero |
| 14 | Asset code not direction-isolated | `AssetsListPage.tsx` | `dir="ltr"` + tabular numerals |
| 15 | Dead active-filter state with no control | `AssetGroupsListPage.tsx` | Wired to `FilterSelect` |
| 16 | Duplicate `Active` option in the status filter | `AssetsListPage.tsx` | Single option per status + explicit all-values option |
| 17 | Custom prev/next pagination with `items.length < pageSize` heuristic | `AssetsListPage.tsx` | Shared grid pagination + `Pagination` with API totals |
| 18 | Raw tables without typed columns (both lists) | `AssetsListPage.tsx`, `AssetGroupsListPage.tsx` | Typed `DataGrid` (assets list and groups grid view) |
| 19 | Group activate used `variant="success"`; edit was `outline` | `AssetGroupDetailPage.tsx` | Edit `primary`; activate `outline` / deactivate `destructive` |
| 20 | Corrupted Arabic strings (`البيانات الت识别ية`, `نسبة القيمة التخليضية`) | `AssetForm.tsx`, group pages | Corrected |
| 21 | Navigation permission identifier `Assets.Read` not defined | `layouts/navigation.ts` | `Assets.View` (matches the route guard) |
| 22 | Toggle failures escaped as unhandled rejections | `AssetGroupsListPage.tsx`, `AssetGroupDetailPage.tsx` | `handleLifecycleError` on failure |
| 23 | `Switch` track invisible: `data-checked:`/`data-unchecked:` classes use Tailwind v4 syntax under Tailwind v3, so no CSS was generated | `components/ui/Switch.tsx` (consumed by the group forms) | Rewritten to v3-arbitrary variants `data-[checked]`/`data-[unchecked]`/`data-[disabled]`; verified the built CSS now contains the selectors; fixes every Switch consumer |
| 24 | `Tabs` had no controlled mode, so validation errors in a hidden tab could not be surfaced | `components/ui/Tabs.tsx` | Additive optional `value` prop (uncontrolled usage unchanged); create/edit switch to the tab holding the first validation error |
| 25 | Labels and layout diverged between the group create/edit/detail pages (كود المجموعة vs الكود، معلمات الأصول vs معلمات الإهلاك، units in values vs labels، الفئة placement، page widths) | Three group pages | Unified: identical field labels across list/detail/form (units in labels, plain read-only values)، الفئة in the always-visible card، one tab name معلمات الإهلاك، `maxWidth xl` everywhere |
| 26 | Tree view flat and washed out: no column header, no row separators, whole-row opacity for inactive groups, RTL chevron pointing the wrong way, hover-only row action | `AssetGroupsListPage.tsx` (tree mode) | `Card` shell with a `primary` header row matching `DataGrid`, fixed code/status/action columns, row separators, full-opacity rows (status carried by `StatusBadge`), RTL chevrons (down/left), always-visible actions |

## Deferred inconsistencies (out of this batch's scope)

| # | Inconsistency | Evidence | Reason deferred |
|---|---------------|----------|-----------------|
| 1 | Non-accounting/non-asset `--color-border-*` refs (30) | token scan | Pages outside validated scope |
| 2 | In-page permission gating absent on asset screens | route guards only | Separate security concern (constitution exception #4 placeholder) |
| 3 | Dormant form option lists (categories, locations, custodians, cost centers, funds) | `AssetForm` props never populated | Would require new queries/endpoints (FR-018) |
| 4 | Remaining `docs/ui-patterns.md` backlog items not touching these screens | backlog table | Out of batch scope |
| 5 | Pre-existing ESLint problems across the app (132) | `npm run lint` | Not caused by this feature; improved from 141 |
| 6 | Asset acquisition/activation (spec 056) and other asset sub-features | specs/056, plan/feature-context-*.md | Explicitly out of scope |
| 7 | `useAssets`/`useAssetGroups` mutation payloads typed as `Record<string, unknown>` | hooks | Type-hygiene refactor outside the visual-unification scope |

## Behavior changes register

| Change | Justification | Risk |
|--------|---------------|------|
| Palette styling → tokens/shared components | Principle X; FR-009 | None — visual only |
| Assets list pagination through the shared grid; heuristic removed | List recipe; fixes hidden full last page | Low — totals now API-driven |
| Duplicate status-filter option removed | FR-019 | None — default `Active` behavior preserved |
| Groups list active filter functional | FR-019 (no dead controls) | Low — uses existing state/params |
| Status rendering via the base-role map with fallback | FR-002; status vocabulary | None — presentational |
| Read-only detail no longer offers a no-op save | FR-006; defect fix | None — no payload involved |
| Asset deactivate uses the shared `ConfirmDialog` (copy preserved) | FR-006 | None — same action/gate |
| Group activate toggle uses `outline` (not `success`) | FR-006 hierarchy | None — presentational |
| Error presentation uses normalized shared messages + retry | Principle XIII; FR-005 | None — safer messages |
| `/assets` navigation permission `Assets.Read` → `Assets.View` | FR-020; matches the route guard | Low — navigation visibility aligns with the guard |
| Corrupted Arabic strings corrected | FR-019 | None |
| Optional numeric fields map empty input to `undefined` (was `NaN`) | Canonical validation feedback; latent defect | Low — optional fields now validate/behave as intended |
| Required numeric fields carry explicit Arabic required messages | Canonical validation feedback | None — message wording |
| Assets-list original value renders via `MoneyDisplay` (no hard-coded `SAR` suffix) | Money-display convention (FR-009/Principle X) | Low — formatting only |
| Group detail reorganized into a read-only basic-info form with tabbed sections | Requested professional presentation (2026-09-15): status/category moved to the page header, fields in one adaptive content-proportional row (compact height), sections in tabs below | None — presentational; same data, actions, and gates |
| Sub-groups tab added to the group detail | Requested (2026-09-15): direct child groups were not displayed; derived from the existing list query with the `parentId` filter (no new endpoint, no backend change) | Low — one extra cached list query scoped to the group's children |
| Group create/edit reorganized to the identity-header + adaptive-row + tabbed-sections pattern | Requested (2026-09-15): same professional pattern as the detail; documented in `docs/ui-patterns.md` | Low — presentational; values survive tab switches (RHF keeps unmounted values) |
