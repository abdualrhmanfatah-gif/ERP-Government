# Research: Asset Attributes

## R1: Data Type Expansion (4 → 5 types)

**Decision**: Expand from 4 types (Text/Numeric/Date/Boolean) to 5 (Text/Integer/Decimal/Date/Boolean)

**Rationale**: The 058 spec FR-020 and data-model section define 5 types. Integer and Decimal are semantically different — Integer is for whole numbers (count, quantity), Decimal for precision values (weight, length). The current code uses a single `NumericValue` column; expanding to `IntegerValue` + `DecimalValue` aligns with the spec.

**Implementation**:
- Add `IntegerValue? int` column to `AssetAttributeValue`
- Rename `NumericValue` to `DecimalValue? decimal(18,4)`
- Update `AssetAttributeDataType` enum: `Text=1, Integer=2, Decimal=3, Date=4, Boolean=5`
- Update `AttributeValueValidator` to check IntegerValue for Integer type
- Update `Apply()` method to handle both columns
- EF migration: ADD COLUMN + RENAME COLUMN + UPDATE enum values
- Regenerate web-api-client

---

## R2: Separate Page for Group Bindings

**Decision**: Dedicated page `/assets/groups/:id/attributes` instead of inline editor

**Rationale**: The user chose "صفحة منفصلة لإدارة الخصائص" (separate page). This provides more space for the bindings table, avoids cluttering the group create/edit form, and allows for future expansion (e.g., bulk operations, attribute templates).

**Trade-off**: Extra navigation step vs. better usability for managing many bindings.

---

## R3: Definitions Management as Full CRUD Screen

**Decision**: Complete list/create/edit screen for attribute definitions

**Rationale**: The user chose "نعم، شاشة كاملة لإدارة التعريفات". This is needed because:
- Definitions are created once but managed over time
- Deactivation/activation is essential for lifecycle management
- Search/filter/pagination needed for many definitions
- Acceptance tests expect data-testid selectors on definition forms

---

## R4: Dynamic Section in Asset Form

**Decision**: Conditional section appears when group is selected

**Rationale**: The user chose "قسم ديناميكي حسب المجموعة". This is the standard pattern in ERP systems — form fields change based on category selection. The existing `ReceiptVoucherForm.tsx` demonstrates this pattern in the codebase.

**Implementation**:
- React Query hook fetches group bindings on group selection
- Zod schema dynamically adds/removes attribute field validation
- Form resets attribute values when group changes (with warning)
- `data-attribute` selectors match acceptance test expectations

---

## R5: Atomic Binding Save

**Decision**: `PUT /api/AssetGroups/{id}/attributes` replaces all bindings atomically

**Rationale**: Already implemented in `SetGroupAttributeBindingsCommand`. The command removes all existing bindings and recreates them from the request list. This ensures consistency and avoids complex diff logic.

**Trade-off**: Full replace means the client must send all bindings every time. For v1 with <50 bindings per group, this is acceptable.

---

## R6: Deactivated Definition Display

**Decision**: Show deactivated definitions with "معطّل" badge in asset detail

**Rationale**: FR-005 says deactivation must not delete existing values. Users need to see that an asset carries a value for a definition that is no longer active. Hiding it would break data traceability.

**Implementation**: The asset detail query joins definitions and includes IsActive; the UI renders a badge when IsActive is false.

---

## R7: Validation Architecture

**Decision**: Server-side enforcement only (Constitution III)

**Rationale**: The project constitution requires server-side validation. Client-side validation (Zod) is for UX only and does not replace server checks.

**Layers**:
1. **Zod schema** (frontend): Real-time feedback, required checks, type hints
2. **FluentValidation** (backend): Request validation, required checks, type checks
3. **AttributeValueValidator** (backend): Cross-table validation (definition type, group bindings, uniqueness)
4. **EF constraints** (database): Unique index on (AssetId, DefinitionId), FK constraints
