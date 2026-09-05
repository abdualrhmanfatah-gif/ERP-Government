# Feature Specification: Receipt Voucher & Deposit Slip Workflow

**Feature Branch**: `017-receipt-voucher-deposit`

**Created**: 2026-09-05

**Status**: Draft

**Input**: User description: "Treasury office revenue collection. Replace the existing RevenueReceipt system with a unified ReceiptVoucher + DepositSlip workflow."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record Revenue Collection (Priority: P1)

A cashier records money collected from a party against its outstanding revenue items. The system creates a receipt voucher in Draft status with a voucher number generated at creation (pre-printed-book style; gaps are auditable). One line is created per revenue item. Both cash and checks are supported; check details (bank, number, date, amount) are captured on the voucher.

**Why this priority**: This is the foundation of the entire revenue collection workflow. Without the ability to record collections, no downstream processing (review, deposit, clearing) can occur.

**Independent Test**: Can be fully tested by creating a receipt voucher with multiple line items (mix of cash and check payments) and verifying voucher number assignment, line-item detail capture, and Draft status.

**Acceptance Scenarios**:

1. **Given** a cashier is logged in with collection permissions, **When** the cashier creates a new receipt voucher for a party with outstanding items, **Then** the system assigns a voucher number at creation (pre-printed-book style) and sets status to Draft.

2. **Given** a receipt voucher is in Draft status, **When** the cashier adds line items for each revenue item being paid, **Then** each line captures the revenue item reference, amount, and payment method (cash or check).

3. **Given** a cashier is recording a check payment, **When** the cashier enters check details, **Then** the system captures bank name, check number, check date, and check amount on the voucher line.

4. **Given** a receipt voucher has been created, **When** the cashier saves the voucher, **Then** the voucher number is permanent and the status history records the creation event with timestamp and actor.

5. **Given** a party has multiple outstanding revenue items, **When** the cashier creates a receipt voucher, **Then** partial payment against one or more items is supported (not all items must be paid in full).

---

### User Story 2 - Review and Approve Voucher (Priority: P1)

An accounts reviewer must examine every voucher before approval. Approval without a reviewer is rejected. Approved vouchers become eligible for deposit slips.

**Why this priority**: This is a critical internal control gate. Revenue cannot be recognized or deposited without independent review, ensuring financial integrity.

**Independent Test**: Can be fully tested by submitting a Draft voucher for review, having a reviewer approve it, and verifying the voucher transitions to Approved status and becomes eligible for deposit slip inclusion.

**Acceptance Scenarios**:

1. **Given** a receipt voucher is in Draft status, **When** the cashier submits the voucher for review, **Then** the voucher status changes to "Pending Review" and the reviewer is notified.

2. **Given** a voucher is Pending Review, **When** an accounts reviewer examines the voucher and approves it, **Then** the status changes to Approved, the reviewer identity is recorded, and the approval timestamp is captured.

3. **Given** a voucher is Pending Review, **When** someone attempts to approve the voucher without being a designated reviewer, **Then** the system rejects the approval and returns an error.

4. **Given** a voucher is in Draft status, **When** someone attempts to approve it directly (bypassing review), **Then** the system rejects the approval attempt.

5. **Given** a voucher is Approved, **When** a cashier creates a deposit slip, **Then** the voucher appears as eligible for inclusion in the deposit slip.

---

### User Story 3 - Create Deposit Slip (Priority: P2)

A cashier batches approved vouchers into a deposit slip: Form 47 (cash-only) or Form 48 (checks-only). Members must match the slip type — mixed slips are rejected. Treasury-manager approval of a Form 47 slip posts the revenue (cash-basis recognition at collection). A Form 48 slip moves its checks to under-collection.

**Why this priority**: Deposit slips are the mechanism for batching and submitting revenue to the bank. This enables the two-stage revenue recognition model (immediate for cash, deferred for checks).

**Independent Test**: Can be tested by creating a Form 47 slip with cash-only vouchers and a Form 48 slip with check-only vouchers, verifying type enforcement and status transitions.

**Acceptance Scenarios**:

1. **Given** a cashier has multiple Approved vouchers, **When** the cashier creates a new deposit slip, **Then** the cashier must select either Form 47 (cash) or Form 48 (checks) as the slip type.

2. **Given** a Form 47 deposit slip is being created, **When** the cashier attempts to add a voucher with check payments, **Then** the system rejects the addition and displays a type mismatch error.

3. **Given** a Form 48 deposit slip is being created, **When** the cashier attempts to add a cash-only voucher, **Then** the system rejects the addition and displays a type mismatch error.

4. **Given** a deposit slip contains only vouchers matching its form type, **When** the cashier saves the slip, **Then** the slip date is validated: it must be on or after the latest member voucher date and not in the future.

5. **Given** a Form 47 deposit slip is approved by a Treasury manager, **When** approval is recorded, **Then** revenue is recognized (cash-basis) and the member vouchers are marked as deposited.

6. **Given** a Form 48 deposit slip is approved by a Treasury manager, **When** approval is recorded, **Then** the checks move to "under-collection" status and revenue recognition is deferred.

7. **Given** a deposit slip is created, **When** the slip is saved, **Then** the status history records the creation event with timestamp, actor, and slip type.

---

### User Story 4 - Check Clearing and Bounced Check Handling (Priority: P2)

When the bank confirms an under-collection check, it is cleared (revenue recognized). A bounced check reopens the member voucher for a replacement check or cash.

**Why this priority**: This completes the two-stage check collection cycle and handles the exception case of bounced checks, ensuring revenue is only recognized when cash is actually received.

**Independent Test**: Can be tested by confirming a check (moving from under-collection to cleared, verifying revenue recognition) and by processing a bounced check (reopening voucher, allowing replacement payment).

**Acceptance Scenarios**:

1. **Given** a check is in under-collection status, **When** the bank confirms the check has cleared, **Then** the system recognizes revenue for that check amount and updates the check status to Cleared.

2. **Given** a check is in under-collection status, **When** the bank reports the check has bounced, **Then** the system reopens the member voucher and allows the party to provide a replacement check or cash payment.

3. **Given** a bounced check has been replaced with a new check or cash payment, **When** the replacement is recorded, **Then** the original bounced check is marked as Bounced and the new payment follows the standard collection workflow.

4. **Given** a check is cleared, **When** the clearing event is recorded, **Then** the status history captures the clearing date, bank reference, and actor.

---

### User Story 5 - Monthly Collections Statement (Priority: P3)

A monthly collections statement is generated per the financial law, for any month and fund.

**Why this priority**: This is a regulatory reporting requirement but does not block daily operations. It can be implemented after the core collection workflow is stable.

**Independent Test**: Can be tested by generating a statement for a specific month and fund, verifying it includes all collections, deposits, and clearing events for that period.

**Acceptance Scenarios**:

1. **Given** collections have been recorded for a specific month and fund, **When** the user requests a monthly collections statement, **Then** the system generates a statement covering all collections, deposits, and check clearing events for that month and fund.

2. **Given** a user requests a statement for a month with no collections, **When** the statement is generated, **Then** the system produces an empty statement with appropriate headers and zero totals.

3. **Given** a statement is generated, **When** the user views the statement, **Then** it includes voucher numbers, dates, parties, amounts, payment methods, and deposit slip references.

---

### Edge Cases

- **Partial payment against one item**: When a party pays only a portion of an outstanding revenue item, the system creates a voucher line for the partial amount and tracks the remaining balance on the original item.

- **Bounced check replaced**: When a bounced check is replaced, the original check is marked Bounced, a new payment (check or cash) is recorded against the reopened voucher, and the new payment follows the standard workflow.

- **Slip member voucher cancelled after slip approval**: If a voucher that was included in an approved deposit slip is subsequently cancelled, the system must record the cancellation with a reason and adjust the financial records accordingly (reversal entry).

- **Voucher-number gaps audit**: The system must support auditing of voucher number gaps. Since numbers are assigned at Draft creation (pre-printed-book style), gaps may occur if vouchers are abandoned. An audit report must list all assigned numbers, their statuses, and any gaps.

- **Check details validation**: Check number, bank, and date are required fields when a voucher line payment method is "check". Incomplete check details must block voucher submission.

- **Deposit slip date validation**: Slip date must be on or after the latest member voucher date and not in the future. This is basic date sanity only — no daily deposit deadline enforcement.

- **Form type homogeneity**: A deposit slip must contain only vouchers matching its form type (Form 47 = cash-only, Form 48 = checks-only). Mixed types must be rejected at the system level.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST generate a unique voucher number at Draft creation using pre-printed-book style sequential numbering.

- **FR-002**: System MUST support voucher number gap auditing — listing all assigned numbers, their statuses, and identifying any gaps.

- **FR-003**: System MUST enforce reviewer gate: approval of a voucher requires an accounts reviewer; approval without a reviewer is rejected.

- **FR-004**: System MUST capture check details (bank name, check number, check date, check amount) when payment method is "check".

- **FR-005**: System MUST enforce Form 47/48 homogeneity: Form 47 slip contains only cash vouchers; Form 48 slip contains only check vouchers.

- **FR-006**: System MUST validate deposit slip date: on or after the latest member voucher date and not in the future.

- **FR-007**: System MUST recognize revenue for Form 47 slips upon Treasury-manager approval (cash-basis recognition at collection).

- **FR-008**: System MUST move Form 48 slip checks to under-collection status upon Treasury-manager approval.

- **FR-009**: System MUST recognize revenue when an under-collection check is confirmed cleared by the bank.

- **FR-010**: System MUST reopen the member voucher when a check bounces, allowing replacement payment (check or cash).

- **FR-011**: System MUST maintain append-only status history for all entities (ReceiptVoucher, DepositSlip, Check).

- **FR-012**: System MUST NOT enforce daily deposit deadlines or 1-day check windows.

- **FR-013**: System MUST support partial payment against individual revenue items.

- **FR-014**: System MUST generate monthly collections statements per financial law for any specified month and fund.

- **FR-015**: System MUST record all status transitions with actor identity, timestamp, and reason (where applicable).

- **FR-016**: System MUST reject voucher approval if the voucher is still in Draft status (must go through review first).

- **FR-017**: System MUST record deposit slip creation with slip type (Form 47 or Form 48), member vouchers, and date.

- **FR-018**: System MUST track check status through lifecycle: Pending → Under-Collection → Cleared or Bounced.

### Key Entities

- **ReceiptVoucher**: Represents a collection record from a party. Key attributes: voucher number (unique, sequential), status (Draft, Pending Review, Approved, Cancelled), collection date, party reference, fund, total amount. Relationships: contains one or more ReceiptVoucherLine items; may be included in a DepositSlip.

- **ReceiptVoucherLine**: Individual line item on a receipt voucher. Key attributes: revenue item reference, amount, payment method (cash/check), check details (if applicable). Relationships: belongs to a ReceiptVoucher; references an outstanding revenue item.

- **Check**: Represents a check payment. Key attributes: check number, bank name, check date, amount, status (Pending, Under-Collection, Cleared, Bounced). Relationships: associated with a ReceiptVoucherLine; may be part of a DepositSlip (Form 48).

- **DepositSlip**: Batches approved vouchers for bank deposit. Key attributes: slip number, slip type (Form 47/48), status (Draft, Approved, Posted), deposit date, total amount. Relationships: contains one or more ReceiptVouchers; approved by Treasury manager.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Cashier completes a collection record (create voucher, add lines, save) in under 2 minutes.

- **SC-002**: Monthly collections statement is generated automatically without manual tallying.

- **SC-003**: Zero revenue is recognized before its trigger event (Form 47 on slip approval, Form 48 on check clearing).

- **SC-004**: 100% of vouchers go through reviewer gate before approval (no bypass possible).

- **SC-005**: 100% of deposit slips enforce form type homogeneity (no mixed cash/check slips).

- **SC-006**: Voucher number gaps are auditable and reportable on demand.

- **SC-007**: All status transitions are recorded in append-only history with actor, timestamp, and reason.

---

## Assumptions

- The existing RevenueReceipt system will be replaced; no backward compatibility with the old system is required.

- Treasury managers have approval authority for deposit slips; accounts reviewers have review authority for vouchers.

- Bank confirmation of check clearing/bouncing is received via an external interface (bank integration is out of scope for this feature).

- Revenue items are pre-existing in the system (created by other modules); this feature references them but does not create them.

- Fund information is associated with revenue items and carried forward to vouchers and statements.

- The system supports Arabic-first RTL UI per the project constitution.

- Partial payments are tracked against the original revenue item's outstanding balance.

- Voucher numbers are system-generated sequential integers, not user-assigned.

- Deposit slip types (Form 47/48) are mutually exclusive — a slip cannot contain both cash and check vouchers.

- Monthly statements are generated on-demand, not automatically scheduled.

---

## Scope

### In Scope

- ReceiptVoucher creation, review, and approval workflow
- ReceiptVoucherLine item management (cash and check payments)
- Check detail capture and status tracking
- DepositSlip creation with Form 47/48 type enforcement
- Revenue recognition triggers (Form 47 on approval, Form 48 on check clearing)
- Bounced check handling and voucher reopening
- Monthly collections statement generation
- Voucher number gap auditing
- Append-only status history for all entities

### Out of Scope

- Bank account balances (delegated to Banking module)
- Payment/disbursement processing
- GL report building
- Bank integration interface (check confirmation/bounce notifications)
- Mobile application support
- Multi-currency support (assumed single base currency)
