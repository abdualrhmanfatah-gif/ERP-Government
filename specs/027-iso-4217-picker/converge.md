# 027 — ISO 4217 Currency Picker — Converge Report

## Field Coverage Status

| Field | In Existing Source | Gap |
|-------|-------------------|-----|
| `id` | ✅ Detail page (param) | — |
| `code` | ✅ List, Create, Detail | — |
| `name` | ✅ List, Create, Detail | — |
| `symbol` | ✅ List, Create, Detail | — |
| `decimalPlaces` | ✅ List, Create, Detail | — |
| `roundingPrecision` | ✅ Create (editable input), Detail | — |
| `isBase` | ✅ List, Create, Detail | — |
| `isActive` | ✅ List, Detail (StatusBadge) | — |
| `rowVersion` | ✅ List (toggle), Detail (edit) | — |
| `createdAt` | ✅ Detail (audit trail) | — |
| `createdBy` | ✅ Detail (audit trail) | — |
| `modifiedAt` | ✅ Detail (audit trail) | — |
| `modifiedBy` | ✅ Detail (audit trail) | — |

## Implementation Gaps — ALL CLOSED

| Item | Type | Status |
|------|------|--------|
| `CurrencyDetailPage.tsx` | New page | ✅ Exists with all 13 fields, edit mode, audit trail |
| `Iso4217Picker.tsx` | New component | ✅ Exists, used in CurrencyCreatePage |
| Route `/financial-settings/currencies/:id` | Route registration | ✅ Registered in routes.tsx:381 |
| `roundingPrecision` editable | Form input | ✅ Create page has editable input (min=0.01, step=0.01) |
| Audit fields in DTO | CurrencyDto | ✅ Added createdAt/createdBy/modifiedAt/modifiedBy |
| RTL align | DataGrid columns | ✅ Uses `align: 'start'` (no text-right found) |
| Focus ring | Token-based | ✅ Uses `focus:ring-[var(--color-focus-ring)]` |

## Design Lint

| Check | Status |
|-------|--------|
| All colors from tokens.ts | ✅ Uses `var(--color-*)` CSS vars |
| No hardcoded colors | ✅ |
| RTL logical properties | ✅ Uses `text-start`, `align: 'start'` |
| Dark mode via CSS vars | ✅ |
| Focus ring from tokens | ✅ `focus:ring-[var(--color-focus-ring)]` |
| IBM Plex Sans Arabic font | ✅ Inherited from body |
| 8px grid spacing | ✅ Uses design-system spacing |

## Dark/RTL Status

| Check | Status |
|-------|--------|
| Dark mode: all surfaces use CSS vars | ✅ |
| Dark mode: StatusBadge variants | ✅ |
| RTL: sidebar moves right | ✅ (handled by layout) |
| RTL: DataGrid columns align start | ✅ |
| RTL: Breadcrumb | ✅ Detail page has breadcrumb |
| RTL: Form labels | ✅ Uses `text-sm font-medium` |

## State Matrix Status

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| List | ✅ skeleton | ✅ EmptyState | ✅ ErrorState | ✅ guard | N/A | ✅ DataGrid |
| Create | N/A | N/A | ✅ toast | ✅ guard | N/A | ✅ form |
| Detail | ✅ skeleton | N/A | ✅ error msg | ✅ guard | ✅ not found msg | ✅ display + edit |

## Converge Verdict

**PASS** — All 13 fields present in source. CurrencyDetailPage with full state matrix. Iso4217Picker component extracted. roundingPrecision editable. Audit fields in DTO and rendered. RTL/Dark mode verified. Focus ring from tokens.
