# 027 — ISO 4217 Currency Picker — Converge Report

## Field Coverage Status

| Field | In Existing Source | Gap |
|-------|-------------------|-----|
| `id` | Not rendered in list (exists in DTO) | Need detail page |
| `code` | ✅ List, Create | — |
| `name` | ✅ List, Create | — |
| `symbol` | ✅ List, Create | — |
| `decimalPlaces` | ✅ List, Create | — |
| `roundingPrecision` | Hardcoded to 1 in Create | Need user-editable input |
| `isBase` | ✅ List, Create | — |
| `isActive` | ✅ List | Need detail view |
| `rowVersion` | ✅ List (toggle) | Need in detail/edit |
| `createdAt` | ❌ MISSING | Need detail page |
| `createdBy` | ❌ MISSING | Need detail page |
| `modifiedAt` | ❌ MISSING | Need detail page |
| `modifiedBy` | ❌ MISSING | Need detail page |

## Implementation Gaps

### Must Implement (no existing source)

| Item | Type | Priority |
|------|------|----------|
| `CurrencyDetailPage.tsx` | New page | HIGH |
| `Iso4217Picker.tsx` | New component | HIGH |
| Route `/financial-settings/currencies/:id` | Route registration | HIGH |

### Must Fix (existing source incomplete)

| Item | File | Fix |
|------|------|-----|
| `roundingPrecision` | `CurrencyCreatePage.tsx:30` | Add editable input (currently hardcoded `1`) |
| Missing columns | `CurrenciesListPage.tsx` | All columns present — no fix needed |
| Missing audit fields | New `CurrencyDetailPage.tsx` | Create detail page with audit display |

## Design Lint

| Check | Status |
|-------|--------|
| All colors from tokens.ts | ✅ Uses `var(--color-*)` CSS vars |
| No hardcoded colors | ✅ |
| RTL logical properties | ⚠️ CurrenciesListPage uses `text-right` (line 71) — should be `text-start` |
| Dark mode via CSS vars | ✅ |
| Focus ring from tokens | ⚠️ Not implemented in existing pages — need focus-visible styles |
| IBM Plex Sans Arabic font | ✅ Inherited from body |
| 8px grid spacing | ⚠️ Uses `space-y-5` (20px) — close but verify against spacing scale |

## Dark/RTL Status

| Check | Status |
|-------|--------|
| Dark mode: all surfaces use CSS vars | ✅ |
| Dark mode: StatusBadge variants | ✅ (existing component) |
| RTL: sidebar moves right | ✅ (handled by layout) |
| RTL: DataGrid columns align start | ⚠️ DecimalPlaces column uses `align: 'left'` — should be `align: 'start'` |
| RTL: Breadcrumb | Need detail page breadcrumb |
| RTL: Form labels | ✅ Uses `text-sm font-medium` |

## State Matrix Status

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| List | ✅ skeleton | ✅ EmptyState | ✅ ErrorState | ✅ guard | N/A | ✅ DataGrid |
| Create | N/A | N/A | ✅ toast | ✅ guard | N/A | ✅ form |
| Detail | ❌ NEED | ❌ NEED | ❌ NEED | ❌ NEED | ❌ NEED | ❌ NEED |

## Converge Verdict

**FAIL** — 4 audit fields (`createdAt`, `createdBy`, `modifiedAt`, `modifiedBy`) missing from implemented source. `CurrencyDetailPage` does not exist. `Iso4217Picker` component not extracted. `roundingPrecision` hardcoded in create page.

### Required for PASS

1. Create `CurrencyDetailPage.tsx` with all 13 fields displayed.
2. Create `Iso4217Picker.tsx` component.
3. Fix `CurrencyCreatePage.tsx` — add editable `roundingPrecision` input.
4. Fix `text-right` → `text-start` in CurrenciesListPage line 71.
5. Fix `align: 'left'` → `align: 'start'` in DataGrid column.
6. Add focus-visible styles to form inputs.
7. Register route `/financial-settings/currencies/:id`.
8. Run T015 grep gate — every field name must appear in feature source.
