# Contract: Page Recipes

**Feature**: 052-unified-ui-language | **Maintained in**: `docs/ui-patterns.md` | **Required recipes**: list, form, document/transaction detail (FR-016)

Every recipe defines information hierarchy, section organization, action priority, spacing, density, responsive behavior, and page states (FR-017). Ordering below is the required order; optional items may be omitted only when not applicable.

## 1. List page

| Slot | Rule |
|------|------|
| Breadcrumbs | Only when a real hierarchy exists |
| Title + description | Entity plural title; description only when it adds context the title does not carry |
| Actions | One primary create (or the list's dominant action); secondary actions only if they serve the whole list; never two primaries |
| Toolbar | `FilterBar`: one `FilterSearch` + one filter per dimension; clear action appears only with active filters |
| Content | `DataGrid` with typed columns; row click navigation; row-level actions as tertiary (`ghost`) |
| Pagination | Only when server-side paging exists |
| States | loading: page skeleton on initial load, grid-level on refresh/filter/page change; empty: distinct dataset-empty vs no-results messages; error: persistent `ErrorState` + retry |

- Density: **compact**.
- Responsive: essential columns remain identifiable; compact representation (e.g., `MobileCard`) where the table cannot reflow; filters wrap; primary action stays reachable.
- Money/numbers: `MoneyDisplay`/shared formatting.
- Status: `StatusBadge` with the contract mapping.

**Validated page**: Parties list — `features/parties/pages/PartiesListPage.tsx`.

## 2. Create/edit form

| Slot | Rule |
|------|------|
| Breadcrumbs / `onBack` | Back navigation only when a meaningful parent exists |
| Title | "… جديد" / "تعديل …" |
| Sections | Fields grouped into labeled sections in task order; long forms split into `Card` sections |
| Fields | Canonical field pattern (control owns label, required marker, error, ARIA); one column on narrow screens, two columns when the field grouping allows |
| Supporting info | `description` hint below the field, not a placeholder substitute |
| Validation | Client Zod validation; server errors bound to full field paths with a visible form-level fallback for untargeted failures; entered values preserved; one notification owner |
| Actions | At the bottom: primary save + secondary cancel (`ghost`/`outline`); destructive actions separated and labeled by outcome |

- Density: **comfortable**.
- States: submitting (primary button loading + disabled), field-level error (`aria-invalid` + message), form-level error, success (navigate or confirmation).
- Responsive: fields stack to one column; labels, required indicators, and error messages never truncated; action bar remains reachable.

**Validated page**: party create/edit — one shared form for create and edit (`features/parties/components/PartyForm.tsx`, `features/parties/shared/schemas.ts`).

## 3. Document / transaction detail

| Slot | Rule |
|------|------|
| Breadcrumbs / `onBack` | `Page.onBack` is the canonical back mechanism; no competing custom back buttons |
| Identity | Document identity (number/title) dominates |
| Status | Current status via `StatusBadge` in the toolbar area, never as decorative metadata |
| Primary information | Summary card: key business fields in a consistent grid |
| Actions | For the current status: at most one primary lifecycle action; secondary actions subordinate; destructive actions labeled and confirmed |
| Secondary information | Related sections (lines, items, history/audit) using the same section pattern |
| Related sections | Consistent section headers (`label-md`); no section competes with the summary |

- Density: **compact**.
- States: loading page skeleton; not-found; error + retry; lifecycle-action pending (button loading), success feedback, failure feedback per the error contract.
- Responsive: identity/status stay visible; sections stack; all actions remain reachable (no action hidden behind hover).

**Validated page**: party detail (read-only/edit) — `features/parties/pages/PartyDetailPage.tsx`.

## 4. Cross-recipe rules

1. One primary action per context; extra primary-weight actions require a documented functional justification in the recipe or page.
2. Logical RTL properties only; no physical left/right styling.
3. Focus visible on every interactive element in both light and dark modes.
4. Page-level vs regional loading/error follows the scope rules above, not convenience.
5. Any deviation from a recipe is recorded where the page is documented, not silently applied (FR-018).
