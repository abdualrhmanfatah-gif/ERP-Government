# Contract: Assets Management Screens (Phase 3)

**Feature**: 057-assets-ui-unification | **Consumes**: spec 052 contracts (`design-token`, `component`, `status-semantics`, `page-recipes`)

## 1. Per-screen contract

| Screen | Recipe slots | Action rule | Status rule | State rule |
|--------|--------------|-------------|-------------|------------|
| Assets list | Page → title + description → primary create → FilterBar (search, status) → `DataGrid` → server pagination + total | Exactly one primary: إنشاء أصل (`primary sm` + icon) | `StatusBadge` with the base-role map (R1) per row | loading at grid scope; dataset-empty vs no-results distinguished; error = persistent `ErrorState` + outline retry |
| Asset create | Page (`maxWidth md`) → `Card` sections (basic / classification / financial / options) → actions | حفظ primary; إلغاء ghost | Status shown only per create rules; never a color-only signal | field errors bound; root `Alert` for untargeted server failures; values preserved; submit disables actions |
| Asset edit | Same form; identity fields per server rules | حفظ primary; إلغاء ghost | Locked status renders through `StatusBadge`, editable status through `Select` | `Page loading`; `Page error` + refetch; not-found `EmptyState` only after a resolved miss |
| Asset detail | Page (`maxWidth md`) → title (name) + `Page.onBack` → toolbar (`StatusBadge` + code/group/date meta) → read-only form sections | تعديل primary (single); تعطيل destructive + `ConfirmDialog` (status-gated as today) | `StatusBadge` in toolbar via the base-role map | loading/error/not-found through `Page`; read-only presentation exposes no operative submit |
| Groups list | Page → title + description → primary create → FilterBar (`FilterSearch`, type, active) → tree or typed `DataGrid` → pagination + total | Exactly one primary: إنشاء مجموعة (`primary sm` + icon); row actions ghost/icon | `StatusBadge variant="active"/"inactive"` | `Page error` + refetch; empty vs no-results distinguished; loading at page/list scope (no double skeleton) |
| Group create | Page (`maxWidth lg`) → basic-info adaptive-row `Card` → `Tabs` (معلمات الأصول / الحسابات المحاسبية with icons) → actions | حفظ primary; إلغاء ghost at the bottom | n/a | field errors bound; root `Alert`; values preserved; invalid submit switches to the tab holding the first error |
| Group edit | Same as create; code read-only + status `StatusBadge` in the header | حفظ primary; إلغاء ghost | `StatusBadge` active/inactive in the header (read-only context) | `Page loading`; `Page error` + refetch; not-found `EmptyState` only after a resolved miss; `rowVersion` round-trips |
| Group detail | Page → header (name + code / `StatusBadge` / category `Badge`) + `Page.onBack` → actions → basic-info read-only `Card` (adaptive single-row fields, content-proportional widths) → `Tabs` (المجموعات الفرعية / معلمات الإهلاك / الحسابات المحاسبية / سجل التدقيق with icons) | تعديل primary; toggle `outline` when activating / `destructive` when deactivating (both `ConfirmDialog`) | `StatusBadge variant="active"/"inactive"` and category metadata `Badge` in the page header (never lifecycle via generic `Badge`) | loading/error through `Page`; sub-groups derived from the existing list query (`parentId` filter, no new endpoint); tab fields use the read-only presentation; the audit log renders through `DataGrid`; toggle failures via `handleLifecycleError` |

## 2. Status contract

- Asset lifecycle map: `Draft`→`draft`, `Active`→`active`, `UnderMaintenance`→`inactive`, `Disposed`→`closed`, `WrittenOff`→`closed`; unknown → `draft` fallback with the raw label.
- Group lifecycle: `isActive` → `active`/`inactive`.
- Generic `Badge` remains allowed only for non-lifecycle metadata (e.g., asset category).
- No new `StatusBadge` variants; labels always accompany color.

## 3. List and pagination contract

- Assets list keeps the existing server-side `page`/`pageSize`/`totalCount` contract; the shared grid's pagination and the shared `Pagination` replace the custom prev/next controls and the `items.length < pageSize` heuristic; totals come from the API.
- Columns are typed; identifiers (`code`, `assetTag`, paths) render `dir="ltr"` with tabular numerals; row activation opens the detail (keyboard included).
- Groups list keeps tree/grid modes; the active filter control becomes functional using the existing state; the grid view uses typed columns.

## 4. Form contract

- Schemas live at `features/assets/assets/shared/schemas.ts` and `features/assets/asset-groups/shared/schemas.ts`; components import them (no inline schemas).
- Create mode validates with the create schema; edit mode validates with the update schema and carries `rowVersion` from the loaded record.
- Submit signature: `onSubmit(values) => Promise<unknown>` with optional `onSuccess`; the form catches rejections and calls `handleApiError(err, setError)`; untargeted failures render in the root `Alert`.
- One notification owner per failed action; pages never also toast the same failure.
- Read-only mode (asset detail) renders fields without label/error chrome misuse and with no action bar; no submit path exists.

## 5. Behavior-change register

| Change | Justification | Risk |
|--------|---------------|------|
| Palette color classes replaced by shared variants/semantic tokens | Principle X; FR-009 | None — visual only |
| Assets list pagination via shared grid/pagination; heuristic removed | List recipe; fixes hidden full last page | Low — totals now API-driven |
| Duplicate `Active` status-filter option removed | FR-019 | None — filter behavior unchanged |
| Groups list active filter becomes functional | FR-019 (no dead controls) | Low — uses existing state/params |
| Status rendering via base-role map (`Badge`/raw span → `StatusBadge`) | FR-002; status vocabulary | None — presentational |
| Read-only detail no longer offers a no-op save | FR-006; defect fix | None — no payload involved |
| Asset deactivate uses the shared `ConfirmDialog` | FR-006; component contract | None — same action, same gate |
| Group activate toggle uses `outline` (not `success`) | FR-006 action hierarchy | None — presentational |
| Query/form errors use normalized shared messages | Principle XIII; FR-005 | None — safer messages |
| `/assets` navigation permission `Assets.Read` → `Assets.View` | FR-020; matching route guard | Low — navigation visibility aligns with the guard |
| Corrupted Arabic strings corrected | FR-019 | None |

## 6. Forbidden patterns in this batch

1. `Badge` for lifecycle state; any color-only status/validation signal.
2. Undefined `var(--color-*)` references, hard-coded palette/hex colors, or physical direction utilities in the touched files.
3. Toast-only server errors on the forms (field or root binding required).
4. New tokens, new shared components, new routes, endpoints, or API changes.
5. More than one primary-weight action per context; custom back buttons competing with `Page.onBack`.
6. Restyling or migrating screens outside the eight (including other asset sub-features).
7. Adding in-page permission gates (deferred; route guards unchanged).
