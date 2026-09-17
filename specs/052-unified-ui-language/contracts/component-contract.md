# Contract: Phase 1 Core Components

**Feature**: 052-unified-ui-language | **Components**: `src/Web/ClientApp/src/components/ui/`

`contractStatus` is `preserved` for every component unless a row states otherwise; Phase 1 does not change public props or variant names without justification (FR-010).

## 1. Component inventory and approved surface

| Component | Category | Approved variants / roles | Required states | Density | Notes |
|-----------|----------|---------------------------|-----------------|---------|-------|
| `Page` | structure | title, description (optional), actions, toolbar, breadcrumbs, `onBack`, `maxWidth`, loading, error+retry | loading, error, normal | compact (list/detail), comfortable (form) | Single structural owner of page heading/actions |
| `Card` | structure | default, flat, outlined | n/a | per recipe | Surfaces follow surface roles |
| `Button` | structure | `primary`, `outline` (secondary), `ghost` (tertiary), `destructive`, `secondary` (limited gold accent), `link`; `success`/`info` remain but follow action state roles; `default` = deprecated alias of `primary` | default, hover, focus, active, disabled, loading, invalid | both | Exactly one primary per context (FR-005) |
| `ButtonBar` | structure | layout only | n/a | comfortable | Action bar placement in form recipe |
| `Breadcrumb` | structure | hierarchy trail | focus | n/a | Logical start/end ordering |
| `Tabs` | structure | selected/unselected panels | hover, focus, selected, disabled | compact | Keyboard arrow navigation required |
| `Label` | form | label text | disabled | comfortable | Use through the canonical field pattern |
| `Input` | form | text/number/date/etc. via native type | default, hover, focus, disabled, invalid, readonly | both | Owns label, required marker, error + ARIA wiring |
| `Textarea` | form | multiline | same as Input | comfortable | Same contract as Input |
| `Select` | form | native select | same as Input | both | Same contract as Input |
| `Combobox` | form | filtered select | same as Input, open/selected | comfortable | Keyboard support: open, navigate, select, escape |
| `Switch` | form | on/off | focus, disabled, checked | both | Label association required |
| `DatePicker` | form | calendar selection | focus, disabled, invalid, open | comfortable | RTL ordering; no ad-hoc date formatting |
| `FormField` | form | wrapper for custom/compound controls | error, disabled | comfortable | MUST NOT wrap an already-labeled control (see research 6) |
| `FilterBar` | filter | search + filter slots + clear | focus, active filters | compact | One search per list; consistent control height |
| `FilterSearch` | filter | text search | focus, disabled | compact | Must match FilterSelect height/typography |
| `FilterSelect` | filter | select filter | focus, disabled | compact | Must match FilterSearch height/typography |
| `FilterDate` | filter | date range | focus, disabled, invalid | compact | RTL-safe date display |
| `DataGrid` | data | columns, row click, sort; `loading`, `emptyMessage` | loading, empty, error (via Page/ErrorState), hover row, focus | compact | Mobile compact representation via MobileCard; typed columns |
| `MobileCard` | data | compact record card | n/a | compact | Alternative presentation, not a hidden action |
| `Pagination` | data | page/size controls | focus, disabled, current | compact | Used only where server-side paging exists |
| `MoneyDisplay` | data | money + prominent numbers | n/a | n/a | Single formatting convention (FR-021) |
| `StatusBadge` | status | six base roles + documented aliases (`status-semantics.md`) | hover n/a, focus n/a; icon/shape cues | both | `role="status"`, accessible label required |
| `Badge` | status | metadata only (default/primary/outline; count) | n/a | both | Never for lifecycle state |
| `Alert` | feedback | info, success, warning, error | static | comfortable | Includes icon + text; not color-only |
| `EmptyState` | feedback | empty dataset vs no-results message variants | static | per recipe | Distinct copy per case (FR-007) |
| `ErrorState` | feedback | message + retry + optional details | static | per recipe | Persistent, distinct from empty/loading |
| `Loading`, `Skeleton` | feedback | spinner, text, card/table/heading variants | static | per recipe | Scope rule: page vs grid (FR-007) |
| `Toast` | feedback | info, success, warning, error | auto-dismiss, manual dismiss | n/a | One notification owner per failed action (FR-028) |
| `Dialog` | overlay | standard modal | focus trap, escape, backdrop, focus-visible | comfortable | Focus returns to trigger on close |
| `ConfirmDialog` | overlay | destructive confirmation | same as Dialog | comfortable | Destructive action labeled by outcome, not color alone |
| `Sheet` | overlay | side panel | same as Dialog | compact | Logical start/end placement |

## 2. Interaction-state matrix (FR-006)

| Family | hover | focus-visible | active/selected | disabled | loading | error | success/warning |
|--------|-------|---------------|-----------------|----------|---------|-------|-----------------|
| Buttons | required | required (focus ring + halo) | required | required | required (`aria-busy`) | invalid styling where applicable | success/info variants use state roles |
| Form controls | optional | required | n/a | required | n/a | required (`aria-invalid` + message) | n/a |
| Filters | optional | required | active-filter indication | required | n/a | n/a | n/a |
| Data grid | row hover | required for interactive cells | selected row where used | n/a | grid-level | via page/grid error | n/a |
| Feedback | n/a | retry button focus | n/a | n/a | required | required | required |
| Overlays | n/a | required inside dialog | n/a | n/a | n/a | n/a | n/a |

Focus indication is never removed without an equivalent visible replacement; it must be visible in both modes (FR-013).

## 3. Forbidden patterns in Phase 1 scope

1. Generic `Badge` color variants for lifecycle state.
2. More than one primary-weight action in one context (unless documented in the recipe).
3. Inline fallback values masking missing tokens.
4. Physical CSS properties (`margin-left`, `padding-right`, `left`, `right`) in touched components/pages.
5. Color as the only channel for status, validation, required-field, or disabled/loading meaning.
6. Spinner as the default loading treatment for an entire page when only a region is loading.
7. Ad-hoc money/number formatting.
