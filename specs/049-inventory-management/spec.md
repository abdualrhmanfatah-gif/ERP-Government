# Feature Specification: Inventory Management

**Feature Branch**: `049-inventory-management`

**Created**: 2026-09-11

**Status**: Draft

**Input**: User description: "إنشاء ميزة إدارة الأصناف والجداول المرتبطة بها ضمن Module 12: Inventory"

## User Scenarios & Testing

### User Story 1 - Manage Items (Priority: P1)

As a warehouse officer, I want to view, search, filter, create, edit, and activate/deactivate inventory items so I can maintain accurate item records.

**Why this priority**: Items are the core entity of the inventory module; all other inventory operations depend on them.

**Independent Test**: Navigate to `/inventory/items`, verify list loads with all columns, search by code/name/barcode, filter by category/unit/type/status, create a new item, edit it, toggle active status.

**Acceptance Scenarios**:

1. **Given** the user is on the items list page, **When** the page loads, **Then** a table displays with columns: Code, Name, NameEn, Category, Unit, Barcode, ItemType, AvailableQuantity, ReservedQuantity, AverageCost, MinimumStock, MaximumStock, ReorderLevel, IsActive, and actions.
2. **Given** the user types in the search box, **When** they enter a code, name, or barcode, **Then** the list filters to matching results.
3. **Given** the user selects a category filter, **When** a category is chosen, **Then** only items in that category are shown.
4. **Given** the user selects a unit filter, **When** a unit is chosen, **Then** only items with that unit are shown.
5. **Given** the user selects an item type filter, **When** a type is chosen, **Then** only items of that type are shown.
6. **Given** the user selects Active/Inactive status filter, **When** a status is chosen, **Then** only items with that status are shown.
7. **Given** the user selects "under reorder level" filter, **When** enabled, **Then** only items where AvailableQuantity <= ReorderLevel are shown.
8. **Given** the user clicks "Create Item", **When** they navigate to `/inventory/items/create`, **Then** an empty form is shown with fields for Code, Name, NameEn, Category, Unit, Barcode, ItemType, stock levels, and notes.
9. **Given** the user fills the create form with valid data, **When** they submit, **Then** the item is created and the user is navigated to the detail page.
10. **Given** the user navigates to `/inventory/items/:id`, **When** the page loads, **Then** the detail page shows basic info, stock levels, category, and associated item units.
11. **Given** the user clicks Edit on a detail page, **When** they navigate to `/inventory/items/:id/edit`, **Then** the form is pre-populated with current values.
12. **Given** the user clicks activate/deactivate on an item, **When** confirmed, **Then** the item's IsActive status toggles.

---

### User Story 2 - Manage Item Categories (Priority: P1)

As a warehouse officer, I want to manage item categories in a hierarchical tree so I can organize items logically.

**Why this priority**: Categories are required before items can be properly classified.

**Independent Test**: Navigate to `/inventory/item-categories`, verify tree/list display, create a category, edit it, create a child category, toggle active status.

**Acceptance Scenarios**:

1. **Given** the user is on the item categories page, **When** the page loads, **Then** categories are displayed in a hierarchical tree or list showing Code, Name, NameEn, Parent, Level, and IsActive.
2. **Given** the user clicks "Create Category", **When** the form opens, **Then** fields for Code, Name, NameEn, Description, Parent (optional dropdown), and account IDs are shown.
3. **Given** the user fills the form with valid data, **When** they submit, **Then** the category is created.
4. **Given** the user tries to set a category as its own parent, **When** they select the same category, **Then** a validation error prevents this.
5. **Given** the user clicks activate/deactivate on a category, **When** confirmed, **Then** the category's IsActive status toggles.

---

### User Story 3 - Manage Units (Priority: P1)

As a warehouse officer, I want to manage measurement units with base unit conversion so I can track items in different units.

**Why this priority**: Units are required for item creation and stock tracking.

**Independent Test**: Navigate to `/inventory/units`, verify list, create a unit, edit it, set base unit and conversion factor, toggle active status.

**Acceptance Scenarios**:

1. **Given** the user is on the units page, **When** the page loads, **Then** a list displays with Code, Name, UnitType, BaseUnit, ConversionToBase, and IsActive.
2. **Given** the user clicks "Create Unit", **When** the form opens, **Then** fields for Code, Name, UnitType, BaseUnit (optional dropdown), and ConversionToBase are shown.
3. **Given** the user selects a BaseUnit, **When** they enter ConversionToBase <= 0, **Then** a validation error prevents submission.
4. **Given** the user clicks activate/deactivate on a unit, **When** confirmed, **Then** the unit's IsActive status toggles.

---

### User Story 4 - Manage Item Units (Priority: P2)

As a warehouse officer, I want to link items to multiple units with conversion factors and designate one as the base unit.

**Why this priority**: Item units enable multi-unit stock tracking; important but depends on Items and Units being available.

**Independent Test**: On item detail page, verify item units section, add a unit to an item, set conversion factor, designate base unit.

**Acceptance Scenarios**:

1. **Given** the user views an item detail page, **When** the item units section loads, **Then** all linked units are displayed with Unit name, ConversionFactor, and IsBase flag.
2. **Given** the user adds a new unit to an item, **When** they select a unit and enter a conversion factor > 0, **Then** the unit is linked.
3. **Given** the user tries to add a second base unit to an item, **When** they set IsBase on a new unit while one is already base, **Then** the previous base unit's IsBase is automatically set to false.
4. **Given** the user enters ConversionFactor <= 0, **When** they submit, **Then** a validation error prevents submission.

---

### User Story 5 - Manage Warehouses (Priority: P2)

As a warehouse officer, I want to manage warehouses with capacity tracking so I can monitor storage utilization.

**Why this priority**: Warehouses are needed for stock transactions but are a supporting entity.

**Independent Test**: Navigate to `/inventory/warehouses`, verify list with capacity info, create warehouse, edit, toggle active.

**Acceptance Scenarios**:

1. **Given** the user is on the warehouses page, **When** the page loads, **Then** a list displays with Code, Name, Location, Manager, TotalCapacity, CurrentLoad, and IsActive.
2. **Given** the user clicks "Create Warehouse", **When** the form opens, **Then** fields for Code, Name, Address, City, Phone, Email, TotalCapacity, CurrentLoad are shown.
3. **Given** the user enters CurrentLoad > TotalCapacity, **When** they submit, **Then** a validation error prevents submission.
4. **Given** the user enters a negative TotalCapacity or CurrentLoad, **When** they submit, **Then** a validation error prevents submission.
5. **Given** the user enters an invalid email format, **When** they submit, **Then** a validation error prevents submission.
6. **Given** the user clicks activate/deactivate on a warehouse, **When** confirmed, **Then** the warehouse's IsActive status toggles.

---

### Edge Cases

- What happens when the user tries to delete a category that has child categories? → Prevention: must delete children first (Restrict FK)
- What happens when the user tries to delete a unit that is referenced by items? → Prevention: Restrict FK, cannot delete
- What happens when the user tries to deactivate an item that has open stock transactions? → System allows deactivation; stock transactions are historical records
- What happens when the user searches with no results? → Empty state message displayed
- What happens when the API returns an error? → 4xx errors inline, 5xx errors as toast
- What happens when the user rapidly clicks a save button? → Button disabled during mutation

## Requirements

### Functional Requirements

- **FR-001**: System MUST display a paginated list of items with all specified columns.
- **FR-002**: System MUST support search by item code, name, or barcode.
- **FR-003**: System MUST support filtering by category, unit, item type, active status, and under-reorder-level.
- **FR-004**: System MUST provide a create form with fields: Code (required, unique), Name (required), NameEn, Description, CategoryId, UnitId (required), SupplierId, Barcode, ItemType (required), OpeningStock, MinimumStock, MaximumStock, ReorderLevel, ReorderQuantity, LeadTimeDays, and IsActive.
- **FR-005**: System MUST validate: Code is required and unique, Name is required, UnitId is required, ItemType is required, quantities are non-negative, MinimumStock does not exceed MaximumStock if both provided.
- **FR-006**: System MUST provide an edit form pre-populated with existing data.
- **FR-007**: System MUST display item detail page with basic info, stock levels, category, and associated item units.
- **FR-008**: System MUST support activate/deactivate toggle for items.
- **FR-009**: System MUST display item categories in a hierarchical tree or flat list with parent information.
- **FR-010**: System MUST validate: Category Code is required and unique, Name is required, a category cannot be its own parent.
- **FR-011**: System MUST support activate/deactivate toggle for categories.
- **FR-012**: System MUST display units with base unit and conversion information.
- **FR-013**: System MUST validate: Unit Code is required and unique, Name is required, ConversionToBase > 0 if BaseUnitId is provided.
- **FR-014**: System MUST support activate/deactivate toggle for units.
- **FR-015**: System MUST link items to multiple units with ConversionFactor and IsBase flag.
- **FR-016**: System MUST validate: ItemId required, UnitId required, ConversionFactor > 0, exactly one base unit per item.
- **FR-017**: System MUST display warehouses with capacity information (TotalCapacity, CurrentLoad).
- **FR-018**: System MUST validate: Warehouse Code is required and unique, Name is required, Email valid if provided, TotalCapacity >= 0, CurrentLoad >= 0, CurrentLoad <= TotalCapacity if both provided.
- **FR-019**: System MUST support activate/deactivate toggle for warehouses.
- **FR-020**: System MUST use Result<T> for all endpoint responses.
- **FR-021**: System MUST require authorization with PermissionCodes for all endpoints.
- **FR-022**: System MUST use PaginatedList for all list endpoints with page/pageSize/search/filter.
- **FR-023**: System MUST display skeleton loading states during data fetch.
- **FR-024**: System MUST display error state with retry button when API fails.
- **FR-025**: System MUST display empty state when no results match filters.
- **FR-026**: System MUST disable action buttons during mutations to prevent double-submit.
- **FR-027**: System MUST display 4xx errors inline near the relevant form field.
- **FR-028**: System MUST display 5xx errors as toast notifications.
- **FR-029**: All UI text MUST be in Arabic. All layout MUST use RTL direction.
- **FR-030**: System MUST use logical CSS properties (ms-/me-/ps-/pe-) instead of physical properties.
- **FR-031**: System MUST render item codes and barcodes in LTR direction.
- **FR-032**: System MUST render monetary amounts with tabular-nums.

### Key Entities

- **Item**: Core inventory record with code, name, category, unit, barcode, type, stock levels, and cost information.
- **ItemCategory**: Hierarchical category with parent-child relationships and breadcrumb trail.
- **Unit**: Measurement unit with optional base unit conversion.
- **ItemUnit**: Junction entity linking items to units with conversion factor and base flag.
- **Warehouse**: Storage location with capacity tracking.

## Success Criteria

### Measurable Outcomes

- **SC-001**: User can create an item with all required fields in under 2 minutes.
- **SC-002**: Items list page loads and displays results within 3 seconds.
- **SC-003**: All form validations complete within 200ms of user interaction.
- **SC-004**: 100% of activate/deactivate actions require confirmation before execution.
- **SC-005**: All monetary amounts display with consistent formatting and tabular alignment.
- **SC-006**: Zero CSS physical properties in inventory feature code.
- **SC-007**: All interactive elements have aria-label attributes for accessibility.

## Assumptions

- Backend API endpoints for inventory entities need to be created (Application commands/queries and Web endpoints do not exist yet).
- Domain entities, EF configurations, seed data, and PermissionCodes already exist.
- The `api` utility in `shared/api/index.ts` handles authentication and error formatting.
- Existing shadcn components (Page, Button, Card, Badge, FilterBar, DataGrid, Dialog, Input, Select, Textarea, Combobox) are available.
- The `result-to-ui.ts` error mapping utility exists in `shared/api/`.
- No database schema changes are needed — existing entities and relationships are sufficient.
- ItemUnits management is accessed from the Item detail page, not as a separate page.
- Locations are out of scope for this feature ( Warehouses reference Locations but location management is separate).
- ItemType uses a fixed set of predefined values: Goods, Service, Raw Material, Consumable, Fixed Asset. The filter uses a dropdown with these options.

## Clarifications

### Session 2026-09-11

- Q: What are the allowed values for the ItemType field on items? → A: Fixed enum — Goods, Service, Raw Material, Consumable, Fixed Asset
