# Feature Specification: Asset Groups CRUD

**Feature Branch**: `054-asset-groups-crud`

**Created**: 2026-09-14

**Status**: Draft

**Input**: User description: "Asset Groups CRUD — إدارة مجموعات الأصول — full vertical slice from CQRS through API to React UI with tree display, GL account linking, and depreciation parameters"

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Create Asset Group (Priority: P1)

As an **asset manager**, I need to **create a new asset group** with a unique code, name, optional parent group, GL account links, and depreciation parameters, so that **new asset categories can be registered and assets can be classified under them**.

**Why this priority**: Without the ability to create groups, the entire Assets module is non-functional. This is the foundational CRUD operation.

**Independent Test**: Can be tested by creating a group via the UI and verifying it appears in the tree and list views.

**Acceptance Scenarios**:

1. **Given** the asset manager is on the Asset Groups page, **When** they click "إضافة مجموعة" and fill in Code, Name, and depreciation parameters, **Then** the group is created and appears in the tree under its parent (or as root if no parent).
2. **Given** a group with code "AG-004" already exists, **When** the asset manager tries to create another group with code "AG-004", **Then** the system rejects with a duplicate code error.
3. **Given** the asset manager selects a parent group, **When** they save, **Then** the new group is nested under the selected parent in the tree.
4. **Given** the asset manager leaves Code or Name empty, **When** they submit, **Then** validation errors appear for required fields.

---

### User Story 2 — View Asset Groups (Priority: P1)

As an **asset manager**, I need to **view all asset groups in a tree structure** with expand/collapse, and in a searchable list, so that **I can quickly find and understand the classification hierarchy**.

**Why this priority**: Viewing is co-dependent with creation — both are essential for the MVP.

**Independent Test**: Can be tested by verifying the tree renders existing seeded groups (AG-001, AG-002, AG-003) and the list shows all groups with correct columns.

**Acceptance Scenarios**:

1. **Given** seeded groups exist, **When** the asset manager opens the Asset Groups page, **Then** a tree displays the hierarchy with AG-001 (المباني), AG-002 (الأثاث), AG-003 (المركبات) as root nodes.
2. **Given** groups exist, **When** the asset manager types in the search box, **Then** the list filters by Code or Name.
3. **Given** a group has children, **When** the asset manager expands it, **Then** child groups are shown indented below.
4. **Given** a group is inactive, **When** displayed in the list or tree, **Then** it shows an inactive badge with muted styling (grayed out or struck through).

---

### User Story 3 — Edit Asset Group (Priority: P2)

As an **asset manager**, I need to **edit an existing asset group's** name, parent, GL accounts, and depreciation parameters, so that **classification data stays current without creating duplicates**.

**Why this priority**: Editing is needed after creation but is less critical than being able to create and view groups initially.

**Independent Test**: Can be tested by editing a group's depreciation method and verifying the change persists and is reflected in the detail view.

**Acceptance Scenarios**:

1. **Given** the asset manager opens a group's detail, **When** they change the Name and click save, **Then** the update is persisted and the list/tree reflect the new name.
2. **Given** a group has GL accounts linked, **When** the asset manager edits the group, **Then** all 7 GL account fields are editable.
3. **Given** the asset manager changes the DepreciationMethod, **When** they save, **Then** the change applies to new assets only (existing assets retain their original parameters).

---

### User Story 4 — Activate/Deactivate Asset Group (Priority: P2)

As an **asset manager**, I need to **deactivate an asset group** (IsActive = false) and **reactivate it** later, so that **I can control which groups appear in selection lists while preserving historical records**.

**Why this priority**: Activation lifecycle is essential for group management but secondary to create/view/edit.

**Independent Test**: Can be tested by deactivating a group, verifying it disappears from asset creation dropdowns, then reactivating it and verifying it reappears.

**Acceptance Scenarios**:

1. **Given** an active group with no assets, **When** the asset manager clicks deactivate, **Then** the group's IsActive is set to false and it shows as inactive in tree and list.
2. **Given** a group that has assets linked to it, **When** the asset manager tries to deactivate it, **Then** the system blocks with an error message explaining assets are still linked.
3. **Given** a group is deactivated, **When** creating a new asset, **Then** the deactivated group does not appear in the group selection dropdown.
4. **Given** a deactivated group, **When** the asset manager clicks the "تفعيل" button, **Then** the group's IsActive is set to true and it reappears in selection dropdowns.
5. **Given** an already-active group, **When** the asset manager views its actions, **Then** the "تفعيل" button is not shown (only "تعطيل" is available).

---

### User Story 5 — View Asset Group Detail (Priority: P2)

As an **asset manager**, I need to **view a single asset group's full details** including all GL accounts, depreciation parameters, and audit information, so that **I can verify configuration before linking assets**.

**Why this priority**: Detail view supports the edit and verification workflows.

**Independent Test**: Can be tested by navigating to a group's detail page and verifying all fields are displayed correctly.

**Acceptance Scenarios**:

1. **Given** a group exists, **When** the asset manager navigates to its detail page, **Then** all fields (Code, Name, Parent, 7 GL accounts, depreciation params, IsActive, audit fields) are displayed.
2. **Given** a group is inactive, **When** viewing its detail, **Then** the inactive status is clearly shown.

---

### Edge Cases

| ID | Scenario | Expected Behavior |
|----|----------|-------------------|
| EC-001 | Create group with duplicate Code | Server returns error "كود المجموعة مستخدم بالفعل" |
| EC-002 | Create group with empty required fields | Validation error for Code and Name |
| EC-003 | Set parent to self (circular reference) | Server rejects — group cannot be its own parent |
| EC-004 | Set parent to a descendant (cycle prevention) | Server rejects — would create a cycle in the tree |
| EC-005 | Deactivate group that has active child groups | Allowed — children remain active (independent operation) |
| EC-006 | Deactivate group that has linked assets | Blocked — must remove or reassign assets first |
| EC-007 | Delete group (hard delete attempt) | Not supported — only deactivation is allowed |
| EC-008 | Edit group that is inactive | Blocked — must activate before editing |
| EC-009 | Concurrent edit (optimistic concurrency conflict) | Server returns conflict error |
| EC-010 | Create root group (no parent) | Allowed — group appears as root in tree |
| EC-011 | Move group to different parent | Allowed via edit — parent field is editable |
| EC-012 | Search with no matching results | Empty state message displayed |
| EC-013 | Inactive group in tree display | Shown with muted styling and inactive badge, not hidden |
| EC-014 | Activate already-active group | Server returns error "المجموعة مفعلة بالفعل" |
| EC-015 | Deactivate already-inactive group | Server returns error "المجموعة معطلة بالفعل" |
| EC-016 | Exceed 3-level hierarchy depth | Server returns error "تم تجاوز الحد الأقصى لمستويات التصنيف" |
| EC-017 | Link asset to group with incomplete GL accounts | Warning displayed; financial posting blocked until all 7 accounts are configured |

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a tree view displaying all asset groups (including inactive) in hierarchical order based on ParentId self-referencing FK. Inactive groups MUST appear with muted styling and an inactive badge.
- **FR-002**: System MUST provide a list view with columns: Code, Name, Parent Name, IsActive, and actions.
- **FR-003**: System MUST allow creating a new asset group with Code (required, unique), Name (required), ParentId (optional), and IsActive (default true).
- **FR-004**: System MUST allow editing all group fields except Code (immutable after creation).
- **FR-005**: System MUST allow deactivating a group (IsActive = false) and reactivating it (IsActive = true). Hard-deletion is NOT supported.
- **FR-005a**: System MUST provide a dedicated "تفعيل" (activate) action for inactive groups, separate from the edit workflow.
- **FR-006**: System MUST prevent deactivation of a group that has linked assets (Asset.AssetGroupId FK).
- **FR-007**: System MUST link 7 GL accounts to each group: AssetAccount, AccumulatedDepreciationAccount, DepreciationExpenseAccount, DisposalAccount, GainOnDisposalAccount, LossOnDisposalAccount, MaintenanceExpenseAccount. GL accounts are optional at creation time; the system MUST warn when the first asset is linked if accounts are incomplete, and MUST block any financial posting until all 7 accounts are configured.
- **FR-008**: System MUST store depreciation parameters per group: DepreciationMethod, DefaultUsefulLifeYears, DefaultSalvageValuePercentage.
- **FR-009**: System MUST prevent circular parent references (a group cannot be its own ancestor).
- **FR-009a**: System MUST enforce a maximum hierarchy depth of 3 levels. Groups beyond the 3rd level MUST be rejected.
- **FR-010**: System MUST support search/filter by Code or Name in the list view.
- **FR-011**: System MUST enforce permission checks: AssetGroups.View, AssetGroups.Create, AssetGroups.Update, AssetGroups.Deactivate, AssetGroups.Activate.
- **FR-012**: System MUST use optimistic concurrency (RowVersion) on all mutations.
- **FR-013**: System MUST return Arabic error messages for all validation failures.
- **FR-014**: System MUST maintain audit fields (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy) on all records.

### Key Entities

- **AssetGroup**: Hierarchical classification of assets. Key attributes: Code (unique identifier), Name (display name), ParentId (self-referencing FK for tree), IsActive (soft-delete flag), 7 GL account references, 3 depreciation parameters, RowVersion (optimistic concurrency), audit fields.
- **Asset**: Linked to AssetGroup via mandatory AssetGroupId FK. The presence of assets prevents group deactivation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Asset manager can create a new asset group in under 2 minutes including all GL account links.
- **SC-002**: Tree view loads and renders the full hierarchy (up to 100 groups) in under 1 second.
- **SC-003**: Search/filter returns results in under 500ms.
- **SC-004**: 100% of CRUD operations enforce server-side permission checks.
- **SC-005**: Zero circular reference incidents — cycle prevention is validated on every parent change.
- **SC-006**: Deactivation of a group with assets is always blocked with a clear Arabic error message.

## Clarifications

### Session 2026-09-14

- Q: How should the tree view display deactivated groups? → A: Show in tree with inactive badge and muted styling.
- Q: How should a deactivated asset group be reactivated? → A: Dedicated "تفعيل" button that toggles IsActive back to true.
- Q: What is the maximum depth of the asset group hierarchy? → A: 3 levels (root → subcategory → group).
- Q: When should GL accounts be validated on an asset group? → A: Optional at creation; warn when first asset is linked; block financial posting until all 7 accounts configured.
- Q: Should asset group operations have additional audit logging beyond entity fields? → A: No — standard entity audit fields (CreatedAt/By, ModifiedAt/By) are sufficient.

## Assumptions

- The AssetGroup entity, EF configuration, and seed data (3 groups) already exist and are correct.
- The 7 GL account IDs reference valid accounts from the Chart of Accounts (managed separately in F2/F3).
- Depreciation parameters are inherited by new assets at creation time; changes to group parameters do not retroactively affect existing assets.
- Deactivating a parent group does NOT cascade to child groups — children remain in their current active/inactive state.
- The Code field is entered manually by the user (not auto-generated) — semantic codes like "AG-001" are preferred.
- GL account linking is optional at group creation time but must be completed before the group is used for asset posting.
- The existing permissions (AssetGroups.View, AssetGroups.Create, AssetGroups.Update, AssetGroups.Deactivate) seeded in PermissionCodes.cs are the authority.
- The UI follows Arabic-first RTL layout consistent with the existing design system.
