# Feature Specification: Purchase Requests UI Completion

**Feature Branch**: `048-purchase-requests-ui`

**Created**: 2026-09-11

**Status**: Draft

**Input**: Frontend-only completion of Purchase Requests feature in Procurement module. No backend changes, no migrations, no frontend tests. Build on existing React 19 + TypeScript + Vite + React Router v7 + TanStack Query + React Hook Form + Zod + shadcn + Tailwind codebase.

## User Scenarios & Testing

### User Story 1 - View Purchase Requests List (Priority: P1)

As a procurement officer, I want to view a paginated list of all purchase requests so I can find and manage them efficiently.

**Why this priority**: Foundation for all other interactions; without a working list, no other action is possible.

**Independent Test**: Navigate to `/procurement/purchase-requests` and verify the list loads with columns, filters, search, and pagination.

**Acceptance Scenarios**:

1. **Given** the user is on the purchase requests list page, **When** the page loads, **Then** a table displays with columns: request number, request date, priority, status, estimated cost, line count, and actions.
2. **Given** the list is loading, **When** data has not yet arrived, **Then** skeleton placeholders are shown in place of rows.
3. **Given** the API returns an error, **When** the list fails to load, **Then** an error state with a retry button is displayed.
4. **Given** the API returns zero results, **When** the list is empty, **Then** an empty state message "لا توجد طلبات شراء" is shown.
5. **Given** the user types in the search box, **When** they enter a request number or note text, **Then** the list filters to matching results.
6. **Given** the user selects a status filter, **When** a status is chosen, **Then** only requests with that status are shown.
7. **Given** the user selects a priority filter, **When** a priority is chosen, **Then** only requests with that priority are shown.
8. **Given** the user changes page, **When** they click a pagination control, **Then** the corresponding page of results is loaded.

---

### User Story 2 - Create Purchase Request (Priority: P1)

As a procurement officer, I want to create a new purchase request with header data and line items so I can initiate the procurement process.

**Why this priority**: Core creation flow; required before any lifecycle actions can occur.

**Independent Test**: Navigate to `/procurement/purchase-requests/create`, fill the form, add lines, and submit. Verify the new request appears in the list.

**Acceptance Scenarios**:

1. **Given** the user is on the create page, **When** the form loads, **Then** empty fields are shown for request date, required date, department, cost center, priority, notes, and an empty lines table.
2. **Given** the user fills the request date and priority, **When** they add a line with item, unit, and quantity, **Then** the line appears in the table with a computed line total.
3. **Given** the user has added lines, **When** they review the form, **Then** the estimated total is displayed at the bottom.
4. **Given** the form has validation errors, **When** the user tries to submit, **Then** errors appear inline next to the invalid fields.
5. **Given** the form is valid, **When** the user submits, **Then** the request is created, a success occurs, and the user is navigated to the detail page.
6. **Given** the API returns a 4xx error, **When** the form is submitted, **Then** the error is displayed inline in the form.
7. **Given** the API returns a 5xx error, **When** the form is submitted, **Then** a toast notification shows the error.

---

### User Story 3 - View Purchase Request Detail (Priority: P1)

As a procurement officer, I want to view the full details of a purchase request including its lines, status, and available actions.

**Why this priority**: Essential for reviewing requests and performing lifecycle actions.

**Independent Test**: Navigate to `/procurement/purchase-requests/:id` and verify all data, lines, and action buttons are displayed correctly.

**Acceptance Scenarios**:

1. **Given** the user navigates to a request detail page, **When** the data loads, **Then** the header shows request number (LTR), status badge, priority badge, and a back button.
2. **Given** the detail page is loaded, **When** the user views the basic info card, **Then** request date, required date, department, cost center, estimated total, and notes are shown.
3. **Given** the detail page is loaded, **When** the user views the lines table, **Then** each line shows item, unit, quantity, unit cost estimate, and line total.
4. **Given** the request is in Draft status, **When** the user views the actions card, **Then** Submit and Edit buttons are shown.
5. **Given** the request is in Submitted status, **When** the user views the actions card, **Then** Approve and Reject buttons are shown.
6. **Given** the request is in Approved status, **When** the user views the actions card, **Then** a Cancel button is shown.
7. **Given** the request is in Rejected or Cancelled status, **When** the user views the actions card, **Then** no action buttons are shown.

---

### User Story 4 - Edit Purchase Request (Priority: P2)

As a procurement officer, I want to edit a purchase request that is still in Draft status so I can correct or update it before submission.

**Why this priority**: Important for correcting mistakes, but less critical than viewing and creating.

**Independent Test**: Navigate to `/procurement/purchase-requests/:id/edit` with a Draft request, modify fields, and save. Verify changes persist.

**Acceptance Scenarios**:

1. **Given** the user opens a Draft request for editing, **When** the form loads, **Then** all fields are pre-populated with current values including existing lines.
2. **Given** the user modifies the request date, priority, or notes, **When** they save, **Then** the changes are persisted and the user returns to the detail page.
3. **Given** the user adds or removes lines, **When** they save, **Then** the lines are updated atomically with the header.
4. **Given** the request is NOT in Draft status, **When** the user tries to access the edit page, **Then** they are redirected to the detail page with a message.
5. **Given** the form has validation errors, **When** the user tries to save, **Then** errors appear inline next to the invalid fields.

---

### User Story 5 - Lifecycle Actions with Confirmation (Priority: P2)

As a procurement officer, I want to perform lifecycle actions (Submit, Approve, Reject, Cancel) with proper confirmation dialogs so I don't accidentally change the status.

**Why this priority**: Prevents accidental status changes; important for data integrity.

**Independent Test**: Perform each lifecycle action and verify confirmation dialogs appear and status updates correctly.

**Acceptance Scenarios**:

1. **Given** the user clicks Submit on a Draft request, **When** the action succeeds, **Then** the status changes to Submitted and the detail page refreshes.
2. **Given** the user clicks Approve on a Submitted request, **When** the action succeeds, **Then** the status changes to Approved.
3. **Given** the user clicks Reject on a Submitted request, **When** a dialog appears asking for a reason, **Then** the user must provide a reason before confirming.
4. **Given** the user confirms rejection with a reason, **When** the action succeeds, **Then** the status changes to Rejected and the reason is recorded.
5. **Given** the user clicks Cancel on an Approved request, **When** a dialog appears asking for confirmation, **Then** the user must confirm before the action proceeds.
6. **Given** any lifecycle action fails, **When** the error is returned, **Then** the error message is displayed and the status is not changed.

---

### Edge Cases

- What happens when the user tries to create a request with zero lines? → Validation error: "يجب إضافة بند واحد على الأقل"
- What happens when the user tries to edit a non-Draft request? → Redirect to detail with message
- What happens when the request number is displayed? → Rendered LTR (left-to-right) regardless of page direction
- What happens when amounts are displayed? → Use tabular-nums font feature and "ر.ي" currency suffix
- What happens when the API returns a network error? → Toast notification with retry option
- What happens when the user rapidly clicks a lifecycle button? → Button is disabled during mutation to prevent double-submit

## Requirements

### Functional Requirements

- **FR-001**: System MUST display a paginated list of purchase requests with columns: request number, date, priority, status, estimated cost, line count, and actions.
- **FR-002**: System MUST support search by request number or notes text.
- **FR-003**: System MUST support filtering by status and priority.
- **FR-004**: System MUST display skeleton loading state during data fetch.
- **FR-005**: System MUST display error state with retry button when API fails.
- **FR-006**: System MUST display empty state when no results match filters.
- **FR-007**: System MUST provide a create form with fields: request date (required), required date (optional), department (optional), cost center (optional), priority (required), notes (optional), and dynamic line items.
- **FR-008**: Each line item MUST contain: item ID (required, selected via searchable combobox backed by a cached catalog query), unit ID (required, selected via searchable combobox backed by a cached catalog query), requested quantity (required, >0), unit cost estimate (optional), and notes (optional).
- **FR-009**: System MUST validate: at least one line, item ID >0, unit ID >0, quantity >0, valid priority, request date present.
- **FR-010**: System MUST compute line total (quantity × unit cost) and estimated total for display.
- **FR-011**: System MUST allow adding and removing lines dynamically in the create/edit form.
- **FR-012**: System MUST provide an edit form pre-populated with existing data, restricted to Draft status only.
- **FR-013**: System MUST display detail page with basic info, lines table, status badge, priority badge, and action buttons.
- **FR-014**: System MUST render request numbers in LTR direction.
- **FR-015**: System MUST render monetary amounts with tabular-nums and "ر.ي" suffix.
- **FR-016**: System MUST show Submit button for Draft requests, Approve/Reject for Submitted, Cancel for Approved.
- **FR-017**: Reject and Cancel actions MUST show a confirmation dialog requesting a reason before proceeding.
- **FR-018**: System MUST refresh list/detail data after successful create, update, or lifecycle action.
- **FR-019**: System MUST display 4xx errors inline near the relevant form field.
- **FR-020**: System MUST display 5xx errors as toast notifications.
- **FR-021**: System MUST disable action buttons during mutations to prevent double-submit.
- **FR-022**: Routes MUST include `/procurement/purchase-requests/create` and `/procurement/purchase-requests/:id/edit`.
- **FR-023**: All UI text MUST be in Arabic. All layout MUST use RTL direction.
- **FR-024**: System MUST use logical CSS properties (ms-/me-/ps-/pe-) instead of physical properties (ml-/mr-/pl-/pr-).
- **FR-025**: System MUST NOT import from other features. Shared components go in `src/components/` with `ProcurementPurchaseRequests*` prefix.

### Key Entities

- **PurchaseRequest**: Header record with number, date, required date, department, cost center, priority, status, estimated total, notes.
- **PurchaseRequestLine**: Line item with item reference, unit reference, quantity, unit cost estimate, line total.
- **PurchaseRequestStatus**: Draft → Submitted → Approved/Rejected → Cancelled lifecycle.
- **PurchaseRequestPriority**: Low, Normal, High, Urgent.

## Success Criteria

### Measurable Outcomes

- **SC-001**: User can create a purchase request with 3 lines in under 2 minutes.
- **SC-002**: List page loads and displays results within 3 seconds on standard connection.
- **SC-003**: All form validations complete within 200ms of user interaction.
- **SC-004**: 100% of lifecycle actions require confirmation before execution.
- **SC-005**: All monetary amounts display with consistent "ر.ي" formatting and tabular alignment.
- **SC-006**: Zero CSS physical properties (ml/mr/pl/pr/text-right/text-left) in procurement purchase request code.
- **SC-007**: All interactive elements have aria-label attributes for accessibility.

## Assumptions

- Backend API endpoints for Purchase Requests are already implemented and functional.
- The `api` utility in `shared/api/index.ts` handles authentication and error formatting.
- Existing shadcn components (Page, Button, Card, Badge, FilterBar, DataGrid, Dialog, Input, Select, Textarea, Combobox) are available.
- The `usePurchaseRequests` hooks already exist and provide correct query/mutation functions.
- Item and Unit selection use a searchable combobox/autocomplete showing item code + name.
- Department and Cost Center selection are optional and can use simple numeric input.
- The `result-to-ui.ts` error mapping utility exists in `shared/api/`.

## Clarifications

### Session 2026-09-11

- Q: How should users select Items and Units in the purchase request form? → A: Combobox / autocomplete — searchable dropdown showing item code + name
- Q: Where should item/unit display labels come from in the lines table? → A: Fetch item/unit catalogs once on form mount, cache in React Query, lookup by ID
