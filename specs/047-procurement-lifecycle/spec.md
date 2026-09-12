# Feature Specification: Procurement Lifecycle Rebuild

**Feature Branch**: `047-procurement-lifecycle`

**Created**: 2026-09-10

**Status**: Draft

**Input**: User description: "إعادة بناء ميزة المشتريات بالكامل وفق متطلبات جديدة، دون اعتماد المواصفات القديمة مصدرًا للسلوك أو التصميم. استخدم التحليل الموجود في `.specify/plans/feature-context-procurement-lifecycle.md` والكود الحالي لجرد البيانات والعلاقات والتكاملات فقط، مع الالتزام باتفاقيات المستودع الحالية."

## Design Decisions (Resolved Before Spec)

The user requested explicit resolution of five design questions before locking the design. These are resolved below as informed defaults based on the feature-context analysis, government procurement standards, and the existing codebase patterns.

| # | Question | Resolution | Rationale |
|---|----------|------------|-----------|
| D1 | **Timing of actual budget reservation (الحجز الفعلي)**: Does approving a Purchase Request immediately reserve budget, or only when the Purchase Order is issued? | **Reservation at PO approval**. PR approval checks availability but does not lock funds. Encumbrance is created when PO is approved and issued. | Aligns with the existing Encumbrance entity which links to PurchaseOrderId. Avoids premature reservation when PR may be rejected or modified. The PR approval step performs an availability check only (soft gate). |
| D2 | **Can PR items be split across multiple suppliers?** | **Yes**. A single PR can produce multiple POs, each covering a subset of PR items, awarded to different suppliers. Each PO line must reference its originating PR detail line. | Government procurement frequently splits requirements across vendors for competitive pricing and partial awards. The existing PurchaseOrderDetail.PurchaseRequestDetailId supports this linkage. |
| D3 | **Are advances before receipt allowed?** | **No**. Advance payments against POs are out of scope for this feature. All procurement payments occur after goods receipt or service completion. | Simplifies the financial flow and reduces budget risk. Advance payments may be reconsidered in a future feature if needed. |
| D4 | **Are evaluation committees mandatory for RFQ?** | **Yes**. A technical/financial evaluation committee is mandatory for all RFQs that proceed to the quotation evaluation stage. The committee must be assigned before quotations are opened/evaluated. | Ensures procedural integrity and audit trail. The existing CommitteeAssignment entity with AssignmentType.Tender supports this linkage. |
| D5 | **Does scope include services alongside inventory items?** | **No**. The procurement lifecycle covers tangible goods (inventory items) only. Services are out of scope. | Simplifies the data model and matching logic. Services may be added in a future feature. |

## Clarifications

### Session 2026-09-12

- Q: Should the "Accept with Notes" action on a Supplier Invoice change the invoice's status, or is it purely a notes-logging action? → A: Notes-only, no status change. The "match" action is what establishes liability. Accept with Notes is a supplementary annotation for audit trail. Correct spec FR-050 language to reflect this.
- Q: Is the Supplier Invoice create page always PO-linked or should it support standalone creation? → A: PO-linked only. `?purchaseOrderId=` always required. No standalone creation path. Backend enforces PurchaseOrderId as non-nullable FK. FR-137 confirmed.
- Q: When creating a Supplier Invoice, should Unit Price on invoice lines default to the PO price and be editable, or be locked? → A: Pre-fill from PO price, keep editable. Officer enters correct price; match action catches variances. FR-139 confirmed.

### Session 2026-09-10

- Q: What is the document numbering convention for supplier invoices? → A: Use prefix `SINV-{D6}` via the existing DocumentSequenceService. A new SupplierInvoice entity (separate from GRN invoice fields) is introduced to capture full invoice details including line-level matching.
- Q: How are attachments handled across the procurement lifecycle? → A: Attachments are stored via the existing document attachment pattern (DocumentAttachment entity linked by DocumentType + DocumentId). Each procurement document type registers its own DocumentType string. No new attachment infrastructure is required.
- Q: What happens when a PO is partially received and the remaining quantity becomes obsolete? → A: The PO can be closed manually via a "Close Remaining" action. Unreceived quantities are released from encumbrance. The close action is recorded in ApprovalHistory and DocumentStatusLog.
- Q: What is the maximum PO value that allows a direct purchase without an RFQ? → A: 5,000,000 YER. POs at or below this threshold may use the direct purchase path (FR-019). Above this threshold, an RFQ is mandatory.
- Q: What is the acceptable price variance tolerance for three-way matching? → A: 0% (exact match). Invoice unit price must exactly match PO unit price. Any variance requires manual review and acceptance with notes.
- Q: Are advance payments against POs allowed? → A: No. Advance payment feature is removed entirely from procurement scope. All advance payment references (User Story 8, FR-039, EncumbranceType.Advance usage) are deleted.
- Q: Must the PR approver be a different user than the requestor? → A: No. SoD is not enforced between requestor and approver on Purchase Requests. The same user may create and approve a PR.
- Q: Should the ServiceCompletion certificate be a separate entity from GoodsReceiptNote? → A: Not applicable. Services are removed entirely from procurement scope. All purchase, receipt, and invoice matching is limited to goods (inventory items) only. FR-057, FR-058, FR-059 and all service-related references are deleted.

### Session 2026-09-12

- Q: When the GRN create page is opened without `?purchaseOrderId=`, how should the user select a purchase order — given no backend endpoint exists for listing eligible POs by status? → A: Require `?purchaseOrderId=` always. If absent, show an error and do not render the form. No standalone PO selection path. Primary navigation is from the PO detail page's "Create Goods Receipt Note" button.
- Q: How should the "Received By" field display the current user's name, given the backend GRN API doesn't expose current-user identity? → A: Use `useUserDetail(getAuthUser()?.userId)` — the same pattern as `DisbursementRequestDetailPage.tsx`. Fall back to user ID if resolution fails.

## User Scenarios & Testing

### User Story 1 - Create and Process Purchase Request (Priority: P1)

A department requestor creates a Purchase Request listing needed items with quantities, units, required date, priority, and estimated cost. The requestor submits it for approval. An approver reviews and either approves or rejects with reason. Approved PRs become available for procurement processing.

**Why this priority**: The Purchase Request is the entry point of the entire procurement lifecycle. Without it, no downstream documents can be created.

**Independent Test**: Create a PR with multiple line items, submit it, approve it, and verify status transitions are recorded in ApprovalHistory and DocumentStatusLog. Verify rejection returns to draft with reason visible.

**Acceptance Scenarios**:

1. **Given** a requestor with PurchaseRequestsCreate permission, **When** they create a PR with 3 line items (item, unit, quantity, required date, priority, estimated cost), **Then** the PR is saved in Draft status with a generated PRQ-{D6} number and total estimated cost computed.
2. **Given** a PR in Draft, **When** the requestor edits line items or header fields, **Then** changes are permitted and totals recomputed.
3. **Given** a PR in Draft, **When** the requestor submits it, **Then** status transitions to Submitted and the submission is recorded in ApprovalHistory and DocumentStatusLog.
4. **Given** a PR in Submitted, **When** an approver approves it, **Then** status transitions to Approved, approval is recorded in ApprovalHistory and DocumentStatusLog, and a budget availability check is performed (soft gate — no reservation yet).
5. **Given** a PR in Submitted, **When** an approver rejects with reason, **Then** status transitions back to Draft with the rejection reason visible, and the requestor can edit and resubmit.
6. **Given** a PR in Approved, **When** the requestor cancels it with reason, **Then** status transitions to Cancelled, cancellation is recorded, and the PR cannot generate downstream documents.
7. **Given** a PR in Approved, **When** an RFQ or PO is created referencing it, **Then** the PR status transitions to UnderProcurement.

---

### User Story 2 - Issue RFQ and Collect Supplier Responses (Priority: P1)

A procurement officer creates a Request for Quotation linked to an approved PR, sets submission deadlines, currency, and terms, then invites multiple suppliers. Suppliers respond with their quotations. The officer records each supplier's response status.

**Why this priority**: RFQ is the standard competitive procurement mechanism. It must work before quotations can be evaluated.

**Independent Test**: Create an RFQ from an approved PR, invite 3 suppliers, record responses, and verify the RFQ tracks invitation and response status per supplier.

**Acceptance Scenarios**:

1. **Given** an approved PR, **When** a procurement officer creates an RFQ, **Then** the RFQ is linked to the PR, inherits its line items, and gets a generated RFQ-{D6} number.
2. **Given** an RFQ in Draft, **When** the officer sets deadline, currency, terms, and invites suppliers (PartyType.Supplier), **Then** RFQSupplier records are created for each invited supplier with InvitationDate.
3. **Given** an RFQ with invited suppliers, **When** the officer publishes it, **Then** status transitions to Published and invitation dates are finalized.
4. **Given** a Published RFQ, **When** a supplier responds (or deadline passes without response), **Then** the RFQSupplier status is updated to Responded or Expired accordingly.
5. **Given** an RFQ where all invited suppliers have responded or expired, **When** the officer closes collection, **Then** status transitions to Closed and no further responses are accepted.
6. **Given** an approved PR, **When** business rules allow direct purchase (single supplier, below threshold), **Then** the officer can skip RFQ and create a PO directly from the PR.

---

### User Story 3 - Evaluate Quotations and Award Selected Offer (Priority: P1)

A procurement officer records supplier quotations with pricing, taxes, discounts, delivery terms, payment terms, and warranty. An evaluation committee performs technical and financial evaluation. The officer documents reasons for excluding non-selected offers and selects the winning offer. The award is approved.

**Why this priority**: Quotation evaluation and award decision is the critical decision point that determines which supplier and price will be used for the Purchase Order.

**Independent Test**: Record 3 quotations from different suppliers, assign an evaluation committee, perform technical/financial scoring, exclude 2 with reasons, select 1, approve the award, and verify all decisions are recorded.

**Acceptance Scenarios**:

1. **Given** a Closed RFQ, **When** the officer records a quotation from a supplier, **Then** the quotation is saved with QT-{D6} number, linked to the RFQ and RFQSupplier, containing line items with unit prices, discounts, taxes, and totals.
2. **Given** quotations recorded, **When** an evaluation committee is assigned (mandatory), **Then** a CommitteeAssignment of type Tender is created linked to the RFQ.
3. **Given** the committee has evaluated, **When** the officer records evaluation results per quotation (technical score, financial score, exclusion reasons for non-selected), **Then** evaluation data is saved on each quotation.
4. **Given** evaluation complete, **When** the officer selects the winning quotation and records selection reason, **Then** the quotation is marked as Selected and approval workflow begins.
5. **Given** a selected quotation, **When** an approver approves the award, **Then** the quotation status transitions to Awarded, the decision is recorded in ApprovalHistory and DocumentStatusLog, and the PR status reflects the awarded amount.
6. **Given** a selected quotation, **When** an approver rejects the award with reason, **Then** the quotation status returns to UnderEvaluation and the officer can re-evaluate or select a different offer.

---

### User Story 4 - Create and Issue Purchase Order (Priority: P1)

A procurement officer creates a Purchase Order from the approved PR and awarded quotation (or directly from PR for direct purchases). The PO is linked to the supplier, delivery location, and includes all commercial terms. The PO goes through approval, is issued, and budget is reserved (encumbrance created).

**Why this priority**: The PO is the binding contractual document that triggers financial commitment (encumbrance) and initiates the fulfillment cycle.

**Independent Test**: Create a PO from an awarded quotation, submit for approval, approve (verify encumbrance created), issue, and verify PO status and budget reservation.

**Acceptance Scenarios**:

1. **Given** an awarded quotation and approved PR, **When** the officer creates a PO, **Then** the PO is saved with PO-{D6} number, linked to the supplier (SupplierPartyId), PR, quotation, delivery location, and all line items with agreed prices.
2. **Given** a PO in Draft, **When** the officer edits quantities or terms, **Then** changes are permitted and totals recomputed.
3. **Given** a PO in Draft, **When** submitted for approval, **Then** status transitions to Submitted.
4. **Given** a PO in Submitted, **When** an approver approves it, **Then** status transitions to Approved, an Encumbrance is created (type Commitment) linked to the PO, budget availability is verified at encumbrance time, and approval is recorded in ApprovalHistory and DocumentStatusLog.
5. **Given** an Approved PO, **When** the officer issues it, **Then** status transitions to Issued, the PO is sent to the supplier, and the PO date is finalized.
6. **Given** an Approved or Issued PO, **When** a cancellation is approved with reason, **Then** the encumbrance is reversed, status transitions to Cancelled, and cancellation is recorded.
7. **Given** a partial PO (items from a PR split across suppliers), **When** created, **Then** each line references its original PurchaseRequestDetailId and the PR total is adjusted to reflect the partial award.

---

### User Story 5 - Receive Goods and Inspect (Priority: P1)

A warehouse officer receives goods against a specific PO line item. Each received item is recorded with accepted and rejected quantities. Partial receipts are supported. The system prevents receiving more than the ordered quantity. Rejected items trigger return processing.

**Why this priority**: Goods receipt is the physical fulfillment step that triggers inventory updates and is required for invoice matching.

**Independent Test**: Receive goods for a PO line (partial first, then remainder), verify accepted/rejected quantities, verify remaining quantity tracking, verify encumbrance liquidation for received amounts.

**Acceptance Scenarios**:
1. **Given** an Issued PO with line item OrderedQuantity=100, **When** the officer receives 60 units (55 accepted, 5 rejected), **Then** a GRN is created with GRN-{D6}, the line records AcceptedQuantity=55, RejectedQuantity=5, RemainingQuantity=45, and the encumbrance line is partially liquidated by the accepted amount.
2. **Given** a PO line with RemainingQuantity=45, **When** the officer attempts to receive 50 units, **Then** the system rejects with "الكمية المستلمة تتجاوز الكمية المتبقية في أمر الشراء".
3. **Given** a PO with multiple lines, **When** receipts are made against individual lines, **Then** each line tracks its own received/accepted/rejected/remaining quantities independently.
4. **Given** a received GRN, **When** the officer confirms receipt, **Then** stock transactions are created for accepted quantities (for inventory items), and the PO line status updates.
5. **Given** a GRN with rejected items, **When** the rejection is recorded, **Then** the rejected quantity is flagged for return and does not reduce the encumbrance (only accepted quantities liquidate encumbrance).
6. **Given** all PO lines are fully received (RemainingQuantity=0), **When** the last receipt is confirmed, **Then** the PO status transitions to Received and the encumbrance is fully liquidated.

---

### User Story 6 - Record Supplier Invoice and Match (Priority: P1)

A procurement officer records a supplier's invoice with invoice number, date, and line items. The system performs three-way matching: PO line ordered/received quantities, accepted quantities from GRN, and invoice quantities/prices. Discrepancies are flagged. Duplicate invoices are prevented.

**Why this priority**: Invoice matching is the financial control that prevents overpayment and duplicate payment. It determines the payment liability.

**Independent Test**: Record an invoice matching a PO/GRN, verify three-way match passes. Record a duplicate invoice number, verify rejection. Record an invoice with price discrepancy, verify flag.

**Acceptance Scenarios**:

1. **Given** a PO with received goods, **When** the officer records a supplier invoice with invoice number, date, and line items, **Then** the invoice is saved with SINV-{D6} number, linked to the PO and supplier.
2. **Given** an invoice with the same invoice number and supplier already recorded, **When** a duplicate is attempted, **Then** the system rejects with "فاتورة مكررة для هذا المورد".
3. **Given** an invoice with line items, **When** three-way matching runs, **Then** each invoice line is matched against the PO line (ordered quantity, unit price) and GRN accepted quantity. Matches, overages, shortages, and price variances are flagged.
4. **Given** a matched invoice, **When** the officer confirms the match, **Then** the invoice status transitions to Matched and the payment liability is established.
5. **Given** an invoice with discrepancies, **When** the officer reviews and accepts with notes, **Then** the acceptance notes are recorded on the invoice (status unchanged) and the officer may proceed to match or cancel.
6. **Given** a matched invoice, **When** the payment is processed, **Then** the invoice status transitions to Paid and no further payments can be made against the same invoice or the same received quantity.

---

### User Story 7 - Process Payment and Close PO (Priority: P1)

A payment officer creates a PaymentOrder linked to the matched invoice and approved PO. The payment goes through approval, is sent to treasury, and is recorded as paid. Upon full payment, the PO status updates and remaining encumbrance is released.

**Why this priority**: Payment completion closes the financial loop and releases budget. Without it, budget remains locked unnecessarily.

**Independent Test**: Create a payment order from a matched invoice, approve, mark as sent to treasury, record payment, verify PO status updates and encumbrance is released.

**Acceptance Scenarios**:

1. **Given** a Matched invoice, **When** a payment officer creates a PaymentOrder, **Then** the PaymentOrder is linked to the PO, invoice, encumbrance, and budget allocation, with the correct amount (net of deductions).
2. **Given** a PaymentOrder in Draft, **When** submitted and approved, **Then** status transitions through Submitted → Approved, with approval recorded in ApprovalHistory.
3. **Given** an Approved PaymentOrder, **When** sent to treasury, **Then** status transitions to SentToTreasury.
4. **Given** a SentToTreasury PaymentOrder, **When** payment is confirmed, **Then** status transitions to Paid, payment date is recorded, and journal entries are posted.
5. **Given** a fully paid PO (all invoices paid, all lines received), **When** the last payment is recorded, **Then** the PO status transitions to Closed and remaining encumbrance (if any) is released.
6. **Given** a PO with partial receipt and partial payment, **When** the officer decides to close the remaining unreceived quantity, **Then** the PO can be manually closed, unreleased encumbrance is freed, and the close is recorded.

---

### Edge Cases

- What happens when a PR is approved but no PO is created within a configurable period? → The PR status can be set to Expired by an authorized user. Expired PRs cannot generate POs but remain in the audit trail.
- What happens when an RFQ deadline passes with zero responses? → The RFQ status transitions to NoResponse. The officer can reissue the RFQ with a new deadline or cancel it.
- What happens when a PO is partially received and the supplier cannot fulfill the remainder? → The officer can close the remaining quantity. The encumbrance for unreceived quantity is released. The close is recorded.
- What happens when an invoice references a PO line that has not been received? → The system flags the mismatch. The invoice cannot be matched until at least partial receipt exists for that line.
- What happens when a payment would exceed the encumbrance amount? → The system rejects with "مبلغ الدفع يتجاوز المبلغ المحجوز". The encumbrance must be adjusted first.
- What happens when the currency exchange rate changes between PO and invoice? → The invoice records its own exchange rate. Variance is recorded but does not block matching. The payment uses the invoice's rate.
- What happens when the same item appears multiple times on a single PO? → Each PO line has a unique PurchaseOrderDetail record. Receipt and matching are per-line, not per-item.

## Requirements

### Functional Requirements

#### Data Model Cleanup (Supplier Unification)

- **FR-001**: System MUST use `SupplierPartyId` (int FK → Parties where PartyType=Supplier) as the sole supplier reference on all procurement documents (RFQ, Quotation, PO, GRN, SupplierInvoice). The legacy `SupplierId` field MUST be removed from PurchaseOrder and all other procurement entities.
- **FR-002**: System MUST remove `PartyId` from RFQSupplier and Quotation (currently a duplicate path to the same supplier). Only `SupplierPartyId` (or `PartyId` renamed to `SupplierPartyId`) is retained.
- **FR-003**: System MUST add a data migration step that maps existing `SupplierId` values to the corresponding `PartyId` in the Parties table (where PartyType=Supplier) and populates `SupplierPartyId`. Records that cannot be matched MUST be flagged in a migration report for manual resolution.
- **FR-004**: System MUST enforce FK constraints on all `SupplierPartyId` references with Restrict on delete. Orphaned references MUST be prevented at the database level.

#### Data Model Cleanup (Broken References)

- **FR-005**: System MUST audit all procurement FK relationships and ensure every FK has a corresponding Restrict constraint. Broken references (FK pointing to deleted or non-existent records) MUST be identified during migration and reported, not silently deleted.
- **FR-006**: System MUST add `PurchaseOrderDetailId` (int FK → PurchaseOrderDetail) to GoodsReceiptNoteDetail to link each receipt line to a specific PO line. This replaces the current pattern of matching by ItemId alone.
- **FR-007**: System MUST add `PurchaseOrderDetailId` (int FK → PurchaseOrderDetail) to SupplierInvoiceDetail to enable three-way matching at the line level.
- **FR-008**: System MUST add `PurchaseRequestId` (int FK → PurchaseRequest) to RequestForQuotation as a required field (currently nullable). Every RFQ must originate from an approved PR.

#### Purchase Request

- **FR-009**: System MUST provide a PurchaseRequest entity with: RequestNumber (PRQ-{D6}), RequestDate, RequiredDate, DepartmentId (FK), CostCenterId (FK), RequesterId (FK → User), Priority (enum: Low=0, Normal=1, High=2, Urgent=3), Status (enum: Draft=0, Submitted=1, Approved=2, UnderProcurement=3, Rejected=4, Cancelled=5, Expired=6), TotalEstimatedCost (computed), Notes, audit fields, RowVersion.
- **FR-010**: System MUST provide a PurchaseRequestDetail entity with: PurchaseRequestId (FK), ItemId (FK), UnitId (FK), RequestedQuantity, ApprovedQuantity, UnitCostEstimate, TotalCostEstimate, Notes, audit fields, RowVersion.
- **FR-011**: PurchaseRequest status transitions MUST be: Draft→Submitted (submit), Submitted→Approved (approve) or Submitted→Rejected (reject), Approved→UnderProcurement (when RFQ or PO created), Approved→Cancelled (cancel), Draft→Draft (edit). Any other transition MUST be rejected.
- **FR-012**: Every status transition MUST be recorded in ApprovalHistory (actor, decision, timestamp, reason) and DocumentStatusLog (from-status, to-status, actor, timestamp).
- **FR-013**: TotalEstimatedCost MUST be computed as SUM(Detail.TotalCostEstimate) and updated atomically on any line change.
- **FR-014**: ApprovedQuantity on each detail line MUST be set by the approver during approval. If not set, it defaults to RequestedQuantity.
- **FR-014a**: Separation of duties between requestor and approver is NOT enforced for Purchase Requests. The same user may create and approve a PR. This is a deliberate policy decision for this procurement scope.

#### Request for Quotation

- **FR-015**: System MUST provide a RequestForQuotation entity with: RFQNumber (RFQ-{D6}), RFQDate, PurchaseRequestId (FK, required), DeadlineDate, CurrencyCode, TermsAndConditions, Status (enum: Draft=0, Published=1, CollectingResponses=2, Closed=3, Cancelled=4, NoResponse=5), Notes, audit fields, RowVersion.
- **FR-016**: System MUST provide an RFQSupplier entity with: RFQId (FK), SupplierPartyId (FK → Parties, PartyType=Supplier), InvitationDate, ResponseDate, Status (enum: Invited=0, Responded=1, Expired=2, Declined=3), Notes, audit fields, RowVersion.
- **FR-017**: RFQ status transitions MUST be: Draft→Published (publish), Published→CollectingResponses (first response received), CollectingResponses→Closed (all responded/expired), any→Cancelled (cancel). NoResponse is set when deadline passes with zero responses.
- **FR-018**: An RFQ MUST NOT be published without at least one invited supplier.
- **FR-019**: Direct purchase path: When the total PO value is at or below 5,000,000 YER (or equivalent in foreign currency at PO creation rate), the system MUST allow creating a PO directly from an approved PR without an RFQ. Emergency procurements may also use the direct path regardless of value, with mandatory approval. The PR must have at least one approved detail line.

#### Quotation (Offer)

- **FR-020**: System MUST provide a Quotation entity with: QuotationNumber (QT-{D6}), RFQId (FK), RFQSupplierId (FK), SupplierPartyId (FK → Parties), QuotationDate, ValidUntil, CurrencyCode, ExchangeRate, SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal, PaymentTerms, DeliveryTerms, LeadTimeDays, WarrantyPeriodMonths, Status (enum: Draft=0, Submitted=1, UnderEvaluation=2, Evaluated=3, Selected=4, Awarded=5, Rejected=6, Expired=7), SelectionReason, RejectionReason, audit fields, RowVersion.
- **FR-021**: System MUST provide a QuotationDetail entity with: QuotationId (FK), PurchaseRequestDetailId (FK → PurchaseRequestDetail), ItemId (FK), UnitId (FK), Quantity, UnitPrice, DiscountPercent, DiscountAmount, NetUnitPrice, TaxPercent, TaxAmount, LineTotal, LineTotalWithTax, Notes, audit fields, RowVersion.
- **FR-022**: GrandTotal MUST be computed as SubTotal - DiscountAmount + TaxAmount + ShippingCost + OtherCharges. Line totals MUST be computed as Quantity × UnitPrice - DiscountAmount + TaxAmount.
- **FR-023**: Quotation status transitions MUST be: Draft→Submitted (submit), Submitted→UnderEvaluation (evaluation starts), UnderEvaluation→Evaluated (evaluation complete), Evaluated→Selected (select winner), Selected→Awarded (award approved) or Selected→Rejected (award rejected), UnderEvaluation→Rejected (exclude from evaluation), any→Expired (validity expired).

#### Evaluation Committee

- **FR-024**: A CommitteeAssignment of type Tender MUST be linked to an RFQ (not to a PO, since evaluation happens before PO creation). The existing CommitteeAssignment.PurchaseOrderId field is repurposed or a new RfqId FK is added.
- **FR-025**: Quotation evaluation (status transition to Evaluated) MUST require that a completed CommitteeAssignment of type Tender exists for the RFQ. The system MUST reject evaluation completion without a committee.
- **FR-026**: Evaluation results (technical score, financial score, exclusion reason) MUST be stored per quotation, not per RFQ. Each quotation carries its own evaluation data.

#### Purchase Order

- **FR-027**: System MUST provide a PurchaseOrder entity with: PONumber (PO-{D6}), PODate, PurchaseRequestId (FK), QuotationId (FK, nullable for direct purchase), SupplierPartyId (FK → Parties, required), WarehouseId (FK, nullable), DeliveryLocationId (FK), CurrencyCode, ExchangeRate, SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal, PaymentTerms, DeliveryTerms, ExpectedDeliveryDate, Status (enum: Draft=0, Submitted=1, Approved=2, Issued=3, PartiallyReceived=4, Received=5, Closed=6, Cancelled=7), Notes, audit fields, RowVersion.
- **FR-028**: System MUST provide a PurchaseOrderDetail entity with: PurchaseOrderId (FK), PurchaseRequestDetailId (FK), QuotationDetailId (FK, nullable), ItemId (FK), UnitId (FK), OrderedQuantity, ReceivedQuantity (default 0), RemainingQuantity (computed: OrderedQuantity - ReceivedQuantity), UnitPrice, DiscountPercent, DiscountAmount, NetUnitPrice, TaxPercent, TaxAmount, LineTotal, LineTotalWithTax, ExpectedDeliveryDate, Status (enum: Pending=0, PartiallyReceived=1, Received=2, Closed=3), Notes, audit fields, RowVersion.
- **FR-029**: PO status transitions MUST be: Draft→Submitted (submit), Submitted→Approved (approve — creates encumbrance), Approved→Issued (issue), Issued→PartiallyReceived (first partial receipt), PartiallyReceived→Received (all lines received), Received→Closed (close) or PartiallyReceived→Closed (manual close), any→Cancelled (cancel — reverses encumbrance).
- **FR-030**: PO approval MUST create an Encumbrance of type Commitment linked to the PO. The encumbrance MUST cover all PO lines at their agreed amounts. Budget availability MUST be verified before encumbrance creation.
- **FR-031**: PO cancellation MUST reverse the associated encumbrance and release budget. Cancellation MUST require approval and a recorded reason.
- **FR-032**: RemainingQuantity on PO lines MUST be updated atomically on each receipt. The PO status MUST transition to PartiallyReceived when any line has RemainingQuantity > 0 and at least one line has been received.

#### Budget Reservation (Encumbrance)

- **FR-033**: Encumbrance creation at PO approval MUST be atomic: budget availability check, encumbrance header, and encumbrance lines (one per PO line, linked to BudgetItem) are all created in a single transaction.
- **FR-034**: Encumbrance lines MUST link to BudgetItemAllocations (per spec 046) via BudgetItemId. The encumbrance amount per line MUST match the PO line NetUnitPrice × OrderedQuantity.
- **FR-035**: Budget availability check MUST use AvailableAmount = RemainingAmount - OutstandingEncumbrance (per spec 046 formulas). The check MUST verify sufficiency for the total PO value across all affected BudgetItemAllocations.
- **FR-036**: When a PO has a higher value than the original PR estimate, the difference MUST be flagged but not blocked (the PR estimate is informational, not a hard gate). The encumbrance is for the actual PO value.
- **FR-037**: Encumbrance liquidation MUST occur at goods receipt: each accepted quantity liquidates a proportional amount of the encumbrance line. LiquidatedAmount is updated atomically with the receipt confirmation.
- **FR-038**: When a PO is cancelled, the encumbrance MUST be reversed (status → Cancelled, ReversalOf set). Released budget becomes available again. This is logged in ApprovalHistory.

#### Goods Receipt

- **FR-040**: System MUST provide a GoodsReceiptNote entity with: GRNNumber (GRN-{D6}), GRNDate, SupplierPartyId (FK), PurchaseOrderId (FK), WarehouseId (FK), LocationId (FK), ReceivedBy (FK → User), Status (enum: Draft=0, Confirmed=1, Rejected=2), Notes, audit fields, RowVersion.
- **FR-041**: System MUST provide a GoodsReceiptNoteDetail entity with: GRNId (FK), PurchaseOrderDetailId (FK, required), ItemId (FK), UnitId (FK), OrderedQuantity (copied from PO line), ReceivedQuantity, AcceptedQuantity, RejectedQuantity, RemainingQuantity (computed: OrderedQuantity - AcceptedQuantity), UnitCost, TotalCost, BatchNumber, ExpiryDate, Notes, audit fields, RowVersion.
- **FR-042**: ReceivedQuantity on a GRN detail MUST NOT exceed RemainingQuantity on the linked PO detail. The system MUST reject with "الكمية المستلمة تتجاوز الكمية المتبقية".
- **FR-043**: Upon GRN confirmation, AcceptedQuantity is used for encumbrance liquidation (proportional amount). RejectedQuantity does NOT liquidate encumbrance.
- **FR-044**: GRN confirmation MUST create StockTransaction records for AcceptedQuantity, updating item stock levels.
- **FR-045**: PO line ReceivedQuantity MUST be updated atomically with GRN confirmation. PO status MUST transition to PartiallyReceived or Received as applicable.

#### Supplier Invoice

- **FR-046**: System MUST provide a SupplierInvoice entity with: InvoiceNumber (SINV-{D6}), SupplierInvoiceNumber (string, the supplier's own invoice number), InvoiceDate, PurchaseOrderId (FK), SupplierPartyId (FK), CurrencyCode, ExchangeRate, SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal, DueDate, Status (enum: Draft=0, Submitted=1, Matched=2, PartiallyPaid=3, Paid=4, Disputed=5, Cancelled=6), Notes, audit fields, RowVersion.
- **FR-047**: System MUST provide a SupplierInvoiceDetail entity with: SupplierInvoiceId (FK), PurchaseOrderDetailId (FK), GoodsReceiptNoteDetailId (FK, nullable), ItemId (FK), Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, Notes, audit fields, RowVersion.
- **FR-048**: System MUST prevent duplicate invoices: UNIQUE constraint on (SupplierPartyId, SupplierInvoiceNumber). The error message MUST be "فاتورة مكررة لهذا المورد".
- **FR-049**: Three-way matching MUST verify: Invoice Quantity ≤ AcceptedQuantity (from GRN) and Invoice UnitPrice = PO UnitPrice (exact match, 0% tolerance). Mismatches are flagged and block automatic matching — the officer must review and accept with notes to proceed.
- **FR-050**: Each invoice line MUST reference exactly one PO line (PurchaseOrderDetailId) and optionally one GRN line (GoodsReceiptNoteDetailId). The same PO line cannot be invoiced twice for the same quantity — the system MUST track cumulative invoiced quantity per PO line and reject over-invoicing with "تم فوترة هذه الكمية بالكامل".
- **FR-051**: Invoice status transitions MUST be: Draft→Submitted (submit), Submitted→Matched (match confirmed), Matched→PartiallyPaid (partial payment), PartiallyPaid→Paid (full payment), any→Cancelled (cancel), any→Disputed (dispute raised).

#### Payment and Closure

- **FR-052**: PaymentOrder creation from a Matched invoice MUST populate: PurchaseOrderId, EncumbranceId, BudgetItemAllocationId, AmountGross (net invoice amount), DeductionAmount, and link to the correct fund and fiscal year.
- **FR-053**: PaymentOrder status MUST follow the existing lifecycle: Draft→Submitted→Approved→SentToTreasury→Paid. Each transition MUST be recorded in ApprovalHistory and DocumentStatusLog.
- **FR-054**: Upon payment completion (Paid status), journal entries MUST be posted. The system MUST update the PO invoice tracking (SupplierInvoice.Status → Paid or PartiallyPaid).
- **FR-055**: When all PO lines are Received (RemainingQuantity=0 for all) AND all invoices are Paid, the PO status MUST transition to Closed and remaining encumbrance (if any) MUST be released.
- **FR-056**: Manual PO close (before full receipt) MUST be allowed with approval. Unreceived quantities are released from encumbrance. The close MUST be recorded in ApprovalHistory.

#### Document Numbering

- **FR-060**: All procurement documents MUST use the existing DocumentSequenceService with prefixes: PRQ (PurchaseRequest), RFQ (RequestForQuotation), QT (Quotation), PO (PurchaseOrder), GRN (GoodsReceiptNote), SINV (SupplierInvoice). Numbers are allocated in the same transaction as document creation.
- **FR-061**: The Quotation prefix QT MUST be registered in the DocumentSequenceService seed data (currently missing from the inspected seed list).

#### Approval and Audit

- **FR-062**: Every procurement document lifecycle transition MUST be recorded in both ApprovalHistory and DocumentStatusLog. No inline approval fields (ApprovedById, ApprovedAt, RejectionReason) are used for approval decisions — these are stored in ApprovalHistory only.
- **FR-063**: Existing inline approval fields on PurchaseRequest, PurchaseOrder, and Quotation (ApprovedById, ApprovedAt, RejectionReason, CancelledById, CancelledAt) MUST be removed from the entities. Approval state is derived from ApprovalHistory records.
- **FR-064**: Attachments MUST be supported on every procurement document via the existing DocumentAttachment pattern (DocumentType + DocumentId). Each document type registers its DocumentType string constant.
- **FR-065**: Every procurement endpoint MUST declare a required permission. New permission codes MUST be added to PermissionCodes for each action (already partially registered: PurchaseRequestsView, PurchaseRequestsCreate, PurchaseRequestsSubmit, PurchaseRequestsApprove, PurchaseRequestsReject, RFQView, RFQCreate, RFQPublish, RFQComplete, RFQCancel, QuotationsView, QuotationsCreate, PurchaseOrdersView, PurchaseOrdersCreate, PurchaseOrdersSubmit, PurchaseOrdersApprove, PurchaseOrdersCancel). Additional codes for: RFQEvaluate, QuotationsEvaluate, QuotationsSelect, QuotationsAward, GoodsReceiptsCreate, GoodsReceiptsConfirm, SupplierInvoicesCreate, SupplierInvoicesMatch, PaymentsCreate, PaymentsApprove, POClose.

#### Data Migration

- **FR-066**: Migration MUST map existing `SupplierId` values to `SupplierPartyId` using the Parties table. Unmatched records MUST be flagged in a migration report.
- **FR-067**: Migration MUST add `PurchaseOrderDetailId` to GoodsReceiptNoteDetail and populate it from existing ItemId-based matching where a unique PO line match can be determined. Ambiguous matches MUST be flagged for manual resolution.
- **FR-068**: Migration MUST NOT modify any applied migrations. All schema changes are new migrations.
- **FR-069**: Migration MUST preserve all existing business documents and audit records. No records are deleted to resolve broken references.

#### Frontend

- **FR-070**: All procurement screens MUST render in Arabic-first RTL with logical CSS properties (ms-/me-, ps-/pe-), design-token-only styling, and dark mode support.
- **FR-071**: Each procurement stage MUST have: list page with search/filter, detail page with line items, lifecycle action buttons (per status), approval history panel, document status log, attachments section, and navigation links to related documents (PR→RFQ→Quotation→PO→GRN→Invoice→PaymentOrder).
- **FR-072**: The procurement dashboard MUST show: pending approvals count, open POs count, pending receipts count, pending invoices count, and encumbrance summary.
- **FR-073**: All screens MUST use the shared UI component library (shadcn primitives). Per-feature duplicates of shared components are prohibited.
- **FR-074**: Loading states, empty states, and error states MUST be implemented for every query-driven screen. No spinners-as-default.

#### Purchase Order — Backend Implementation Gaps

- **FR-075**: `GET /api/PurchaseOrders` endpoint MUST be wired in the endpoint group using `GetPurchaseOrdersQuery`. The endpoint MUST support query parameters: `status?`, `supplierPartyId?`, `purchaseRequestId?`, `quotationId?`, `search?` (PONumber), `expectedDeliveryDateFrom?`, `expectedDeliveryDateTo?`, `page?`, `pageSize?`. The list response MUST resolve `SupplierName` by joining the Parties table (not return null).
- **FR-076**: `GET /api/PurchaseOrders/{id}` endpoint MUST dispatch `GetPurchaseOrderByIdQuery` and return the full detail response including lines, approval history, status log, and navigation links to related documents (PR, Quotation, GRNs, SupplierInvoices, PaymentOrders). MUST NOT return a placeholder/stub response.
- **FR-077**: `PATCH /api/PurchaseOrders/{id}/submit` endpoint MUST dispatch `SubmitPurchaseOrderCommand` and return the result. MUST NOT return an empty `Results.Ok()` without command dispatch.
- **FR-078**: `PUT /api/PurchaseOrders/{id}` endpoint MUST be wired in the endpoint group using `UpdatePurchaseOrderCommand`. The endpoint is only available for POs in Draft status.
- **FR-079**: `UpdatePurchaseOrderCommandHandler` MUST process the `Lines` parameter: add new lines, update existing lines (by line Id), and remove lines not present in the request. Line changes MUST trigger recomputation of header totals (SubTotal, DiscountAmount, TaxAmount, GrandTotal). Only Draft status POs are editable.
- **FR-080**: `SubmitPurchaseOrderCommand` MUST use `[Authorize(PurchaseOrdersSubmit)]` permission (not `PurchaseOrdersCreate`). The permission code `PurchaseOrdersSubmit` MUST exist in `PermissionCodes.cs`.
- **FR-081**: Every PO lifecycle transition handler (Submit, Approve, Issue, Cancel, Close) MUST call `IDocumentStatusLogger.LogStatusChangeAsync` to record the transition in DocumentStatusLog. The log entry MUST include: document type "PurchaseOrder", document ID, from-status, to-status, actor (current user), timestamp, and optional reason.
- **FR-082**: `ApprovePurchaseOrderCommandHandler` MUST publish the `PurchaseOrderApproved` domain event after successful approval and encumbrance creation. The event MUST carry SourceEntityId, SupplierId, GrandTotal, and CurrencyCode.
- **FR-083**: `CancelPurchaseOrderCommand` and `ClosePurchaseOrderCommand` MUST accept an optional `Reason` parameter (string, max 2000 chars). The reason MUST be recorded in ApprovalHistory with the cancellation/close decision. If the backend currently lacks reason storage, the entity or ApprovalHistory entry MUST be extended to capture it.
- **FR-084**: All PO endpoints MUST declare `.RequireAuthorization(PermissionCodes.PurchaseOrders*)` on the route definition in the endpoint group. The `[Authorize]` attribute on commands is not sufficient — endpoints MUST also enforce authorization.
- **FR-085**: `GetPurchaseOrdersQuery` list filter MUST support `SupplierPartyId` filter and `ExpectedDeliveryDate` range filter (From/To). The query MUST join Parties table to resolve supplier name for the list response.
- **FR-086**: FluentValidation validators MUST exist for every PO command: `CreatePurchaseOrderCommandValidator`, `UpdatePurchaseOrderCommandValidator`, `SubmitPurchaseOrderCommandValidator`, `ApprovePurchaseOrderCommandValidator`, `IssuePurchaseOrderCommandValidator`, `CancelPurchaseOrderCommandValidator`, `ClosePurchaseOrderCommandValidator`. Validation rules: SupplierPartyId required, at least one line required, OrderedQuantity > 0, UnitPrice >= 0, DiscountPercent between 0 and 100, TaxPercent between 0 and 100. Only Draft status editable.

#### Purchase Order — Frontend Implementation

- **FR-087**: Purchase Order frontend MUST provide four pages: list (`/procurement/purchase-orders`), create (`/procurement/purchase-orders/create`), detail (`/procurement/purchase-orders/:id`), and edit (`/procurement/purchase-orders/:id/edit`). All routes MUST be registered in `src/app/routes.tsx`.
- **FR-088**: The list page (`PurchaseOrdersListPage`) MUST display columns: PO Number (dir="ltr", mono font), Supplier Name, Purchase Request number, Quotation number, Status (with badge), Grand Total (bold, tabular-nums), Expected Delivery Date, Created Date, and action buttons. Actions MUST be status-gated: Submit (Draft), Approve (Submitted), Issue (Approved), Cancel (any non-Cancelled/Closed), Close (PartiallyReceived/Received).
- **FR-089**: The list page MUST support filters: search by PO number, filter by Status (select), filter by PurchaseRequestId, filter by QuotationId, filter by SupplierPartyId (combobox), filter by ExpectedDeliveryDate range (date pickers).
- **FR-090**: The create page (`PurchaseOrderCreatePage`) MUST load catalog data (items, units, suppliers, warehouses, locations, currencies) via catalog hooks. The form uses React Hook Form + Zod with schema in `features/procurement/purchase-orders/shared/schemas.ts`. On successful creation, navigate to the detail page.
- **FR-091**: The edit page (`PurchaseOrderEditPage`) MUST load the existing PO detail and catalog data. A guard MUST redirect to the detail page if the PO status is not Draft. The form uses the same Zod schema as create. On successful update, navigate to the detail page.
- **FR-092**: The detail page (`PurchaseOrderDetailPage`) MUST display: PO summary (number, supplier, dates, status), financial summary (SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal — all with tabular-nums), line items table (with OrderedQuantity, ReceivedQuantity, RemainingQuantity, status per line), approval history panel, document status log, related documents navigation (links to PR, Quotation, GRNs, SupplierInvoices, PaymentOrders), and lifecycle action buttons (per current status). Cancel and Close actions MUST open a dialog requesting a reason (textarea, optional for Close, required for Cancel).
- **FR-093**: The purchase order form component (`ProcurementPurchaseOrdersForm.tsx`) MUST be placed in `src/Web/ClientApp/src/components/` (NOT inside `features/`). It MUST use `useFieldArray` for line items. Lines MUST support: selecting Item, Unit, OrderedQuantity, UnitPrice, DiscountPercent, TaxPercent. The form MUST compute line totals client-side (LineTotal = OrderedQuantity × UnitPrice - DiscountAmount, LineTotalWithTax = LineTotal + TaxAmount) and header totals (SubTotal = sum of LineTotal, DiscountAmount = sum of line discounts, TaxAmount = sum of line taxes, GrandTotal = SubTotal - DiscountAmount + TaxAmount + ShippingCost + OtherCharges) for immediate display. Backend recalculates on save.
- **FR-094**: The purchase order Zod schema (`features/procurement/purchase-orders/shared/schemas.ts`) MUST validate: SupplierPartyId required (number > 0), at least one line required (array min(1)), each line: OrderedQuantity > 0, UnitPrice >= 0, DiscountPercent between 0 and 100 (optional), TaxPercent between 0 and 100 (optional). The schema MUST be reused for both create and edit forms.
- **FR-095**: The purchase orders feature MUST have `shared/catalog-hooks.ts` providing `useItems()`, `useUnits()`, `useSuppliers()`, `useWarehouses()`, `useLocations()`, `useCurrencies()` hooks that fetch reference data from the API with `staleTime: Infinity`.
- **FR-096**: The purchase orders feature MUST have `shared/types.ts` exporting: `PurchaseOrder`, `PurchaseOrderDetail`, `PurchaseOrderDetailLine` interfaces, `PurchaseOrderStatus` type union, `statusLabels` record (Arabic), and `statusVariants` map for `StatusBadge`. The file MUST NOT have circular self-imports.
- **FR-097**: The purchase orders hooks (`hooks/usePurchaseOrders.ts`) MUST include: `usePurchaseOrdersList(params)`, `usePurchaseOrderDetail(id)`, `useCreatePurchaseOrder()`, `useUpdatePurchaseOrder()`, `useSubmitPurchaseOrder()`, `useApprovePurchaseOrder()`, `useIssuePurchaseOrder()`, `useCancelPurchaseOrder()`, `useClosePurchaseOrder()`. All mutations MUST invalidate relevant queries on success. Error handling MUST use `handleApiError` for 4xx and `handleLifecycleError` for 5xx.
- **FR-098**: PO Number MUST render with `dir="ltr"` and monospace font. Grand Total MUST render bold with `tabular-nums`. Received quantities MUST render with a progress indicator or summary showing received/ordered ratio. All monetary values MUST use `tabular-nums`.
- **FR-099**: Cancel and Close action dialogs MUST include a textarea for Reason. Cancel reason is required; Close reason is optional. On submission, the reason is sent in the PATCH request body. The dialog MUST disable the confirm button during mutation and show loading state.
- **FR-100**: All PO pages MUST use Arabic-only hardcoded strings (no `t()` calls, no locale files). RTL layout with logical CSS properties only (`ms-/me-/ps-/pe-`, `text-start/text-end`). No physical CSS properties (`ml/mr/pl/pr/text-right/text-left`).
- **FR-101**: Permission codes for PO frontend MUST include: `PurchaseOrders.View`, `PurchaseOrders.Create`, `PurchaseOrders.Submit`, `PurchaseOrders.Approve`, `PurchaseOrders.Issue`, `PurchaseOrders.Cancel`, `PurchaseOrders.Close` in `shared/constants/permissions.ts`.

#### Goods Receipt Note — Frontend Implementation

- **FR-110**: GRN frontend MUST provide three pages: list (`/procurement/goods-receipt-notes`), create (`/procurement/goods-receipt-notes/create`), and detail (`/procurement/goods-receipt-notes/:id`). All routes MUST be registered in `src/app/routes.tsx`. The create page MUST support `?purchaseOrderId=` query parameter to pre-select a purchase order.
- **FR-111**: The list page (`GRNsListPage`) MUST display columns: GRN Number (dir="ltr", mono font), Purchase Order Number (dir="ltr"), Receipt Date (ar-YE format), Status (with `StatusBadge`), Created Date (ar-YE format). The list MUST use server-side pagination with page/pageSize parameters. A purchase order filter (select/combobox) MUST be available in addition to status and search filters.
- **FR-112**: The list page MUST handle the API response shape: the backend returns a paginated list (not a plain array). The hook MUST correctly type the response as `PaginatedList<GRN>` with `items` and `totalCount` properties. If the NSwag-generated client types the response as `any`, a manual wrapper MUST be created in `shared/client.ts` with a comment explaining why.
- **FR-113**: The list page MUST provide: skeleton loading state (table skeleton rows), error state with retry button, empty state with appropriate Arabic message. When filters change (status, search, purchaseOrderId), the page MUST reset to page 1.
- **FR-114**: The list page MUST NOT expose confirm/reject actions inline in the table. Confirm and reject are detail-page-only actions. The list table actions are limited to view (navigate to detail).
- **FR-115**: The create page (`GRNCreatePage`) MUST require `?purchaseOrderId=<id>` query parameter. If the parameter is absent or invalid, the page MUST display an error and not render the form. The PO is pre-selected and the user cannot change it. The page MUST load PO details and display PO line items that have `RemainingQuantity > 0`. The primary navigation path is from the PO detail page's "Create Goods Receipt Note" button (FR-124).
- **FR-116**: The create form header fields: Receipt Date (date picker, default today), Warehouse (select, required), Location (displayed as read-only, derived from selected warehouse's `LocationId` — if warehouse has no location, display a blocking validation message), Received By (current user name via `useUserDetail(getAuthUser()?.userId)` — same pattern as `DisbursementRequestDetailPage.tsx`; fall back to user ID if resolution fails), Notes (textarea, optional).
- **FR-117**: The create form line items table: each eligible PO line (RemainingQuantity > 0) is displayed with: Item Name (read-only, from PO line), Unit (read-only), Ordered Quantity (read-only, from PO line), Remaining Quantity (read-only, computed as PO OrderedQuantity − PO AcceptedQuantity), Unit Cost (read-only, from PO line). User-input fields per line: Received Quantity (number, required, ≥ 0), Accepted Quantity (number, required, ≥ 0), Rejected Quantity (number, required, ≥ 0), Batch Number (text, optional), Expiry Date (date, optional), Notes (text, optional).
- **FR-118**: The Zod schema for GRN creation MUST validate: `purchaseOrderId` required (number > 0), `warehouseId` required (number > 0), `locationId` required (number > 0 — derived from warehouse, blocked if missing), at least one line item with `receivedQuantity > 0`. Per-line validation: `receivedQuantity ≥ 0`, `acceptedQuantity ≥ 0`, `rejectedQuantity ≥ 0`, `acceptedQuantity + rejectedQuantity = receivedQuantity`, `receivedQuantity ≤ remainingQuantity` (remaining from PO line). Non-negative values enforced. Reference values copied from PO (ordered quantity, unit cost) MUST NOT be editable.
- **FR-119**: On successful GRN creation, the user MUST be navigated to the detail page of the newly created GRN. The new GRN ID is returned by the POST endpoint. 4xx errors MUST display field-level or form-summary errors. 5xx errors MUST display a toast notification. Error mapping MUST use `shared/api/result-to-ui.ts`.
- **FR-120**: The detail page (`GRNDetailPage`) MUST display: GRN summary (number, receipt date, status badge, created date), related Purchase Order number (as a clickable link to PO detail), Warehouse name, Location name (derived from warehouse), Received By (user name or ID fallback), Notes. A "Back to Purchase Order" link MUST be shown when the GRN has a `purchaseOrderId`.
- **FR-121**: The detail page MUST display a line items table with columns: Item (name or ID fallback), Unit (name or ID fallback), Ordered Quantity, Received Quantity, Accepted Quantity, Rejected Quantity, Remaining Quantity, Unit Cost, Total Cost, Batch Number, Expiry Date, Notes. All monetary values MUST use `tabular-nums`. Quantities MUST use Latin numerals.
- **FR-122**: The detail page MUST show Confirm and Reject action buttons ONLY when the GRN status is `Draft`. When status is `Confirmed` or `Rejected`, no action buttons are shown. The Confirm button MUST open a confirmation dialog that prevents accidental execution (requires explicit "Confirm" click in the dialog). The Reject button MUST open a dialog with an optional Notes textarea; the notes are sent in the request body as `{ notes: string }`.
- **FR-123**: During confirm/reject mutation execution: the action button MUST be disabled to prevent double-submission. On success: invalidate both list and detail queries, show success toast, and update the displayed status. On error: display error message (4xx inline or toast for 5xx), re-enable the button. The confirm/reject mutations MUST NOT be accessible from the list page — only from the detail page.
- **FR-124**: The GRN detail page MUST provide a link back to the related Purchase Order detail page. The PO detail page MUST show a "Create Goods Receipt Note" button when the PO status is `Issued` or `PartiallyReceived` AND the user has `GoodsReceipts.Create` permission. The button MUST navigate to `/procurement/goods-receipt-notes/create?purchaseOrderId=<poId>`.
- **FR-125**: Permission codes for GRN frontend MUST use `GoodsReceipts.View`, `GoodsReceipts.Create`, `GoodsReceipts.Confirm`, `GoodsReceipts.Reject` — matching the backend `PermissionCodes.cs` constants. The current frontend `INVENTORY_PERMISSIONS.GoodsReceiptNotes` constants (`GoodsReceiptNotes.View`, `GoodsReceiptNotes.Create`, `GoodsReceiptNotes.Approve`) MUST be replaced with a new `PROCUREMENT_PERMISSIONS.GoodsReceipts` object using the correct codes. Route guards MUST enforce these permissions.
- **FR-126**: The GRN feature MUST follow project structure: pages in `features/procurement/goods-receipt-notes/pages/`, hooks in `features/procurement/goods-receipt-notes/hooks/`, shared types/schemas/client in `features/procurement/goods-receipt-notes/shared/`. Zod schema in `shared/schemas.ts`. Reusable GRN components in `src/components/` with `ProcurementGoodsReceipt` prefix. No `components/` folder inside the feature. Cross-entity procurement sharing in `features/procurement/shared/`. All GRN pages MUST use Arabic-only hardcoded strings, RTL with logical CSS properties, design tokens from `tokens.ts`, shadcn components, dark mode support, minimum touch targets 44×44, ARIA labels on icon buttons, keyboard navigation with focus-visible.

#### Supplier Invoice — Backend Implementation Gaps

- **FR-127**: `MatchSupplierInvoiceCommandHandler` (`src/Application/Procurement/Commands/SupplierInvoices/MatchSupplierInvoice/MatchSupplierInvoiceCommandHandler.cs`) currently changes status from `Submitted` to `Matched` without performing any three-way matching verification. **CONTRADICTION with spec FR-049**: the spec requires matching against PO ordered/received quantities and GRN accepted quantities, with exact price match (0% tolerance). The handler does not query PO lines, GRN lines, or compare quantities/prices. This is a **documented backend gap** — the frontend MUST NOT display match results or variances that the backend does not compute. The frontend match action is a simple status transition trigger only.

- **FR-128**: `AcceptInvoiceWithNotesCommandHandler` (`src/Application/Procurement/Commands/SupplierInvoices/AcceptInvoiceWithNotes/AcceptInvoiceWithNotesCommandHandler.cs`) currently sets `entity.Notes = request.Notes` without changing the invoice status. **CONTRADICTION with spec FR-050**: the spec implies this action establishes liability at the accepted amount, but the handler does not change status. The backend command exists but the endpoint is **NOT registered** in `src/Web/Endpoints/Procurement/SupplierInvoices.cs` — there is no `MapPatch("/{id:int}/accept-with-notes", ...)` route. This is a **documented backend gap** — the frontend cannot call this action until the endpoint is wired.

- **FR-129**: `GetSupplierInvoiceByIdQueryHandler` returns `SupplierName: null` (hardcoded) and `ItemNameAr: null` for detail lines. `GetSupplierInvoicesQueryHandler` does not join the Parties table and does not return `SupplierName` in list items. The frontend MUST handle null supplier names gracefully (display ID fallback or '-'). This is a **documented backend gap** — no frontend workaround can resolve names the backend does not provide.

- **FR-129a**: The `SupplierInvoices` endpoint group (`src/Web/Endpoints/Procurement/SupplierInvoices.cs`) does not declare `.RequireAuthorization(PermissionCodes.SupplierInvoices*)` on any route. The `[Authorize]` attributes exist on individual command records but the endpoint group routes are unprotected. This is a **documented backend gap** — the frontend MUST still declare permission codes and use route guards, but enforcement is pending backend wiring.

#### Supplier Invoice — Frontend Implementation

- **FR-130**: Supplier Invoice frontend MUST provide three pages: list (`/procurement/supplier-invoices`), create (`/procurement/supplier-invoices/create`), and detail (`/procurement/supplier-invoices/:id`). All routes MUST be registered in `src/app/routes.tsx`. **Current state**: only the list page exists (`SupplierInvoicesListPage.tsx`). The create and detail pages and their routes do not exist.

- **FR-131**: The list page (`SupplierInvoicesListPage`) MUST display columns: Invoice Number (dir="ltr", mono font, system-generated `SINV-{D6}`), Supplier Invoice Number (dir="ltr"), Supplier Name (resolved from Parties — **backend gap: returns null currently**), Purchase Order Number (dir="ltr", linked to PO detail), Status (with `StatusBadge`), Grand Total (bold, tabular-nums), Invoice Date (ar-YE format), Created Date (ar-YE format). **Current state**: list page exists but is missing Supplier Name column, Purchase Order Number column, server-side pagination, error state, and skeleton loading.

- **FR-132**: The list page MUST use server-side pagination with `page`/`pageSize` parameters. The backend `GetSupplierInvoicesQuery` returns `PaginatedList<SupplierInvoiceListItem>` with `items`, `totalCount`, `page`, `pageSize`, `totalPages`. **Current state**: the hook types the response as `SupplierInvoice[]` (plain array) instead of `PaginatedList<SupplierInvoice>`. The list page accesses `response?.items` which works only if the API returns `{ items: [...] }` — this is a **type mismatch** that may cause runtime errors if the API shape changes.

- **FR-133**: The list page MUST support filters: search by invoice number or supplier invoice number (the backend `Search` parameter matches both), filter by Status (select dropdown), filter by PurchaseOrderId (optional, for navigating from PO detail). **Current state**: only status filter exists. Search and purchaseOrderId filter are missing.

- **FR-134**: The list page MUST provide: skeleton loading state, error state with retry button, empty state with Arabic message "لا توجد فواتير موردين". When filters change, the page MUST reset to page 1. **Current state**: no skeleton, no error state, no page reset on filter change.

- **FR-135**: The list page MUST NOT expose lifecycle action buttons (Submit, Match, Cancel) inline in the table. These are detail-page-only actions. The list table actions MUST be limited to view (navigate to detail). **CONTRADICTION with current code**: the existing `SupplierInvoicesListPage.tsx` renders Submit, Match, and Cancel buttons inline in the table rows (lines 43–57). These MUST be removed per the project pattern established by GRNs (FR-114).

- **FR-136**: The list page MUST enforce permission-based visibility: the "فاتورة جديدة" (Create) button MUST only appear when the user has `SupplierInvoices.Create` permission. The `usePermission` hook from `@/shared/hooks/usePermission` MUST be used, following the GRN list page pattern (`GRNsListPage.tsx` line 17). **Current state**: no permission check exists on the create button.

- **FR-137**: The create page (`SupplierInvoiceCreatePage`) MUST load the purchase order for which the invoice is being created. The page MUST require `?purchaseOrderId=<id>` query parameter (same pattern as GRN create page, FR-115). If the parameter is absent or invalid, the page MUST display an error and not render the form. The PO is pre-selected and the user cannot change it. The page MUST load PO details and display PO line items that have `RemainingQuantity > 0` (for lines not yet fully invoiced). The primary navigation path is from the PO detail page's "Create Supplier Invoice" button.

- **FR-138**: The create form header fields: Supplier Invoice Number (text input, required — the supplier's own invoice number), Invoice Date (date picker, default today, required), Currency Code (select, optional), Exchange Rate (number, optional), Due Date (date picker, optional), Notes (textarea, optional). The Supplier Party is inherited from the PO and MUST NOT be editable. The system-generated Invoice Number (`SINV-{D6}`) is assigned by the backend and is NOT shown in the form.

- **FR-139**: The create form line items table: each eligible PO line (RemainingQuantity > 0 or not fully invoiced) is displayed with: Item Name (read-only, from PO line), Unit (read-only), Ordered Quantity (read-only), Received Quantity (read-only, from GRN), Unit Price (read-only, from PO line). User-input fields per line: Quantity (number, required, > 0 — the invoiced quantity), Unit Price (number, required, >= 0 — defaults to PO unit price but editable for price variance), Discount Amount (number, optional), Tax Amount (number, optional), Notes (text, optional). The `PurchaseOrderDetailId` is set automatically from the PO line. The `GoodsReceiptNoteDetailId` is optional and set if the line is linked to a specific GRN line.

- **FR-140**: The Zod schema for Supplier Invoice creation (`features/procurement/supplier-invoices/shared/schemas.ts`) MUST validate: `purchaseOrderId` required (number > 0), `supplierInvoiceNumber` required (string, min 1), `invoiceDate` required (date), at least one line item required (array min(1)). Per-line validation: `purchaseOrderDetailId` required (number > 0), `itemId` required (number > 0), `quantity` > 0, `unitPrice` >= 0. The schema MUST be reused for create form validation and default values.

- **FR-141**: On successful creation, the user MUST be navigated to the detail page of the newly created invoice. The POST endpoint returns the new invoice ID. 4xx errors MUST display field-level or form-summary errors. 5xx errors MUST display a toast notification. Error mapping MUST use `shared/api/result-to-ui.ts`.

- **FR-142**: The detail page (`SupplierInvoiceDetailPage`) MUST display: invoice summary (system-generated number, supplier invoice number, invoice date, due date, status badge, created date), related Purchase Order number (as a clickable link to PO detail), Supplier Name (or ID fallback — **backend gap: returns null**), financial summary (SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal — all with tabular-nums), Notes. A "Back to Purchase Order" link MUST be shown when the invoice has a `purchaseOrderId`.

- **FR-143**: The detail page MUST display a line items table with columns: Item (name or ID fallback — **backend gap: ItemNameAr returns null**), Quantity, Unit Price, Discount Amount, Tax Amount, Line Total, Notes. All monetary values MUST use `tabular-nums`. Quantities MUST use Latin numerals.

- **FR-144**: The detail page MUST show lifecycle action buttons ONLY when applicable to the current status:
  - **Draft**: Submit button (calls `PATCH /{id}/submit`)
  - **Submitted**: Match button (calls `PATCH /{id}/match`), Cancel button (opens dialog)
  - **Matched/PartiallyPaid**: Cancel button (opens dialog) — **Note**: the backend `CancelSupplierInvoiceCommandHandler` blocks cancellation for Paid/PartiallyPaid statuses (line 18–19 of handler), but allows Cancelled and other statuses. The frontend MUST reflect this: Cancel is available for Draft, Submitted, Matched, Disputed statuses only.
  - **Paid/Cancelled**: No action buttons.
  - **CONTRADICTION**: the backend `AcceptInvoiceWithNotesCommand` exists but its endpoint is NOT registered. The frontend MUST NOT render an "Accept with Notes" button until the endpoint is wired. If/when the endpoint is registered, the detail page MUST show an "Accept with Notes" button for Matched status that opens a dialog with a Notes textarea.

- **FR-145**: The Cancel action MUST open a dialog with an optional Notes textarea (the backend `CancelSupplierInvoiceCommand` accepts `Notes` as optional string). On submission, the notes are sent in the PATCH request body. The dialog MUST disable the confirm button during mutation and show loading state. On success: invalidate list and detail queries, show success toast, update displayed status.

- **FR-146**: The detail page MUST show a link to the related Purchase Order detail page (`/procurement/purchase-orders/:id`). The PO detail page MUST show a "Create Supplier Invoice" button when the PO status is `Issued` or `PartiallyReceived` AND the user has `SupplierInvoices.Create` permission. The button MUST navigate to `/procurement/supplier-invoices/create?purchaseOrderId=<poId>`.

- **FR-147**: The supplier invoices feature MUST have `shared/catalog-hooks.ts` providing `useItems()`, `useUnits()`, `useCurrencies()` hooks that fetch reference data from the API with `staleTime: Infinity`. The `useSuppliers()` hook MUST also be available (for future use when supplier selection becomes editable). These follow the same pattern as `features/procurement/goods-receipt-notes/shared/catalog-hooks.ts`.

- **FR-148**: The supplier invoices feature MUST have `shared/types.ts` exporting: `SupplierInvoice`, `SupplierInvoiceDetail`, `SupplierInvoiceDetailLine` interfaces, `SupplierInvoiceStatus` type union, `supplierInvoiceStatusLabels` record (Arabic), and `supplierInvoiceStatusVariant` map for `StatusBadge`. **CONTRADICTION with current code**: the existing `shared/types.ts` has a circular self-import (`import type { SupplierInvoiceStatus } from './types'` on line 1) and is missing fields: `purchaseOrderNumber`, `supplierName`, `subTotal`, `discountAmount`, `taxAmount`, `shippingCost`, `otherCharges`, `dueDate`, `created`. The `SupplierInvoiceDetailLine` is missing `itemNameAr`, `discountAmount`, `taxAmount`.

- **FR-149**: The supplier invoices hooks (`hooks/useSupplierInvoices.ts`) MUST include: `useSupplierInvoicesList(params)`, `useSupplierInvoiceDetail(id)`, `useCreateSupplierInvoice()`, `useSubmitSupplierInvoice()`, `useMatchSupplierInvoice()`, `useCancelSupplierInvoice()`. All mutations MUST invalidate relevant queries on success. Error handling MUST use `handleApiError` for form errors and `handleLifecycleError` for toast errors, following the project pattern. **Current state**: hooks exist but lack error handling utilities, the list hook types response as `SupplierInvoice[]` instead of `PaginatedList<SupplierInvoice>`, and `useCreateSupplierInvoice` is missing.

- **FR-150**: Invoice Number (system-generated `SINV-{D6}`) MUST render with `dir="ltr"` and monospace font. Supplier Invoice Number MUST render with `dir="ltr"`. Grand Total MUST render bold with `tabular-nums`. All monetary values MUST use `tabular-nums`.

- **FR-151**: Permission codes for Supplier Invoice frontend MUST include: `SupplierInvoices.View`, `SupplierInvoices.Create`, `SupplierInvoices.Submit`, `SupplierInvoices.Match`, `SupplierInvoices.Cancel` in `shared/constants/permissions.ts`. **Current state**: these codes do not exist in the permissions file. The `PROCUREMENT_PERMISSIONS` object only contains `PurchaseRequests` and `GoodsReceipts`.

- **FR-152**: All Supplier Invoice pages MUST use Arabic-only hardcoded strings (no `t()` calls, no locale files). RTL layout with logical CSS properties only (`ms-/me-/ps-/pe-`, `text-start/text-end`). No physical CSS properties (`ml/mr/pl/pr/text-right/text-left`).

- **FR-153**: The supplier invoices feature MUST follow project structure: pages in `features/procurement/supplier-invoices/pages/`, hooks in `features/procurement/supplier-invoices/hooks/`, shared types/schemas/client in `features/procurement/supplier-invoices/shared/`. Zod schema in `shared/schemas.ts`. Reusable components in `src/components/` with `ProcurementSupplierInvoices` prefix. No `components/` folder inside the feature. Cross-entity procurement sharing in `features/procurement/shared/`.

#### Purchase Order — Test Coverage Requirements

- **FR-102**: Unit tests MUST exist for `SubmitPurchaseOrderCommandHandler`: test Draft→Submitted transition succeeds, test non-Draft status rejects submission. Follow pattern in `PurchaseRequestTests.cs`.
- **FR-103**: Unit tests MUST exist for `UpdatePurchaseOrderCommandHandler`: test Draft status update succeeds with line modifications, test non-Draft status rejects update, test line add/update/remove logic, test header totals recomputation.
- **FR-104**: Unit tests MUST exist for `ClosePurchaseOrderCommandHandler`: test PartiallyReceived→Closed succeeds, test Released quantity released from encumbrance, test non-closeable statuses reject closure.
- **FR-105**: Unit tests MUST cover all invalid state transitions for PO: submit from non-Draft, approve from non-Submitted, issue from non-Approved, cancel from Cancelled/Closed, close from non-PartiallyReceived/Received. Each MUST be rejected with appropriate error.
- **FR-106**: Existing unit test `ApprovePO_Draft_ShouldTransitionToApproved` in `PurchaseOrderTests.cs` has a lifecycle bug: it calls Approve from Draft status but the handler requires Submitted. This test MUST be fixed to submit first, then approve.
- **FR-107**: Functional tests MUST cover: `UpdatePurchaseOrder` (create → update lines → verify totals), `ClosePurchaseOrder` (receive partial → close → verify status), `SubmitPurchaseOrder` as a standalone lifecycle step, direct purchase threshold validation (PO > 5M YER without RFQ must fail).
- **FR-108**: All four stub tests in `ApprovalEvaluationTests.cs` (EvaluatePurchaseOrder_LowAmount, EvaluatePurchaseOrder_MediumAmount, EvaluatePurchaseOrder_HighAmount, EvaluateWithFundSpecificRule) MUST be replaced with real assertions or marked as tracked debt with explicit justification.
- **FR-109**: PO tests MUST verify: encumbrance creation on approve (type=Commitment, lines match PO lines, amounts match), encumbrance reversal on cancel, budget availability check failure blocks approval, direct purchase path (≤5M YER, no RFQ required), non-Draft edit prevention.

### Key Entities

- **PurchaseRequest**: Header for a procurement need. Fields: RequestNumber, RequestDate, RequiredDate, DepartmentId, CostCenterId, RequesterId, Priority, Status, TotalEstimatedCost, Notes, audit, RowVersion. Contains PurchaseRequestDetail lines.
- **PurchaseRequestDetail**: Line item on a PR. Fields: PurchaseRequestId, ItemId, UnitId, RequestedQuantity, ApprovedQuantity, UnitCostEstimate, TotalCostEstimate, Notes, audit, RowVersion.
- **RequestForQuotation**: Competitive bidding document. Fields: RFQNumber, RFQDate, PurchaseRequestId, DeadlineDate, CurrencyCode, TermsAndConditions, Status, Notes, audit, RowVersion. Contains RFQSupplier invitations.
- **RFQSupplier**: Supplier invitation/response on an RFQ. Fields: RFQId, SupplierPartyId, InvitationDate, ResponseDate, Status, Notes, audit, RowVersion.
- **Quotation**: Supplier offer in response to RFQ. Fields: QuotationNumber, RFQId, RFQSupplierId, SupplierPartyId, QuotationDate, ValidUntil, CurrencyCode, ExchangeRate, SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal, PaymentTerms, DeliveryTerms, LeadTimeDays, WarrantyPeriodMonths, Status, SelectionReason, RejectionReason, audit, RowVersion. Contains QuotationDetail lines.
- **QuotationDetail**: Line item on a quotation. Fields: QuotationId, PurchaseRequestDetailId, ItemId, UnitId, Quantity, UnitPrice, DiscountPercent, DiscountAmount, NetUnitPrice, TaxPercent, TaxAmount, LineTotal, LineTotalWithTax, Notes, audit, RowVersion.
- **PurchaseOrder**: Binding procurement contract. Fields: PONumber, PODate, PurchaseRequestId, QuotationId, SupplierPartyId, WarehouseId, DeliveryLocationId, CurrencyCode, ExchangeRate, SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal, PaymentTerms, DeliveryTerms, ExpectedDeliveryDate, Status, Notes, audit, RowVersion. Contains PurchaseOrderDetail lines.
- **PurchaseOrderDetail**: Line item on a PO. Fields: PurchaseOrderId, PurchaseRequestDetailId, QuotationDetailId, ItemId, UnitId, OrderedQuantity, ReceivedQuantity, RemainingQuantity, UnitPrice, DiscountPercent, DiscountAmount, NetUnitPrice, TaxPercent, TaxAmount, LineTotal, LineTotalWithTax, ExpectedDeliveryDate, Status, Notes, audit, RowVersion.
- **GoodsReceiptNote**: Physical receipt document. Fields: GRNNumber, GRNDate, SupplierPartyId, PurchaseOrderId, WarehouseId, LocationId, ReceivedBy, Status, Notes, audit, RowVersion. Contains GoodsReceiptNoteDetail lines.
- **GoodsReceiptNoteDetail**: Received item line. Fields: GRNId, PurchaseOrderDetailId, ItemId, UnitId, OrderedQuantity, ReceivedQuantity, AcceptedQuantity, RejectedQuantity, RemainingQuantity, UnitCost, TotalCost, BatchNumber, ExpiryDate, Notes, audit, RowVersion.
- **SupplierInvoice**: Supplier billing document. Fields: InvoiceNumber, SupplierInvoiceNumber, InvoiceDate, PurchaseOrderId, SupplierPartyId, CurrencyCode, ExchangeRate, SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal, DueDate, Status, Notes, audit, RowVersion. Contains SupplierInvoiceDetail lines.
- **SupplierInvoiceDetail**: Invoice line item. Fields: SupplierInvoiceId, PurchaseOrderDetailId, GoodsReceiptNoteDetailId, ItemId, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, Notes, audit, RowVersion.
- **Encumbrance** (existing, extended): Budget reservation. Existing fields plus: link to PO via PurchaseOrderId (already exists). EncumbranceType.Commitment used for PO commitments.
- **PaymentOrder** (existing, extended): Disbursement order. Already links to PurchaseOrderId and EncumbranceId. No structural changes needed beyond ensuring correct population.
- **CommitteeAssignment** (existing, extended): Committee assignment. Existing PurchaseOrderId field is repurposed: for evaluation committees, link to RFQ via a new RfqId FK (or the existing field is used for RFQ ID with type indicator).

## Success Criteria

### Measurable Outcomes

- **SC-001**: A complete procurement cycle (PR creation to payment completion) can be executed end-to-end with all 8 stages linked and traceable.
- **SC-002**: All 8 document types have working list/detail screens with Arabic RTL rendering, search, filter, and status indicators.
- **SC-003**: Every lifecycle transition is recorded in both ApprovalHistory and DocumentStatusLog with complete audit metadata (actor, timestamp, decision, reason).
- **SC-004**: Duplicate supplier invoices are rejected 100% of the time.
- **SC-005**: Receiving more than the ordered quantity is rejected 100% of the time.
- **SC-006**: Over-invoicing (invoice quantity exceeds received quantity) is rejected 100% of the time.
- **SC-007**: Budget encumbrance is created atomically on PO approval and reversed atomically on PO cancellation — no orphaned encumbrances exist.
- **SC-008**: The legacy `SupplierId` field is completely removed from all procurement entities and replaced by `SupplierPartyId`.
- **SC-009**: All existing procurement data is preserved through migration. No business documents or audit records are deleted.
- **SC-010**: Navigation between linked documents (PR→RFQ→Quotation→PO→GRN→Invoice→Payment) works in both directions.
- **SC-011**: Evaluation committee assignment is mandatory and enforced — quotation evaluation cannot complete without a committee.
- **SC-012**: The full cycle (PR→PO→Receipt→Invoice→Payment→Close) completes without any step leaving orphaned references or broken foreign keys.
- **SC-013**: Existing procurement-related tests (if any) continue to pass after the rebuild. No test is weakened, skipped, or deleted.
- **SC-014**: All PO backend endpoints return real data (no stubs/placeholders): GET list resolves supplier names, GET by ID returns full detail with lines, submit dispatches the command, update processes line changes.
- **SC-015**: Every PO lifecycle transition (submit, approve, issue, cancel, close) is recorded in DocumentStatusLog with actor, timestamps, and from/to statuses.
- **SC-016**: PO cancel and close dialogs capture reason; the reason is stored in ApprovalHistory and visible in the audit trail.
- **SC-017**: PO form validation rejects: missing supplier, zero lines, quantity ≤ 0, discount/tax percent outside 0–100, editing non-Draft PO. All rejection messages display inline.
- **SC-018**: PO frontend has four working pages (list, create, detail, edit) with Arabic RTL rendering, skeleton loading, error retry, and empty states.
- **SC-019**: PO list filters work correctly: search by number, filter by status/supplier/PR/quotation/delivery date range.
- **SC-020**: PO create/edit forms compute line and header totals client-side for immediate display; backend recalculates on save.
- **SC-021**: Navigation links between PO and related documents (PR, Quotation, GRN, SupplierInvoice, PaymentOrder) work in both directions from the detail page.
- **SC-022**: All PO commands have FluentValidation validators enforcing: required fields, positive quantities, valid percentages, Draft-only editability.
- **SC-023**: Unit test coverage for PO includes all status transitions (valid and invalid), update with line changes, close with encumbrance release, and direct purchase threshold.
- **SC-024**: No circular imports in frontend types; purchase-orders feature types, schemas, and catalog hooks are properly structured per convention.
- **SC-025**: GRN list page displays correct columns with Arabic RTL, server-side pagination, status badge, and filters (search, status, purchase order). Skeleton, error retry, and empty states work correctly.
- **SC-026**: GRN create page loads PO details when `?purchaseOrderId=` is provided, shows only lines with remaining quantity, validates all Zod rules (accepted + rejected = received, received ≤ remaining, warehouse has location), and navigates to detail on success.
- **SC-027**: GRN detail page shows full summary with PO link, warehouse, location, receiver, and line items table. Confirm/reject actions are only available for Draft status, require explicit dialog confirmation, and disable during mutation.
- **SC-028**: GRN permission codes in frontend match backend (`GoodsReceipts.*` not `GoodsReceiptNotes.*`). Route guards enforce correct permissions. PO detail page shows "Create GRN" button only for eligible PO statuses with correct permission.
- **SC-029**: GRN feature has no inline confirm/reject in list page; actions are detail-page-only. Double-submission is prevented. List and detail queries are invalidated after successful mutations.
- **SC-030**: Supplier Invoice list page displays correct columns with Arabic RTL, server-side pagination, status badge, and filters (search, status). Skeleton, error retry, and empty states work correctly.
- **SC-031**: Supplier Invoice create page loads PO details when `?purchaseOrderId=` is provided, shows only lines not fully invoiced, validates all Zod rules, and navigates to detail on success.
- **SC-032**: Supplier Invoice detail page shows full summary with PO link, supplier name (or fallback), financial summary, and line items table. Lifecycle action buttons are status-gated. Cancel opens dialog with optional notes. Double-submission is prevented.
- **SC-033**: Supplier Invoice permission codes in frontend match backend (`SupplierInvoices.*`). Route guards enforce correct permissions. PO detail page shows "Create Supplier Invoice" button only for eligible PO statuses with correct permission.
- **SC-034**: Supplier Invoice feature has no inline lifecycle actions in list page; actions are detail-page-only. List and detail queries are invalidated after successful mutations.
- **SC-035**: Supplier Invoice types file has no circular self-imports. All backend response fields are represented in frontend interfaces.

## Gap & Contradiction Table

| # | Type | Location | Description | Status |
|---|------|----------|-------------|--------|
| G1 | **Backend Gap** | `MatchSupplierInvoiceCommandHandler` | Handler changes status only; does not perform three-way matching (FR-049). No quantity/price comparison against PO/GRN lines. | Backend gap — frontend match action is status-transition-only |
| G2 | **Backend Gap** | `AcceptInvoiceWithNotesCommandHandler` | Handler sets Notes only; does not change status. Endpoint NOT registered in `SupplierInvoices.cs`. | Backend gap — frontend cannot call this action |
| G3 | **Backend Gap** | `GetSupplierInvoiceByIdQueryHandler` | Returns `SupplierName: null` and `ItemNameAr: null` (hardcoded). | Backend gap — frontend uses ID/fallback |
| G4 | **Backend Gap** | `GetSupplierInvoicesQueryHandler` | Does not join Parties table; `SupplierName` not in list response. | Backend gap — frontend omits supplier name column or uses fallback |
| G5 | **Backend Gap** | `SupplierInvoices` endpoint group | No `.RequireAuthorization()` on any route. Commands have `[Authorize]` but routes are unprotected. | Backend gap — frontend still declares permissions |
| G6 | **Backend Gap** | `AcceptInvoiceWithNotes` endpoint | Command and handler exist but endpoint is not wired in `SupplierInvoices.cs`. | Backend gap — blocked until endpoint registered |
| G7 | **Frontend Bug** | `shared/types.ts` line 1 | Circular self-import: `import type { SupplierInvoiceStatus } from './types'`. | Must fix — type should be defined or imported from a non-circular source |
| G8 | **Frontend Bug** | `hooks/useSupplierInvoices.ts` line 14 | `useSupplierInvoicesList` types response as `SupplierInvoice[]` instead of `PaginatedList<SupplierInvoice>`. | Must fix — response shape mismatch |
| G9 | **Spec–Code Contradiction** | `SupplierInvoicesListPage.tsx` lines 43–57 | List page renders Submit/Match/Cancel buttons inline in table. Spec FR-135 (and GRN pattern FR-114) requires detail-page-only actions. | Must fix — remove inline actions |
| G10 | **Spec–Code Contradiction** | `SupplierInvoicesListPage.tsx` | No server-side pagination, no skeleton, no error state, no page reset on filter change. Spec FR-132/FR-134 requires these. | Must fix — align with GRN list pattern |
| G11 | **Spec–Code Contradiction** | `shared/types.ts` | Missing fields: `purchaseOrderNumber`, `supplierName`, `subTotal`, `discountAmount`, `taxAmount`, `shippingCost`, `otherCharges`, `dueDate`, `created`. Spec FR-148 requires full field set. | Must fix — extend interfaces |
| G12 | **Spec–Code Contradiction** | `shared/types.ts` | `SupplierInvoiceDetailLine` missing `itemNameAr`, `discountAmount`, `taxAmount`. Spec FR-143 requires these columns. | Must fix — extend interface |
| G13 | **Spec–Code Contradiction** | `shared/constants/permissions.ts` | `SupplierInvoices.*` permission codes not defined. Spec FR-151 requires them. | Must fix — add to `PROCUREMENT_PERMISSIONS` |
| G14 | **Spec–Code Contradiction** | `src/app/routes.tsx` | Only list route registered. Create and detail routes missing. Spec FR-130 requires three routes. | Must fix — add routes |
| G15 | **Missing** | Create page | Does not exist. Spec FR-137 requires it. | Must create |
| G16 | **Missing** | Detail page | Does not exist. Spec FR-142 requires it. | Must create |
| G17 | **Missing** | `shared/schemas.ts` | Does not exist. Spec FR-140 requires Zod schema. | Must create |
| G18 | **Missing** | `shared/catalog-hooks.ts` | Does not exist. Spec FR-147 requires catalog hooks. | Must create |
| G19 | **Missing** | PO detail page | "Create Supplier Invoice" button not wired. Spec FR-146 requires it. | Must add |
| G20 | **Spec Clarification** | FR-050 (Accept with Notes) | Spec said "liability is established" but backend handler only sets Notes. **Resolved**: Notes-only, no status change. Spec corrected. | Resolved — notes-only action |

## Questions That Cannot Be Resolved by Evidence

| # | Question | Context | Impact |
|---|----------|---------|--------|
| Q1 | Should `AcceptInvoiceWithNotes` change the invoice status (e.g., from Matched to a different status), or is it purely a notes-logging action? | Spec FR-050 says "acceptance is recorded and the liability is established at the accepted amount." Backend handler only sets Notes. No status change. | Affects detail page action availability and status transition diagram |
| Q2 | ~~Should the Supplier Invoice create page support creating an invoice without a `?purchaseOrderId=` (standalone), or is PO-linked creation mandatory?~~ | **Resolved**: PO-linked only. `?purchaseOrderId=` always required. | Resolved |
| Q3 | ~~What is the acceptable price variance behavior when the frontend allows editing Unit Price on invoice lines (FR-139)?~~ | **Resolved**: Pre-fill from PO, editable. Match catches variances. | Resolved |

## Assumptions

- The supplier is a Party with PartyType.Supplier. The Parties module (spec 022) is already in place and functional.
- The budget module (spec 046) with BudgetItemAllocations, Encumbrance, and BudgetAvailabilityService is the basis for all budget reservation logic.
- The payment module (spec 045) with PaymentOrder lifecycle is the basis for all payment processing.
- The Committee module with CommitteeAssignment entity and CommitteeAssignmentType enum (Tender, Receiving, Inspection) is already functional.
- The document numbering service (DocumentSequenceService) is operational and supports new prefixes.
- The existing approval workflow engine handles approval routing; this spec defines the procurement lifecycle but not the approval rule evaluation engine internals.
- Frontend language remains Arabic-only with RTL layout; no localization files or language switchers are introduced.
- Existing procurement database data is preserved through migration; this is not a clean-slate database redesign.
- The existing Encumbrance entity and its lifecycle (Draft→PendingApproval→Approved→Active→PartiallyReleased→PartiallyLiquidated→FullyLiquidated→Closed→Cancelled→Reversed→Suspended) are reused without modification.
- The existing PaymentOrder entity and its lifecycle (Draft→Submitted→Approved→SentToTreasury→Paid→Cancelled→Rejected→Voided) are reused without modification.
- Advance payments are out of scope. All payments occur after receipt.
- Evaluation committees are mandatory for all RFQs that proceed to quotation evaluation.
- Purchase request items can be split across multiple suppliers via separate POs.
- Budget reservation (encumbrance) occurs at PO approval, not at PR approval. PR approval performs an availability check only.
- The CancelPurchaseOrderCommand and ClosePurchaseOrderCommand currently lack a Reason parameter. This spec requires adding Reason to the command, storing it in ApprovalHistory, and capturing it via frontend dialog. If the backend entity lacks reason storage, the reason is stored in the ApprovalHistory decision field.
- The UpdatePurchaseOrderCommandHandler currently ignores the Lines parameter. This spec requires implementing line-level add/update/remove logic with header total recomputation.
- The ApprovePurchaseOrderCommandHandler currently does not publish the PurchaseOrderApproved domain event. This spec requires publishing it after successful approval.
- The current endpoint group lacks GET / (list) and PUT /{id} (update) routes. This spec requires adding them.
