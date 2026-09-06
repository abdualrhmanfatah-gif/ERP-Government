# 027 — ISO 4217 Currency Picker

## User Story

As a **financial settings administrator**, I need to **create, view, edit, activate, and deactivate currencies** using a validated ISO 4217 picker, so that **all currency records conform to international standards and the base currency is correctly managed**.

## Scope Boundary

- **In scope**: Currency CRUD pages (list, create, detail/edit), ISO 4217 picker component, activate/deactivate toggle.
- **Out of scope**: Exchange rates (spec 024), fiscal years, document sequences, closing entries. Do not plan screens belonging to sibling financial-settings specs.

## Key Entities

### Currency

| Field | Type | Constraint | Notes |
|-------|------|-----------|-------|
| `Id` | int | PK, auto-increment | BaseEntity PK |
| `Code` | string(10) | required, unique, ISO 4217 validated | 3-letter code, e.g. YER, USD |
| `Name` | string(100) | required | Arabic or English currency name |
| `Symbol` | string(10) | required | Currency symbol, e.g.﷼, $ |
| `DecimalPlaces` | int | required, 0–6 | Derived from ISO 4217 on create |
| `RoundingPrecision` | decimal(23,2) | required, > 0 | Default 0.01 |
| `IsBase` | bool | required, one true at a time | Base currency; unsets previous on set |
| `IsActive` | bool | required, default true | Toggle via activate/deactivate |
| `RowVersion` | byte[] | optimistic concurrency | Verified on every update |
| `CreatedAt` | DateTime | auto | Audit field |
| `CreatedBy` | string | auto | Audit field |
| `ModifiedAt` | DateTime | auto | Audit field |
| `ModifiedBy` | string | auto | Audit field |

### Iso4217Code (reference, read-only)

| Field | Type | Notes |
|-------|------|-------|
| `Code` | string | ISO 4217 alphabetic code |
| `Name` | string | Currency name in English |
| `DecimalPlaces` | int | Minor unit digits (0–6) |

### Field Contract

```
Currency.Code           — string(10)  — ISO 4217 alphabetic, unique, validated server-side against Iso4217Codes static list
Currency.Name           — string(100) — user-editable, shown in list + detail
Currency.Symbol         — string(10)  — user-editable, displayed next to monetary amounts
Currency.DecimalPlaces  — int(0–6)    — auto-populated from ISO 4217 on create, read-only after creation
Currency.RoundingPrecision — decimal(23,2) — user-editable, must be > 0
Currency.IsBase         — bool        — exactly one currency must be true; setting true unsets previous base
Currency.IsActive       — bool        — toggled via activate/deactivate endpoints; base currency cannot be deactivated
Currency.RowVersion     — byte[]      — optimistic concurrency token, round-tripped on every mutation
Currency.CreatedAt      — DateTime    — audit, read-only
Currency.CreatedBy      — string      — audit, read-only
Currency.ModifiedAt     — DateTime    — audit, read-only
Currency.ModifiedBy     — string      — audit, read-only
```

## Functional Requirements

### FR-001 — Currency List

- Display all currencies in a DataGrid with columns: Code, Name, Symbol, DecimalPlaces, IsBase badge, IsActive toggle, actions.
- FilterBar with search by Code or Name.
- Empty state: "لا توجد عملات بعد".

### FR-002 — Create Currency

- Open ISO 4217 picker: search box queries `/api/Currencies/iso4217?query=`.
- On selection: auto-fill Name and DecimalPlaces from ISO 4217 entry.
- User fills: Symbol (required).
- Code field is read-only after ISO selection (prevents invalid codes).
- RoundingPrecision defaults to 0.01, editable.
- IsBase defaults to false.
- Submit: `POST /api/Currencies`.

### FR-003 — View Currency Detail

- Display all fields in read-only card layout.
- Show audit trail: CreatedAt, CreatedBy, ModifiedAt, ModifiedBy.
- IsBase shown as badge.
- IsActive shown as status indicator.

### FR-004 — Edit Currency

- Editable fields: Name, Symbol, DecimalPlaces, RoundingPrecision, IsBase.
- Code is read-only (immutable after creation).
- RowVersion must be sent for optimistic concurrency.
- Submit: `PUT /api/Currencies/{id}`.
- Cannot edit inactive currencies (server rejects).

### FR-005 — Activate Currency

- Toggle from inactive → active.
- Requires RowVersion.
- Server blocks if currency is already active.
- `POST /api/Currencies/{id}/activate`.

### FR-006 — Deactivate Currency

- Toggle from active → inactive.
- Requires RowVersion.
- Server blocks if currency is the base currency.
- Server blocks if currency has active exchange rates.
- `POST /api/Currencies/{id}/deactivate`.

### FR-007 — Base Currency Management

- Only one currency can have IsBase = true at any time.
- Setting IsBase = true on a currency automatically unsets the previous base.
- Base currency cannot be deactivated.
- Base currency badge displayed in list and detail.

### FR-008 — ISO 4217 Validation

- Code must be a valid ISO 4217 alphabetic code.
- Server validates against hardcoded `Iso4217Codes` list.
- Client shows autocomplete dropdown from `/api/Currencies/iso4217`.
- Invalid codes rejected at both client and server.

### FR-009 — Permission Enforcement

Every endpoint requires its declared permission. Frontend permission checks are UX only; server is authority.

## Permissions

- `Currencies.View` — list, detail, ISO 4217 codes
- `Currencies.Create` — create new currency
- `Currencies.Update` — edit currency fields
- `Currencies.Activate` — activate inactive currency
- `Currencies.Deactivate` — deactivate active currency

## Edge Cases

| ID | Scenario | Expected Behavior |
|----|----------|------------------|
| EC-001 | Create with duplicate Code | Server returns 400 "Currency code already exists" |
| EC-002 | Create with invalid ISO 4217 code | Server returns 400 "Code must be a valid ISO 4217 currency code" |
| EC-003 | Deactivate base currency | Server returns 400 "Cannot deactivate the base currency" |
| EC-004 | Deactivate currency with active exchange rates | Server returns 400 "Cannot deactivate currency with active exchange rates" |
| EC-005 | Update inactive currency | Server returns 400 "Cannot update inactive currency" |
| EC-006 | Concurrent edit (RowVersion mismatch) | Server returns 400 "Currency has been modified by another user" |
| EC-007 | Set IsBase = true when another is base | Previous base unset, new base set, both in same transaction |
| EC-008 | Activate already-active currency | Server returns 400 "Currency is already active" |
| EC-009 | Deactivate already-inactive currency | Server returns 400 "Currency is already inactive" |
| EC-010 | Update with DecimalPlaces out of range (0–6) | Server returns 400 "Decimal places must be between 0 and 6" |
| EC-011 | Update with RoundingPrecision ≤ 0 | Server returns 400 "Rounding precision must be greater than 0" |
| EC-012 | Create with empty Name | Server returns 400 "Name is required" |
| EC-013 | Create with empty Symbol | Server returns 400 "Symbol is required" |
| EC-014 | Search ISO 4217 with no results | Empty dropdown, no selection possible |
| EC-015 | Currency list with no currencies | Empty state message displayed |

## UI States Required

### CurrenciesListPage
| State | Condition | UI |
|-------|-----------|-----|
| Loading | `isLoading = true` | Skeleton rows in DataGrid |
| Empty | `items.length = 0` | "لا توجد عملات بعد" message |
| Error | query error | Error banner with retry |
| Unauthorized | no Currencies.View | Access denied (not rendered) |
| Normal | data loaded | DataGrid with rows |
| Search active | filter applied | Filtered DataGrid + clear button |

### CurrencyCreatePage
| State | Condition | UI |
|-------|-----------|-----|
| Idle | no ISO selection | Search box only |
| ISO selected | code selected from dropdown | Form fields revealed (Name, Symbol, DecimalPlaces, RoundingPrecision) |
| Submitting | `createMutation.isPending` | Button shows "جاري الإنشاء...", disabled |
| Success | mutation success | Toast + navigate to list |
| Error | mutation error | Toast error message |
| Validation error | server 400 | Inline error messages |

### CurrencyDetailPage
| State | Condition | UI |
|-------|-----------|-----|
| Loading | `isLoading` | Skeleton card |
| Not found | `data = null` | 404 message |
| Unauthorized | no Currencies.View | Access denied |
| Normal | data loaded | Read-only card with all fields |
| Editing | edit mode active | Editable form fields |

### CurrencyEditPage (within detail)
| State | Condition | UI |
|-------|-----------|-----|
| Idle | form not dirty | Save disabled |
| Dirty | form modified | Save enabled |
| Submitting | `updateMutation.isPending` | Button disabled, spinner |
| Success | mutation success | Toast + exit edit mode |
| Conflict | RowVersion mismatch | Toast "تم تعديل العملة من مستخدم آخر" |
| Validation error | server 400 | Inline errors |

## Tests Expected

| ID | Test | Type |
|----|------|------|
| T-001 | CurrenciesListPage renders empty state when no currencies exist | Unit |
| T-002 | CurrenciesListPage renders currency rows with correct columns | Unit |
| T-003 | CurrenciesListPage search filters by code and name | Unit |
| T-004 | CurrenciesListPage activate toggle triggers confirm dialog | Unit |
| T-005 | CurrencyCreatePage renders search box for ISO 4217 | Unit |
| T-006 | CurrencyCreatePage shows form fields after ISO selection | Unit |
| T-007 | CurrencyCreatePage auto-fills Name and DecimalPlaces from ISO | Unit |
| T-008 | CurrencyCreatePage disables submit when no ISO selected | Unit |
| T-009 | CurrencyCreatePage sends correct payload on submit | Unit |
| T-010 | CurrencyDetailPage renders all currency fields | Unit |
| T-011 | CurrencyDetailPage shows IsBase badge when true | Unit |
| T-012 | CurrencyDetailPage shows IsActive status | Unit |
| T-013 | CurrencyDetailPage edit mode enables field editing | Unit |
| T-014 | CurrencyDetailPage save sends RowVersion for concurrency | Unit |
| T-015 | CurrencyDetailPage shows conflict toast on RowVersion mismatch | Unit |
| T-016 | CurrenciesListPage hides create button without Currencies.Create permission | Unit |
| T-017 | CurrencyCreatePage validates Symbol required before submit | Unit |
| T-018 | ISO 4217 picker filters results by code and name | Unit |
