# Feature Specification: Asset Register CRUD (سجل الأصول)

**Feature Branch**: `055-asset-register-crud`

**Created**: 2026-09-15

**Status**: Draft

**Input**: User description: "تقسيم وحدة الأصول إلى features صغيرة. F2 هو قلب الوحدة: إدارة الأصول الفردية (إنشاء/تعديل/عرض/تعطيل) — السجل الذي تعتمد عليه كل features الأخرى (F3–F9)."

## Clarifications

### Session 2026-09-15

- Q: When deactivating an asset, which specific status value should the system assign — "Disposed" or "WrittenOff"? → A: Disposed
- Q: What should the asset list display while data is loading from the server? → A: Skeleton/shimmer rows matching list column layout
- Q: Should the asset list default to showing only Active assets, or show all assets regardless of status? → A: Active only by default, with "Show All" toggle

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create New Asset Record (Priority: P1)

As an **Asset Accountant (محاسب الأصول)**, I want to register a new asset with all its details (classification, location, custodian, cost center, fund, financial values, dates, acquisition type, and unique identifiers) so that it becomes part of the official asset register and can be tracked.

**Why this priority**: Without the ability to create assets, no other asset features (acquisition, depreciation, disposal, inventory) can function. This is the foundation.

**Independent Test**: Can be fully tested by creating an asset via the form and verifying it appears in the list with a system-generated AST-xxxx code. Delivers immediate value: assets can now be registered instead of manual SQL inserts.

**Acceptance Scenarios**:

1. **Given** an asset accountant is on the asset list page, **When** they click "New Asset", **Then** a full entry form opens with all required fields
2. **Given** the form is filled with valid data (name, group, value, location, custodian, dates), **When** the user submits, **Then** the system generates an auto-numbered AssetCode (AST-xxxx), saves the asset in Draft status, and returns to the list
3. **Given** the user submits with missing required fields, **When** validation fails, **Then** field-level errors are displayed inline and the form preserves user input
4. **Given** an asset is created, **When** viewing the asset card, **Then** all entered fields are displayed correctly including the auto-generated code

---

### User Story 2 - View Asset List with Filtering and Search (Priority: P1)

As an **Asset Custodian (أمين الأصول)**, I want to browse and filter the asset register by group, status, location, custodian, or cost center, and search by code, name, tag, barcode, or serial number so that I can quickly locate any asset.

**Why this priority**: Viewing and searching is equally critical to creation — users need to find assets for maintenance requests, inventory checks, and daily operations.

**Independent Test**: Can be tested by populating test assets with varied attributes, then verifying that all filter combinations and search terms return correct results. Delivers value: assets are now findable.

**Acceptance Scenarios**:

1. **Given** the asset list page is loaded, **When** the user views the list, **Then** only Active assets are displayed by default with columns for code, name, group, status, location, custodian, and value
2. **Given** the user selects a filter (e.g., status = Active), **When** the filter is applied, **Then** only assets matching that criterion are shown
3. **Given** the user types a search term (e.g., "AST-001" or "HP LaserJet"), **When** the search executes, **Then** matching assets appear across code, name, tag, barcode, and serial fields
4. **Given** multiple filters are combined, **When** applied, **Then** results satisfy all active filters simultaneously
5. **Given** no results match the filters/search, **When** the list is empty, **Then** a clear "No assets found" message is displayed

---

### User Story 3 - Edit Asset Details (Priority: P1)

As an **Asset Accountant**, I want to update asset details (custodian, location, status, notes) so that the register reflects current reality.

**Why this priority**: Asset data changes frequently (custodian transfers, status updates). Without edit capability, the register becomes stale immediately.

**Independent Test**: Can be tested by creating an asset, modifying its custodian and status, and verifying the changes persist. Delivers value: assets stay accurate over time.

**Acceptance Scenarios**:

1. **Given** an asset exists in the system, **When** the user opens its edit form, **Then** all editable fields are pre-populated with current values
2. **Given** the user changes the custodian to a different person, **When** saving, **Then** the asset record is updated and the change is reflected immediately
3. **Given** the user changes the asset status (e.g., from Draft to Active), **When** saving, **Then** the status transition is validated against allowed transitions
4. **Given** the user attempts to edit a field that is locked (e.g., OriginalValue after activation), **When** the field is locked, **Then** the field is read-only with a visual indicator explaining why
5. **Given** the user saves with an optimistic-concurrency conflict, **When** the conflict is detected, **Then** an error message is shown and the user is prompted to refresh

---

### User Story 4 - View Asset Detail Card (Priority: P2)

As an **Auditor (المراجع)**, I want to view a complete asset card showing all its details, identifiers, and status so that I can audit or investigate a specific asset.

**Why this priority**: Read-only detail view supports audit and investigation workflows. Less critical than CRUD but essential for compliance.

**Independent Test**: Can be tested by clicking any asset in the list and verifying all fields display correctly on the detail page. Delivers value: full asset information is accessible.

**Acceptance Scenarios**:

1. **Given** the user clicks an asset in the list, **When** the asset card loads, **Then** all fields (code, name, group, status, location, custodian, cost center, fund, values, dates, identifiers) are displayed
2. **Given** the asset card is displayed, **When** the user views related information, **Then** acquisition type and status are shown with their display labels
3. **Given** the asset card is displayed, **When** the user navigates back, **Then** they return to the list at their previous scroll position and filter state

---

### User Story 5 - Deactivate (Disable) an Asset (Priority: P2)

As an **Asset Accountant**, I want to deactivate (disable) an asset that is no longer in service so that it is excluded from active operations but remains in the register for historical records.

**Why this priority**: Deactivation prevents use of disposed/retired assets without losing data. Soft-delete preserves audit trail.

**Independent Test**: Can be tested by deactivating an Active asset and verifying it disappears from default filtered lists but remains accessible via "Show All" filter. Delivers value: retired assets don't clutter operations.

**Acceptance Scenarios**:

1. **Given** an Active asset exists, **When** the user clicks "Deactivate" and confirms, **Then** the asset status changes to Disposed and it is no longer shown in default lists
2. **Given** an asset is in Disposed status, **When** the user applies "Show All" or filters for Disposed status, **Then** the disposed asset appears in results
3. **Given** an asset has pending depreciation schedules (F4), **When** deactivation is attempted, **Then** the system warns the user about dependent records before proceeding

---

### Edge Cases

- What happens when the user tries to create an asset with a duplicate AssetTag/Barcode/SerialNumber? → System MUST reject with a clear error indicating the duplicate identifier
- What happens when the DocumentSequenceService fails to generate an AssetCode? → System MUST return a user-friendly error and not create the asset without a code
- What happens when a user tries to edit an asset that another user just deactivated? → System MUST detect the conflict and inform the user of the current state
- What happens when the user tries to delete an asset? → Deletion is not supported; only deactivation is allowed. Delete operations MUST return a clear message
- What happens when the asset group reference is deleted while assets still reference it? → Foreign key constraint MUST prevent group deletion (Restrict), ensuring referential integrity
- What happens when a user searches with an empty search term? → All assets matching active filters are displayed without search filtering

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST generate a unique AssetCode with prefix "AST" using the DocumentSequenceService when creating a new asset
- **FR-002**: System MUST allow users to create assets with all entity fields: Name, Description, AssetGroup, Location, Custodian, CostCenter, Fund, OriginalValue, CurrentValue, PurchaseDate, SalvageValue, UsefulLifeMonths, DepreciationMethod, Status, AcquisitionType, AssetTag, Barcode, SerialNumber, ImageUrl, and Notes
- **FR-003**: System MUST assign Draft status to newly created assets by default, requiring explicit activation through the Acquisition workflow (F3)
- **FR-004**: System MUST enforce field locking after activation: OriginalValue, PurchaseDate, and AssetGroup MUST become read-only once the asset status is Active or beyond
- **FR-005**: System MUST provide a paginated list view with sortable columns and filterable fields (AssetGroup, Status, Location, Custodian, CostCenter, Fund). The default view MUST show only Active assets; a "Show All" option MUST be available to include all statuses.
- **FR-006**: System MUST support free-text search across AssetCode, Name, AssetTag, Barcode, and SerialNumber fields
- **FR-007**: System MUST allow users to edit asset details with optimistic-concurrency checking
- **FR-008**: System MUST support deactivation (soft-delete) by transitioning the asset to Disposed status, with a confirmation prompt before proceeding
- **FR-009**: System MUST enforce uniqueness constraints on AssetTag, Barcode, and SerialNumber (no duplicates across all assets)
- **FR-010**: System MUST validate that Status transitions follow allowed paths: Draft → Active, Active → UnderMaintenance, Active → Disposed, UnderMaintenance → Active, UnderMaintenance → Disposed. All other transitions MUST be rejected.
- **FR-011**: System MUST require authentication for all asset operations
- **FR-012**: System MUST enforce the following permissions: Assets.View, Assets.Create, Assets.Update, Assets.Delete
- **FR-013**: System MUST display asset detail cards with all fields including status and acquisition type labels
- **FR-014**: System MUST preserve user input when validation errors occur on the create/edit form
- **FR-015**: System MUST show "No assets found" when list filters/search return empty results
- **FR-015a**: System MUST display skeleton/shimmer rows matching the list column layout while asset data is loading
- **FR-016**: System MUST reject hard-deletion attempts with a clear message that only deactivation is permitted
- **FR-017**: System MUST prevent deletion of AssetGroup records that have assets referencing them (Restrict)
- **FR-018**: System MUST support Arabic-first RTL layout for all asset forms and list views
- **FR-019**: System MUST format monetary values (OriginalValue, CurrentValue, SalvageValue) using shared money-display conventions
- **FR-020**: System MUST log all create, update, and deactivation operations for audit purposes

### Key Entities

- **Asset**: The core entity representing a physical asset. Key attributes: AssetCode (auto-generated AST-xxxx), Name, Status, AssetGroup, Location, Custodian, CostCenter, Fund, OriginalValue, CurrentValue, PurchaseDate, AcquisitionType, AssetTag, Barcode, SerialNumber. Relationships: belongs to AssetGroup, references Location, Custodian (User), CostCenter, Fund.
- **AssetGroup**: Classification category for assets (e.g., "Vehicles", "Office Equipment", "Buildings"). Referenced by Asset. Must not be deletable while assets reference it.
- **AssetStatus (enum)**: Allowed values: Draft, Active, UnderMaintenance, Disposed, WrittenOff. Governs field locking and feature availability.
- **AcquisitionType (enum)**: Allowed values: Purchase, Grant, Transfer, Donation, Inherited. Categorizes how the asset was obtained.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Asset Accountant can create a new asset record in under 3 minutes (from opening form to successful save)
- **SC-002**: Asset Custodian can locate any specific asset by its tag, barcode, or serial number in under 30 seconds using search
- **SC-003**: Asset list loads and displays with filters applied in under 2 seconds for registers up to 10,000 assets
- **SC-004**: 100% of asset creation attempts result in a valid AST-xxxx code with no manual intervention
- **SC-005**: Zero duplicate assets created through AssetTag/Barcode/SerialNumber constraint enforcement
- **SC-006**: All asset operations (create, update, deactivate) produce audit log entries with actor, timestamp, and changed fields
- **SC-007**: Arabic-first RTL interface renders correctly on all asset screens with no layout breakage

## Assumptions

- The Asset entity (`src/Domain/Assets/Entities/Asset.cs`) with all 39 fields, EF configuration, and migrations are already complete and functional
- The AssetGroupSeedData provides at least 3 seed groups for initial binding
- The `AST` prefix is already reserved in DocumentSequenceService and ready for use
- Permissions `Assets.View`, `Assets.Create`, `Assets.Update`, `Assets.Delete` are already seeded
- The Inventory Items CRUD pattern (`src/Web/ClientApp/src/features/inventory/items/`) serves as the UI reference pattern
- The CQRS vertical slice pattern (`src/Application/Accounting/` and `src/Application/Inventory/`) serves as the backend reference pattern
- The existing Asset entity fields include all needed fields (no schema changes required for F2)
- Frontend uses the shared design token system and RTL-first layout conventions
- The asset register does not include accounting journal entries (that is F3's responsibility)
- Barcode/QR code printing is deferred to a future version (Parking Lot)
- Image upload is deferred; the ImageUrl field is stored but no upload UI is provided in v1
- Bulk import from Excel is deferred to v2
