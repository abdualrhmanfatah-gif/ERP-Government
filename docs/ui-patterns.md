# UI Patterns — Component Recipes & Page Composition

Reference for building consistent ERP-Government interfaces. Read `DESIGN.md` and `src/Web/ClientApp/src/design-system/tokens.ts` alongside.

**Phase 1 references (spec 052)**: approved contracts in `specs/052-unified-ui-language/contracts/` (design-token, component, status-semantics, page-recipes). Dev-only gallery `/__gallery__` single visual reference for variants/states.

## Semantic Status Roles

`StatusBadge` exposes six base roles. Domain statuses map to base role at feature boundary — never leak business enum names into generic UI primitive.

| Base role | Meaning | When to use |
|-----------|---------|-------------|
| `draft` | Not yet submitted/started | Documents in initial state |
| `pending` | Awaiting review/approval | Submitted, not yet decided |
| `approved` | Affirmed/accepted | Officially approved, ready next step |
| `active` | In workflows | Live entities: open years, active budgets, enabled parties |
| `closed` | Lifecycle ended | Completed/ended lifecycle: posted/reversed/cancelled/voided/locked |
| `inactive` | Exists, not participating | Deactivated: disabled parties, inactive items |

Aliases render **exactly like their base role** (no alias-specific colors), keep existing names for contract compat. Validation aliases carry icon cue.

| Alias | Base role | Meaning |
|-------|-----------|---------|
| `posted`, `reversed`, `cancelled`, `voided`, `locked`, `rejected`, `failed` | `closed` | Final/ended states |
| `submitted`, `sentToTreasury` | `pending` | Awaiting review/treasury |
| `paid`, `disbursed` | `approved` | Settled successfully |
| `partiallyPaid`, `passed` | `active` | In progress/passed |
| `overBudget`, `warning`, `overridden` | `pending` + icon | Validation warnings |
| `unbalanced` | `pending` + icon | Accounting imbalance |

Rules:

- Same meaning → same base role everywhere. Never `Badge` color variants for lifecycle state; `Badge` for metadata (counts, categories, system flags).
- Every state carries Arabic text label; color never only cue.
- Unknown future values degrade to closest base role + raw label — never unstyled badge.
- New aliases added to `contracts/status-semantics.md` and gallery before first use.

### Mapping pattern

At feature boundary, create local mapping object:

```tsx
// features/treasury/shared/status-mapping.ts
import { type BadgeVariant } from '@/components/ui/StatusBadge';

export const receiptVoucherStatusMap: Record<string, BadgeVariant> = {
  Draft: 'draft',
  PendingReview: 'pending',
  Approved: 'approved',
  Cancelled: 'cancelled',
};
```

## Button Action Hierarchy

One primary action per context. Visual weight decreases in this order:

| Role | Variant | Use |
|------|---------|-----|
| Primary | `variant="primary"` | Highest-priority action: create, submit, approve, save |
| Secondary | `variant="outline"` | Supporting: export, print, edit, retry |
| Tertiary | `variant="ghost"` | Inline/low-emphasis: row actions, cancel |
| Destructive | `variant="destructive"` | Irreversible/high-risk: delete, cancel document, deactivate |
| Gold accent | `variant="secondary"` | Limited brand emphasis — not default for lower-priority actions |
| Link | `variant="link"` | Navigational text links |

- Never assign multiple buttons to same visual weight in one context.
- `ButtonBar` defaults unspecified actions to `outline`; mark single primary explicitly.
- Button `default` variant deprecated alias for `primary` — use `primary` in new code.
- Destructive identified by label + confirmation, not color alone.

### Create button pattern

```tsx
<Button variant="primary" size="sm" onClick={...}>
  <Plus size={16} className="ms-1" />
  اسم الزر
</Button>
```

### Back navigation

Use `Page`'s `onBack` as canonical back-navigation when page has meaningful parent/list context. No custom competing back buttons in page actions.

## Canonical Form Field Pattern

Form controls own label, required indicator, error message, ARIA wiring (`Input`, `Textarea`, `Select`, `Combobox`, `DatePicker`):

```tsx
<Input label="الاسم بالعربية" required error={errors.nameAr?.message} {...register('nameAr')} />
```

- `FormField` only for custom/compound controls that can't own label; pass `htmlFor` + matching child `id`. Never wrap labeled control in `FormField` (double labels).
- Required fields show `*` marker + `required` attribute — never color alone.
- Errors render beside field with `role="alert"` + `aria-invalid` on control.
- Server errors bind via `handleApiError` (`shared/api/result-to-ui.ts`): full field paths first, then visible form-level fallback. One notification owner per failed action.
- Forms use React Hook Form + Zod schema co-located in `features/<domain>/<entity>/shared/schemas.ts`.

## Content Density

Two approved densities (`tokens.ts` → `--density-*`). Recipes pick default; components never get per-page arbitrary spacing.

| Density | Used by | Control height | Field gap | Section spacing | Table cell padding |
|---------|---------|----------------|-----------|-----------------|--------------------|
| `compact` | Lists, document/transaction detail, filters | 36px | 8px | 16px | 8px |
| `comfortable` | Create/edit forms | 44px | 16px | 24px | 12px |

## Page Composition Recipes

### List page

```
Page
├── breadcrumbs (when hierarchy exists)
├── title — entity plural, concise Arabic
├── description — only when it adds context the title does not communicate
├── actions — primary create button; optional secondary export/print
├── toolbar — FilterBar (compact density)
│   ├── FilterSearch — text search (one per list)
│   └── FilterSelect / FilterDate — one per filter dimension
└── content
    ├── DataGrid (typed columns)
    │   ├── loading → grid-level loading (not full-page)
    │   ├── error → Page error + onRetry (fetch failure)
    │   └── empty → EmptyState with the correct message variant
    └── Pagination (when server-side paging)
```

**Action priority**: one primary (usually create); row actions tertiary (`ghost`).

**Empty vs no-results**: genuinely empty dataset → `"لا توجد [entities] بعد"`; filters returning nothing → `"لا توجد نتائج مطابقة لمعايير البحث"`.

**Loading scope**: initial load / full-page dependency → `Page loading`; table refresh / filter / pagination → `DataGrid loading`.

**Error scope**: expected fetch failure → `Page error` + `onRetry` or `DataGrid error` + `onRetry`; form submission error → `handleApiError`; unexpected global failure → `ErrorBoundary`.

**Responsive**: filters wrap; `DataGrid` → `MobileCard` below `md`; primary action reachable.

**Tree variation** (validated: asset groups): hierarchical list may replace `DataGrid` when depth matters — same header colors as `DataGrid` (`primary` background / `on-primary` text), fixed column widths for code/status/actions, logical indentation (`paddingInlineStart`), RTL-correct chevrons (expanded = down, collapsed = left), row separators, always-visible row actions (never hover-only).

### Create/edit form

```
Page (maxWidth sm/md, comfortable density)
├── title — " entity جديد " / " تعديل entity "
├── onBack — when editing an existing record
└── content
    └── Card
        ├── sections with `label-md` headings, in task order
        ├── canonical fields (label, required marker, error, description)
        └── actions — primary save + secondary cancel at the bottom
```

**Action priority**: `primary` save, `ghost`/`outline` cancel; destructive actions separated and labeled.

**Validation**: client Zod; server errors bound to full field paths + visible form-level fallback; entered values preserved; submitting state disables actions.

**Responsive**: fields stack to one column; labels, required markers, errors never truncate; action bar reachable.

### Document/transaction detail

```
Page (compact density)
├── breadcrumbs / Page.onBack
├── title — document identity (e.g., "سند قبض {number}")
├── actions — lifecycle transitions for the current status
├── toolbar — StatusBadge + MetaItems (date, party, amount)
└── content
    ├── summary Card — key fields in a grid, `label-md` section heading
    ├── detail sections — lines, items, related records
    └── history/audit panel (optional)
```

**Action priority**: at most one primary lifecycle action for current status; secondary subordinate; destructive labeled + confirmed.

**Responsive**: identity/status visible; sections stack; actions reachable.

### Identity header + tabbed sections (validated on asset groups)

For form/detail screens built from small identity block + secondary sections.

```
Page (maxWidth lg)
├── header — title (entity name); description = code + StatusBadge (+ category Badge)
├── actions — تعديل primary (detail) / save primary + ghost cancel at the bottom (form)
├── primary Card — always-visible fields in ONE adaptive row
│   └── flex flex-wrap items-end gap-3, widths by content need:
│       code → w-36 shrink-0 · names/labels → min-w-44/56 flex-1 · description → min-w-64 flex-[2] · flags → shrink-0
└── Tabs (secondary sections, icons on labels)
    ├── read-only values → compact boxes (h-compact, muted surface, truncate + title)
    ├── editable fields → canonical controls inside the tab cards
    ├── record lists / logs → DataGrid (typed columns)
    └── required fields stay in the default-visible section
```

Rules:

1. Identity (code, status, category) in page header — never duplicated in body cards.
2. Heterogeneous fields never use uniform column grid; width follows content (codes narrow, descriptions widest).
3. Read-only displays use compact density; editable controls keep comfortable.
4. Required fields stay in always-visible block (or default tab). Submit fails validation on another tab → switch to tab with first error (controlled `Tabs value/onChange`).
5. Secondary record lists + audit/history render via `DataGrid`, not read-only field lists.
6. Validated pages: asset group detail / create / edit (spec 057).
7. Field labels identical across list/detail/form for same field; units kept in label (`العمر الإنتاجي (سنوات)`, `نسبة القيمة التخريدية (%)`); read-only values plain (no repeated units).

### Cross-recipe rules

1. One primary action per context; extra primary-weight actions need documented functional justification.
2. Logical RTL properties only (`ms-`, `me-`, `ps-`, `pe-`, `start`, `end`).
3. Focus visible on every interactive element in light + dark modes.
4. Page-level vs regional loading/error follows scope rules, not convenience.
5. Deviation from recipe recorded where page documented, not silently applied.

## Reuse Before Invention

Before introducing visual convention:

1. Search this file's recipes + `contracts/` for existing pattern.
2. Check gallery (`/__gallery__`) for approved variant/state.
3. Reuse closest approved role/component; genuinely new need → record as below.

**Proposing a new pattern or alias**:

1. Open `specs/052-unified-ui-language/contracts/`, add proposed variant/role with meaning, light/dark values, non-color cue — or extend `tokens.ts` for new reusable role with demonstrated accessibility gap.
2. Add variant to component + demonstrate in gallery.
3. Update relevant recipe + status alias table when status involved.
4. Regenerate tokens (`npm run generate-tokens`) and verify contrast for both modes.
5. One-off page-local style is a defect; new patterns join this guidance or don't ship.

## Component Quick Reference

### StatusBadge

```tsx
import { StatusBadge } from '@/components/ui/StatusBadge';

<StatusBadge variant="active">نشط</StatusBadge>
<StatusBadge variant="draft" size="sm">مسودة</StatusBadge>
```

Variants: six base roles + documented aliases (see table above). Sizes: `sm`, `md` (default).

### EmptyState / ErrorState

```tsx
<EmptyState message="لا توجد أطراف بعد" />
<EmptyState message="لا توجد نتائج مطابقة لمعايير البحث" />
<ErrorState message="حدث خطأ أثناء تحميل البيانات" onRetry={() => refetch()} />
```

### FilterBar

```tsx
<FilterBar hasFilters={hasActiveFilters} onClear={clearAll}>
  <FilterSearch value={search} onChange={setSearch} />
  <FilterSelect label="الحالة" value={status} onChange={setStatus} options={statusOptions} />
</FilterBar>
```

### MoneyDisplay

```tsx
<MoneyDisplay value={amount} currency="YER" />
```

## Accessibility Checklist

For every page/component modification:

- [ ] RTL: logical CSS properties only (`ms-`, `me-`, `ps-`, `pe-`, `start`, `end`)
- [ ] Light + dark mode: verify status colors, button states, borders, focus rings
- [ ] 320 CSS px equivalent: content reflows, no page-level two-dimensional scrolling (inherent 2-D content excepted)
- [ ] 400% zoom from 1280 CSS px: no loss of content or functionality
- [ ] Grayscale / forced-colors: status, validation, required, disabled, action hierarchy readable
- [ ] Contrast: normal text ≥ 4.5:1, large text and non-text indicators ≥ 3:1 in both modes
- [ ] Keyboard: all interactive controls reachable via Tab, focus-visible ring shown, no traps
- [ ] Focus management: focus not trapped unexpectedly, not obscured by sticky bars
- [ ] Status consistency: `StatusBadge` for lifecycle states, `Badge` for metadata
- [ ] Action hierarchy: one primary action per context
- [ ] Density: compact for lists/detail, comfortable for forms (no arbitrary per-page spacing)
- [ ] Loading: appropriate scope (page vs grid)
- [ ] Error: appropriate handler (Page, DataGrid, form, boundary)
- [ ] Empty: correct message variant (empty dataset vs no results)
- [ ] Disabled: visual state matches interactive state
- [ ] Screen reader: `role="status"` on StatusBadge, `role="alert"` on errors, aria-labels on icon buttons

## Deferred Migration Backlog

Confirmed inconsistencies for future work. Don't fix during design-system validation tasks unless directly blocking a representative page.

After Phase 3 (spec 057): items marked **fixed** resolved in validated scope (core components + Parties pages + 6 Accounts screens + 8 Assets screens); rest remain deferred.

| # | Issue | Affected pages | Status / notes |
|---|-------|---------------|----------------|
| 1 | `Badge` used for status instead of `StatusBadge` | BudgetsList, ReceiptVouchersList, EncumbrancesList, BudgetDetail | **Accounts list + groups list/detail fixed** (spec 053); **assets list + asset groups list/detail fixed** (spec 057) |
| 2 | Feature-specific status badge wrappers | AccountingStatusBadge, PaymentsStatusBadge, FiscalYearStatusBadge | Open |
| 3 | `render` vs `cell` column API inconsistency | ReceiptVouchersList, RolesList | Open |
| 4 | JournalEntries Card-based custom filter | JournalEntriesListPage | Open |
| 5 | Missing error handling | BudgetsList, JournalEntriesList, PaymentOrdersList, DisbursementRequestsList, FiscalYearsList, EncumbrancesList | **Accounts list + groups list/detail fixed** (spec 053); **all 8 assets screens fixed** (spec 057) |
| 6 | Create button variant/size/icon inconsistency | FiscalYearsList, ItemsList, RolesList, JournalEntriesList, ReceiptVouchersList | **Accounts list + groups list fixed** (spec 053); **assets + asset groups lists fixed** (spec 057) |
| 7 | Custom back buttons instead of `Page.onBack` | BudgetDetail, PartyDetail, PaymentOrderDetail | **Account detail + group detail fixed** (spec 053); **asset detail/edit + group detail/edit fixed** (spec 057) |
| 8 | Page description inconsistency | Various list pages | Open |
| 9 | Empty state phrasing inconsistency | 4 list pages | **Accounts list fixed** — distinct empty vs no-results (spec 053); **assets list + asset groups list fixed** (spec 057) |
| 10 | Detail page section header typography | PaymentDetail, others | Open (PartyDetail fixed) |
| 11 | Console.log in production | PaymentOrderDetailPage | Open |
| 12 | Naming: "الموردون" vs "الأطراف" | PartiesListPage (fixed), PartyCreatePage (fixed) | Partially resolved |
| 13 | Permission check inconsistency | Most pages lack checks | Separate security concern; **/assets nav identifier corrected to `Assets.View`** (spec 057) |
| 14 | `DataGridColumn<any>` type usage | All list pages using `any` for column generic | **Accounts grid fixed** — typed `AccountDto` columns (spec 053); **assets + asset groups lists typed** (spec 057) |
| 15 | Button `default` variant still used in some pages | Various | Open |
| 16 | `FilterSelect` h-11 height mismatch with `FilterSearch` h-8 | FilterSelect, FilterSearch | **Fixed** (shared compact control height) |
| 17 | Undefined token references outside core components | `features/**` (33 references) | **3 accounting components fixed** — 30 remain outside scope (spec 053) |
| 18 | Hard-coded palette color classes outside the token system | 8 assets screens | **Fixed** — shared components + semantic tokens (spec 057) |
| 19 | Dead filter state (control never rendered) | AssetGroupsListPage | **Fixed** — active filter wired (spec 057) |
| 20 | Duplicate/conflicting filter options | AssetsListPage status filter | **Fixed** — one option per status (spec 057) |
| 21 | Tailwind v4-style bare data variants (`data-checked:`, `data-open:`, …) do not compile under Tailwind v3, so state styling is missing | `Switch` (invisible track; **fixed** — spec 057 follow-up), `dropdown-menu.tsx`, `popover.tsx`, `tooltip.tsx` (open/closed/disabled state styling) | **Switch fixed**; dropdown/popover/tooltip still open — separate remediation (also contain other v4 syntax such as `max-h-(--var)` and `not-data-[…]`) |