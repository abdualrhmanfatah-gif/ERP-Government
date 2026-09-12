# Feature Specification: Receipt Vouchers (TRE-01)

**Feature Branch**: `031-receipt-vouchers`

**Created**: 2026-09-07

**Status**: Draft

**Input**: User description: "SPEC: TRE-01 — سندات القبض (receipt-vouchers)"

## Binding Contract Note

The Data Contract section below is **literal and binding** for implementation — field names, DTO shapes, lifecycle, and permission codes MUST be implemented exactly as written here. Deviations require a spec amendment, not silent divergence.

## User Scenarios & Testing

### User Story 1 — Create Receipt Voucher (Priority: P1)

As a cashier, I want to create a receipt voucher with revenue lines so that incoming revenue is recognized immediately with an instant sequential number.

**Why this priority**: The first gateway to revenue recognition — the voucher must carry its sequential number the moment it exists.

**Independent Test**: Create a cash voucher with one line → number is displayed immediately + total computed server-side.

**Acceptance Scenarios**:

1. **Given** a party and revenue lines, **When** I create a voucher (Cash), **Then** it is saved as Draft with a DSL number displayed immediately.
2. **Given** I select the Check payment method, **When** the form updates, **Then** the checks section appears (bank / number / date / amount per check) and becomes required.
3. **Given** a voucher with no lines, **When** I attempt to submit it, **Then** the submission is rejected.

---

### User Story 2 — Review and Approval (Priority: P1)

As a reviewer, I want to review submitted vouchers and record my approval so that no revenue is recognized without documented review.

**Why this priority**: No recognized revenue without documented review — the lifecycle is the control.

**Independent Test**: Submit → PendingReview → approve records reviewedById/At.

**Acceptance Scenarios**:

1. **Given** a Draft voucher, **When** I submit it, **Then** its status becomes PendingReview.
2. **Given** a PendingReview voucher, **When** a reviewer approves it (reason optional), **Then** it becomes Approved with reviewedById/reviewedAt recorded.
3. **Given** a Draft or PendingReview voucher, **When** it is cancelled (reason required), **Then** it becomes Cancelled — permanently.

---

### User Story 3 — Browse (Priority: P2)

As a cashier or reviewer, I want to filter vouchers by party, period, status, and method so that I can find vouchers quickly.

**Why this priority**: Cashiers and reviewers need fast retrieval; secondary to creation and review but essential for daily operation.

**Independent Test**: Filters (party / period / status / method) work on the list.

**Acceptance Scenarios**:

1. **Given** vouchers spanning a period, **When** I filter by payment method Check, **Then** only check vouchers are shown.

---

### Edge Cases

- Σ(checks) > voucher total → server-side rejection at create and submit; Σ(checks) < total is allowed (partial check coverage of the total).
- Disabled party → party selector excludes it; selecting/submitting with a disabled party is rejected.
- Cancel after a deposit slip exists → server-side rejection.
- RowVersion conflict → user sees a conflict toast, no silent overwrite.
- Number gaps are visible in the list — sequence numbers are never reused or renumbered.

## Requirements

### Functional Requirements

- **FR-001**: System MUST issue the voucher number (DSL-{D6}) at Draft creation and display it immediately.
- **FR-002**: System MUST support multiple revenue lines and multiple checks (checks[]).
- **FR-003**: System MUST require check fields (bank, number, date, amount) when method = Check.
- **FR-004**: System MUST compute the total server-side (sum of lines).
- **FR-005**: System MUST prohibit edit/delete — POST-lifecycle actions only (submit / approve / cancel).
- **FR-006**: System MUST require a reason for cancellation.
- **FR-007**: System MUST display the deposit slip link (depositSlipId / depositSlipNumber) for deposited vouchers.
- **FR-008**: System MUST reject server-side (at create and submit) when the sum of check amounts exceeds the voucher total; Σ(checks) < total is allowed.

### Key Entities

- **ReceiptVoucher**: A receipt voucher (cash / checks) with revenue lines and a review lifecycle.
  - Status enum (ReceiptVoucherStatus): Draft, PendingReview, Approved, Cancelled.
  - PaymentMethod enum: Cash, Check.
  - Key attributes: voucherNumber (DSL-{D6}), voucherDate, partyId, receivedFrom?, notes?, depositSlipId/depositSlipNumber (link to TRE-02), submittedById/At, reviewedById/At, cancellationReason.
  - Lines: revenueAccountId (required), amount (required), description (optional).
  - Checks: bankName, checkNumber, checkDate, amount (all required).
- **Check**: A check attached to a voucher (managed in TRE-03 for clearing).

### Lifecycle

- Draft → PendingReview (submit) → Approved (approve).
- Draft → Cancelled, PendingReview → Cancelled (cancel, reason required).
- Cancelled is terminal. Approved is terminal for this feature (no un-approve in scope).

## Success Criteria

### Measurable Outcomes

- **SC-001**: Users can create a complete receipt voucher in under 2 minutes.
- **SC-002**: Every approved voucher displays its reviewer (name / time).
- **SC-003**: 100% of cancellations carry a documented reason.

## Data Contract (binding)

### ReceiptVoucherDto (literal)

`id, voucherNumber, voucherDate*, partyId*, partyName, paymentMethod* (enum: Cash/Check), paymentMethodName, receivedFrom?, notes?, depositSlipId?, depositSlipNumber?, status (enum ReceiptVoucherStatus: Draft/PendingReview/Approved/Cancelled), statusName, totalAmount, submittedById?, submittedAt?, reviewedById?, reviewedAt?, cancellationReason?, lines[], checks[], rowVersion, created, createdBy, lastModified, lastModifiedBy`

- Line: `revenueAccountId*, amount*, description?`
- CreateCheckDto: `bankName*, checkNumber*, checkDate*, amount*`

### Operations

- Create: `{voucherDate, partyId, paymentMethod, receivedFrom?, notes?, lines[], checks[]}`
- Submit: `{id, rowVersion}`
- Approve: `{id, reason?, rowVersion}`
- Cancel: `{id, reason*, rowVersion}`

### API Surface

- `GET /api/Revenue/ReceiptVouchers` (+ `/by-party/{id}`, `/by-period`)
- `GET /api/Revenue/ReceiptVouchers/{id}`
- `POST /api/Revenue/ReceiptVouchers` + `/{id}/submit | approve | cancel`

### Permissions

Revenue codes are currently absent — add per the contract: `ReceiptVouchers.View / Create / Submit / Approve / Cancel` (contract-gap path).

## Clarifications

### Session 2026-09-07

- Q: Can the user who created and submitted a receipt voucher also approve it themselves, or must approval come from a different user? → A: Same user may approve their own voucher — no submitter/approver separation requirement in this feature.
- Q: When the sum of check amounts exceeds the voucher total, should the system reject the voucher outright, or only enforce equality between check total and line total? → A: Reject if Σ(checks) > total; Σ(checks) < total is allowed (partial coverage) — enforced server-side at create and submit.
- Q: When should `receivedFrom` be used instead of relying on `partyName`? → A: Different-payer only — `receivedFrom` is optional free text for the actual payer when it differs from the registered party; `partyName` always comes from the party record.

## Out of Scope

- Deposit slips (TRE-02)
- Check clearing (TRE-03)
- Ledger posting (الترحيل)

## Assumptions

- Receipt vouchers are create-once documents: no edit/delete after creation (FR-005), consistent with the financial system-of-record principle.
- Optimistic concurrency: every mutating operation round-trips `rowVersion` (part of the data contract).
- Numbering follows the existing document sequence pattern (PREFIX-{D6}) allocated in the same transaction as Draft creation.
- The `Σ(checks) > total` rule is enforced server-side at create and submit (block); Σ(checks) < total is allowed. Resolved via clarification (OQ1).
- Party selector excludes disabled parties; server also rejects a disabled partyId.
- `receivedFrom` is optional free text used only when the actual payer differs from the registered party; `partyName` always reflects the party record. Resolved via clarification (OQ2).
- Approval does not require a different user: the submitter may approve their own voucher (no SoD restriction in this feature). Approval decision is still recorded via approval history per project convention.

## Open Questions

_None — OQ1 and OQ2 resolved during clarification session 2026-09-07 (see Clarifications)._
