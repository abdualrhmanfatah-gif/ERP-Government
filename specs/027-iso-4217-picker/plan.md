# 027 — ISO 4217 Currency Picker — Implementation Plan

## 1. FIELD-COVERAGE TABLE

Every field from spec.md mapped to screen, component, editable/read-only, formatter, permission.

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| `Id` | Detail | Read-only text | read-only | — | `Currencies.View` |
| `Code` | List, Create, Detail, Edit | DataGrid col / Input (readonly) | read-only after ISO select | mono font | `Currencies.View` |
| `Name` | List, Create, Detail, Edit | DataGrid col / Input | editable (Create, Edit) | — | `Currencies.View` (read), `Currencies.Create` / `Currencies.Update` (write) |
| `Symbol` | List, Create, Detail, Edit | DataGrid col / Input | editable (Create, Edit) | — | `Currencies.View` (read), `Currencies.Create` / `Currencies.Update` (write) |
| `DecimalPlaces` | List, Create, Detail, Edit | DataGrid col / read-only display | read-only after ISO select | integer | `Currencies.View` (read), `Currencies.Create` (write) |
| `RoundingPrecision` | Create, Detail, Edit | Input (number) | editable (Create, Edit) | decimal | `Currencies.Create` / `Currencies.Update` |
| `IsBase` | List, Create, Detail, Edit | Badge / Checkbox | editable (Create, Edit) | badge if true | `Currencies.View` (read), `Currencies.Create` / `Currencies.Update` (write) |
| `IsActive` | List, Detail | Switch / StatusBadge | toggle (List) | — | `Currencies.View` (read), `Currencies.Activate` / `Currencies.Deactivate` (write) |
| `RowVersion` | Detail, Edit | hidden field | hidden | — | `Currencies.Update`, `Currencies.Activate`, `Currencies.Deactivate` |
| `CreatedAt` | Detail | Read-only text | read-only | datetime | `Currencies.View` |
| `CreatedBy` | Detail | Read-only text | read-only | — | `Currencies.View` |
| `ModifiedAt` | Detail | Read-only text | read-only | datetime | `Currencies.View` |
| `ModifiedBy` | Detail | Read-only text | read-only | — | `Currencies.View` |

No field excluded. No field missing from coverage.

## 2. DESIGN SECTION

### Design System Reference

All values from `src/Web/ClientApp/src/design-system/tokens.ts` and `DESIGN.md`. No invented colors/spacing/typography.

### Existing Components to Reuse

| Component | Path | Usage |
|-----------|------|-------|
| `DataGrid` | `@/components/ui/DataGrid` | Currency list table |
| `Button` | `@/components/ui/Button` | Create, save, cancel actions |
| `Switch` | `@/components/ui/Switch` | IsActive toggle in list |
| `FilterBar` | `@/components/ui/FilterBar` | List filter container |
| `FilterSearch` | `@/components/ui/FilterSearch` | Search by code/name |
| `ConfirmDialog` | `@/components/ui/ConfirmDialog` | Activate/deactivate confirm |
| `Loading` | `@/components/ui/Loading` | Skeleton states |
| `Badge` | `@/components/ui/Badge` | IsBase badge |
| `StatusBadge` | `@/components/ui/StatusBadge` | IsActive status display |
| `Input` | `@/components/ui/Input` | Form fields |
| `FormField` | `@/components/ui/FormField` | Label + input wrapper |
| `PageShell` | `@/components/ui/PageShell` | Page layout |
| `PageHeader` | `@/components/ui/PageHeader` | Title + subtitle + action |
| `EmptyState` | `@/components/ui/EmptyState` | No currencies message |
| `ErrorState` | `@/components/ui/ErrorState` | Error display |
| `Breadcrumb` | `@/components/ui/Breadcrumb` | Navigation trail |

### New Components

| Component | Purpose | File |
|-----------|---------|------|
| `Iso4217Picker` | Searchable dropdown for ISO 4217 code selection | `src/Web/ClientApp/src/features/financial-settings/currencies/components/Iso4217Picker.tsx` |

### Per-Page Layout

#### CurrenciesListPage (EXISTING — needs column additions)

- `PageShell` > `PageHeader` (title "العملات", subtitle "إدارة العملات ودعمها", create button) > `FilterBar` > `DataGrid`.
- Columns: Code (mono font, `typography.mono`), Name, Symbol, DecimalPlaces, IsBase (Badge), IsActive (Switch), Actions (Eye icon).
- Tokens: `surface` bg, `onSurface` text, `onSurfaceVariant` for subtitle, `primary` for create button, `surfaceContainerLowest` for card bg, `outlineVariant` borders.
- RTL: all layout uses logical properties. DataGrid columns align `start` (right in RTL).

#### CurrencyCreatePage (EXISTING — needs ISO picker enhancement)

- `PageShell` > back button + title "عملة جديدة" > card (`surfaceContainerLowest`, `rounded.lg`, `spacing[6]` padding).
- Form: `Iso4217Picker` (search input + dropdown) > conditional fields (Name input, Symbol input, DecimalPlaces display, RoundingPrecision input, IsBase checkbox) > action buttons.
- Tokens: `input` component tokens, `button-primary` for submit, `button-secondary` for cancel.

#### CurrencyDetailPage (NEW)

- `PageShell` > `Breadcrumb` (العملات > {Code}) > card layout.
- Read-only display of all fields.
- Edit button (if `Currencies.Update` permission) toggles edit mode.
- Audit section: CreatedAt, CreatedBy, ModifiedAt, ModifiedBy.
- Tokens: `card` component, `headline-sm` for currency name, `body-md` for field values, `label-sm` for field labels.

### RTL Compliance

- All `margin-left` → `margin-inline-start`. All `padding-right` → `padding-inline-end`.
- Text alignment: `text-start` (renders right in RTL).
- DataGrid columns: first column on the right in RTL.
- Breadcrumb: arrow icon flipped via RTL-aware layout.

### Dark Mode

- All surfaces use CSS custom properties that switch at runtime.
- No hardcoded colors — all via `var(--color-*)` tokens.
- StatusBadge variants already support dark mode.
- Input borders use `var(--color-border-container)`.

### Keyboard & Focus

- Tab order: search → list rows → action buttons.
- Focus ring: `focusRing` token (gold, `primitives.gold[500]`).
- Focus halo: `focusHalo` token (`primitives.gold[100]`).
- ISO picker: arrow keys navigate dropdown, Enter selects, Escape closes.

## 3. STATE MATRIX

### CurrenciesListPage

| State | Trigger | UI |
|-------|---------|-----|
| Loading | `isLoading = true` | Skeleton DataGrid rows |
| Empty | `items.length = 0` | EmptyState: "لا توجد عملات بعد" |
| Error | query fails | ErrorState with retry |
| Unauthorized | 403 | Access denied (page not rendered) |
| Normal | data loaded | DataGrid with currency rows |
| Search active | search input non-empty | Filtered DataGrid + clear button |
| Confirm dialog | toggle switch clicked | ConfirmDialog overlay |

### CurrencyCreatePage

| State | Trigger | UI |
|-------|---------|-----|
| Idle | no ISO selection | Search input only |
| ISO selected | code picked from dropdown | Form fields revealed |
| Submitting | `createMutation.isPending` | Submit button disabled + "جاري الإنشاء..." |
| Success | mutation 200 | Toast success + navigate to list |
| Error | mutation 400/500 | Toast error |
| Validation | server 400 with errors | Inline error messages |

### CurrencyDetailPage

| State | Trigger | UI |
|-------|---------|-----|
| Loading | `isLoading` | Skeleton card |
| Not found | `data = null` after load | NotFound message |
| Unauthorized | 403 | Access denied |
| Normal | data loaded | Read-only card |
| Edit mode | edit button clicked | Editable form |
| Submitting | `updateMutation.isPending` | Save button disabled |
| Success | mutation 200 | Toast + exit edit mode |
| Conflict | RowVersion mismatch | Toast conflict error |
| Validation | server 400 | Inline errors |

## 4. TEST MAP

One Vitest test per spec "Tests Expected" item. Behavior-level assertions.

| Spec Test | Vitest Test | File |
|-----------|-------------|------|
| T-001 | renders empty state | `__tests__/CurrenciesListPage.test.tsx` |
| T-002 | renders currency rows | `__tests__/CurrenciesListPage.test.tsx` |
| T-003 | search filters by code/name | `__tests__/CurrenciesListPage.test.tsx` |
| T-004 | activate toggle shows confirm | `__tests__/CurrenciesListPage.test.tsx` |
| T-005 | create renders ISO search | `__tests__/CurrencyCreatePage.test.tsx` |
| T-006 | create shows form after selection | `__tests__/CurrencyCreatePage.test.tsx` |
| T-007 | create auto-fills from ISO | `__tests__/CurrencyCreatePage.test.tsx` |
| T-008 | create disables submit without ISO | `__tests__/CurrencyCreatePage.test.tsx` |
| T-009 | create sends correct payload | `__tests__/CurrencyCreatePage.test.tsx` |
| T-010 | detail renders all fields | `__tests__/CurrencyDetailPage.test.tsx` |
| T-011 | detail shows IsBase badge | `__tests__/CurrencyDetailPage.test.tsx` |
| T-012 | detail shows IsActive status | `__tests__/CurrencyDetailPage.test.tsx` |
| T-013 | detail edit mode enables editing | `__tests__/CurrencyDetailPage.test.tsx` |
| T-014 | detail save includes RowVersion | `__tests__/CurrencyDetailPage.test.tsx` |
| T-015 | detail shows conflict toast | `__tests__/CurrencyDetailPage.test.tsx` |
| T-016 | list hides create without permission | `__tests__/CurrenciesListPage.test.tsx` |
| T-017 | create validates Symbol required | `__tests__/CurrencyCreatePage.test.tsx` |
| T-018 | ISO picker filters results | `__tests__/Iso4217Picker.test.tsx` |

## 5. COMPLETENESS GATE

Every field name from spec.md that MUST appear in implemented source:

```
Id, Code, Name, Symbol, DecimalPlaces, RoundingPrecision, IsBase, IsActive, RowVersion, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy
```

Converge step verifies each appears in at least one of:
- `src/Web/ClientApp/src/features/financial-settings/currencies/pages/*.tsx`
- `src/Web/ClientApp/src/features/financial-settings/currencies/components/*.tsx`
- `src/Web/ClientApp/src/features/financial-settings/__tests__/*.test.tsx`

Any missing field = FAIL with gap list. No partial pass.

## Scope Boundary Reminder

This plan covers ONLY currency CRUD pages and the ISO 4217 picker component. Exchange rates, fiscal years, document sequences, closing entries belong to sibling specs (024-financial-settings-admin). Do not plan screens for those domains.
