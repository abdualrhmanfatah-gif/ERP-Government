# Feature Specification: Asset Attributes (خصائص الأصول المرنة)

**Feature Branch**: `059-asset-attributes`

**Created**: 2026-09-16

**Status**: Draft

**Input**: User description: "ميزة مواصفات الأصول AssetGroupAttributes والجداول التابعة لها في 058-asset-module-spec — تغطية الواجهة الأمامية والخلفية"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manage Attribute Definitions (Priority: P1)

As an **asset configuration administrator (مسؤول إعدادات الأصول)**, I want to create, view, update, and deactivate attribute definitions so that I can define the custom properties that assets in my organization can carry (e.g., color, weight, model, serial number).

**Why this priority**: Attribute definitions are the foundation of the entire flexible attributes system. Without them, no group bindings or asset values can exist.

**Independent Test**: Can be fully tested by creating definitions of all 5 data types, verifying uniqueness of codes, blocking duplicate codes, deactivating a definition, and verifying it no longer appears in active lists but retains its values.

**Acceptance Scenarios**:

1. **Given** an administrator on the attribute definitions list page, **When** they click "إضافة تعريف صفة", **Then** a form appears with fields for Code (unique), Name, Data Type (dropdown: Text/Integer/Decimal/Date/Boolean), Unit (optional text), Sort Order (optional number), and an Active toggle (default: on)
2. **Given** the administrator fills all required fields (Code, Name, Data Type) and clicks save, **When** the Code is unique, **Then** the definition is saved and appears in the list with the correct data type badge
3. **Given** the administrator fills a Code that already exists, **When** they click save, **Then** a field-level error is shown on Code and the form is not submitted
4. **Given** an existing definition, **When** the administrator changes its Data Type while it has linked values on assets, **Then** the save is blocked with a clear error message explaining that type changes require value migration
5. **Given** an existing definition with no linked values, **When** the administrator changes its Data Type, **Then** the save succeeds
6. **Given** an active definition, **When** the administrator clicks "تعطيل", **Then** the definition's IsActive flag is set to false and it disappears from the active list
7. **Given** a deactivated definition, **When** the administrator clicks "تفعيل", **Then** the definition's IsActive flag is set to true and it reappears in the active list
8. **Given** the administrator is on the definitions list, **When** they type in the search box, **Then** the list filters by Code or Name in real time

---

### User Story 2 - Bind Attributes to Asset Groups (Priority: P1)

As an **asset group manager (مسؤول مجموعات الأصول)**, I want to manage which attribute definitions are bound to a specific asset group, specifying whether each attribute is required or optional, so that every asset registered under that group inherits the correct set of attributes.

**Why this priority**: Group bindings connect definitions to assets. Without bindings, attribute definitions exist in isolation and cannot be used during asset registration.

**Independent Test**: Can be fully tested by navigating to a group's attribute management page, adding/removing bindings, setting required flags and sort order, and verifying the bindings are saved atomically with the group.

**Acceptance Scenarios**:

1. **Given** the administrator is on the group attributes page, **When** they view the current bindings, **Then** they see a table of all bound definitions with columns for Definition Name, Data Type, Required (toggle), and Sort Order
2. **Given** the administrator clicks "إضافة صفة", **When** they select an active definition from the dropdown, **Then** a new binding row is added with the definition's data type shown
3. **Given** multiple bindings exist, **When** the administrator changes the sort order of a binding, **Then** the new order is saved and reflected in the list
4. **Given** a definition is bound to a group with existing assets, **When** the administrator removes the binding, **Then** existing asset values for that definition are retained (not deleted) and flagged for review
5. **Given** the administrator saves the bindings, **When** the save completes, **Then** all bindings are saved atomically (either all succeed or all fail) — no partial saves

---

### User Story 3 - Enter Attribute Values During Asset Registration (Priority: P1)

As an **asset registrar (مسؤول سجل الأصول)**, I want to enter typed attribute values when creating or editing an asset, so that each asset carries its specific properties alongside its core identification data.

**Why this priority**: This is the primary value delivery of the flexible attributes system — the actual data entry that makes assets searchable and distinguishable.

**Independent Test**: Can be fully tested by selecting a group on the asset form, seeing the dynamic attributes section appear, filling values of the correct types, saving, and verifying the values persist and appear in the asset detail.

**Acceptance Scenarios**:

1. **Given** a registrar on the asset create/edit form, **When** they select a group, **Then** a dynamic section "الخصائص" appears showing all bound definitions with appropriate input controls for each data type (text input, number input, date picker, boolean switch)
2. **Given** a required attribute is bound to the selected group, **When** the registrar leaves it empty and tries to save, **Then** a field-level error is shown indicating the attribute is required
3. **Given** a decimal attribute, **When** the registrar enters a text value, **Then** the save is rejected with a type-mismatch error
4. **Given** an integer attribute, **When** the registrar enters a decimal value (e.g., 3.5), **Then** the save is rejected with a type-mismatch error
5. **Given** an asset has values and the registrar changes its group, **When** the new group lacks some of the old definitions, **Then** the old values are retained and shown with a warning that they are incompatible with the new group
6. **Given** an optional attribute, **When** the registrar leaves it empty, **Then** no value row is created for it (not even a null row)
7. **Given** a boolean attribute, **When** the registrar sets it to false, **Then** false is saved as a real value (not treated as empty)

---

### User Story 4 - View Attribute Values in Asset Detail (Priority: P1)

As an **asset reviewer or auditor (مراجع الأصول)**, I want to see all attribute values of an asset in its detail view, so that I can verify correctness and completeness of the asset record.

**Why this priority**: Review and audit are essential for data quality and compliance in government systems.

**Independent Test**: Can be fully tested by navigating to an asset's detail page and verifying that all bound attributes are displayed with their values, including empty optional attributes and inherited defaults.

**Acceptance Scenarios**:

1. **Given** an asset with attribute values, **When** the user opens the asset detail page, **Then** a tab or section "الخصائص" displays all bound attributes with their values
2. **Given** an optional attribute with no value, **When** the user views the asset detail, **Then** the attribute shows "—" (em dash) to indicate no value
3. **Given** a deactivated definition that the asset still carries, **When** the user views the asset detail, **Then** the attribute appears with a visual indicator that it is deactivated (e.g., grayed out or "معطّل" badge)

---

### User Story 5 - Bulk Manage Definitions via List (Priority: P2)

As an **asset configuration administrator**, I want to see all attribute definitions in a filterable, sortable list so that I can manage many definitions efficiently.

**Why this priority**: Scales the definitions management beyond a handful of items; P2 because basic CRUD (US1) is sufficient for small deployments.

**Independent Test**: Can be fully tested by creating 10+ definitions, filtering by name/code/type, sorting by name/type/status, and verifying pagination works correctly.

**Acceptance Scenarios**:

1. **Given** multiple definitions exist, **When** the user views the list page, **Then** definitions are displayed in a table with columns for Code, Name, Data Type, Unit, Status (Active/Inactive), and actions (Edit, Toggle Active)
2. **Given** the list has 20+ definitions, **When** the user scrolls or navigates pages, **Then** pagination works correctly with 20 items per page by default
3. **Given** the user types in the search box, **When** the search term matches Code or Name, **Then** the list filters in real time
4. **Given** the user clicks a column header, **When** the sort is applied, **Then** the list re-sorts by that column (ascending/descending toggle)

---

### Edge Cases

- What happens when a definition is deactivated while an asset registration form is open with that definition bound? → The form shows a warning that the definition is now inactive but allows saving with existing values
- What happens when two administrators try to bind the same definition to the same group simultaneously? → The second save is rejected with a concurrency conflict error
- What happens when a definition's sort order is changed and the group page is already open? → The group attributes page re-fetches on next interaction; stale data is flagged
- What happens when the API client (web-api-client.ts) is out of date after contract changes? → The frontend build fails with type errors; regeneration is required
- What happens when an asset has values for a definition that is then deactivated? → Values are retained; definition appears as "معطّل" in the asset detail view

## Requirements *(mandatory)*

### Functional Requirements

**Attribute Definitions**

- **FR-001**: System MUST support 5 data types for attribute definitions: Text, Integer, Decimal, Date, Boolean
- **FR-002**: Each definition MUST have a unique Code (case-insensitive), a required Name, optional Description, optional Unit, optional SortOrder, and an IsActive flag (default: true)
- **FR-003**: System MUST prevent creating two definitions with the same Code
- **FR-004**: System MUST prevent changing a definition's Data Type when it has linked AssetAttributeValue rows (FR-021 from 058)
- **FR-005**: Deactivating a definition MUST NOT delete its existing values on assets; deactivated definitions MUST be excluded from new binding dropdowns
- **FR-006**: System MUST provide a list endpoint with search (by Code/Name), filter (by DataType, IsActive), and pagination

**Group Bindings**

- **FR-007**: Group-attribute bindings MUST use the composite identity (AssetGroupId, AssetAttributeDefinitionId) with IsRequired flag and optional SortOrder (FR-022 from 058)
- **FR-008**: Bindings MUST be saved atomically with the group in one operation (FR-039 from 058)
- **FR-009**: Absent SortOrder on a binding MUST fall back to the definition's default SortOrder
- **FR-010**: Removing a binding from a group MUST retain existing asset values for that definition and flag them for review (FR-026 from 058)

**Asset Values**

- **FR-011**: Attribute values MUST store exactly one filled value column matching the definition's data type (FR-023 from 058)
- **FR-012**: Values MUST be unique per (AssetId, AssetAttributeDefinitionId) (FR-024 from 058)
- **FR-013**: Zero and false MUST be treated as real values, not blanks (FR-024 from 058)
- **FR-014**: No value row MUST be created for an optional attribute left unentered (FR-025 from 058)
- **FR-015**: The card and its attribute values MUST be saved in one operation (FR-039 from 058)
- **FR-016**: Required-ness is validated from the group binding at save time, not from the definition alone

**Frontend - Attribute Definitions Management**

- **FR-017**: System MUST provide a dedicated page for managing attribute definitions with list/create/edit views
- **FR-018**: The definitions list MUST display Code, Name, Data Type (as badge), Unit, Status, and actions
- **FR-019**: The create/edit form MUST include all definition fields with appropriate input types for each data type
- **FR-020**: System MUST show a clear error when attempting to change Data Type on a definition with linked values

**Frontend - Group Attributes Management**

- **FR-021**: System MUST provide a dedicated page for managing group attribute bindings (separate from the group create/edit form)
- **FR-022**: The bindings page MUST display a table of bound definitions with editable Required toggle and Sort Order
- **FR-023**: Adding a new binding MUST show a dropdown of active definitions not yet bound to this group

**Frontend - Asset Form Dynamic Attributes**

- **FR-024**: When a group is selected on the asset form, a dynamic section MUST appear showing all bound definitions with type-appropriate input controls
- **FR-025**: Text → text input, Integer → number input (step=1), Decimal → number input (step=0.01), Date → date picker, Boolean → toggle switch
- **FR-026**: Required attributes MUST be visually marked with asterisk (*)
- **FR-027**: Changing the group on an existing asset MUST retain old values and show warnings for incompatible attributes

**Frontend - Asset Detail Attributes**

- **FR-028**: The asset detail page MUST include a tab or section displaying all attribute values
- **FR-029**: Empty optional attributes MUST show an em dash (—) indicator
- **FR-030**: Deactivated definitions MUST show a visual indicator (e.g., "معطّل" badge)

### Key Entities

- **Attribute Definition**: Defines a custom property (code, name, data type, unit, active status, sort order). Belongs to no specific group; reusable across groups.
- **Group Attribute Binding**: Links a definition to an asset group with required/optional flag and sort order. Composite key (group + definition).
- **Attribute Value**: Stores the actual value for one definition on one asset. Exactly one typed column filled per the definition's data type. Unique per (asset + definition).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An administrator can create a new attribute definition in under 30 seconds
- **SC-002**: An administrator can bind 5 attributes to a group in under 1 minute
- **SC-003**: A registrar can enter attribute values for a new asset in under 1 minute (for 5 attributes)
- **SC-004**: 100% of type-mismatch errors are caught and shown with clear field-level messages
- **SC-005**: 100% of required-attribute violations are caught before save
- **SC-006**: Asset detail page loads all attribute values without noticeable delay (< 2 seconds)
- **SC-007**: Deactivated definitions do not appear in new binding or value entry contexts
- **SC-008**: Changing an asset's group never deletes existing attribute values

## Assumptions

- The 5 data types (Text, Integer, Decimal, Date, Boolean) are sufficient for v1; no custom types or complex nested structures
- One value per (asset, definition) is sufficient; multi-value attributes are out of scope
- The generated web-api-client (nswag) will be regenerated after contract changes
- Existing EF migrations will be updated; no separate migration script generation
- Arabic is the primary UI language; English labels are not required in v1
- SortOrder is optional; when absent, definition-level default is used
- Attribute definitions are system-wide (not per-group); groups select from the global pool
