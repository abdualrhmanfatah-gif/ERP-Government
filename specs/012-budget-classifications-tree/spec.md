# Feature Specification: Budget Classifications Tree #2 of 5

**Feature Branch**: `012-budget-classifications-tree`

**Created**: 2026-09-04

**Status**: Draft

**Input**: User description: "Budgeting frontend #2 of 5 - Budget Classifications tree page. Hierarchical tree view with expand/collapse, level badges, search, create/edit dialog with parent tree-select, cycle prevention, route + sidebar activation."

## Clarifications

### Session 2026-09-04

- Q: Should search auto-expand ancestors of matching nodes or show flat list? → A: Auto-expand ancestors to show matches in context; non-matching branches stay collapsed.
- Q: What happens if backend returns malformed tree data (orphaned parentId, cycles)? → A: Treat malformed nodes as root-level; show warning toast.
- Q: Should parent tree-select exclude inactive nodes on edit? → A: No. Allow inactive nodes as parents (no exclusion beyond cycle prevention).
- Q: What is the initial expand state when the page loads? → A: First level expanded, rest collapsed.
- Q: Should the dialog stay open after save or close automatically? → A: Close automatically after successful save.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Budget Classification Tree View (Priority: P1)

A budget administrator navigates to the Budget Classifications page to view the full classification hierarchy as an expandable tree. Each node shows its code, name, and computed level. The tree supports RTL indentation, search across the entire hierarchy, and filtering by active state.

**Why this priority**: Budget Classifications are prerequisite reference data for BudgetItems (spec #3). Without the tree, there's no way to assign classification codes to budget lines.

**Independent Test**: Can be fully tested by navigating to /budgeting/budget-classifications, viewing the tree, expanding/collapsing nodes, searching for nodes, and filtering by active state.

**Acceptance Scenarios**:

1. **Given** a user with BudgetClassifications.View permission, **When** they navigate to /budgeting/budget-classifications, **Then** a tree view displays all budget classifications with Code, Name, and Level (as a numeric badge) per node.
2. **Given** the tree is rendered, **When** a node has children, **Then** an expand/collapse toggle is visible and children are indented correctly in RTL layout.
3. **Given** a node is expanded, **When** the user clicks the collapse toggle, **Then** all descendants are hidden and the toggle icon updates.
4. **Given** a user types search text, **When** the search matches a node's Code or Name, **Then** the matching node and all its ancestors are auto-expanded and visible, while non-matching branches remain collapsed.
5. **Given** the IsActive filter is applied, **When** the user selects "نشط" or "معطل", **Then** only matching nodes and their ancestors are displayed.
6. **Given** the tree has 5+ levels of nesting, **When** the tree is rendered, **Then** each level is indented correctly in RTL, no layout overflow occurs, and all nodes are accessible.

---

### User Story 2 - Create and Edit Budget Classifications (Priority: P1)

A budget administrator creates a new budget classification or edits an existing one via a dialog. The dialog includes a parent tree-select for establishing hierarchy, with cycle prevention to prevent selecting self or descendants.

**Why this priority**: CRUD operations are essential for maintaining the classification hierarchy as organizational needs change.

**Independent Test**: Can be fully tested by creating a root-level classification, creating a child classification, editing a classification's name/code, and verifying cycle prevention prevents invalid parent selection.

**Acceptance Scenarios**:

1. **Given** a user with BudgetClassifications.Create permission, **When** they click "إضافة تصنيف جديد", **Then** a dialog opens with fields: Code (required), Name (required), ParentId (optional tree-select), IsActive (switch).
2. **Given** a user selects a parent in the tree-select, **When** they confirm, **Then** the new classification is created as a child of the selected parent.
3. **Given** a user creates a classification without selecting a parent, **When** they confirm, **Then** the classification is created as a root-level node.
4. **Given** a user with BudgetClassifications.Update permission, **When** they click edit on a classification, **Then** the dialog opens pre-filled with current values and the ParentId tree-select excludes the classification itself and all its descendants (cycle prevention).
5. **Given** a user attempts to save with empty Code or Name, **When** they click save, **Then** validation errors are shown and the form is not submitted.
6. **Given** a classification is updated, **When** the save succeeds, **Then** the dialog closes, the tree refreshes, and the updated node shows new values.

---

### User Story 3 - Toggle Active State (Priority: P2)

A budget administrator toggles the active state of a budget classification. The toggle shows a confirmation dialog and handles RowVersion conflicts.

**Why this priority**: Deactivating obsolete classifications prevents them from being used in new budget items without deleting the hierarchy.

**Independent Test**: Can be tested by toggling a classification's active state and confirming the tree reflects the change.

**Acceptance Scenarios**:

1. **Given** a user with BudgetClassifications.Update permission, **When** they click the IsActive switch on a classification, **Then** a confirmation dialog appears.
2. **Given** the confirmation is accepted, **When** the mutation succeeds, **Then** the tree refreshes and a success toast is shown.
3. **Given** a RowVersion conflict occurs, **When** the backend rejects the mutation, **Then** an error toast is shown and the tree refetches.

---

### User Story 4 - Route and Sidebar Navigation (Priority: P1)

A user can reach the Budget Classifications page via the sidebar link in the "الموازنة" group. The route is registered and the sidebar shows the active state.

**Why this priority**: Without the route and sidebar link, the page is unreachable.

**Independent Test**: Can be tested by loading the app and verifying the sidebar link appears and navigates to the correct page.

**Acceptance Scenarios**:

1. **Given** the app loads, **When** the sidebar "الموازنة" group is rendered, **Then** a "التصنيفات المالية" link is visible.
2. **Given** the user clicks the sidebar link, **When** navigation completes, **Then** the Budget Classifications page is displayed at /budgeting/budget-classifications.
3. **Given** the user is on the Budget Classifications page, **When** the sidebar is rendered, **Then** the "التصنيفات المالية" link is highlighted as active.

---

## Functional Requirements *(mandatory)*

### FR-001: Classification Tree View

**Priority**: P1

**Description**: The Budget Classifications page displays all classifications as a hierarchical tree with expand/collapse, level badges, search, and filtering.

**Requirements**:

- FR-001.1: Tree view shows Code, Name, and Level (computed badge) for each classification node.
- FR-001.2: Nodes with children have an expand/collapse toggle. Children are indented in RTL layout. On initial page load, the first level of the tree is expanded and all deeper levels are collapsed.
- FR-001.2.1: The initial expand state is: root nodes with children are expanded; all deeper nodes are collapsed.
- FR-001.3: Search text matches against Code and Name across the entire tree. When matches are found, all ancestor nodes of matching nodes are auto-expanded to reveal the matches in their hierarchical context. Non-matching branches remain collapsed. If no text is entered, the tree returns to its previous expand/collapse state.
- FR-001.4: IsActive filter (نشط/معطل) shows only matching nodes and their ancestors.
- FR-001.5: Deep hierarchy (5+ levels) renders correctly with proper RTL indentation and no layout overflow.
- FR-001.6: Tree supports keyboard navigation: arrow keys expand/collapse nodes.

---

### FR-002: Create and Edit Dialog

**Priority**: P1

**Description**: Dialog-based create/edit for budget classifications with parent tree-select and cycle prevention.

**Requirements**:

- FR-002.1: Create dialog fields: Code (Input, required), Name (Input, required), ParentId (Combobox tree-select, optional), IsActive (Switch).
- FR-002.2: Edit dialog pre-fills all fields from the selected classification.
- FR-002.3: Parent tree-select on edit excludes only the classification itself and all its descendants (cycle prevention). Inactive nodes are NOT excluded — they remain selectable as parents.
- FR-002.4: Parent tree-select on create shows all classifications (no exclusion).
- FR-002.5: Validation: Code and Name are required. Empty values prevent submission.
- FR-002.6: Dialog is permission-gated: BudgetClassifications.Create for new, BudgetClassifications.Update for edit.

---

### FR-003: Toggle Active State

**Priority**: P2

**Description**: Switch-based toggle for IsActive with confirmation dialog.

**Requirements**:

- FR-003.1: IsActive switch visible per row, permission-gated (BudgetClassifications.Update).
- FR-003.2: Toggle shows ConfirmDialog before applying.
- FR-003.3: Success toast on mutation success.
- FR-003.4: RowVersion conflict: error toast + tree refetch.

---

### FR-004: Route and Sidebar

**Priority**: P1

**Description**: Route registration and sidebar link for Budget Classifications.

**Requirements**:

- FR-004.1: Route /budgeting/budget-classifications registered in app routes.
- FR-004.2: Sidebar "الموازنة" group includes "التصنيفات المالية" link with BudgetClassifications.View permission.
- FR-004.3: Active state detection highlights the sidebar link when on the page.

---

## Edge Cases *(mandatory)*

1. **Parent cycle prevention**: When editing a classification, the parent tree-select must exclude the classification itself and all its descendants to prevent circular references.
2. **Delete with children**: If a classification with children is deleted, the backend returns a Restrict error. The UI surfaces this as an explicit error toast.
3. **Root-level create**: Parent field is optional. Creating without a parent places the classification at the root level.
4. **Empty tree**: When no classifications exist, show an empty state with a create action.
5. **Single-node tree**: A root classification with no children renders without expand/collapse toggle.
6. **RTL overflow**: Deeply nested trees must not cause horizontal scrolling beyond the viewport.
7. **Malformed tree data**: If the backend returns a classification with a `parentId` referencing a non-existent node, or a cycle, the node is treated as root-level and a warning toast is shown. The tree still renders.

---

## Success Criteria *(mandatory)*

- **SC-001**: Build and TypeScript compilation produce zero errors.
- **SC-002**: 5+ level classification tree renders with correct RTL indentation at every level.
- **SC-003**: Cycle prevention prevents selecting self or descendants in parent tree-select; backend errors are surfaced.
- **SC-004**: RowVersion conflict handling shows error toast and refetches tree.
- **SC-005**: Dark mode renders all elements correctly; keyboard navigation (arrow keys) expands/collapses tree nodes.

---

## Key Entities

- **BudgetClassification**: id, code, name, parentId (nullable), level (computed), isActive, rowVersion
- **BudgetClassificationTree**: BudgetClassification + children array (recursive)

---

## Assumptions

1. Spec #1 (shared types/client/query keys) is fully landed and available.
2. Backend endpoints for BudgetClassifications (GET tree, GET by id, POST, PUT, toggle-active) are live.
3. Level is computed server-side in the DTO, not stored in the database.
4. Shared components (Dialog, FilterBar, FilterSearch, FilterSelect, Switch, Badge, ConfirmDialog, Button) are available from spec #1.
5. The existing usePermission hook stubs all permissions as granted during development.
6. Combobox component from the UI kit supports tree-select mode or will be adapted.
7. The backend enforces referential integrity on delete (Restrict) and returns ProblemDetails errors.

---

## Dependencies

- **Spec #011**: Budgeting Frontend Foundation (shared types, client, components)
- **Backend API**: BudgetClassifications endpoints per specs/010-rebuild-budgeting-module/contracts/budgeting-api.md

---

## Out of Scope

- Backend implementation of BudgetClassification endpoints
- Shared types/client modifications (consumed as-is from spec #1)
- BudgetItem management (spec #3)
- Other budgeting entities (Budgets, Appropriations, Encumbrances)
