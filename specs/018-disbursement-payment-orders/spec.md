# Feature Specification: Disbursement of Approved Payment Orders

**Feature Branch**: `018-disbursement-payment-orders`

**Created**: 2026-09-05

**Status**: Draft

**Input**: User description: "Disbursement of approved payment orders, with budget-availability control as the ONLY gate. US1 (P1): An accountant creates a disbursement request against an approved payment order (exactly one disbursement per payment order). Creating the request runs a budget availability check; when the budget item control is Blocking and funds are insufficient, creation is rejected with the availability breakdown. US2 (P1): Disbursement requires dual signature: at least two distinct approvers, at least one holding AccountsManager or AuthorizingOfficer role. All decisions recorded in the approval history. US3 (P2): Once approved, the payment is executed and recorded (method, reference, amount equal to the payment order net total). The payment posts to the ledger automatically. US4 (P3): A disbursement register report lists requests and payments by period, fund, and status. Edge cases: second disbursement attempt on the same payment order; request cancelled after approval; payment order amended after request creation. Requirements: availability check is the ONLY gate — NO tender-law evidence or monthly-plan gate (documented deviations); dual-signature rule; 1:1 disbursement-to-payment-order; approval history records every decision. Entities: DisbursementRequest, Payment. Out of scope: monthly spending plans (informational, live in budgeting), receipt vouchers, evidence documentation."

## Context

The existing Payment module delivers the PaymentOrder entity through its lifecycle: Draft → Submitted → Approved → SentToTreasury → Paid → Cancelled → Rejected → Voided. Approval uses the standard ApprovalHistory + IApprovalRuleEvaluationService pipeline. Budget availability is computed by BudgetAvailabilityService which returns BudgetAvailabilitySummary (net appropriated − encumbered, control method, overrun policy).

This spec adds the final payment-execution layer: a DisbursementRequest that gates spending against budget availability (the ONLY control gate), enforces a dual-signature approval rule, then executes the payment and posts to the ledger.

**Documented deviations from Constitution Principles**:
- Principle V (Budget Control Before Expenditure) is honored — the availability check IS the gate. However, this feature explicitly does NOT enforce tender-law evidence or monthly spending plan gates. Those are out of scope per user requirements.
- The dual-signature rule (US2) goes beyond the standard single-approval pattern used elsewhere; it is a domain-specific control for disbursements.

## Clarifications

### Session 2026-09-05

- Q: Which user roles are authorized to cancel an approved but unpaid disbursement request? → A: Any user holding the disbursement permission.
- Q: How should concurrent disbursement requests competing for the same budget item be handled? → A: Point-in-time budget check with optimistic concurrency; Blocking control catches overruns.

### Session 2026-09-08 — Contract alignment check (PAY-01/PAY-02)

Verified as-built code against the PAY contract (specs PAY-01/PAY-02, group payments). As-built implementation was aligned to this spec; three deviations from the PAY contract are documented here as binding as-built behavior:

- Q: Which approver must hold the AccountsManager or AuthorizingOfficer role? → A (as-built, kept): the FIRST approver (step 1). Implementation: ApproveDisbursementRequestCommand.cs:52-68. The PAY-02 contract states the SECOND approver must be role-qualified — recorded as a documented deviation, not silently changed.
- Q: When does the budget availability gate run? → A (as-built, kept): at request CREATION (CreateDisbursementRequestCommand runs BudgetAvailabilityService; Blocking + insufficient ⇒ rejected at create). The PAY-02 contract places the Blocking reject at submit — recorded as a documented deviation.
- Q: Is the request status after payment "Paid" or "Disbursed"? → A: Disbursed (matches DisbursementRequestStatus enum and PAY-02 contract). Spec prose corrected below; the payment order itself still transitions to Paid (PaymentOrderStatus enum).

Code gaps discovered by the check (tracked as follow-up tasks in tasks.md):

- DisbursementRequests endpoints (`src/Web/Endpoints/DisbursementRequests/DisbursementRequests.cs`) have NO RequireAuthorization — the DisbursementRequests.* permission codes exist (PermissionCodes.cs:186-191) but are not bound to routes.
- PaymentOrders.Update permission code and an Update payment-order command (edit Draft only) do not exist; PAY-01 FR-001/BR-2 require them.
- PAY-01 contract routes (`/api/Payments/PaymentOrders`, `/api/Payments/DisbursementRequests`) deviate from the as-built `/api/{ClassName}` convention (`/api/PaymentOrders`, `/api/DisbursementRequests`) — AGENTS.md binding convention wins; contract route table marked deviating.
- Void semantics: as-built `VoidPaymentOrderCommand` allows voiding ONLY Paid orders (reversal semantics, VoidPaymentOrderCommand.cs:27). PAY-01 contract lifecycle expects void of unpaid Approved/SentToTreasury orders, blocked for partially paid (OQ-N1). Out of scope for 018 (payment-order lifecycle is PAY-01); flagged for the PAY-01 owner.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create Disbursement Request Against Approved Payment Order (Priority: P1)

An accountant selects an approved payment order and creates a disbursement request. The system enforces a strict 1:1 relationship: only one disbursement request may exist per payment order. Upon creation, the system runs a budget availability check against the payment order's budget item. When the budget item's control method is Blocking and available funds are insufficient, creation is rejected with a full availability breakdown showing net appropriated, encumbered, available, and the shortfall.

**Why this priority**: This is the foundation of the entire disbursement workflow. Without the ability to create a request gated by budget availability, no downstream processing (approval, payment execution) can occur.

**Independent Test**: Can be fully tested by creating a disbursement request against an approved payment order with sufficient funds (verify Draft status, 1:1 link) and by attempting to create a request against an approved payment order with insufficient funds on a Blocking budget item (verify rejection with availability breakdown).

**Acceptance Scenarios**:

1. **Given** a payment order is in Approved status with sufficient budget availability, **When** an accountant with disbursement permissions creates a disbursement request against it, **Then** the system creates a DisbursementRequest in Draft status linked to the payment order, and the payment order's status transitions to indicate a disbursement is in progress.

2. **Given** a payment order is in Approved status, **When** an accountant attempts to create a disbursement request and the budget item control is Blocking with insufficient funds, **Then** the system rejects creation and returns an availability breakdown: net appropriated amount, encumbered amount, available amount, requested amount, and shortfall amount.

3. **Given** a payment order already has a disbursement request (in any status), **When** another accountant attempts to create a second disbursement request against the same payment order, **Then** the system rejects the creation with an error indicating a disbursement request already exists for this payment order.

4. **Given** a payment order is not in Approved status, **When** an accountant attempts to create a disbursement request, **Then** the system rejects creation with an error indicating the payment order must be approved first.

5. **Given** a payment order is Approved and budget availability is sufficient (regardless of control method), **When** the disbursement request is created, **Then** the request number is assigned at creation using the document sequence service, and the status is Draft.

---

### User Story 2 - Dual-Signature Approval of Disbursement Request (Priority: P1)

Disbursement requests require dual signature for approval: at least two distinct approvers must approve, and at least one must hold the AccountsManager or AuthorizingOfficer role. Every approval decision is recorded in the approval history. A disbursement request cannot be approved by a single person, even if that person holds the required role.

**Why this priority**: Dual signature is a critical internal control for disbursements — the highest-risk financial operation. Without this gate, funds could be disbursed without adequate authorization.

**Independent Test**: Can be tested by submitting a Draft disbursement request, having one approver with AccountsManager role approve (verify pending — not yet approved), then having a second distinct approver approve (verify approved), and by verifying rejection when the same person tries to approve twice.

**Acceptance Scenarios**:

1. **Given** a disbursement request is in Draft status, **When** the requesting accountant submits it for approval, **Then** the status changes to Pending Approval and both designated approvers are notified.

2. **Given** a disbursement request is Pending Approval, **When** the first approver (who must hold AccountsManager or AuthorizingOfficer role) approves, **Then** the approval is recorded in ApprovalHistory with step 1, the approver's identity, timestamp, role, and decision. The request remains Pending Approval (not yet fully approved).

3. **Given** a disbursement request has one approval recorded, **When** a second distinct approver approves, **Then** the approval is recorded in ApprovalHistory with step 2, and the request status transitions to Approved.

4. **Given** a disbursement request has one approval recorded, **When** the same approver attempts to approve again, **Then** the system rejects the approval with an error indicating a different approver is required.

5. **Given** a disbursement request is Pending Approval, **When** an approver who does not hold AccountsManager or AuthorizingOfficer role attempts to approve as the first approver, **Then** the system rejects the approval with an error indicating the required role is missing.

6. **Given** a disbursement request is Pending Approval, **When** an approver rejects the request, **Then** the rejection is recorded in ApprovalHistory and the request status transitions to Rejected.

---

### User Story 3 - Execute Payment and Post to Ledger (Priority: P2)

Once a disbursement request is fully approved, the payment is executed: the payment method, reference number, and amount (equal to the payment order net total) are recorded. The payment automatically posts to the general ledger via the existing posting pipeline (domain event → AccountingEvent → JournalEntry + lines).

**Why this priority**: This completes the disbursement cycle — the actual movement of funds. It depends on US1 (request creation) and US2 (dual-signature approval) being in place.

**Independent Test**: Can be tested by approving a disbursement request (with dual signatures), executing the payment with method and reference, verifying the payment record is created, and verifying the ledger entry is posted with correct debits and credits.

**Acceptance Scenarios**:

1. **Given** a disbursement request is in Approved status, **When** the payment is executed, **Then** a Payment record is created with payment method, reference number, amount equal to the payment order net total, and execution timestamp.

2. **Given** a disbursement request is Approved and payment is executed, **When** the payment is recorded, **Then** a domain event is raised that triggers ledger posting via the AccountingEvent pipeline, creating a balanced journal entry with the correct debit and credit lines.

3. **Given** a disbursement request is Approved, **When** the payment is executed, **Then** the disbursement request status transitions to Disbursed and the linked payment order status transitions to Paid.

4. **Given** a disbursement request is not in Approved status, **When** payment execution is attempted, **Then** the system rejects execution with an error indicating the request must be approved first.

5. **Given** a payment has been executed for a disbursement request, **When** the payment record is viewed, **Then** it shows the payment method, reference, amount, execution date, and linked disbursement request and payment order references.

---

### User Story 4 - Disbursement Register Report (Priority: P3)

A disbursement register report lists disbursement requests and their associated payments, filterable by period, fund, and status. The report shows request number, payment order number, payee, amount, status, request date, approval date, and payment date.

**Why this priority**: This is a reporting requirement that does not block daily operations. It provides visibility into the disbursement pipeline for management oversight.

**Independent Test**: Can be tested by generating a report for a specific period and fund, verifying it includes all disbursement requests and payments for that period, with correct totals and filtering.

**Acceptance Scenarios**:

1. **Given** disbursement requests exist for multiple periods and funds, **When** the user generates a disbursement register report filtered by period and fund, **Then** the report shows only requests matching the selected period and fund.

2. **Given** disbursement requests exist in various statuses, **When** the user generates a report filtered by status, **Then** the report shows only requests matching the selected status.

3. **Given** a disbursement request has an associated payment, **When** the report is generated, **Then** the report row includes both the request details (number, date, status) and the payment details (method, reference, amount, date).

4. **Given** a disbursement request has no associated payment yet, **When** the report is generated, **Then** the report row shows the request details with payment columns blank and status reflecting the current state (Draft, Pending Approval, Approved, Rejected).

5. **Given** the report is generated, **When** totals are computed, **Then** the report shows the total requested amount and total paid amount for the filtered set.

---

### Edge Cases

- **Second disbursement attempt on the same payment order**: System MUST reject at creation time with an explicit error. The 1:1 constraint is enforced by uniqueness on the PaymentOrderId foreign key.

- **Request cancelled after approval**: A disbursement request that has been approved but not yet paid MAY be cancelled by any user holding the disbursement permission. Cancellation records the reason in ApprovalHistory, transitions the request to Cancelled status, and does NOT affect the payment order status (it remains Approved, eligible for a new disbursement request). The payment order's disbursement-request link is cleared.

- **Payment order amended after request creation**: If a payment order is amended (amount changed) after a disbursement request has been created, the disbursement request MUST be invalidated: its status transitions to Invalidated and a new disbursement request must be created reflecting the amended amount. The budget availability check runs again on the new request.

- **Budget availability check with Warning control method**: When the budget item control is Warning (not Blocking), the disbursement request is created with a warning flag but is NOT rejected. The availability breakdown is returned as informational.

- **Budget availability check with None control method**: When the budget item control is None, no availability check is performed and the request is created directly.

- **Payment order amount equals zero**: Disbursement request creation is rejected — there is nothing to disburse.

- **Concurrent disbursement requests for different payment orders**: Must not interfere with each other; each request is independent and checked against its own payment order and budget item. Budget availability is evaluated at point-in-time of creation (optimistic); the Blocking control catches any overrun if concurrent requests consume the same budget item's funds.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST enforce a 1:1 relationship between DisbursementRequest and PaymentOrder — exactly one disbursement request per payment order.

- **FR-002**: System MUST run a budget availability check at disbursement request creation. The check MUST evaluate the budget item's control method (None, Warning, Blocking) and available funds.

- **FR-003**: System MUST reject disbursement request creation when the budget item control is Blocking and available funds are less than the payment order net total. The rejection MUST include a full availability breakdown.

- **FR-004**: System MUST allow disbursement request creation when the budget item control is Warning and funds are insufficient, but MUST attach a warning flag to the request.

- **FR-005**: System MUST allow disbursement request creation when the budget item control is None without performing an availability check.

- **FR-006**: System MUST enforce dual-signature approval: at least two distinct approvers, with at least one holding the AccountsManager or AuthorizingOfficer role.

- **FR-007**: System MUST record every approval decision in the ApprovalHistory entity with document type, document ID, step number, approver identity, role, decision, timestamp, and evaluation snapshot.

- **FR-008**: System MUST reject approval attempts where the same user tries to approve twice on the same disbursement request.

- **FR-009**: System MUST create a Payment record upon payment execution with payment method, reference number, amount equal to the payment order net total, and execution timestamp.

- **FR-010**: System MUST trigger ledger posting via the domain event pipeline when a payment is executed. The journal entry MUST be balanced and follow the existing posting rules.

- **FR-011**: System MUST transition the disbursement request status to Disbursed and the payment order status to Paid upon successful payment execution.

- **FR-012**: System MUST generate a disbursement register report filterable by period, fund, and status, showing request and payment details with totals.

- **FR-013**: System MUST assign a disbursement request number at creation using the document sequence service.

- **FR-014**: System MUST invalidate a disbursement request (transition to Invalidated status) when the linked payment order is amended after request creation.

- **FR-015**: System MUST allow cancellation of an approved disbursement request that has not yet been paid, clearing the payment order's disbursement link.

- **FR-016**: System MUST maintain append-only status history for DisbursementRequest and Payment entities.

- **FR-017**: System MUST reject disbursement request creation when the payment order net total is zero.

- **FR-018**: System MUST require the disbursement request payment method to match or be compatible with the payment order's payment method.

### Key Entities

- **DisbursementRequest**: Represents a request to disburse funds against an approved payment order. Key attributes: request number (unique, sequential), status (Draft, Pending Approval, Approved, Rejected, Cancelled, Disbursed, Invalidated), linked PaymentOrderId (unique — 1:1), requested amount (equals payment order net total), budget availability snapshot at creation, dual-approval flag. Relationships: exactly one to one PaymentOrder; contains ApprovalHistory records; may have one Payment.

- **Payment**: Represents the executed payment. Key attributes: payment method, reference number, amount, execution timestamp, status (Completed, Failed). Relationships: belongs to a DisbursementRequest; linked to a PaymentOrder; triggers AccountingEvent for ledger posting.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A payment order moves from Approved to Paid in one session (create request → dual approval → execute payment) without leaving the disbursement workflow.

- **SC-002**: Every rejected disbursement request shows the full availability breakdown (net appropriated, encumbered, available, requested, shortfall).

- **SC-003**: Zero disbursements bypass the dual-signature requirement — 100% of approved disbursements have at least two distinct approvers with the required role composition.

- **SC-004**: Disbursement request creation completes in under 30 seconds, including the budget availability check.

- **SC-005**: Payment execution posts to the ledger within the same transaction — no unposted payments.

- **SC-006**: Disbursement register report is generated in under 5 seconds for a fiscal year of data.

- **SC-007**: All status transitions are recorded in append-only history with actor, timestamp, and reason (where applicable).

## Assumptions

- Payment orders in Approved status are eligible for disbursement requests. The SentToTreasury status is not used in this workflow — disbursement requests link directly from Approved.

- The payment order net total (AmountGross minus DeductionAmount) is the amount to disburse. No partial disbursements are allowed.

- Budget availability is computed at the BudgetItem level using the existing BudgetAvailabilityService. The disbursement request does not introduce new availability logic — it consumes the existing service.

- The document sequence service is available and will be configured with a DSB prefix for disbursement request numbers.

- The posting pipeline (domain event → AccountingEvent → JournalEntry) is functional for disbursement payments. The journal entry template for disbursements follows the existing payment posting rules.

- The dual-signature roles (AccountsManager, AuthorizingOfficer) are defined in the existing role system. These roles are pre-seeded or configured before this feature is used.

- Disbursement requests do not require tender-law evidence or monthly spending plan gates. This is an explicit deviation from any governance requirement that might otherwise apply — it is a user-stated requirement for this feature.

- The payment order's EncumbranceId link (if present) is not directly involved in the disbursement workflow. The budget availability check operates at the BudgetItem level.

- Frontend support (report UI, disbursement request pages) is out of scope for this spec — only backend entities, commands, queries, and endpoints are specified.

- The 1:1 constraint is enforced at the database level (unique index on DisbursementRequest.PaymentOrderId) and validated at the application level.

## Scope

### In Scope

- DisbursementRequest entity, status lifecycle, and document numbering
- Budget availability check at request creation (consuming existing BudgetAvailabilityService)
- Dual-signature approval workflow with role enforcement
- Payment entity and payment execution
- Ledger posting via domain event pipeline
- Disbursement register report (query/filter)
- Status history (append-only) for DisbursementRequest and Payment
- Edge case handling: duplicate request prevention, cancellation, invalidation on amendment

### Out of Scope

- Monthly spending plan enforcement (informational only, lives in budgeting)
- Tender-law evidence documentation
- Receipt vouchers (handled by spec 017)
- Frontend UI for disbursement requests or reports
- Bank integration for payment execution (payment method and reference are recorded manually)
- Multi-currency disbursement (assumed single base currency per payment order)
- Partial disbursements (payment order net total is always the disbursement amount)
