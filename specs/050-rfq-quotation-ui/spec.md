# Feature Specification: RFQ & Quotation Management

**Feature Branch**: `050-rfq-quotation-ui`

**Created**: 2026-09-11

**Status**: Draft

**Input**: User description: "إنشاء ميزة إدارة عروض الأسعار والجداول المرتبطة بها ضمن Module 6: Procurement، اعتمادًا على الكود الحالي في specs/047-procurement-lifecycle و Domain/Procurement و Application/Procurement و Web/Endpoints/Procurement وواجهة procurement الحالية. الميزة تشمل RequestForQuotation و RFQSupplier و Quotation و QuotationDetail."

## User Scenarios & Testing

### User Story 1 - Create and Manage RFQs (Priority: P1)

A procurement officer creates a Request for Quotation (RFQ) linked to an approved Purchase Request. The officer selects suppliers from the Parties module, sets submission deadlines and currency, publishes the RFQ to invited suppliers, monitors response status, and closes collection when all suppliers have responded or the deadline has passed.

**Why this priority**: RFQ is the entry point for competitive procurement. Without a working RFQ flow, no quotations can be collected or evaluated.

**Independent Test**: Create an RFQ from an approved PR, invite 3 suppliers, publish it, record responses from 2 suppliers (one expires), close collection, verify RFQ status transitions and supplier response tracking.

**Acceptance Scenarios**:

1. **Given** a procurement officer on the RFQ list page, **When** the page loads, **Then** a paginated table displays RFQs with columns: RFQNumber, RFQDate, PurchaseRequestId, Status, DeadlineDate, CurrencyCode, SupplierCount, Created, and action buttons.
2. **Given** the officer clicks "Create RFQ", **When** they navigate to the create page, **Then** a form is shown with PurchaseRequestId (required dropdown of approved PRs), DeadlineDate (required), CurrencyCode (optional), TermsAndConditions, Notes, and a supplier selection section.
3. **Given** the officer fills the RFQ form with a valid PR, at least one supplier, and a DeadlineDate not before RFQDate, **When** they submit, **Then** the RFQ is created in Draft status with a generated RFQ-{D6} number.
4. **Given** an RFQ in Draft, **When** the officer clicks Publish, **Then** status transitions to Published and the RFQ is visible to invited suppliers.
5. **Given** a Published or CollectingResponses RFQ, **When** the officer clicks Close, **Then** status transitions to Closed and no further responses are accepted.
6. **Given** an RFQ in any status except Closed or Cancelled, **When** the officer clicks Cancel with confirmation, **Then** status transitions to Cancelled.
7. **Given** the officer searches by RFQ number or notes, **When** they enter text in the search box, **Then** the list filters to matching results.
8. **Given** the officer filters by status or deadline date, **When** filters are applied, **Then** only matching RFQs are shown.
9. **Given** an RFQ in Draft status, **When** the officer clicks Edit, **Then** the edit form opens at `/procurement/rfqs/:id/edit` pre-populated with all current values.
10. **Given** the edit form is open, **When** the officer modifies fields and saves, **Then** changes are persisted and the user is returned to the detail page.
11. **Given** an RFQ not in Draft status, **When** the officer views the detail page, **Then** the Edit button is not shown.

---

### User Story 2 - View RFQ Details and Supplier Responses (Priority: P1)

A procurement officer views the full details of an RFQ including its basic data, invited suppliers with their response status, and links to quotations submitted by each supplier. The officer can record a supplier's response directly from the detail page.

**Why this priority**: The RFQ detail page is the central hub for monitoring the bidding process and tracking which suppliers have responded.

**Independent Test**: Navigate to an RFQ detail page, verify basic info display, verify invited suppliers list with status, verify links to submitted quotations, record a supplier response.

**Acceptance Scenarios**:

1. **Given** the officer navigates to `/procurement/rfqs/:id`, **When** the page loads, **Then** the detail page displays: RFQNumber, RFQDate, PurchaseRequestId, Status, DeadlineDate, CurrencyCode, TermsAndConditions, Notes, and the list of invited suppliers.
2. **Given** the RFQ detail page, **When** the supplier list loads, **Then** each supplier shows: SupplierPartyId (name), InvitationDate, ResponseDate, Status (Invited/Responded/Expired/Declined), and a link to their quotation if submitted.
3. **Given** the officer views the supplier list, **When** they click "Record Response" for an Invited supplier, **Then** a dialog opens to record the response date and notes, updating the supplier status to Responded.
4. **Given** the RFQ is in Draft status, **When** the officer views available actions, **Then** Publish and Cancel buttons are shown.
5. **Given** the RFQ is in Published or CollectingResponses status, **When** the officer views available actions, **Then** Close and Cancel buttons are shown.
6. **Given** the RFQ is in Closed or Cancelled status, **When** the officer views available actions, **Then** no action buttons are shown (read-only).

---

### User Story 3 - Create and Manage Quotations (Priority: P1)

A procurement officer creates a quotation for a supplier who has been invited to an RFQ. The quotation includes line items linked to the PR details, with unit prices, discounts, taxes, and computed totals. The officer can edit draft quotations, submit them for evaluation, and manage the full evaluation lifecycle through to award or rejection.

**Why this priority**: Quotation management is the core competitive evaluation mechanism. It determines which supplier and pricing will be used for the Purchase Order.

**Independent Test**: Create a quotation for an RFQ supplier, add line items with pricing, submit for evaluation, start evaluation, complete evaluation with scores, select winner, award. Verify all status transitions.

**Acceptance Scenarios**:

1. **Given** the officer navigates to the quotations list, **When** the page loads, **Then** a paginated table displays: QuotationNumber, RFQId, SupplierPartyId, QuotationDate, ValidUntil, Status, GrandTotal, Created, and action buttons.
2. **Given** the officer clicks "Create Quotation", **When** the form opens, **Then** fields for RFQId (required), RFQSupplierId (required), SupplierPartyId (required), QuotationDate (required), ValidUntil, CurrencyCode, ExchangeRate, PaymentTerms, DeliveryTerms, LeadTimeDays, WarrantyPeriodMonths, and a line items section are shown.
3. **Given** the officer adds line items, **When** each item is added with Quantity (>0), UnitPrice (≥0), DiscountPercent (0-100), and TaxPercent (0-100), **Then** the system computes SubTotal, DiscountAmount, TaxAmount, and GrandTotal for display.
4. **Given** a quotation in Draft, **When** the officer clicks Submit, **Then** status transitions to Submitted.
5. **Given** a Submitted quotation, **When** the officer clicks Start Evaluation, **Then** status transitions to UnderEvaluation.
6. **Given** an UnderEvaluation quotation, **When** the officer clicks Complete Evaluation and provides TechnicalScore and FinancialScore, **Then** status transitions to Evaluated.
7. **Given** an Evaluated quotation, **When** the officer clicks Select and provides a SelectionReason in a required dialog, **Then** status transitions to Selected.
8. **Given** a Selected quotation, **When** the officer clicks Award, **Then** status transitions to Awarded.
9. **Given** an UnderEvaluation or Evaluated quotation, **When** the officer clicks Reject and provides a RejectionReason in a required dialog, **Then** status transitions to Rejected.
10. **Given** the officer searches by quotation number, **When** they enter text, **Then** the list filters to matching results.
11. **Given** the officer filters by RFQId, SupplierPartyId, or Status, **When** filters are applied, **Then** only matching quotations are shown.

---

### User Story 4 - View Quotation Details (Priority: P1)

A procurement officer views the full details of a quotation including commercial terms, line items with computed totals, evaluation scores, selection/rejection reasons, and the linked RFQ and supplier information.

**Why this priority**: The quotation detail page is where evaluation decisions are made and recorded. It must show all information needed for informed award decisions.

**Independent Test**: Navigate to a quotation detail page, verify all commercial data, line items, evaluation scores, status, and available actions.

**Acceptance Scenarios**:

1. **Given** the officer navigates to `/procurement/quotations/:id`, **When** the page loads, **Then** the detail page displays: QuotationNumber, RFQId, SupplierPartyId, QuotationDate, ValidUntil, Status, CurrencyCode, ExchangeRate, PaymentTerms, DeliveryTerms, LeadTimeDays, WarrantyPeriodMonths, SubTotal, DiscountAmount, TaxAmount, GrandTotal, SelectionReason, RejectionReason, TechnicalScore, FinancialScore, and the list of line items.
2. **Given** the quotation detail page, **When** the line items load, **Then** each item shows: Item name, Unit, Quantity, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, LineTotal, LineTotalWithTax, and Notes.
3. **Given** the quotation is in Draft status, **When** the officer views available actions, **Then** Submit and Edit buttons are shown.
4. **Given** the quotation is in Submitted status, **When** the officer views available actions, **Then** Start Evaluation button is shown.
5. **Given** the quotation is in UnderEvaluation status, **When** the officer views available actions, **Then** Complete Evaluation and Reject buttons are shown.
6. **Given** the quotation is in Evaluated status, **When** the officer views available actions, **Then** Select button is shown.
7. **Given** the quotation is in Selected status, **When** the officer views available actions, **Then** Award button is shown.
8. **Given** the quotation is in Rejected, Awarded, or Expired status, **When** the officer views available actions, **Then** no action buttons are shown (read-only).

---

### User Story 5 - Edit Draft Quotations (Priority: P2)

A procurement officer can edit a quotation that is still in Draft status, modifying commercial terms and line items before submitting for evaluation.

**Why this priority**: Editing capability prevents the need to delete and recreate quotations when corrections are needed during the drafting phase.

**Independent Test**: Create a quotation, verify it's in Draft, edit line items and commercial terms, verify changes are saved, submit the edited quotation.

**Acceptance Scenarios**:

1. **Given** a quotation in Draft status, **When** the officer clicks Edit, **Then** the edit form opens pre-populated with all current values.
2. **Given** the edit form is open, **When** the officer modifies fields and saves, **Then** changes are persisted and the user is returned to the detail page.
3. **Given** a quotation not in Draft status, **When** the officer views the detail page, **Then** the Edit button is not shown.

---

### Edge Cases

- What happens when the user tries to create an RFQ without selecting any suppliers? → Validation error: must select at least one supplier.
- What happens when the user sets a DeadlineDate before the RFQDate? → Validation error: deadline cannot be before RFQ date.
- What happens when the user tries to publish an RFQ with no suppliers? → Validation error: at least one supplier required.
- What happens when the user tries to submit a quotation with no line items? → Validation error: at least one line item required.
- What happens when the user enters Quantity ≤ 0 on a quotation line? → Validation error: quantity must be greater than zero.
- What happens when the user enters DiscountPercent or TaxPercent outside 0-100? → Validation error: must be between 0 and 100.
- What happens when the user tries to complete evaluation without providing scores? → Validation error: TechnicalScore and FinancialScore are required.
- What happens when the user tries to select a quotation without providing a reason? → Validation error: SelectionReason is required.
- What happens when the user tries to reject a quotation without providing a reason? → Validation error: RejectionReason is required.
- What happens when the API returns a 4xx error? → Error displayed inline near the relevant form field.
- What happens when the API returns a 5xx error? → Error displayed as a toast notification.
- What happens when the user rapidly clicks a save/submit button? → Button is disabled during mutation to prevent double-submit.
- What happens when data is loading? → Skeleton placeholders are shown.
- What happens when no results match filters? → Empty state message is displayed.

## Requirements

### Functional Requirements

#### RFQ Management

- **FR-001**: System MUST display a paginated list of RFQs with columns: RFQNumber, RFQDate, PurchaseRequestId, Status, DeadlineDate, CurrencyCode, SupplierCount, Created, and actions.
- **FR-002**: System MUST support search by RFQ number or notes.
- **FR-003**: System MUST support filtering by status and deadline date.
- **FR-004**: System MUST provide an RFQ create form with: PurchaseRequestId (required, dropdown of approved PRs), DeadlineDate (required), CurrencyCode (optional), TermsAndConditions, Notes, and supplier selection (minimum one supplier required).
- **FR-005**: System MUST validate: PurchaseRequestId is required, at least one supplier must be selected, DeadlineDate must not be before RFQDate, CurrencyCode if provided must be a valid currency code.
- **FR-006**: System MUST display RFQ detail page with basic data, list of invited suppliers with their response status, and links to submitted quotations.
- **FR-006a**: System MUST provide an RFQ edit form for Draft-status quotations, pre-populated with current values, allowing modification of DeadlineDate, CurrencyCode, TermsAndConditions, Notes, and supplier list.
- **FR-007**: System MUST show available actions based on RFQ status: Draft → Publish and Edit, Published/CollectingResponses → Close, any non-Closed/Cancelled → Cancel.
- **FR-008**: Cancel action MUST require a confirmation dialog before execution.
- **FR-009**: RFQ list and detail pages MUST use skeleton loading states during data fetch.
- **FR-010**: RFQ list and detail pages MUST display error state with retry button on API failure.
- **FR-011**: RFQ list page MUST display empty state when no results match filters.

#### RFQ Supplier Response

- **FR-012**: System MUST allow recording a supplier's response to an RFQ invitation via a dedicated endpoint: PATCH /api/RequestForQuotations/suppliers/{rfqSupplierId}/response.
- **FR-013**: Recording a response MUST update the RFQSupplier status to Responded and set the ResponseDate.
- **FR-014**: The response recording MUST be available from the RFQ detail page for suppliers in Invited status.

#### Quotation Management

- **FR-015**: System MUST display a paginated list of quotations with columns: QuotationNumber, RFQId, SupplierPartyId, QuotationDate, ValidUntil, Status, GrandTotal, Created, and actions.
- **FR-016**: System MUST support search by quotation number.
- **FR-017**: System MUST support filtering by RFQId, SupplierPartyId, and Status.
- **FR-018**: System MUST provide a quotation create form with: RFQId (required), RFQSupplierId (required), SupplierPartyId (required), QuotationDate (required), ValidUntil, CurrencyCode, ExchangeRate, PaymentTerms, DeliveryTerms, LeadTimeDays, WarrantyPeriodMonths, and a line items section.
- **FR-019**: System MUST validate: RFQId is required, RFQSupplierId is required, SupplierPartyId is required, QuotationDate is required, at least one line item is required, Quantity > 0, UnitPrice ≥ 0, DiscountPercent between 0 and 100, TaxPercent between 0 and 100.
- **FR-020**: System MUST compute and display SubTotal, DiscountAmount, TaxAmount, and GrandTotal on the form based on line item values (display only, not sent to API unless the contract requires it).
- **FR-021**: System MUST provide a quotation edit form for Draft-status quotations, pre-populated with current values.
- **FR-022**: System MUST display quotation detail page with all commercial data, line items, evaluation scores, selection/rejection reasons, and linked RFQ/supplier information.
- **FR-023**: System MUST show available actions based on quotation status: Draft → Submit, Submitted → StartEvaluation, UnderEvaluation → CompleteEvaluation or Reject, Evaluated → Select, Selected → Award.
- **FR-024**: CompleteEvaluation action MUST require TechnicalScore and FinancialScore inputs.
- **FR-025**: Select action MUST require SelectionReason in a mandatory dialog.
- **FR-026**: Reject action MUST require RejectionReason in a mandatory dialog.
- **FR-027**: Quotation list and detail pages MUST use skeleton loading states during data fetch.
- **FR-028**: Quotation list and detail pages MUST display error state with retry button on API failure.
- **FR-029**: Quotation list page MUST display empty state when no results match filters.

#### Backend Endpoint Fixes

- **FR-030**: GET /api/Quotations MUST return a paginated list of quotations using GetQuotationsQuery (currently missing).
- **FR-031**: GET /api/Quotations/{id} MUST return quotation details using GetQuotationByIdQuery (currently returns only `{ Id }`).
- **FR-032**: PATCH /api/Quotations/{id}/submit MUST be available for submitting draft quotations (currently missing).
- **FR-033**: PATCH /api/Quotations/{id}/reject MUST be available for rejecting quotations with RejectionReason (currently missing).
- **FR-033a**: PUT /api/Quotations/{id} MUST be available for updating draft quotations via a dedicated UpdateQuotationCommand (currently missing).
- **FR-034**: PATCH /api/RequestForQuotations/suppliers/{rfqSupplierId}/response MUST be available for recording supplier responses (currently missing).

#### Backend Validators

- **FR-035**: CreateRFQCommandValidator MUST validate: PurchaseRequestId required, at least one supplier, DeadlineDate not before RFQDate.
- **FR-036**: CreateQuotationCommandValidator MUST validate: RFQId required, RFQSupplierId required, SupplierPartyId required, QuotationDate required, at least one detail line, Quantity > 0, UnitPrice ≥ 0, DiscountPercent 0-100, TaxPercent 0-100.

#### UI/UX Standards

- **FR-037**: All UI text MUST be in Arabic. All layout MUST use RTL direction.
- **FR-038**: System MUST use logical CSS properties (ms-/me-/ps-/pe-) instead of physical properties (ml/mr/pl/pr).
- **FR-039**: RFQNumber and QuotationNumber MUST render in LTR direction (dir="ltr").
- **FR-040**: All monetary amounts MUST use tabular-nums font feature and clear currency formatting.
- **FR-041**: All interactive elements MUST have aria-label attributes for accessibility.
- **FR-042**: System MUST use existing shadcn primitives and lucide-react icons.
- **FR-043**: Feature-scoped components MUST be placed in src/components/ with ProcurementRFQ* or ProcurementQuotations* prefix (not inside features/).
- **FR-044**: No imports between features. Cross-feature sharing via shared/ or events only.
- **FR-045**: 4xx errors MUST display inline near the relevant form field. 5xx errors MUST display as toast notifications via shared/api/result-to-ui.ts.
- **FR-046**: Action buttons MUST be disabled during mutations to prevent double-submit.
- **FR-047**: All list endpoints MUST use PaginatedList with page/pageSize/search/filter parameters.

#### Routes

- **FR-048**: System MUST provide the following routes:
  - `/procurement/rfqs` — RFQ list page
  - `/procurement/rfqs/create` — RFQ create page
  - `/procurement/rfqs/:id` — RFQ detail page
  - `/procurement/rfqs/:id/edit` — RFQ edit page (Draft status only)
  - `/procurement/rfqs/:id/quotations/create` — Quotation create page (linked to specific RFQ)
  - `/procurement/quotations` — Quotation list page
  - `/procurement/quotations/:id` — Quotation detail page
  - `/procurement/quotations/:id/edit` — Quotation edit page (Draft status only)

#### Authorization

- **FR-049**: All RFQ endpoints MUST require authorization with PermissionCodes: RFQView, RFQCreate, RFQPublish, RFQComplete, RFQCancel.
- **FR-050**: All Quotation endpoints MUST require authorization with PermissionCodes: QuotationsView, QuotationsCreate, QuotationsEvaluate, QuotationsSelect, QuotationsAward, QuotationsReject.

### Key Entities

- **RequestForQuotation**: Competitive bidding document linked to an approved Purchase Request. Contains RFQ number, dates, deadline, currency, terms, status, and a collection of invited suppliers.
- **RFQSupplier**: Supplier invitation and response tracking on an RFQ. Records invitation date, response date, and response status per supplier.
- **Quotation**: Supplier offer in response to an RFQ. Contains commercial terms (currency, exchange rate, payment/delivery terms, warranty), evaluation data (technical/financial scores), selection/rejection reasons, status, and a collection of line items.
- **QuotationDetail**: Line item on a quotation. Links to PurchaseRequestDetail, Item, and Unit. Contains pricing (quantity, unit price, discounts, taxes) and computed totals.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A procurement officer can create an RFQ from an approved PR, invite suppliers, publish, record responses, and close collection without leaving the procurement module.
- **SC-002**: A procurement officer can create a quotation with line items, submit it, complete evaluation with scores, select a winner, and award — all from the quotation management screens.
- **SC-003**: All RFQ status transitions (Draft→Published→CollectingResponses→Closed, any→Cancelled) are enforced by the system and recorded in the audit trail.
- **SC-004**: All quotation status transitions (Draft→Submitted→UnderEvaluation→Evaluated→Selected→Awarded, UnderEvaluation→Rejected) are enforced by the system.
- **SC-005**: Form validations prevent submission of invalid data (missing required fields, invalid numeric ranges, date logic violations) 100% of the time.
- **SC-006**: The quotations list page loads and displays results within 3 seconds.
- **SC-007**: The RFQ list page loads and displays results within 3 seconds.
- **SC-008**: All action dialogs (Cancel, Select, Reject, Complete Evaluation) require explicit user input before execution.
- **SC-009**: Zero CSS physical properties (ml/mr/pl/pr/text-right/text-left) in RFQ and quotation feature code.
- **SC-010**: All interactive elements have aria-label attributes for accessibility.

## Assumptions

- Domain entities (RequestForQuotation, RFQSupplier, Quotation, QuotationDetail) and their enums already exist in src/Domain/Procurement/.
- Application commands and queries for RFQ and Quotation already exist in src/Application/Procurement/.
- The RFQ list page frontend (RFQsListPage.tsx) and hooks (useRFQs.ts) already exist and are functional.
- PermissionCodes for RFQ and Quotation actions are already defined in PermissionCodes.cs.
- The DocumentSequenceService supports RFQ and Quotation number generation with prefixes RFQ-{D6} and QT-{D6}.
- Parties module is available for supplier selection (PartyType.Supplier).
- PurchaseRequests and PurchaseRequestDetails are available as reference data for RFQ creation and quotation line linking.
- Items and Units are available as reference data for quotation line items.
- The existing result-to-ui.ts utility handles error mapping for 4xx and 5xx responses.
- No database schema changes are needed — existing entities and relationships are sufficient.
- The frontend uses Arabic-only with hardcoded strings (no i18n framework).
- No mock data or mock servers are used — all data comes from the live backend API.

## Out of Scope

- PDF/print export of RFQs and quotations
- Email notifications to suppliers
- Approval workflow for quotation awards (awards are recorded, not routed for approval)
- Multi-currency conversion logic
- Attachment upload on RFQ and quotation forms

## Clarifications

### Session 2026-09-11

- Q: Should the quotation form send computed totals (SubTotal, DiscountAmount, TaxAmount, GrandTotal) to the API or compute them server-side? → A: The frontend computes them for display only. The API contract determines whether computed fields are sent or calculated server-side. The form sends the required fields per the existing CreateQuotationCommand contract.
- Q: Is there a separate "UpdateQuotation" command for editing draft quotations? → A: If the existing CreateQuotationCommand cannot be reused for updates, an UpdateQuotationCommand should be added. The spec assumes edit functionality is needed for Draft quotations.
- Q: How should draft RFQs be edited? → A: Dedicated edit page at `/procurement/rfqs/:id/edit` with a pre-populated form, matching the quotation edit pattern.
- Q: Should quotation editing use a new UpdateQuotation endpoint or extend Create? → A: New UpdateQuotationCommand + handler + validator, exposed as PUT /api/Quotations/{id}, separate from Create.
