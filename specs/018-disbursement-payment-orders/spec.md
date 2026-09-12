# Feature Specification: Disbursement of Approved Payment Orders

**Feature Branch**: `018-disbursement-payment-orders`

**Created**: 2026-09-05

**Updated**: 2026-09-09

**Status**: Draft

**Input**: User description: "أوامر الصرف — مستند مالي مستقل داخل دورة الصرف. أمر الصرف مستند مالي مستقل إذا كان الأمر ينشأ من طلب أو مستند معتمد، فلا يسمح بإنشائه إلا بعد اعتماد المصدر. مبلغ أمر الصرف يجب أن يعتمد على المبلغ المعتمد فعليًا وليس المبلغ المطلوب. يجب الحفاظ على إمكانية تتبع مصدر أمر الصرف وقرار الاعتماد الذي أدى إلى إنشائه. يجب التمييز بين المستخدم الذي نفذ العملية والجهة التي صدر الأمر باسمها. إعادة استخدام نموذج الجهات التنظيمية والصلاحيات الموجود حاليًا. دعم المستفيد وفق النموذج الموجود فعليًا. يبدأ أمر الصرف في حالة مسودة. يسمح أثناء المسودة باستكمال البيانات المالية والموازنية المطلوبة. لا يسمح باعتماد الأمر قبل اكتمال البيانات المالية المطلوبة. يجب أن تمر تغييرات الحالة عبر Workflow واضح. دعم الإرسال، الاعتماد، الرفض، الإلغاء، والتنفيذ المالي. بعد الاعتماد يجب منع التعديل الحر للبيانات المالية الجوهرية. استخدام نظام الخصومات الموجود حاليًا. صافي المبلغ يحسب من المبلغ الإجمالي ناقص الخصومات. لا يجوز أن تتجاوز الخصومات المبلغ الإجمالي. لا يسمح بتنفيذ الدفع إلا من أمر صرف في حالة تسمح بالدفع. منع إنشاء عمليات مكررة. حماية العمليات الحساسة من التنفيذ المتزامن. الحفاظ على آلية RowVersion. تسجيل انتقالات الحالات والعمليات المالية الحساسة. فصل صلاحيات العرض، التعديل، الاعتماد، الرفض، الإلغاء، وتنفيذ الدفع. تطوير شاشة قائمة أوامر الصرف وشاشة التفاصيل. عرض البيانات الأساسية، المستفيد، البيانات المالية، الخصومات، الصافي، مصدر الأمر، بيانات الدفع، وسجل العمليات."

## Clarifications

### Session 2026-09-09

- Q: Who is permitted to edit a draft disbursement request before submission? → A: Any user holding the DisbursementRequestsUpdate permission can edit any draft request. Ownership is not a constraint — the permission is role-based, matching the existing RBAC model and PaymentOrder editing behavior.
- Q: When are FundId and AppropriationId populated on the PaymentOrder auto-generated from an approved request? → A: Deferred. The order is generated as a Draft with FundId=0 and AppropriationId=null. A user with PaymentOrdersUpdate permission must prepare the order (set Fund, Appropriation, account, deductions) before submission. This gives the accountant flexibility to prepare orders in batches.
- Q: If the first approver approves for an amount less than the requested amount, can the request amount be edited before the second approval? → A: No. The requested amount is frozen once the first signature is recorded. Only notes and purpose can be updated after first approval. The first approved amount becomes the binding amount for the second signature. This prevents silent amount drift between signatures.
- Q: Which specific permission is required to cancel a disbursement request? → A: DisbursementRequestsCancel — the dedicated cancel permission code already defined in PermissionCodes.cs and bound to the cancel endpoint. This provides clean separation of duties.

## Context

The Payment module implements a **request-first** disbursement workflow:

1. An accountant creates a **DisbursementRequest** (طلب صرف) specifying beneficiary, amount, currency, purpose, and fiscal year.
2. The request enters a **dual-signature approval** process (≥2 distinct approvers, ≥1 with AccountsManager/AuthorizingOfficer role). Each approver specifies the **approved amount** (must not exceed requested amount), **issuing authority name**, and **issuing authority capacity**.
3. Upon the second approval, a **PaymentOrder** (أمر صرف) is **automatically generated** with the approved amount, carrying the issuing authority information.
4. The order proceeds through its lifecycle: Draft → Submitted (budget check) → Approved → SentToTreasury → Paid.
5. Payment execution records a **Payment** entity and triggers ledger posting via domain event.

The following requirements map to the existing codebase entities, commands, queries, endpoints, and frontend pages.

**Documented deviations from Constitution Principles**:
- Principle V (Budget Control Before Expenditure): Budget check runs at order submission (not at request creation). The approved amount on the request gates creation; the budget check gates order approval. Tender-law evidence and monthly spending plan gates are intentionally excluded.
- The dual-signature rule (US2) goes beyond the standard single-approval pattern used elsewhere; it is a domain-specific control for disbursements.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Disbursement Request (Priority: P1)

An accountant creates a disbursement request specifying the beneficiary name, requested amount, currency, purpose, and fiscal year. The system assigns a sequential request number and creates the request in Draft status. The request captures the identity of the user who created it (RequestedById/RequestedByName) separately from the beneficiary.

**Why this priority**: This is the entry point of the entire disbursement workflow. Without the ability to create a request, no downstream processing can occur.

**Independent Test**: Can be fully tested by creating a disbursement request with valid data (verify Draft status, assigned number, requester recorded) and by attempting creation with invalid data (zero amount, missing beneficiary, invalid currency/fiscal year — verify rejection).

**Acceptance Scenarios**:

1. **Given** an accountant with disbursement permissions, **When** they create a disbursement request with beneficiary name, amount > 0, valid currency, purpose, and valid fiscal year, **Then** the system creates the request in Draft status with a server-assigned sequential request number and records the requester identity.

2. **Given** an accountant, **When** they create a disbursement request with amount ≤ 0, **Then** the system rejects creation with an error indicating the amount must be greater than zero.

3. **Given** an accountant, **When** they create a disbursement request with a blank beneficiary name or blank purpose, **Then** the system rejects creation with appropriate validation errors.

4. **Given** an accountant, **When** they create a disbursement request with an invalid currency or invalid fiscal year, **Then** the system rejects creation with an error indicating the referenced entity is invalid.

---

### User Story 2 - Edit Draft Disbursement Request (Priority: P2)

While a disbursement request is in Draft status, any user holding the DisbursementRequestsUpdate permission may update the financial details (beneficiary, amount, currency, purpose, notes) before submitting for approval. Once submitted, financial details are locked. Ownership is not a constraint — the permission is role-based.

**Why this priority**: Draft requests frequently need corrections before formal submission. Without edit capability, users must cancel and recreate.

**Independent Test**: Can be tested by creating a draft request, updating its details (verify changes persist), submitting it, then attempting to update (verify rejection).

**Acceptance Scenarios**:

1. **Given** a disbursement request in Draft status, **When** an authorized user updates the beneficiary name, requested amount, currency, purpose, or notes, **Then** the system accepts the update and persists the changes.

2. **Given** a disbursement request in Draft status, **When** an authorized user updates the requested amount, **Then** the system validates the new amount is greater than zero.

3. **Given** a disbursement request that is NOT in Draft status, **When** any user attempts to update the financial details, **Then** the system rejects the update with an error indicating only draft requests can be edited.

4. **Given** a disbursement request in Draft status, **When** an update is attempted with a RowVersion that does not match the current record, **Then** the system rejects the update with a concurrency conflict error.

5. **Given** a disbursement request that has one approval recorded (PendingApproval status), **When** an authorized user attempts to update the requested amount, **Then** the system rejects the update with an error indicating the amount is frozen after the first approval. Only notes and purpose may be updated at this stage.

6. **Given** a disbursement request that has one approval recorded, **When** an authorized user updates the notes or purpose, **Then** the system accepts the update without affecting the recorded approval or the frozen approved amount.

---

### User Story 3 - Dual-Signature Approval of Disbursement Request (Priority: P1)

Disbursement requests require dual signature for approval: at least two distinct approvers must approve, and at least one must hold the AccountsManager or AuthorizingOfficer role. Each approver records the **approved amount** (must not exceed the requested amount), the **issuing authority name** (the officeholder's name), and the **issuing authority capacity** (the officeholder's title, e.g., General Manager or Finance Director). Both signatures must authorize the same amount. Every approval decision is recorded in the approval history with step number, approver identity, role, decision, timestamp, and evaluation snapshot containing the approved amount and authority information.

**Why this priority**: Dual signature is a critical internal control for disbursements — the highest-risk financial operation. Without this gate, funds could be disbursed without adequate authorization.

**Independent Test**: Can be tested by submitting a Draft request, having one qualified approver approve with an amount and authority details (verify step 1 recorded, request still pending), then having a second distinct qualified approver approve with the same amount (verify step 2 recorded, request Approved, PaymentOrder generated atomically).

**Acceptance Scenarios**:

1. **Given** a disbursement request in Draft status, **When** the requester submits it for approval, **Then** the status changes to PendingApproval.

2. **Given** a disbursement request is PendingApproval, **When** the first approver (who must hold AccountsManager or AuthorizingOfficer role) approves with an approved amount ≤ requested amount, issuing authority name, and issuing authority capacity, **Then** the approval is recorded in ApprovalHistory with step 1, the approver's identity, timestamp, role, decision, and an evaluation snapshot containing the approved amount and authority details. The request remains PendingApproval.

3. **Given** a disbursement request has one approval recorded, **When** a second distinct approver approves with the same approved amount as the first, **Then** the approval is recorded in ApprovalHistory with step 2, and the request status transitions to Approved. A PaymentOrder is automatically generated as a Draft with the approved amount and issuing authority information, with FundId=0 and AppropriationId=null (to be prepared before submission).

4. **Given** a disbursement request has one approval recorded, **When** the same approver attempts to approve again, **Then** the system rejects the approval with an error indicating a different approver is required.

5. **Given** a disbursement request is PendingApproval, **When** an approver who does not hold AccountsManager or AuthorizingOfficer role attempts to approve as the first approver, **Then** the system rejects the approval with an error indicating the required role is missing.

6. **Given** a disbursement request has one approval recorded with amount X, **When** the second approver attempts to approve with amount Y where Y ≠ X, **Then** the system rejects the approval with an error indicating signatures on different amounts cannot finalize.

7. **Given** a disbursement request has one approval recorded, **When** an approver attempts to approve with an amount exceeding the requested amount, **Then** the system rejects the approval with an error indicating the approved amount cannot exceed the requested amount.

8. **Given** a disbursement request is PendingApproval, **When** an approver rejects the request with a reason, **Then** the rejection is recorded in ApprovalHistory and the request status transitions to Rejected.

---

### User Story 4 - Cancel Disbursement Request (Priority: P2)

A disbursement request may be cancelled at various stages: Draft, PendingApproval, or Approved (if the linked PaymentOrder has not yet been paid). Cancellation requires the DisbursementRequestsCancel permission and a reason, and records the cancellation in ApprovalHistory. If a PaymentOrder has been generated (Approved status), the order is invalidated.

**Why this priority**: Cancellation is essential for correcting mistakes and handling changed circumstances before funds are disbursed.

**Independent Test**: Can be tested by cancelling a Draft request (verify status change), cancelling a PendingApproval request (verify status change and approval history), and cancelling an Approved request with a linked Draft PaymentOrder (verify both statuses updated).

**Acceptance Scenarios**:

1. **Given** a disbursement request in Draft status, **When** a user with DisbursementRequestsCancel permission cancels it with a reason, **Then** the status transitions to Cancelled and the cancellation is recorded in ApprovalHistory.

2. **Given** a disbursement request in PendingApproval status, **When** a user with DisbursementRequestsCancel permission cancels it with a reason, **Then** the status transitions to Cancelled and the cancellation is recorded in ApprovalHistory.

3. **Given** a disbursement request in Approved status with a linked PaymentOrder that has NOT been paid, **When** a user with DisbursementRequestsCancel permission cancels it with a reason, **Then** the request status transitions to Cancelled, the linked PaymentOrder is invalidated, and the cancellation is recorded in ApprovalHistory.

4. **Given** a disbursement request in Approved status with a linked PaymentOrder that HAS been paid, **When** any user attempts to cancel, **Then** the system rejects the cancellation with an error indicating the order has already been paid.

---

### User Story 5 - Order Lifecycle and Budget Check (Priority: P1)

The PaymentOrder generated from an approved disbursement request follows a strict lifecycle: Draft → Submitted → Approved → SentToTreasury → Paid. The order is generated as a Draft with FundId=0 and AppropriationId=null. A user with PaymentOrdersUpdate permission must prepare the order (set Fund, Appropriation, account, deductions) before submission. At submission, the system runs a budget availability check. Budget check failure blocks approval unless a documented override is applied (with dedicated permission). The order tracks TreasuryStatus, TreasuryReference, and TreasurySentAt when sent to treasury.

**Why this priority**: The order lifecycle enforces financial controls (budget check, treasury tracking) before funds can be paid.

**Independent Test**: Can be tested by preparing a Draft order (adding FundId, AppropriationId, deductions), submitting it (verify budget check runs), approving it (verify status change), sending to treasury (verify treasury details recorded), and verifying that a failed budget check blocks approval.

**Acceptance Scenarios**:

1. **Given** a PaymentOrder in Draft status with FundId and AppropriationId set, **When** it is submitted, **Then** the status transitions to Submitted and a budget availability check runs, setting BudgetCheckStatus to Passed, Failed, or Overridden.

2. **Given** a PaymentOrder with BudgetCheckStatus = Failed, **When** approval is attempted without an override, **Then** the system rejects approval with an error indicating the budget check failed.

3. **Given** a PaymentOrder with BudgetCheckStatus = Failed, **When** a user with override permission approves with OverrideFailedBudgetCheck = true, **Then** the approval proceeds and the override is documented in the approval history.

4. **Given** a PaymentOrder in Submitted status, **When** it is approved, **Then** the status transitions to Approved. The approval is recorded in ApprovalHistory with the approver's identity, timestamp, and decision.

5. **Given** a PaymentOrder in Approved status, **When** it is sent to treasury with a treasury reference, **Then** the status transitions to SentToTreasury, and TreasuryReference and TreasurySentAt are recorded.

6. **Given** a PaymentOrder that is NOT in Draft status, **When** an update to financial details (AmountGross, DeductionAmount, BeneficiaryName) is attempted, **Then** the system rejects the update. Only Draft orders can be edited.

---

### User Story 6 - Deductions on Payment Orders (Priority: P1)

Payment orders support line-item deductions (tax, withholding tax, insurance, penalty, advance recovery, legal deduction, other). Each deduction has a type, description, amount, optional percentage, account, and mandatory flag. The header-level DeductionAmount must equal the sum of line-item deductions. Deductions with IsMandatory=true cannot be removed on update. Net amount is computed as AmountGross minus DeductionAmount. Deductions exist only on Draft orders and are replaced atomically on update.

**Why this priority**: Deductions directly affect the net payment amount and are a core financial control. Without proper deduction handling, payments would be incorrect.

**Independent Test**: Can be tested by creating an order with deductions (verify sum match), updating deductions (verify mandatory ones cannot be removed), verifying net amount calculation, and verifying that non-Draft orders reject deduction changes.

**Acceptance Scenarios**:

1. **Given** a PaymentOrder in Draft status, **When** deductions are added with total matching DeductionAmount, **Then** the order is created with the deductions persisted as line items.

2. **Given** a PaymentOrder in Draft status with existing deductions, **When** it is updated with a new set of deductions, **Then** all existing deductions are replaced atomically with the new set.

3. **Given** a PaymentOrder with a deduction where IsMandatory = true, **When** the order is updated and the mandatory deduction is not included in the new set, **Then** the system rejects the update with an error indicating mandatory deductions cannot be removed.

4. **Given** a PaymentOrder with AmountGross and DeductionAmount, **When** the net amount is computed, **Then** NetAmount = AmountGross - DeductionAmount, and DeductionAmount must not exceed AmountGross.

5. **Given** a PaymentOrder with a deduction where IsTaxDeduction = true, **When** the deduction is created without a TaxAuthorityId, **Then** the system rejects the deduction with an error indicating a tax authority is required for tax deductions.

6. **Given** a PaymentOrder that is NOT in Draft status, **When** any attempt to modify deductions is made, **Then** the system rejects the modification.

---

### User Story 7 - Record Payment Against Approved Order (Priority: P1)

Once a payment order is Approved or SentToTreasury, a payment can be executed. The system records the payment method, reference number, amount (server-computed as AmountGross - DeductionAmount), and execution timestamp. Only one payment per order is allowed. The payment triggers a domain event for ledger posting. Upon payment recording, the order status transitions to Paid, and if a linked DisbursementRequest exists, its status transitions to Disbursed.

**Why this priority**: Payment execution is the culmination of the disbursement workflow — the actual movement of funds.

**Independent Test**: Can be tested by recording a payment against an Approved order (verify payment created, order status Paid, disbursement request status Disbursed), and by attempting a second payment (verify rejection), and by attempting payment against a non-approved order (verify rejection).

**Acceptance Scenarios**:

1. **Given** a PaymentOrder in Approved or SentToTreasury status, **When** a payment is recorded with payment method and optional reference number, **Then** a Payment entity is created with Amount = AmountGross - DeductionAmount, the order status transitions to Paid, and a domain event is raised for ledger posting.

2. **Given** a PaymentOrder with a linked DisbursementRequest, **When** the payment is recorded, **Then** the DisbursementRequest status transitions to Disbursed and PaymentDate is set.

3. **Given** a PaymentOrder that already has a completed payment, **When** another payment is attempted, **Then** the system rejects the payment with an error indicating a payment has already been recorded for this order.

4. **Given** a PaymentOrder that is NOT in Approved or SentToTreasury status, **When** payment execution is attempted, **Then** the system rejects the payment with an error indicating the order must be approved first.

5. **Given** a PaymentOrder in Approved status, **When** it is voided (if no completed payments exist), **Then** the status transitions to Voided and any linked DisbursementRequest in Draft/PendingApproval/Approved(unpaid) status is invalidated.

---

### User Story 8 - Disbursement List and Detail Views (Priority: P1)

The system provides list and detail views for both disbursement requests and payment orders. The list view shows key fields (number, beneficiary, amount, status, date) with filtering by status and requester. The detail view shows complete information including financial data (amount, deductions, net), source linkage (linked PaymentOrder number for requests, linked DisbursementRequest number for orders), beneficiary details, approval history steps, and payment information when executed.

**Why this priority**: Users need visibility into the disbursement pipeline to track and manage requests and orders.

**Independent Test**: Can be tested by creating requests and orders with various statuses, verifying the list shows correct filtering, and verifying the detail view shows all required fields.

**Acceptance Scenarios**:

1. **Given** disbursement requests exist in various statuses, **When** the user views the list, **Then** they see request number, beneficiary name, requested amount, status, and request date, with the ability to filter by status and requester.

2. **Given** a disbursement request, **When** the user views its detail, **Then** they see the request number, beneficiary, requested amount, currency, purpose, fiscal year, requester identity, request date, status, notes, linked PaymentOrder number (if approved), approval history steps with approver details and amounts, and payment information (if disbursed).

3. **Given** payment orders exist in various statuses, **When** the user views the list, **Then** they see order number, beneficiary, gross amount, net amount (after deductions), status, and order date, with the ability to filter by status, fund, and fiscal year.

4. **Given** a payment order, **When** the user views its detail, **Then** they see the order number, beneficiary details (name, IBAN, account number, bank), gross amount, deduction amount, net amount, deduction line items, budget check status, treasury details, linked DisbursementRequest number, approval history, and payment information (if paid).

5. **Given** a payment order, **When** the user views its totals, **Then** the system shows AmountGross, TotalDeductions, NetAmount, PaidAmount, RemainingAmount, and IsFullyPaid.

---

### User Story 9 - Void Payment Order (Priority: P2)

A payment order in Approved or SentToTreasury status may be voided if no completed payment exists. Voiding transitions the order to Voided status and invalidates any linked DisbursementRequest in Draft/PendingApproval/Approved(unpaid) status. Voiding is a terminal state — the order cannot be reactivated.

**Why this priority**: Void provides a controlled mechanism to permanently deactivate an order that should not be paid, without allowing reuse.

**Independent Test**: Can be tested by voiding an Approved order with no payment (verify status Voided, linked request invalidated), and by attempting to void a Paid order (verify rejection).

**Acceptance Scenarios**:

1. **Given** a PaymentOrder in Approved or SentToTreasury status with no completed payment, **When** it is voided, **Then** the status transitions to Voided and any linked DisbursementRequest in Draft/PendingApproval/Approved(unpaid) is invalidated.

2. **Given** a PaymentOrder with a completed payment, **When** voiding is attempted, **Then** the system rejects the void with an error indicating the order has already been paid.

3. **Given** a PaymentOrder in Voided status, **When** any lifecycle action is attempted, **Then** the system rejects the action.

---

### Edge Cases

- **Second disbursement request for the same beneficiary/purpose**: The system allows multiple requests for the same beneficiary — the 1:1 constraint is between DisbursementRequest and PaymentOrder, not between requests. Each request is independent.

- **Request cancelled after approval**: When an approved request (with generated PaymentOrder) is cancelled, the order is invalidated. A new request can be created if needed.

- **Order amount amendment after request approval**: The PaymentOrder is generated with the approved amount from the request. Amendments to the order after creation follow the standard order lifecycle rules (Draft-only editing).

- **Concurrent approvals on the same request**: RowVersion optimistic concurrency prevents two simultaneous approvals from both succeeding. One will fail with a concurrency conflict error.

- **Budget availability check with Warning control method**: When the budget item control is Warning (not Blocking), the order submission proceeds with a warning flag but is NOT blocked.

- **Budget availability check with None control method**: When the budget item control is None, no availability check is performed.

- **Payment order amount equals zero**: Order creation from approval is rejected if the approved amount is zero or negative.

- **Requester and executor distinction**: The system records RequestedById/RequestedByName on the request (who created it), PaidById/PaidByName on the payment (who executed it), and IssuingAuthorityName/IssuingAuthorityCapacity on the order (on whose authority it was issued). These are separate, traceable identities.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST create a DisbursementRequest with a server-assigned sequential request number at Draft creation.

- **FR-002**: System MUST validate that RequestedAmount > 0, BeneficiaryName is not blank, Purpose is not blank, CurrencyId references a valid currency, and FinancialYearId references a valid fiscal year at creation.

- **FR-003**: System MUST allow any user with DisbursementRequestsUpdate permission to edit Draft disbursement requests (beneficiary, amount, currency, purpose, notes) with RowVersion concurrency check. Ownership is not required. Once the first approval is recorded (PendingApproval status with ≥1 approval), the requested amount is frozen — only notes and purpose may be updated.

- **FR-004**: System MUST reject edits to disbursement requests that are NOT in Draft status.

- **FR-005**: System MUST enforce dual-signature approval: at least two distinct approvers, with at least one holding the AccountsManager or AuthorizingOfficer role.

- **FR-006**: System MUST record every approval decision in ApprovalHistory with document type, document ID, step number, approver identity, role, decision, timestamp, and evaluation snapshot containing approved amount and issuing authority details.

- **FR-007**: System MUST reject approval attempts where the same user tries to approve twice on the same request.

- **FR-008**: System MUST reject approval where the approved amount exceeds the requested amount.

- **FR-009**: System MUST reject final approval (step 2) where the approved amount differs from the first approval's amount.

- **FR-010**: System MUST automatically generate a PaymentOrder atomically upon the second approval, carrying the approved amount, beneficiary, issuing authority name, issuing authority capacity, and DisbursementRequestId link.

- **FR-011**: System MUST enforce a unique DisbursementRequestId on PaymentOrder — exactly one order per approved request.

- **FR-012**: System MUST require FundId and AppropriationId to be set at order submission (not at auto-generation from approval) and run a budget availability check, setting BudgetCheckStatus to Passed, Failed, or Overridden. Orders generated from approved requests start with FundId=0 and AppropriationId=null and must be prepared before submission.

- **FR-013**: System MUST block order approval when BudgetCheckStatus is Failed, unless OverrideFailedBudgetCheck is set by a user with the override permission.

- **FR-014**: System MUST restrict PaymentOrder edits to Draft status only. After submission, financial details (AmountGross, DeductionAmount, BeneficiaryName) MUST NOT be modified.

- **FR-015**: System MUST support line-item deductions on PaymentOrders with types: Tax, WithholdingTax, Insurance, Penalty, AdvanceRecovery, LegalDeduction, Other.

- **FR-016**: System MUST validate that the sum of deduction line items equals the header DeductionAmount (within 0.01 tolerance).

- **FR-017**: System MUST prevent removal of deductions with IsMandatory=true during order updates.

- **FR-018**: System MUST reject deductions with IsTaxDeduction=true that lack a TaxAuthorityId.

- **FR-019**: System MUST compute NetAmount = AmountGross - DeductionAmount. DeductionAmount MUST NOT exceed AmountGross.

- **FR-020**: System MUST record a Payment with Amount = AmountGross - DeductionAmount, payment method, reference number, and execution timestamp upon payment execution.

- **FR-021**: System MUST enforce exactly one payment per PaymentOrder (ADR-001 D-6).

- **FR-022**: System MUST restrict payment execution to PaymentOrders in Approved or SentToTreasury status.

- **FR-023**: System MUST trigger a domain event for ledger posting upon payment recording. The journal entry MUST be balanced.

- **FR-024**: System MUST transition PaymentOrder status to Paid and linked DisbursementRequest status to Disbursed upon payment recording.

- **FR-025**: System MUST record all status transitions and financial operations in append-only history (ApprovalHistory, DocumentStatusLog) with actor, timestamp, and reason where applicable.

- **FR-026**: System MUST use RowVersion optimistic concurrency on all mutable entities (DisbursementRequest, PaymentOrder, Payment, PaymentOrderDeduction).

- **FR-027**: System MUST prevent cancellation of disbursement requests whose linked PaymentOrder has been paid.

- **FR-028**: System MUST invalidate linked DisbursementRequests (Draft/PendingApproval/Approved+unpaid) when a PaymentOrder is cancelled or voided.

- **FR-029**: System MUST assign permission codes for all operations: DisbursementRequestsView, DisbursementRequestsCreate, DisbursementRequestsUpdate, DisbursementRequestsSubmit, DisbursementRequestsApprove, DisbursementRequestsReject, DisbursementRequestsCancel; PaymentOrdersView, PaymentOrdersCreate, PaymentOrdersUpdate, PaymentOrdersSubmit, PaymentOrdersApprove, PaymentOrdersReject, PaymentOrdersCancel, PaymentOrdersSendToTreasury, PaymentOrdersVoid, PaymentOrdersOverrideBudgetCheck; PaymentsView, PaymentsCreate.

- **FR-030**: System MUST provide list and detail views for disbursement requests and payment orders with filtering by status, requester (requests), fund, and fiscal year (orders).

- **FR-031**: System MUST display on disbursement request detail: request number, beneficiary, amount, currency, purpose, requester identity, date, status, linked order number, approval history with amounts and authority details, and payment info.

- **FR-032**: System MUST display on payment order detail: order number, beneficiary details (name, IBAN, account, bank), gross amount, deductions (line items), net amount, budget check status, treasury details, linked request number, approval history, and payment info.

### Key Entities

- **DisbursementRequest**: Represents a request to disburse funds. Key attributes: request number (unique, sequential), status (Draft, PendingApproval, Approved, Rejected, Cancelled, Disbursed, Invalidated), beneficiary name, requested amount, currency, purpose, fiscal year, requester identity, request date. Relationships: one-to-one with PaymentOrder (via PaymentOrder.DisbursementRequestId UNIQUE); contains ApprovalHistory records.

- **PaymentOrder**: Represents the official disbursement order. Key attributes: order number (unique, sequential), status (Draft, Submitted, Approved, SentToTreasury, Paid, Cancelled, Rejected, Voided), beneficiary details, gross amount, deduction amount, deductions (line items), budget check status, treasury details, issuing authority. Relationships: belongs to DisbursementRequest (optional — direct creation path also supported); contains PaymentOrderDeductions; has one Payment when paid.

- **Payment**: Represents the executed payment. Key attributes: payment number (unique, sequential), amount (server-computed net), payment method, reference number, paid-by identity, execution timestamp, status (Completed, Failed). Relationships: belongs to PaymentOrder (one-to-one, UNIQUE); linked to DisbursementRequest via PaymentOrder.

- **PaymentOrderDeduction**: Represents a deduction line item on a payment order. Key attributes: line number (unique per order), deduction type, description, amount, percentage, mandatory flag, tax deduction flag, tax authority, account. Relationships: belongs to PaymentOrder.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A disbursement request moves from creation to Approved (dual-signed) to PaymentOrder generated in one session without leaving the workflow.

- **SC-002**: Every approved disbursement request has at least two distinct approvers with the required role composition — zero bypasses of the dual-signature requirement.

- **SC-003**: Every approval records the approved amount, issuing authority name, and issuing authority capacity in the approval history evaluation snapshot.

- **SC-004**: Disbursement request creation completes in under 30 seconds.

- **SC-005**: Payment execution posts to the ledger within the same transaction — zero unposted payments.

- **SC-006**: All status transitions are recorded in append-only history with actor, timestamp, and reason (where applicable).

- **SC-007**: Net amount computation (AmountGross - DeductionAmount) is consistent across order detail, payment recording, and reporting — zero discrepancies.

- **SC-008**: Disbursement request and payment order list and detail pages load in under 3 seconds.

- **SC-009**: Concurrency conflicts on simultaneous edits are detected and surfaced to the user with a clear error message.

## Assumptions

- Disbursement requests do not require a source document (e.g., purchase order) as input — they are standalone requests. The user creates the request directly.

- The dual-signature roles (AccountsManager, AuthorizingOfficer) are defined in the existing role system and are pre-seeded.

- The document sequence service is available and configured with appropriate prefixes (DSB for requests, PO for orders, PAY for payments).

- The posting pipeline (domain event → AccountingEvent → JournalEntry) is functional for disbursement payments. Journal entry templates follow existing payment posting rules.

- Single base currency is assumed per transaction.

- Frontend list and detail pages exist for disbursement requests, payment orders, and payments.

- The deduction system (PaymentOrderDeduction with IsMandatory protection, type taxonomy, sum validation) is fully implemented.

- Disbursement requests do not require tender-law evidence or monthly spending plan gates. This is a documented deviation from Principle V.

## Scope

### In Scope

- DisbursementRequest entity, lifecycle, creation, editing, submission, dual-signature approval, cancellation
- PaymentOrder entity, lifecycle, preparation, submission, budget check, approval, treasury tracking, void
- PaymentOrderDeduction entity, creation, update with mandatory protection, sum validation
- Payment entity, recording, one-per-order enforcement, ledger posting trigger
- ApprovalHistory recording for all decisions
- Permission codes for all operations
- List and detail views for requests and orders
- Edge case handling: duplicate prevention, cancellation cascading, invalidation on void, concurrency

### Out of Scope

- Tender-law evidence documentation
- Monthly spending plan enforcement (informational only, lives in budgeting)
- Receipt vouchers (handled by spec 017)
- Bank integration for payment execution (payment method and reference are recorded manually)
- Multi-currency conversion (single base currency per transaction assumed)
- Partial disbursements (order net total is always the payment amount)
