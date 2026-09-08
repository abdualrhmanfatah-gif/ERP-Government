# Feature Specification: Payments Group (PAY-01..04)

**Feature Branch**: `045-payments-group`

**Created**: 2026-09-08

**Status**: Draft

**Input**: User description: "المجموعة: المدفوعات (PAY) — PAY-01 أوامر الدفع (payment-orders)، PAY-02 طلبات الصرف (disbursement-requests)، PAY-03 تنفيذ الدفع (payment-execution)، PAY-04 الحسابات البنكية (bank-accounts)."

> **Scope note**: The user input describes a payments group (PAY) with four sub-specs. Per Spec Kit rules, ONE feature per invocation — this spec covers the **entire PAY group** as a single feature with sub-modules. Each sub-module (PAY-01..04) is documented as a distinct section within this spec.
>
> **CONTRACT NOTE**: Field names, route paths, permission codes, and enum values in this spec are **binding requirements** from the documented backend contract — they are not implementation details to be renegotiated during checklist validation. Per Constitution IX the backend-generated OpenAPI document remains the single source of truth for the HTTP contract.

## Clarifications

### Session 2026-09-08

- Q: Which payment orders may be voided — approved-but-unpaid ones (as the spec contract states) or paid ones (as the current backend code enforces)? → A: Void unpaid Approved/SentToTreasury orders; partially paid = refused (spec contract). Code-governance note: current `VoidPaymentOrderCommand` (Paid-only guard + TODO reversing entry) must be brought in line with this decision during implementation.
- Q: Does the dual-signature second approval require a qualified role, or only a different user (as current backend code enforces)? → A: Both steps require a qualified role (AccountsManager/AuthorizingOfficer) + distinct users. Code-governance note: current `ApproveDisbursementRequestCommand` enforces the role check on step 1 only; step 2 must add the role check during implementation.
- Q: Who is permitted to override a Failed budget check on a payment order? → A: A dedicated permission code (`PaymentOrders.OverrideBudgetCheck`), independent of roles — role binding follows later in the RBAC enforcement wiring.
- Q: What sets a disbursement request to the `Invalidated` state? → A: Automatic — the server invalidates a Draft/PendingApproval request when its linked order is cancelled/voided.
- Q: When a bank account is set as default while another account already holds the default flag, should the server switch the default automatically or refuse? → A: Auto-switch — setting a new default clears the previous one in the same transaction (exactly one default at all times).
- Code-verified (GAP-READ resolved, no user decision needed): `SendToTreasuryCommand` = `{Id, TreasuryReference (required), RowVersion}` · order reject/cancel and request reject/cancel all require a reason · `RecordPaymentCommand.referenceNumber` is optional · payment `amount` is a server-computed snapshot (netTotal).

## Shared Group Context

**Dependency chain**: PAY-01 (order) → PAY-02 (request) → PAY-03 (payment). PAY-04 (bank accounts) feeds PAY-01's `bankAccountId` picker and is independent of the chain ordering.

**Consumers**: PAY-02, PAY-03, RPT-03, CTRL-03 read payment-order state; ACC-05 receives the posting event raised by successful payments.

**Shared invariants (inherited by all sub-modules, not repeated below)**:

- **CC-1 (Server numbers)**: Every financial amount, status, and computed total displayed or exported comes from the server; the frontend never recomputes financial values (Constitution III/IV).
- **CC-2 (State UI)**: Every status renders as a color-coded badge; every blocking server refusal renders its server message verbatim plus actionable detail — never silently swallowed.
- **CC-3 (Approvals via 022 panels)**: Every approval/reject decision is recorded through the 022 ApprovalHistory panels — no inline approval columns (Constitution VIII, fix 016).
- **CC-4 (Concurrency)**: All mutating endpoints accept and honor `rowVersion`; conflicts surface as a server message.
- **CC-5 (Lifecycle actions are conditional)**: Action buttons appear only in the states their lifecycle table lists; server re-validates independently.

## Sub-Module: PAY-01 — Payment Orders (payment-orders)

### User Scenarios

#### US1 (P1) — Create a complete order

An accountant issues the official disbursement order with its vendor, lines, deductions, and beneficiary.

**Acceptance Scenarios**:

1. **Given** a vendor, an appropriation, and line items, **When** the accountant creates an order (header + lines[] + deductions[] + beneficiary), **Then** it is created as Draft with a server-issued `paymentOrderNumber` (`PO-{D6}`) and server-computed `amountGross`/`deductionAmount`.
2. **Given** a deduction with `isMandatory=true`, **When** the accountant tries to delete it, **Then** the deletion is blocked.
3. **Given** an order with no lines, **When** the accountant tries to submit it, **Then** the submission is refused.
4. **Given** an order without `appropriationId`, **When** it is created, **Then** the creation is refused (field is required).

#### US2 (P1) — Budget check

No approval may bypass the budget check without a conscious, documented decision.

**Acceptance Scenarios**:

1. **Given** an order, **When** the budget check runs, **Then** `budgetCheckStatus` is one of Pending/Passed/Failed/Overridden, rendered as a color-coded badge.
2. **Given** `budgetCheckStatus=Failed`, **When** approval is attempted, **Then** the approval is blocked with a message showing the check status and its detail.
3. **Given** `budgetCheckStatus=Overridden`, **When** the order is approved, **Then** the override is documented and gated by the `PaymentOrders.OverrideBudgetCheck` permission (clarified 2026-09-08).

#### US3 (P1) — Lifecycle and treasury

The official path: submit → approve → treasury → pay.

**Acceptance Scenarios**:

1. **Given** a Draft, **When** submitted, **Then** status becomes Submitted; approval (with optional `reason`) moves it to Approved.
2. **Given** an Approved order, **When** sent to treasury, **Then** status becomes SentToTreasury with `treasuryStatus`/`treasuryReference`/`treasurySentAt` recorded.
3. **Given** a Rejected order, **When** the user opens the details, **Then** the rejection reason is visible in the 022 panels.

#### US4 (P1) — Totals and partial payment

Large orders are paid in installments.

**Acceptance Scenarios**:

1. **Given** an order, **When** the user opens `/{id}/totals`, **Then** the six values (`amountGross`, `totalDeductions`, `netAmount`, `paidAmount`, `remainingAmount`, `isFullyPaid`) are server-issued.
2. **Given** partial payments (PAY-03), **When** the order updates, **Then** status becomes PartiallyPaid and `remainingAmount` reflects the new state.

#### US5 (P2) — Voiding

Correcting an approved, unpaid order.

**Acceptance Scenarios**:

1. **Given** an Approved, unpaid order, **When** the user voids it (with confirmation), **Then** it becomes Voided — final state.
2. **Given** a partially paid order, **When** voiding is attempted, **Then** it is refused (server rule — clarified 2026-09-08).

### Edge Cases

- Editing a non-Draft order — blocked in UI and refused by the server.
- Cancelling/voiding an order that has an existing disbursement request (PAY-02 reads the order) — server rule decides.
- Deduction total exceeding gross amount — server refuses.
- Non-base currency — `exchangeRate` relevance (OQ-N3).
- RowVersion conflict on concurrent edit.
- Order without `appropriationId` — refused (field required ✓).

### Functional Requirements

- **FR-001**: The system MUST provide order CRUD: a list with filters (status / budgetCheckStatus / period / vendor) + create + edit (Draft only).
- **FR-002**: Order lines MUST support 5 line types (Invoice/Advance/Deduction/Adjustment/Other) — a typed grid row per line.
- **FR-003**: Deductions MUST support 7 types (Tax/WithholdingTax/Insurance/Penalty/AdvanceRecovery/LegalDeduction/Other) with `isMandatory`/`isTaxDeduction` flags + `taxAuthorityId`.
- **FR-004**: `budgetCheckStatus` MUST carry its four states — Failed MUST block approval.
- **FR-005**: The full lifecycle MUST be driven by conditional actions: submit/approve/reject/cancel/send-to-treasury/void.
- **FR-006**: Totals MUST come exclusively from `/{id}/totals` — six numbers + `isFullyPaid`; no client-side computation (CC-1).
- **FR-007**: The treasury trace (`treasuryStatus`/`treasuryReference`/`treasurySentAt`) and `journalEntryId` (journal entry link) MUST be displayed.
- **FR-008**: Every approval decision MUST be captured through the 022 panels — no inline columns (CC-3).

### Data Contract

**PaymentOrderDto** ✓ (L34412):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| paymentOrderNumber | str | ✓ | PO-{D6} |
| paymentOrderDate | date | ✓ | — |
| dueDate | date | ○ | — |
| paymentOrderType | str | ✓ | server values (OQ) |
| vendorId | int | ✓ | 022 picker (Vendor type — migration 016) |
| fundId / fiscalYearId / appropriationId | int | ✓ | budget |
| budgetClassificationId / costCenterId / projectId | int | ○ | dimensions |
| purchaseOrderId / encumbranceId | int | ○ | procurement/encumbrance |
| currencyId | int | ✓ | 024 |
| exchangeRate | dec | ○ | non-base currency |
| amountGross / deductionAmount | dec | ✓ | server-computed |
| paymentMethod | enum? | ○ | Cash/Check |
| paymentMethodName | str | — | display |
| bankAccountId | int | ○ | PAY-04 (active only) |
| beneficiaryName | str | ✓ | — |
| beneficiaryIban / beneficiaryAccountNumber / beneficiaryBankName | str | ○ | — |
| status | enum PaymentOrderStatus | ✓ | **Draft/Submitted/Approved/SentToTreasury/Paid/PartiallyPaid/Cancelled/Rejected/Voided** (L34712) |
| budgetCheckStatus | enum | ✓ | **Pending/Passed/Failed/Overridden** (L23452) |
| treasuryStatus / treasuryReference / treasurySentAt | str? / str? / dt? | ○ | treasury trace |
| paidAt | dt? | ○ | — |
| journalEntryId | int? | ○ | journal link |
| notes | str | ○ | — |
| lines[] / deductions[] | array | ✓ | — |

**PaymentOrderLineDto** ✓ (L34612): lineNumber, lineType (enum 5), description?, accountId*, amount*, taxAmount?, fundId?, appropriationId?, organizationUnitId?, costCenterId?, projectId?

**PaymentOrderDeductionDto** ✓ (L34320): lineNumber, deductionType (enum 7), deductionCode?, description?, accountId*, amount*, deductionPercent?, isMandatory, isTaxDeduction, taxAuthorityId?, referenceNumber?

**PaymentOrderTotalsDto** ✓ (L34724): id, amountGross, totalDeductions, netAmount, paidAmount, remainingAmount, isFullyPaid, status

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| Draft | submit | Submitted | ≥1 line |
| Draft | cancel | Cancelled | — |
| Submitted | approve | Approved | budgetCheck ≠ Failed |
| Submitted | reject | Rejected | reason required (code-verified) |
| Approved | send-to-treasury | SentToTreasury | treasury* recorded |
| SentToTreasury | (PAY-03 payment) | Paid / PartiallyPaid | server |
| Approved/SentToTreasury | void | Voided | unpaid only (partially paid refused) |

### Permissions

| Code | Status |
|---|---|
| PaymentOrders.View/Create/Update/Submit/Approve/Reject/Cancel/SendToTreasury/Void | GAP-ADD — (Payments.Create/View exist for PAY-03; the order family is verified/added) |
| PaymentOrders.OverrideBudgetCheck | GAP-ADD — gates the Failed-check override (clarified 2026-09-08) |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/PaymentOrders |
| GET | /{id} · /{id}/totals |
| POST | /{id}/submit · /approve · /reject · /cancel · /send-to-treasury · /void |

### Business Rules

- **BR-1**: Net = gross − deductions (server — Totals).
- **BR-2**: Draft-only editing.
- **BR-3**: Failed blocks approval; Overridden documents its bypass.
- **BR-4**: A mandatory deduction cannot be deleted.
- **BR-5**: Void is final (unpaid only; partially paid refused) · every decision lands in ApprovalHistory.

### UI/UX

- `/payments/payment-orders`: table (number/vendor/date/net/status/check badge) + filters.
- `/create`: sections — header (vendor/fund/fiscal year/appropriation/classification/dimensions/currency+rate/due date) · lines (typed grid) · deductions (typed grid + mandatory/tax + tax authority) · beneficiary (name/Iban/account/bank) · totals preview.
- `/{id}`: header + lines + deductions + Totals card (6 numbers) + budgetCheckStatus badge + 022 panels + treasury trace + journal link · send-to-treasury/void dialogs (confirmation).
- States: CC-2 + the four budget-check badges with colors.

### Success Criteria

- **SC-001**: A complete order (5+ lines, 3+ deductions) can be created in under 5 minutes.
- **SC-002**: Zero approvals for Failed orders without a documented override.
- **SC-003**: Totals match the server 100% (no client-side computation).

### Tests

- **T1** (FR-001..003/US1): full creation with line types and deduction types + mandatory.
- **T2** (FR-004/US2): the four check badges + Failed blocking.
- **T3** (FR-005/US3): the 9-state lifecycle with conditional actions.
- **T4** (FR-006/US4): the six Totals + PartiallyPaid.
- **T5** (FR-007/US5): treasury trace + void.
- **T6** (FR-008): 022 panels contain every decision.

### Open Questions

- **OQ-N1** (Engineering): ~~Void rule for partially paid~~ — RESOLVED 2026-09-08: unpaid only, partial refused. Remaining: void of an order with an existing disbursement request (server rule) — verify against PAY-02 cancel behavior.
- **OQ-N2** (User/Engineering): ~~Override permission for a Failed check~~ — RESOLVED 2026-09-08: dedicated `PaymentOrders.OverrideBudgetCheck` permission (GAP-ADD).
- **OQ-N3** (Engineering): exchangeRate — when is it required (non-base currency?).

---

## Sub-Module: PAY-02 — Disbursement Requests (disbursement-requests)

### User Scenarios

#### US1 (P1) — Create a request on an approved order

Separates the decision (the order) from the execution (the request).

**Acceptance Scenarios**:

1. **Given** an Approved order without a request, **When** the user creates a request (paymentOrderId + notes?), **Then** it is created as Draft with `requestNumber` (`DSB-{D6}`) and `requestedAmount` snapshotted.
2. **Given** an order that already has a request, **When** a second request is attempted, **Then** the server refuses with a verbatim 1:1 message.
3. **Given** `hasWarning=true`, **When** the request is displayed, **Then** the availability warning badge shows.

#### US2 (P1) — Dual signature

A regulatory legal requirement.

**Acceptance Scenarios**:

1. **Given** a PendingApproval request, **When** the first approver approves (reason?), **Then** approvals records `{step, approverName, role, decision, decisionAt}`.
2. **Given** the same user, **When** the second signature is attempted, **Then** the server refuses.
3. **Given** the second role is not qualified (not AccountsManager/AuthorizingOfficer), **When** approval is attempted, **Then** the server refuses.
4. **Given** two valid signatures, **When** complete, **Then** the request becomes Approved + `approvalDate`.

#### US3 (P1) — Rejection and cancellation

Exit paths that release resources.

**Acceptance Scenarios**:

1. **Given** a PendingApproval request, **When** rejected (reason), **Then** it becomes Rejected with the reason visible.
2. **Given** an Approved, unpaid request, **When** cancelled, **Then** it becomes Cancelled and the order returns to PAY-02 pickers (a new request becomes possible).
3. **Given** a Disbursed request, **When** cancellation is attempted, **Then** it is refused.

#### US4 (P1) — Availability gate

The legal barrier before disbursement.

**Acceptance Scenarios**:

1. **Given** a blocking availability result (Blocking), **When** the request is submitted, **Then** the server refuses with a verbatim detailed message.
2. **Given** a warning availability result, **When** the request is created/submitted, **Then** `hasWarning=true` passes through with the badge shown.

### Edge Cases

- Availability changed between load and submit (refetch + fresh message).
- Order cancelled/voided after loading (server refusal).
- Two different users sharing one qualified role — allowed; each signer must individually hold the role (clarified 2026-09-08).
- RowVersion conflict.
- Invalidated (7th state — set automatically when the linked order is cancelled/voided; clarified 2026-09-08).

### Functional Requirements

- **FR-001**: Strict 1:1 uniqueness with the order (second request refused with the server message verbatim).
- **FR-002**: `hasWarning` as a warning badge (contract is boolean — no tri-state).
- **FR-003**: Submission runs through the availability check — a blocking refusal displays its detail verbatim.
- **FR-004**: Signatures are rendered via `approvals[]` — a Stepper component with two steps (step/actor/role/decision/time).
- **FR-005**: PATCH lifecycle: submit/approve/reject/cancel, conditional on status.
- **FR-006**: Cancelling an Approved unpaid request releases the order (the picker offers it again).
- **FR-007**: The qualified second-signature role (AccountsManager/AuthorizingOfficer) — server-verified, refusal displayed.
- **FR-008**: When the linked order is cancelled/voided, the server MUST automatically set any Draft/PendingApproval request on it to Invalidated.

### Data Contract

**DisbursementRequestDto** ✓ (L30153):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | ✓ | — |
| requestNumber | str | ✓ | DSB-{D6} |
| paymentOrderId / paymentOrderNumber | int / str | ✓ | unique 1:1 |
| requestedById / requestedByName | int / str | ✓ | — |
| requestDate | date | ✓ | — |
| status | enum DisbursementRequestStatus | ✓ | **Draft/PendingApproval/Approved/Rejected/Cancelled/Disbursed/Invalidated** (L30257) |
| hasWarning | bool | ✓ | availability warning |
| notes | str | ○ | — |
| requestedAmount | dec | ✓ | snapshot from the order |
| payeeName / fundName | str | ○ | display |
| approvalDate / paymentDate | dt? | ○ | — |

**DisbursementRequestDetailDto** ✓ (L30136): + `approvals: ApprovalStepDto[] {step, approverUserId, approverName, role, decision, decisionAt}`

**CreateDisbursementRequestRequest** ✓ (L27277): `{paymentOrderId!, notes!}` · **ApproveDisbursementRequestRequest** ✓ (L21476): `{reason!}`

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| Draft | submit | PendingApproval | availability (Blocking ⇒ refusal) |
| PendingApproval | approve (1st) | PendingApproval | step 1 recorded |
| PendingApproval | approve (2nd) | Approved | different user + qualified role (both steps role-checked) |
| PendingApproval | reject | Rejected | reason |
| Draft/PendingApproval | cancel | Cancelled | releases the order |
| Approved | cancel | Cancelled | unpaid only · releases the order |
| Approved | (PAY-03 payment) | Disbursed | server |
| — | — | Invalidated | automatic: linked order cancelled/voided |

### Permissions

| Code | Status |
|---|---|
| DisbursementRequests.View/Create/Submit/Approve/Reject/Cancel | GAP-ADD (not present in the PermissionCodes scan) |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/DisbursementRequests |
| GET | /{id} |
| PATCH | /{id}/submit · /approve · /reject · /cancel |

### Business Rules

- **BR-1**: Strict 1:1.
- **BR-2**: Availability is the only gate (Blocking/Warning) — detail verbatim.
- **BR-3**: Dual signature: ≥2 distinct users, BOTH steps require a qualified role (AccountsManager/AuthorizingOfficer), same user forbidden.
- **BR-4**: Cancellation releases the order for a new request.

### UI/UX

- `/payments/disbursement-requests`: table (number/order/amount/status/warning badge) + filters (status/order).
- `/create`: picker of Approved orders without a request + notes.
- `/{id}`: header + **dual-signature Stepper** (from approvals) + hasWarning badge + linked order detail · conditional PATCH buttons · approve/reject/cancel dialogs (reason).
- States: CC-2 + the availability refusal state (verbatim detail panel).

### Success Criteria

- **SC-001**: Zero second requests on the same order.
- **SC-002**: 100% of approvals carry two distinct signatures with matching roles.
- **SC-003**: Every cancellation releases its order immediately (visible in the create picker).

### Tests

- **T1** (FR-001/US1): second-request refusal + the message.
- **T2** (FR-004/US2): Stepper with two signatures + same-user/role refusals.
- **T3** (FR-003/US4): Blocking refuses + verbatim detail · Warning with badge.
- **T4** (FR-006/US3): cancellation releases the order.
- **T5** (FR-005): the seven states with conditional actions.

### Open Questions

- **OQ-N1** (Engineering): ~~Fields of Reject/Cancel commands~~ — RESOLVED by code read: both require a reason (validator-enforced).
- **OQ-N2** (Engineering): ~~When is Invalidated set?~~ — RESOLVED 2026-09-08: automatic on linked-order cancel/void.

---

## Sub-Module: PAY-03 — Payment Execution (payment-execution)

> **Known legal gap**: Transfer/InKind are legal under mandate m26 but the contract enum is Cash/Check — record as a gap, do not invent.

### User Scenarios

#### US1 (P1) — Record a payment

The cashier executes an approved request — closing the triad.

**Acceptance Scenarios**:

1. **Given** an Approved request, **When** the cashier records a payment (paymentMethod + referenceNumber? + notes?; amount is a readonly snapshot), **Then** it becomes Completed with `paymentNumber` (`PAY-{D6}`).
2. **Given** success, **When** the request is opened, **Then** it is Disbursed + `paymentDate`; the order is Paid (or PartiallyPaid).
3. **Given** a non-approved request, **When** recording is attempted, **Then** the server refuses verbatim.

#### US2 (P1) — Visible failure

No silent failures.

**Acceptance Scenarios**:

1. **Given** a Failed payment, **When** displayed, **Then** the server message shows verbatim + a retry path.

#### US3 (P2) — Payments list

Reviewing actual cash flows.

**Acceptance Scenarios**:

1. **Given** payments in a period, **When** filtered (period/method/status), **Then** the list follows.

### Edge Cases

- Concurrent double payment (server constraint — its message displays).
- Check without referenceNumber — allowed (referenceNumber optional, code-verified).
- Request cancelled after the dialog loaded.
- Posting failure after a successful record (state reflects it — ACC-05 surfaces the event).
- amount ≠ requestedAmount (impossible — snapshot).

### Functional Requirements

- **FR-001**: Recording exclusively from an Approved request (picker/context).
- **FR-002**: Methods: **Cash/Check only** (enum L34315) — the legal gap is recorded, not invented.
- **FR-003**: `amount` is a snapshot of the request — readonly in the dialog.
- **FR-004**: Success closes the triad (request Disbursed + order Paid/PartiallyPaid) — shown on the success screen.
- **FR-005**: status: Completed/Failed — failure with its verbatim reason + retry.
- **FR-006**: No PUT/DELETE — single-time recording (immutable).
- **FR-007**: The posting event is raised automatically — its result link (ACC-05) shown on failure.

### Data Contract

**PaymentDto** ✓ (L34295):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | ✓ | — |
| paymentNumber | str | ✓ | PAY-{D6} |
| disbursementRequestId / disbursementRequestNumber | int / str | ✓ | — |
| paymentOrderId / paymentOrderNumber | int / str | ✓ | — |
| paymentMethod | enum | ✓ | Cash/Check |
| amount | dec | ✓ | request snapshot |
| paidById / paidByName / paidAt | int / str / dt | ✓ | server-issued |
| referenceNumber | str | ○ | check/reference number (optional, code-verified) |
| notes | str | ○ | — |
| status | enum PaymentStatus | ✓ | **Completed/Failed** (L34800) |
| payeeName | str | ○ | display |

**RecordPaymentRequest** ✓ (L35672): `{disbursementRequestId!, paymentMethod!, referenceNumber?, notes?}` — **no amount field** (snapshot is server-side).

### Lifecycle

Single-shot recording: → Completed / Failed. No subsequent transitions (immutable). Effects on the request/order are server-side.

### Permissions

| Code | Status |
|---|---|
| Payments.View | ✓ confirmed |
| Payments.Create | ✓ confirmed |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/Payments |
| GET | /api/Payments/Payments/{id} |

### Business Rules

- **BR-1**: Approved is the recording precondition.
- **BR-2**: Success raises the AccountingEvent → posting (pipeline).
- **BR-3**: A server constraint prevents concurrent double payment.
- **BR-4**: The payment is immutable — correction via another payment/server action (OQ if any).

### UI/UX

- `/payments/payments`: table (number/request/order/method/amount/status/payer/date) + filters (period/method/status).
- Record dialog (from PAY-02 details): readonly amount snapshot + method select + referenceNumber + notes + confirmation.
- Success screen: the closed triad (request/order/payment) with their numbers · failure state: verbatim reason + retry button.
- States: CC-2.

### Success Criteria

- **SC-001**: Recording a payment takes under 30 seconds.
- **SC-002**: 100% of successes close the triad (tested).
- **SC-003**: Every failure shows a verbatim reason.

### Tests

- **T1** (FR-001/US1): Approved precondition + refusal otherwise.
- **T2** (FR-003/FR-004): snapshot readonly + triad closure.
- **T3** (FR-002): only the two methods in the picker.
- **T4** (FR-005/US2): Failed with reason + retry.
- **T5** (FR-006/US3): list and filters + no edit buttons.

### Open Questions

- **OQ-N1** (Engineering): ~~referenceNumber required for Check?~~ — RESOLVED by code read: optional (no conditional validation in `RecordPaymentCommandValidator`).
- **OQ-N2** (User): the Transfer/InKind legal gap (m26) — raise as a separate backend-contract request? (Do not invent in the UI.)

---

## Sub-Module: PAY-04 — Bank Accounts (bank-accounts)

> **Correction note**: `bankName`/`openingBalance`/`currentBalance` **do exist** in the current contract (contrary to rumors of removal under mandate m26) — the code is the truth (AGENTS.md).

### User Scenarios

#### US1 (P1) — Accounts registry

The banks reference for orders and statements.

**Acceptance Scenarios**:

1. **Given** accounts, **When** the registry opens, **Then** columns show (name/bankName/accountNumber/currency/isDefault/limits/isActive/lastReconciliationDate).
2. **Given** a deactivated account, **When** the PAY-01 picker opens, **Then** it is not listed.

#### US2 (P1) — Create/edit with full details

Bank-level control.

**Acceptance Scenarios**:

1. **Given** complete data, **When** created (name/bankName/accountNumber + iban/swift/branch + currency/fund/GL + limits/dual-approval/openingBalance), **Then** it is saved and displayed in full.
2. **Given** a duplicate accountNumber, **When** created, **Then** the server refuses.

#### US3 (P2) — Activate/deactivate

Pausing without deleting history.

**Acceptance Scenarios**:

1. **Given** an active account, **When** deactivated (confirmation), **Then** `isActive=false` + excluded from pickers.
2. **Given** an account with active statements, **When** deactivation is attempted, **Then** a server rule decides (OQ-N2) and its message shows.

### Edge Cases

- isDefault conflict (auto-switch: new default clears the previous in the same transaction — clarified 2026-09-08).
- Editing `glAccountId` after it was used in posting (server rule).
- `currentBalance` displayed but never hand-edited (auto-updated — OQ-N3).
- RowVersion conflict.
- Limits (maxDaily/maxTransaction) exceeded by a payment (effect in PAY-03? — shown as a warning).

### Functional Requirements

- **FR-001**: Full CRUD with all literal contract fields (17 fields).
- **FR-002**: `isDefault` as a single flag — setting it auto-clears the previous default in the same transaction (clarified 2026-09-08).
- **FR-003**: Limits + `requiresDualApproval` displayed (badge; documented effect on PAY-02/03).
- **FR-004**: activate/deactivate with `{id, rowVersion}` + confirmation.
- **FR-005**: `openingBalance` entered at creation · `currentBalance`/`lastReconciliationDate` display-only (BANK-01 feeds them).
- **FR-006**: A deactivated account is excluded from all new pickers.

### Data Contract

**BankAccountDto** ✓ (L22908):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| name | str | ✓ | — |
| bankName | str | ✓ | exists (corrected) |
| accountNumber | str | ✓ | unique |
| iban / swiftCode | str | ○ | — |
| branchName / branchCode | str | ○ | — |
| currencyId | int | ✓ | 024 |
| fundId | int | ○ | fund |
| glAccountId | int | ○ | posting link |
| isDefault | bool | ✓ | single default |
| maxDailyLimit / maxTransactionLimit | dec | ○ | limits |
| requiresDualApproval | bool | ✓ | badge |
| lastReconciliationDate | dt? | ○ | display only (BANK-01) |
| openingBalance / currentBalance | dec | ○ | opening entered · current display |
| isActive | bool | ✓ | — |

**CreateBankAccountCommand** ✓ (L26313): all of the above minus id/lastReconciliationDate/currentBalance/isActive

**Activate/DeactivateBankAccountCommand** ✓ (L20509/L29362): `{id, rowVersion}` (verify — GAP-READ partial)

### Lifecycle

`isActive` toggled via activate/deactivate — no status cycle.

### Permissions

| Code | Status |
|---|---|
| BankAccounts.View/Create/Update/Activate/Deactivate | ✓ all five confirmed |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/BankAccounts |
| GET / PUT | /{id} |
| POST | /{id}/activate · /{id}/deactivate |

### Business Rules

- **BR-1**: accountNumber unique.
- **BR-2**: single isDefault — auto-switch on reassignment.
- **BR-3**: deactivation never deletes history — old statements stay linked.
- **BR-4**: current balances are never hand-edited.

### UI/UX

- `/payments/bank-accounts`: table + filters (active/currency/fund).
- Create/edit dialog with sections: identity (name/bank/account) · branch (iban/swift/branch) · financial (currency/fund/GL/opening) · controls (limits/dual/default).
- Details: all details + lastReconciliationDate + activate/deactivate buttons (confirmation).
- States: CC-2.

### Success Criteria

- **SC-001**: A complete account can be created in under 2 minutes.
- **SC-002**: A deactivated account is 100% absent from pickers.
- **SC-003**: Every activation/deactivation happens with confirmation.

### Tests

- **T1** (FR-001/US2): CRUD with details + duplicate refused.
- **T2** (FR-004/FR-006/US3): deactivation + exclusion from the PAY-01 picker.
- **T3** (FR-005): currentBalance/lastReconciliation read-only.
- **T4** (FR-003): dual-approval and limits badges.

### Open Questions

- **OQ-N1** (User): ~~isDefault behavior when a second is set~~ — RESOLVED 2026-09-08: auto-switch in the same transaction.
- **OQ-N2** (Engineering): deactivation rule with active statements/reconciliations.
- **OQ-N3** (Engineering): is currentBalance auto-updated from BANK-01?

---

## Requirements *(mandatory)*

### Group-level Functional Requirements

- **FR-G01**: The four sub-modules MUST ship under `/payments/*` routes with Arabic RTL UI per the design tokens (Constitution X).
- **FR-G02**: Every sub-module endpoint MUST declare its permission code; GAP-ADD codes (PaymentOrders.* including `PaymentOrders.OverrideBudgetCheck`, DisbursementRequests.*) MUST be added to the permission registry and registered as policies.
- **FR-G03**: Document numbering (PO-{D6}/DSB-{D6}/PAY-{D6}) MUST be allocated via the document sequence service within the same transaction (Constitution IV).
- **FR-G04**: All approval decisions across PAY-01/02 MUST flow through ApprovalHistory + DocumentStatusLog (CC-3).
- **FR-G05**: Successful payments MUST raise the accounting event for posting through the existing domain-event → PostingRules pipeline (Constitution II).

### Key Entities *(include if feature involves data)*

- **PaymentOrder**: the official disbursement decision — header (vendor, fund, fiscal year, appropriation, currency, beneficiary) + lines (5 types) + deductions (7 types); status (9 states) + budgetCheckStatus (4 states) + treasury trace + journal link.
- **DisbursementRequest**: the execution trigger, 1:1 with its order; status (7 states); dual-signature approvals; availability warning flag.
- **Payment**: the single immutable execution record on an Approved request; Cash/Check; Completed/Failed; closes the order-request-payment triad.
- **BankAccount**: the bank reference (identity/branch/financial/controls); unique accountNumber; activate/deactivate; balance display fields fed by BANK-01.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-G01**: The full disbursement chain (order → request → dual approval → payment) completes in under 15 minutes for a trained user.
- **SC-G02**: Zero expenditures occur without passing the availability and budget checks (or a documented override/decision).
- **SC-G03**: 100% of displayed financial numbers are server-issued (verified by a client-computation audit).
- **SC-G04**: Every lifecycle decision (submit/approve/reject/cancel/send-to-treasury/void) is auditable from ApprovalHistory/DocumentStatusLog alone.

## Assumptions

- The four sub-specs are implemented on the existing PaymentOrder/DisbursementRequest/Payment/BankAccount backend contract (verified DTO line references); no new HTTP contract is invented.
- Permission codes marked GAP-ADD are added in the same feature; enforcement wiring follows the pending RBAC enforcement spec (open placeholder `RequireAssertion(_ => true)` remains until then).
- Two different users may share one qualified role for the dual signature — each signer must individually hold AccountsManager or AuthorizingOfficer (clarified 2026-09-08); verify against SEC-02 rules.
- The Transfer/InKind payment-method legal gap is tracked as a backend-contract request and is NOT invented in the UI.
- Availability checking reuses the existing BudgetAvailabilityService (021); the group does not re-implement it.
- Ordering within the group: PAY-04 and PAY-01 can proceed in parallel first; PAY-02 then PAY-03 follow the chain.
