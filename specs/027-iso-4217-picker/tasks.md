# 027 — ISO 4217 Currency Picker — Tasks

## User Story 1: Currency List

### T001 — CurrencyListPage: add missing columns + field coverage
- Add `Symbol`, `DecimalPlaces`, `IsBase` badge columns to DataGrid.
- IsBase: render `Badge` with "أساسية" label when true.
- Verify: Code, Name, Symbol, DecimalPlaces, IsBase, IsActive columns present in source.
- Tests: T-001, T-002, T-003, T-004, T-016.

### T002 — CurrencyListPage: state matrix
- Implement: Loading (skeleton), Empty (EmptyState), Error (ErrorState), Unauthorized.
- FilterBar + FilterSearch for code/name search.
- ConfirmDialog for activate/deactivate toggle.
- RTL: all logical properties. Dark mode: CSS vars.
- Tests: T-001 (empty), T-003 (search), T-004 (confirm).

## User Story 2: Create Currency with ISO 4217 Picker

### T003 — Iso4217Picker component
- New component: `src/Web/ClientApp/src/features/financial-settings/currencies/components/Iso4217Picker.tsx`.
- Search input queries `useIso4217Codes(query)`.
- Dropdown list: code + name, keyboard navigable (arrow keys, Enter, Escape).
- On select: emit `{ code, name, decimalPlaces }`.
- Tokens: `input`, `surfaceContainer`, `onSurface`, `outlineVariant`.
- RTL: logical properties.
- Tests: T-018.

### T004 — CurrencyCreatePage: integrate ISO picker + form
- Replace manual search with `Iso4217Picker`.
- Auto-fill Name from ISO selection (editable).
- Auto-fill DecimalPlaces (read-only display).
- Symbol: required input.
- RoundingPrecision: input, default 0.01.
- IsBase: checkbox, default false.
- Code: hidden read-only display after selection.
- Submit: `POST /api/Currencies` via `useCreateCurrency`.
- States: Idle, ISO selected, Submitting, Success, Error, Validation.
- Tests: T-005, T-006, T-007, T-008, T-009, T-017.

### T005 — CurrencyCreatePage: state matrix + edge cases
- Loading: N/A (form always renders).
- Validation: inline errors from server 400.
- Success: toast + navigate.
- EC-001 (duplicate code): toast error.
- EC-002 (invalid ISO): server rejects, toast.
- EC-012, EC-013, EC-014: validation messages.
- Tests: T-005, T-008, T-009.

## User Story 3: View + Edit Currency Detail

### T006 — CurrencyDetailPage: read-only view
- New page: `src/Web/ClientApp/src/features/financial-settings/currencies/pages/CurrencyDetailPage.tsx`.
- Route: `/financial-settings/currencies/:id`.
- Breadcrumb: العملات > {Code}.
- Card layout: all fields displayed read-only.
- IsBase: Badge. IsActive: StatusBadge.
- Audit section: CreatedAt, CreatedBy, ModifiedAt, ModifiedBy.
- Edit button (if `Currencies.Update` permission).
- States: Loading, Not found, Unauthorized, Normal.
- Tests: T-010, T-011, T-012.

### T007 — CurrencyDetailPage: edit mode
- Toggle edit mode on button click.
- Editable: Name, Symbol, DecimalPlaces, RoundingPrecision, IsBase.
- Read-only: Code.
- Hidden: RowVersion (sent in payload).
- Submit: `PUT /api/Currencies/{id}` via `useUpdateCurrency`.
- Optimistic concurrency: RowVersion round-trip.
- States: Idle, Dirty, Submitting, Success, Conflict, Validation.
- Tests: T-013, T-014, T-015.

### T008 — CurrencyDetailPage: activate/deactivate from detail
- Activate button (if inactive + `Currencies.Activate` permission).
- Deactivate button (if active + `Currencies.Deactivate` permission).
- ConfirmDialog for both.
- EC-003 (deactivate base): server blocks, toast.
- EC-004 (deactivate with exchange rates): server blocks, toast.
- EC-005 (update inactive): server blocks, toast.
- Tests: EC coverage via server validation.

### T009 — CurrencyDetailPage: state matrix + edge cases
- Loading: skeleton card.
- Not found: NotFound message.
- Unauthorized: Access denied.
- Conflict: RowVersion mismatch toast.
- Validation: inline errors.
- Tests: T-010, T-013, T-014, T-015.

## User Story 4: Navigation + Permission Wiring

### T010 — Route registration + navigation
- Add `/financial-settings/currencies/:id` route to `src/Web/ClientApp/src/app/routes.tsx`.
- Update navigation if needed.
- Verify permission constants: `Currencies.View`, `Currencies.Create`, `Currencies.Update`, `Currencies.Activate`, `Currencies.Deactivate`.

### T011 — Permission wiring
- List page: hide create button without `Currencies.Create`.
- Detail page: hide edit without `Currencies.Update`.
- Detail page: hide activate without `Currencies.Activate`.
- Detail page: hide deactivate without `Currencies.Deactivate`.
- All pages: guard with `usePermission`.
- Tests: T-016.

## User Story 5: Tests + Verification

### T012 — Unit tests: CurrenciesListPage
- T-001: renders empty state.
- T-002: renders currency rows with correct columns.
- T-003: search filters by code and name.
- T-004: activate toggle triggers confirm dialog.
- T-016: hides create button without permission.

### T013 — Unit tests: CurrencyCreatePage + Iso4217Picker
- T-005: renders ISO search.
- T-006: shows form after ISO selection.
- T-007: auto-fills Name and DecimalPlaces.
- T-008: disables submit without ISO selection.
- T-009: sends correct payload.
- T-017: validates Symbol required.
- T-018: ISO picker filters results.

### T014 — Unit tests: CurrencyDetailPage
- T-010: renders all currency fields.
- T-011: shows IsBase badge.
- T-012: shows IsActive status.
- T-013: edit mode enables field editing.
- T-014: save includes RowVersion.
- T-015: shows conflict toast on RowVersion mismatch.

## Final: Field-Coverage Verification

### T015 — Grep every field name in feature source
- Run: `rg "Id|Code|Name|Symbol|DecimalPlaces|RoundingPrecision|IsBase|IsActive|RowVersion|CreatedAt|CreatedBy|ModifiedAt|ModifiedBy" src/Web/ClientApp/src/features/financial-settings/currencies/`
- Every field name from spec.md must appear in at least one file under `currencies/`.
- Any missing field = FAIL with gap list.
