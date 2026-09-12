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

### Session 2026-09-09 — Request-First Amendment

Stakeholder clarifications Q1–Q3 from `feature-context-request-first-disbursement.md` are adopted into the canonical spec. The amendment changes the workflow direction: requests now initiate the process and orders are produced as the approval outcome.

- Q1 (Issuing authority): The authority represents the officeholder (المدير العام أو المدير المالي), not a department. The actual person and their issuing capacity (General Manager or Finance Director) are both recorded and traceable server-side. Identity and authority are validated server-side; selecting a title alone does not grant authority.
- Q2 (Dual signature retained): The existing two-signature requirement is retained. Both signatures must be by distinct qualified users. Both must authorize the same request revision and amount; signatures on different amounts must not be combined as final authorization. The existing signer qualification rules (AccountsManager/AuthorizingOfficer) remain applicable until role-to-position mapping is explicitly defined; the two named issuing capacities do not imply that both positions must sign every request.
- Q3 (Approved amount limit): The reviewer may approve an amount equal to or lower than the requested amount. Approval above the requested amount is rejected. A positive approved amount is the default validation. A mandatory reduction reason is a proposal, unlike the explicitly required rejection reason.
- Q4 (Request-first order approval path): A Draft order generated from an approved request follows the full lifecycle: Submitted → budget check → Approved → SentToTreasury → Paid. Request approval authorizes creation only; the order still passes through budget check and internal approval. Budget check failure blocks order approval unless a documented override is applied (Constitution V).

### Session 2026-09-09 — Schema Simplification (ADR-001)

Six structural conflicts resolved per ADR-001:

- D-1: BeneficiaryName only — no Party ID on request or order. `BeneficiaryId` removed from DisbursementRequest; `BeneficiaryPartyId` removed from PaymentOrder.
- D-2: `PaymentOrder.DisbursementRequestId` is UNIQUE; `PaymentOrderId` removed from DisbursementRequest. Link is one-directional (order → request).
- D-3: Budget check and `HasWarning` removed from request. Check runs only at order submit after FundId/AppropriationId are filled.
- D-4: `PaymentOrderLine` removed from target model. `AccountId` added to PaymentOrder (optional in Draft, mandatory at Submit).
- D-5: Approval data (`ApprovedAmount`, `IssuingAuthorityName`, `IssuingAuthorityCapacity`, `ApprovalDate`) removed from DisbursementRequest. Stored in ApprovalHistory only.
- D-6: Payment links to order via `PaymentOrderId` UNIQUE. Request link derived from `PaymentOrder.DisbursementRequestId`.

## Shared Group Context

**Dependency chain (request-first)**: PAY-02 (request) initiates the workflow → PAY-01 (order) is generated upon final approval → PAY-03 (payment) executes once. PAY-04 (bank accounts) feeds PAY-01's `bankAccountId` picker and is independent of the chain ordering.

**Consumers**: PAY-01 reads request-approved amount/authority; PAY-03 reads order state; RPT-03, CTRL-03 read payment-order state; ACC-05 receives the posting event raised by successful payments.

**Frontend screen specifications**: see [frontend-requirements.md](./frontend-requirements.md) for detailed field-level requirements for all screens across PAY-01..04.

**Shared invariants (inherited by all sub-modules, not repeated below)**:

- **CC-1 (Server numbers)**: Every financial amount, status, and computed total displayed or exported comes from the server; the frontend never recomputes financial values (Constitution III/IV).
- **CC-2 (State UI)**: Every status renders as a color-coded badge; every blocking server refusal renders its server message verbatim plus actionable detail — never silently swallowed.
- **CC-3 (Approvals via 022 panels)**: Every approval/reject decision is recorded through the 022 ApprovalHistory panels — no inline approval columns (Constitution VIII, fix 016).
- **CC-4 (Concurrency)**: All mutating endpoints accept and honor `rowVersion`; conflicts surface as a server message.
- **CC-5 (Lifecycle actions are conditional)**: Action buttons appear only in the states their lifecycle table lists; server re-validates independently.

## Sub-Module: PAY-01 — Payment Orders (payment-orders)

> **Amendment note (2026-09-09)**: PAY-01 orders can now originate from two paths: (1) the legacy order-first flow (direct creation by an accountant), and (2) the request-first flow (auto-generated from an approved PAY-02 request). The request-first path produces header-only orders (single account, no order lines) carrying the approved amount and issuing authority from the request. Existing multi-line orders remain representable and are not silently collapsed.

### User Scenarios

#### US1 (P1) — Create a complete order (order-first path)

An accountant issues the official disbursement order with its beneficiary, lines, deductions, and financial details.

**Acceptance Scenarios**:

1. **Given** a beneficiary, an appropriation, and line items, **When** the accountant creates an order (header + lines[] + deductions[] + beneficiary), **Then** it is created as Draft with a server-issued `paymentOrderNumber` (`PO-{D6}`) and server-computed `amountGross`/`deductionAmount`.
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

#### US4 (P1) — Totals

Server-issued financial summary.

**Acceptance Scenarios**:

1. **Given** an order, **When** the user opens `/{id}/totals`, **Then** the four values (`amountGross`, `totalDeductions`, `netAmount`, `isFullyPaid`) are server-issued.
2. **Given** an order from the request-first path, **When** totals are displayed, **Then** `amountGross` reflects the approved amount from the request.

#### US5 (P2) — Voiding

Correcting an approved, unpaid order.

**Acceptance Scenarios**:

1. **Given** an Approved, unpaid order, **When** the user voids it (with confirmation), **Then** it becomes Voided — final state.
2. **Given** a partially paid order, **When** voiding is attempted, **Then** it is refused (server rule — clarified 2026-09-08).
3. **Given** a voided order with an associated request (request-first path), **When** the void is committed, **Then** any Draft/PendingApproval request on that order is automatically set to Invalidated.

#### US6 (P1) — Request-first order creation (amended 2026-09-09)

When a disbursement request receives final approval, the server generates a linked order.

**Acceptance Scenarios**:

1. **Given** a request that receives final approval (two valid signatures by distinct qualified users, same amount), **When** the approval completes atomically, **Then** exactly one Draft order is generated with the approved amount as `amountGross`, the request's beneficiary, currency, and issuing authority recorded.
2. **Given** the generated order, **When** displayed, **Then** it shows the issuing authority (officeholder name + capacity: General Manager or Finance Director) and is linked back to the source request.
3. **Given** a generated Draft order from the request-first path, **When** the user opens it, **Then** it carries header-level data only (no order lines) — fundId and appropriationId are not yet set; the order uses the single-account/header model.
4. **Given** an approved request for 80,000 (requested 100,000), **When** the order is generated, **Then** `amountGross` = 80,000, and the original requested amount remains visible on the request.
5. **Given** an order generated from a request, **When** the user tries to edit `amountGross` or beneficiary, **Then** the server follows the declared edit/resubmission rules — changes affecting authorization require reauthorization.
6. **Given** a generated Draft order, **When** submitted, **Then** the standard lifecycle applies: Submitted → budget check → Approved → SentToTreasury → Paid. Request approval authorizes creation only; the order still passes through budget check and internal approval.

### Edge Cases

- Editing a non-Draft order — blocked in UI and refused by the server.
- Cancelling/voiding an order that has an existing disbursement request (PAY-02 reads the order) — server rule decides; request set to Invalidated automatically.
- Deduction total exceeding gross amount — server refuses.
- Non-base currency — `exchangeRate` relevance (OQ-N3).
- RowVersion conflict on concurrent edit.
- Order without `appropriationId` — refused (field required ✓).
- Concurrent final-approval attempts on the same request — at most one order generated (atomicity via RFD-006/RFD-007).
- First signature on a request — no order generated yet.
- Signatures on different amounts — cannot finalize; reauthorization required.

### Functional Requirements

**Confirmed**:

- **FR-001** ✓: The system MUST provide order CRUD: a list with filters (status / budgetCheckStatus / period) + create + edit (Draft only).
- **FR-002** ✓: Orders MUST support deductions (7 types) with `isMandatory`/`isTaxDeduction` flags + `taxAuthorityId`. PaymentOrderLine removed from target model (ADR-001 D-4). `AccountId` is optional in Draft, mandatory at Submit.
- **FR-003** ✓: `budgetCheckStatus` MUST carry its four states — Failed MUST block approval.
- **FR-004** ✓: The full lifecycle MUST be driven by conditional actions: submit/approve/reject/cancel/send-to-treasury/void.
- **FR-005** ✓: Totals MUST come exclusively from `/{id}/totals` — server-issued values; no client-side computation (CC-1).
- **FR-006** ✓: The treasury trace (`treasuryStatus`/`treasuryReference`/`treasurySentAt`) and `journalEntryId` (journal entry link) MUST be displayed.
- **FR-007** ✓: Every approval decision MUST be captured through the 022 panels — no inline columns (CC-3).
- **FR-008** ✓: Orders generated from approved requests MUST carry the issuing authority (officeholder name + capacity: General Manager or Finance Director) recorded in ApprovalHistory. Identity and capacity MUST be validated server-side.
- **FR-009** ✓: Voiding or cancelling an order MUST automatically set any Draft/PendingApproval request on that order to Invalidated.
- **FR-010** ✓: Orders from the request-first path MUST NOT be eligible for payment before budget controls, required order approvals, and treasury gates are satisfied.
- **FR-011** ✓: `DisbursementRequestId` on PaymentOrder MUST be UNIQUE — one request produces at most one order (ADR-001 D-2).

### Data Contract

**PaymentOrderDto** ✓ (L34412):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | ✓ | — |
| paymentOrderNumber | str | ✓ | PO-{D6} |
| paymentOrderDate | date | ✓ | — |
| dueDate | date | ○ | — |
| paymentOrderType | str | ✓ | server values (OQ) |
| fundId / fiscalYearId / appropriationId | int | ✓ | budget — fundId/appropriationId nullable during Draft on request-first orders; required at submit |
| budgetClassificationId / costCenterId / projectId | int | ○ | dimensions |
| accountId | int | ○ | GL account link — optional in Draft, mandatory at Submit (ADR-001 D-4) |
| purchaseOrderId / encumbranceId | int | ○ | procurement/encumbrance links |
| currencyId | int | ✓ | 024 |
| exchangeRate | dec | ○ | non-base currency |
| amountGross / deductionAmount | dec | ✓ | server-computed |
| paymentMethod | enum? | ○ | Cash/Check |
| paymentMethodName | str | — | display |
| bankAccountId | int | ○ | PAY-04 (active only) |
| beneficiaryName | str | ✓ | required — copied from request (request-first) or supplier (procurement); BeneficiaryPartyId removed (ADR-001 D-1) |
| beneficiaryIban / beneficiaryAccountNumber / beneficiaryBankName | str | ○ | — |
| status | enum PaymentOrderStatus | ✓ | **Draft/Submitted/Approved/SentToTreasury/Paid/Cancelled/Rejected/Voided** (8 states — PartiallyPaid removed, single payment per order) |
| budgetCheckStatus | enum | ✓ | **Pending/Passed/Failed/Overridden** (L23452) |
| treasuryStatus / treasuryReference / treasurySentAt | str? / str? / dt? | ○ | treasury trace |
| paidAt | dt? | ○ | — |
| journalEntryId | int? | ○ | journal link |
| notes | str | ○ | — |
| disbursementRequestId | int? | ○ | UNIQUE link to source request (request-first path); null on order-first (ADR-001 D-2) |
| issuingAuthorityName / issuingAuthorityCapacity | str / str | ○ | set on request-first orders; stored in ApprovalHistory (ADR-001 D-5) |

**PaymentOrderDeductionDto** ✓ (L34320): lineNumber, deductionType (enum 7), deductionCode?, description?, accountId*, amount*, deductionPercent?, isMandatory, isTaxDeduction, taxAuthorityId?, referenceNumber?

**PaymentOrderTotalsDto** ✓ (L34724): id, amountGross, totalDeductions, netAmount, paidAmount, remainingAmount, isFullyPaid, status

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| — | create (order-first) | Draft | appropriationId required; deductions present; accountId optional |
| — | create (request-first) | Draft | generated from approved request; amountGross = approved amount; no lines; issuing authority recorded in ApprovalHistory; fundId/appropriationId null (deferred to preparation) |
| Draft | submit | Submitted | fundId + appropriationId + accountId required; budget check runs |
| Draft | cancel | Cancelled | — |
| Submitted | approve | Approved | budgetCheck ≠ Failed |
| Submitted | reject | Rejected | reason required |
| Approved | send-to-treasury | SentToTreasury | treasuryReference required |
| SentToTreasury | (PAY-03 payment) | Paid | server (single payment) |
| Approved/SentToTreasury | void | Voided | unpaid only; linked requests → Invalidated |

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
- **BR-6**: Request-first orders carry issuing authority (officeholder name + capacity); identity validated server-side.
- **BR-7**: Void/cancel of an order automatically invalidates associated Draft/PendingApproval requests.

### UI/UX

> **Screen-level field/behavior specifications**: see [frontend-requirements.md](./frontend-requirements.md) for complete field definitions, data sources, editability rules, validation, state-based actions, loading/empty/error handling.

- `/payments/payment-orders`: table (number/beneficiary/date/net/status/check badge) + filters.
- `/create`: sections — header (fund/fiscal year/appropriation/classification/dimensions/currency+rate/due date) · lines (typed grid) · deductions (typed grid + mandatory/tax + tax authority) · beneficiary (name/party/iban/account/bank) · totals preview.
- `/{id}`: header + lines + deductions + Totals card (4 numbers) + budgetCheckStatus badge + 022 panels + treasury trace + journal link · issuing authority badge (request-first orders) · send-to-treasury/void dialogs (confirmation).
- States: CC-2 + the four budget-check badges with colors.

### Success Criteria

- **SC-001**: A complete order (5+ lines, 3+ deductions) can be created in under 5 minutes.
- **SC-002**: Zero approvals for Failed orders without a documented override.
- **SC-003**: Totals match the server 100% (no client-side computation).
- **SC-004**: Request-first order creation from final approval is atomic — no order exists without an approved request, and no approved request lacks its order.

### Tests

- **T1** (FR-001..003/US1): full creation with line types and deduction types + mandatory.
- **T2** (FR-004/US2): the four check badges + Failed blocking.
- **T3** (FR-005/US3): the lifecycle with conditional actions.
- **T4** (FR-006/US4): the totals + server-issued values.
- **T5** (FR-007/US5): treasury trace + void + request invalidation.
- **T6** (FR-008): 022 panels contain every decision.
- **T7** (FR-009/US6): request-first order creation + issuing authority + atomicity.
- **T8** (FR-010): void/cancel propagates invalidation to requests.

### Open Questions

- **OQ-N1** (Engineering): ~~Void rule for partially paid~~ — RESOLVED 2026-09-08: unpaid only, partial refused. Remaining: void of an order with an existing disbursement request (server rule) — verify against PAY-02 cancel behavior.
- **OQ-N2** (User/Engineering): ~~Override permission for a Failed check~~ — RESOLVED 2026-09-08: dedicated `PaymentOrders.OverrideBudgetCheck` permission (GAP-ADD).
- **OQ-N3** (Engineering): exchangeRate — when is it required (non-base currency?).
- **OQ-N4** (Engineering): Which signer supplies the issuing authority on the generated order — the final signer, or a designated capacity? Not silently assumed; to be specified during planning.

---

## Sub-Module: PAY-02 — Disbursement Requests (disbursement-requests)

> **Amendment note (2026-09-09)**: PAY-02 is rewritten to a request-first model. Requests are now independent — they do not require an existing payment order. The request captures the original need, is reviewed with an approved amount, and upon final approval the server generates the linked order. The approved amount may equal or be lower than the requested amount; exceeding it is prohibited. Issuing authority (General Manager / Finance Director) is recorded on approval. The two-signature approval requirement is retained.

### User Scenarios

#### US1 (P1) — Submit an independent request

A requester initiates a disbursement without an existing order.

**Acceptance Scenarios**:

1. **Given** no payment order exists, **When** a submitter creates a request with beneficiary, positive requested amount, currency, purpose, financial year, and optional details, **Then** it is created as Draft with a server-issued `requestNumber` (`DSB-{D6}`).
2. **Given** a submitted draft request, **When** its submitter tries to change the requested amount or beneficiary before approval, **Then** the server follows the declared edit/resubmission rules rather than silently changing the decision basis.
3. **Given** a request with `requestedAmount ≤ 0`, **When** creation is attempted, **Then** the server refuses with a validation message.
4. **Given** a request, **When** the user opens it, **Then** the original requested amount, currency, beneficiary, purpose, requester, and financial year are visible.

#### US2 (P1) — Approve an amount and generate an order

The regulatory review with amount authorization.

**Acceptance Scenarios**:

1. **Given** a submitted request for 100,000, **When** the first qualified approver approves with an approved amount of 80,000 and identifies the issuing authority (officeholder name + capacity: General Manager or Finance Director), **Then** the approval record is saved with `{step, approverName, role, decision, decisionAt, approvedAmount, issuingAuthorityName, issuingAuthorityCapacity}`. No order is generated yet.
2. **Given** the same user, **When** a second signature is attempted, **Then** the server refuses.
3. **Given** the second approver is not a qualified role (not AccountsManager/AuthorizingOfficer), **When** approval is attempted, **Then** the server refuses.
4. **Given** two valid signatures by distinct qualified users both authorizing the same amount (80,000), **When** the final approval completes atomically, **Then** the request becomes Approved + `approvalDate` and exactly one linked Draft order is generated with `amountGross` = 80,000, the beneficiary, currency, and issuing authority.
5. **Given** a request for 100,000, **When** 100,001 is proposed for approval, **Then** the server refuses without recording that approval or generating an order.
6. **Given** the first signature authorizes 80,000, **When** a second signature proposes 75,000, **Then** the signatures cannot finalize an order together; the specified reauthorization procedure is required.
7. **Given** two concurrent final-approval attempts, **When** processed, **Then** at most one resulting order exists and history contains no duplicate final decision.
8. **Given** order creation fails during final approval, **When** approval returns, **Then** neither the new final approval nor a newly approved request is committed.

#### US3 (P1) — Reject with a reason

Exit path that releases resources.

**Acceptance Scenarios**:

1. **Given** a request awaiting review, **When** rejected with a nonblank reason, **Then** the request becomes Rejected with the reason visible and no order is generated.
2. **Given** a blank or whitespace-only rejection reason, **When** rejection is attempted, **Then** it is refused without changing state.
3. **Given** an unauthorized actor, **When** approval or rejection is attempted, **Then** the decision is refused server-side.

#### US4 (P1) — Submission as pure status transition

> **Replaced (2026-09-09)**: The old availability gate (HasWarning/Blocking) has been removed from the request level per ADR-001 D-3. Budget check runs only at order submit. Request submit is a pure status transition.

**Acceptance Scenarios**:

1. **Given** a Draft request, **When** the submitter submits it, **Then** the status changes to PendingApproval with no budget validation at request level.
2. **Given** a request missing required fields (beneficiaryName, requestedAmount, currencyId, purpose, financialYearId), **When** submission is attempted, **Then** the server refuses with field-level validation messages.

#### US5 (P1) — Cancellation and invalidation

Exit paths.

**Acceptance Scenarios**:

1. **Given** a Draft/PendingApproval request, **When** cancelled (reason required), **Then** it becomes Cancelled.
2. **Given** an Approved request whose order has not been paid, **When** cancelled, **Then** it becomes Cancelled and the order is released.
3. **Given** a Disbursed request, **When** cancellation is attempted, **Then** it is refused.
4. **Given** a Draft/PendingApproval request whose linked order is cancelled or voided, **When** the order state changes, **Then** the request is automatically set to Invalidated.

#### US6 (P1) — Trace the full chain

From request to payment.

**Acceptance Scenarios**:

1. **Given** a request with an approved amount and linked order, **When** the user opens the detail view, **Then** the original requested amount, final approved amount, issuing authority, order number, deductions, net amount, and payment (if completed) are all visible.
2. **Given** an approved request with a generated order, **When** viewed, **Then** the approval state and payment progress are distinguishable.

### Edge Cases

- Order cancelled/voided after loading (server refusal).
- Two different users sharing one qualified role — allowed; each signer must individually hold the role (clarified 2026-09-08).
- RowVersion conflict.
- Invalidated (automatic when linked order cancelled/voided).
- Concurrent final-approval attempts — at most one order produced (atomicity).
- First signature on a request — no order generated; second signature required.
- Signatures on different amounts — cannot finalize; reauthorization required.
- Request submitted with zero/negative amount — refused.

### Functional Requirements

**Confirmed by stakeholder (Q1–Q3 answered 2026-09-09)**:

- **FR-001** ✓: The system MUST allow creation of a disbursement request without an existing payment order and allocate its unique request number server-side.
- **FR-002** ✓: The system MUST preserve the originally requested amount and currency. Requester identity MUST be distinct from beneficiary.
- **FR-003** ✓: The system MUST restrict review decisions to authorized actors and record the actor, time, decision, reason (for rejections), approved amount, issuing authority name, and issuing capacity (General Manager or Finance Director) for approval decisions in ApprovalHistory. Identity and authority MUST be validated server-side.
- **FR-004** ✓: Rejection MUST require a nonblank reason, leave the request rejected, expose the reason to the submitter, and create no order.
- **FR-005** ✓: Completion of two valid signatures by distinct qualified users MUST generate the linked order. The first signature MUST NOT generate an order. Both signatures MUST authorize the same request revision and amount; signatures on different amounts MUST NOT be combined as final authorization.
- **FR-006** ✓: Final approval, numbering, order creation and decision/status history MUST succeed atomically. Failure MUST NOT leave a newly approved request without its required order.
- **FR-007** ✓: Repeated submission of the same final decision and concurrent approval attempts MUST NOT create duplicate orders or final approval records. Stale decisions MUST produce an explicit conflict or a documented idempotent result.
- **FR-008** ✓: A request MUST have at most one resulting order. An order MUST retain traceability to its source request via `DisbursementRequestId` UNIQUE (ADR-001 D-2).
- **FR-009** ✓: The generated order MUST carry the approved gross amount, beneficiary, currency and issuing authority. It MUST NOT be eligible for payment before financial preparation, budget controls, required order approvals and treasury gates are satisfied.
- **FR-010** ✓: Finance MUST NOT silently replace the approved amount, currency or beneficiary. Changes affecting the authorization MUST follow a defined reauthorization process.
- **FR-011** ✓: The system MUST reject an approved amount greater than the requested amount. Both the original requested amount and the final authorized amount MUST remain visible in ApprovalHistory.
- **FR-012** ✓: Submission is a pure status transition — no availability check at request level. Budget check runs only at order submit (ADR-001 D-3).

**Design proposals (require planning validation)**:

- **FR-013** [DESIGN]: Signatures are rendered via `approvals[]` from ApprovalHistory — a Stepper component with two steps (step/actor/role/decision/time/approvedAmount/issuingAuthority).
- **FR-014** [DESIGN]: PATCH lifecycle: submit/approve/reject/cancel, conditional on status.
- **FR-015** [DESIGN]: Cancelling an Approved unpaid request releases the order.
- **FR-016** ✓: When the linked order is cancelled/voided, the server MUST automatically set any Draft/PendingApproval request on it to Invalidated.

### Data Contract

**DisbursementRequestDto** (amended per ADR-001):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | ✓ | — |
| requestNumber | str | ✓ | DSB-{D6} |
| requestedById / requestedByName | int / str | ✓ | requester identity |
| beneficiaryName | str | ✓ | distinct from requester — BeneficiaryId removed (ADR-001 D-1) |
| requestedAmount | dec | ✓ | original amount requested (positive) |
| currencyId | int | ✓ | 024 |
| purpose | str | ✓ | reason for the disbursement |
| financialYearId | int | ✓ | fiscal year |
| requestDate | date | ✓ | — |
| status | enum DisbursementRequestStatus | ✓ | **Draft/PendingApproval/Approved/Rejected/Cancelled/Disbursed/Invalidated** (7 states) |
| notes | str | ○ | — |
| paymentDate | dt? | ○ | set by PAY-03 payment completion |
| hasWarning | bool | ✓ | **REMOVED** (ADR-001 D-3) — availability check moved to order submit |

**DisbursementRequestDetailDto**: + `approvals: ApprovalStepDto[] {step, approverUserId, approverName, role, decision, decisionAt}` — approval data (approvedAmount, issuingAuthority) stored in ApprovalHistory only (ADR-001 D-5).

**CreateDisbursementRequestRequest**: `{beneficiaryName!, requestedAmount!, currencyId!, purpose!, financialYearId!, notes?}` — **no paymentOrderId** (request-first); **no beneficiaryId** (ADR-001 D-1).

**ApproveDisbursementRequestRequest**: `{approvedAmount!, issuingAuthorityName!, issuingAuthorityCapacity!, reason?}` — approval with amount and authority; stored in ApprovalHistory (ADR-001 D-5).

**RejectDisbursementRequestRequest**: `{reason!}` — nonblank reason required.

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| — | create (independent) | Draft | no order required; positive requestedAmount; beneficiaryName + currencyId + purpose + financialYearId |
| Draft | submit | PendingApproval | pure status transition — no availability check at request level (ADR-001 D-3); budget check at order submit |
| PendingApproval | approve (1st) | PendingApproval | qualified role + approvedAmount ≤ requestedAmount + issuing authority recorded in ApprovalHistory; different user enforced at step 2 |
| PendingApproval | approve (2nd) | Approved + order generated | different user + qualified role + same amount as 1st; atomic: order created (DisbursementRequestId UNIQUE), status log + approval history written |
| PendingApproval | reject | Rejected | nonblank reason; no order created |
| Draft/PendingApproval | cancel | Cancelled | reason required |
| Approved | cancel | Cancelled | unpaid only; releases the order |
| Approved | (PAY-03 payment) | Disbursed | server; paymentDate set |
| Draft/PendingApproval/(Approved unpaid) | order cancel/void | Invalidated | automatic |

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

- **BR-1**: Independent creation — no order prerequisite; positive requestedAmount required.
- **BR-2**: Dual signature: ≥2 distinct users, BOTH steps require a qualified role (AccountsManager/AuthorizingOfficer), same user forbidden.
- **BR-3**: Approved amount ≤ requested amount; exceeding refused.
- **BR-4**: Two valid signatures on the same amount generate the order atomically.
- **BR-5**: Issuing authority = officeholder name + capacity (General Manager or Finance Director), validated server-side; stored in ApprovalHistory (ADR-001 D-5).
- **BR-6**: Cancellation releases the order for a new request.
- **BR-7**: Both original requested amount and final approved amount remain visible in ApprovalHistory.
- **BR-8**: Submission is a pure status transition — no availability check at request level (ADR-001 D-3).

### UI/UX

> **Screen-level field/behavior specifications**: see [frontend-requirements.md](./frontend-requirements.md) for the complete field definitions, data sources, editability rules, validation, state-based actions, loading/empty/error handling for every screen. The summary below is for quick reference.

- `/payments/disbursement-requests`: table (number/requester/beneficiary/amount/approvedAmount/status/warning badge) + filters (status/requester).
- `/create`: form with beneficiary picker, positive requested amount, currency, purpose, financial year, optional notes.
- `/{id}`: header (requester/beneficiary/requested amount/approved amount/purpose/financial year) + **dual-signature Stepper** (from approvals with approvedAmount and issuingAuthority per step) + hasWarning badge + linked order detail (after approval) · conditional PATCH buttons · approve/reject/cancel dialogs (approve: amount + authority; reject: reason).
- States: CC-2 + the availability refusal state (verbatim detail panel).

### Success Criteria

**Confirmed**:

- **SC-001** ✓: Every newly finalized approved request has exactly one linked order with matching approved gross amount, currency, beneficiary and issuing authority.
- **SC-002** ✓: Every rejected request has a visible nonblank rejection reason and no generated order.
- **SC-003** ✓: Zero second requests on the same order.
- **SC-004** ✓: 100% of approvals carry two distinct signatures with matching roles and same amount.
- **SC-005** ✓: Duplicate/concurrent approval produces zero duplicate orders or final approval records.
- **SC-006** ✓: Every cancellation releases its order immediately (visible in the create picker).
- **SC-007** ✓: Both the original requested amount and the final authorized amount remain visible on the request detail.

### Tests

**Confirmed**:

- **T1** ✓ (FR-001/US1): independent request creation without an order + number allocation.
- **T2** ✓ (FR-002/US1): requested amount preserved separately from approved amount.
- **T3** ✓ (FR-003/US5/US2): Stepper with two signatures + same-user/role/amount refusals + issuing authority in ApprovalHistory.
- **T4** ✓ (FR-004/US3): rejection with nonblank reason + blank refusal.
- **T5** ✓ (FR-005/US6/US2): atomic order generation on final approval + no order on first signature.
- **T6** ✓ (FR-006/FR-007): atomicity + no duplicate orders on concurrent attempts.
- **T7** ✓ (FR-011/US2): approved amount > requested amount refused.
- **T8** ✓ (FR-015/US5): cancellation releases the order.
- **T9** ✓ (FR-016/US5): order cancel/void propagates invalidation.
- **T10** ✓ (FR-014): the seven states with conditional actions.

### Open Questions

- **OQ-N1** (Engineering): ~~Fields of Reject/Cancel commands~~ — RESOLVED by code read: both require a reason (validator-enforced).
- **OQ-N2** (Engineering): ~~When is Invalidated set?~~ — RESOLVED 2026-09-08: automatic on linked-order cancel/void.
- **OQ-N3** (Engineering): Which signer's identity populates `issuingAuthorityName` on the generated order — the final signer, or a separately designated authority? Not silently assumed; to be specified during planning.
- **OQ-N4** (User): Mandatory reduction reason when approved amount < requested amount — proposal, not yet confirmed.
- **OQ-N5** (User/Engineering): **Beneficiary Party linkage** — The current model uses BeneficiaryName string only (ADR-001 D-1). A `BeneficiaryPartyId` linking to `Parties` was removed. Should the beneficiary Party linkage be re-introduced in a future version? No stakeholder decision recorded; deferred to a separate ADR if needed.
- **OQ-N6** (User): **Issuing authority identity between signers** — When two signers approve in different capacities (General Manager / Finance Director), which signer's identity and capacity populate the order's issuing authority? The system records both signatures in ApprovalHistory but does not yet designate which becomes the order's issuing authority. Not silently assumed; to be specified during planning.
- **OQ-N7** (User): **Cancellation/re-authorization after order generation** — When an Approved request's order is generated, the request enters a terminal state. If a correction is needed (e.g., amount error), what is the re-authorization process? Should the request be cancelled and a new one created, or should there be an amendment flow? No stakeholder decision recorded.
- **OQ-N8** (User): **Zero-net balance scenario** — If deductions equal or exceed the approved gross amount, producing a net amount ≤ 0, should the system refuse at approval time or at payment time? The current model requires approvedAmount > 0 but does not explicitly address zero-net. To be specified during planning.
- **OQ-N9** (User): **Payment gateway** — The current payment methods are Cash/Check only (enum). Transfer and InKind are legal under mandate m26 but not in the contract. Should a payment gateway integration be added, or is the current model sufficient? No stakeholder decision recorded.

---

## Sub-Module: PAY-03 — Payment Execution (payment-execution)

> **Amendment note (2026-09-09)**: Each order is paid once; partial payment is not a target capability. The payment amount is the server-computed net amount from the order. The payment links to both the order and the source request.

> **Known legal gap**: Transfer/InKind are legal under mandate m26 but the contract enum is Cash/Check — record as a gap, do not invent.

### User Scenarios

#### US1 (P1) — Record a payment

The cashier executes an approved request — closing the triad.

**Acceptance Scenarios**:

1. **Given** an Approved request with a linked order, **When** the cashier records a payment (paymentMethod + referenceNumber? + notes?; amount is a readonly snapshot of the order net amount), **Then** it becomes Completed with `paymentNumber` (`PAY-{D6}`).
2. **Given** success, **When** the request is opened, **Then** it is Disbursed + `paymentDate`; the order is Paid.
3. **Given** a non-approved request, **When** recording is attempted, **Then** the server refuses verbatim.
4. **Given** an order that has already been paid, **When** another payment is attempted for the same order, **Then** it is refused, including concurrent attempts.

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
- amount ≠ netAmount (impossible — snapshot).

### Functional Requirements

- **FR-001**: Recording exclusively from an Approved request (picker/context).
- **FR-002**: Methods: **Cash/Check only** (enum L34315) — the legal gap is recorded, not invented.
- **FR-003**: `amount` is a snapshot of the order's net amount — readonly in the dialog.
- **FR-004**: Success closes the triad (request Disbursed + order Paid) — shown on the success screen.
- **FR-005**: status: Completed/Failed — failure with its verbatim reason + retry.
- **FR-006**: No PUT/DELETE — single-time recording (immutable).
- **FR-007**: The posting event is raised automatically — its result link (ACC-05) shown on failure.
- **FR-008**: Each order supports at most one completed payment. The server enforces uniqueness against concurrent attempts.

### Data Contract

**PaymentDto** ✓ (L34295):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | ✓ | — |
| paymentNumber | str | ✓ | PAY-{D6} |
| paymentOrderId / paymentOrderNumber | int / str | ✓ | UNIQUE — one payment per order (ADR-001 D-6) |
| disbursementRequestId / disbursementRequestNumber | int / str | ✓ | derived from PaymentOrder.DisbursementRequestId |
| paymentMethod | enum | ✓ | Cash/Check |
| amount | dec | ✓ | order net amount snapshot |
| paidById / paidByName / paidAt | int / str / dt | ✓ | server-issued |
| referenceNumber | str | ○ | check/reference number (optional, code-verified) |
| notes | str | ○ | — |
| status | enum PaymentStatus | ✓ | **Completed/Failed** (L34800) |
| payeeName | str | ○ | display |

**RecordPaymentRequest** ✓ (L35672): `{paymentOrderId!, paymentMethod!, referenceNumber?, notes?}` — **no amount field** (snapshot is server-side); `disbursementRequestId` derived from order.

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
- **BR-5**: Each order is paid once — payment uniqueness enforced against order ID.

### UI/UX

> **Screen-level field/behavior specifications**: see [frontend-requirements.md](./frontend-requirements.md) for complete field definitions, data sources, editability rules, validation, state-based actions, loading/empty/error handling.

- `/payments/payments`: table (number/request/order/method/amount/status/payer/date) + filters (period/method/status).
- Record dialog (from PAY-02 details): readonly amount snapshot + method select + referenceNumber + notes + confirmation.
- Success screen: the closed triad (request/order/payment) with their numbers · failure state: verbatim reason + retry button.
- States: CC-2.

### Success Criteria

- **SC-001**: Recording a payment takes under 30 seconds.
- **SC-002**: 100% of successes close the triad (tested).
- **SC-003**: Every failure shows a verbatim reason.
- **SC-004**: Zero duplicate payments for the same order (including concurrent attempts).

### Tests

- **T1** (FR-001/US1): Approved precondition + refusal otherwise.
- **T2** (FR-003/FR-004): snapshot readonly + triad closure.
- **T3** (FR-002): only the two methods in the picker.
- **T4** (FR-005/US2): Failed with reason + retry.
- **T5** (FR-006/US3): list and filters + no edit buttons.
- **T6** (FR-008): one payment per order enforced + concurrent refusal.

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

> **Screen-level field/behavior specifications**: see [frontend-requirements.md](./frontend-requirements.md) for complete field definitions, data sources, editability rules, validation, state-based actions, loading/empty/error handling.

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
- **FR-G04**: All approval decisions across PAY-01/02 MUST flow through ApprovalHistory + DocumentStatusLog (CC-3). Approval data (amount, issuing authority) MUST NOT be duplicated on entity columns (ADR-001 D-5).
- **FR-G05**: Successful payments MUST raise the accounting event for posting through the existing domain-event → PostingRules pipeline (Constitution II).
- **FR-G06**: The request-first workflow MUST produce atomic final approval + order creation. Failure MUST NOT leave an approved request without its order (RFD-006).
- **FR-G07**: Issuing authority (officeholder name + capacity: General Manager or Finance Director) MUST be validated server-side and stored in ApprovalHistory. Selecting a title MUST NOT grant authority (RFD-003, ADR-001 D-5).
- **FR-G08**: Each order MUST support at most one completed payment. `Payment.PaymentOrderId` MUST be UNIQUE. Concurrent payment attempts MUST be rejected (RFD-013, ADR-001 D-6).
- **FR-G09**: The approved amount MUST NOT exceed the requested amount. Both amounts MUST remain visible in ApprovalHistory (RFD-018, ADR-001 D-5).
- **FR-G10**: Voiding or cancelling an order MUST automatically invalidate associated Draft/PendingApproval requests (RFD-008).
- **FR-G11**: `PaymentOrder.DisbursementRequestId` MUST be UNIQUE — one request produces at most one order (ADR-001 D-2).
- **FR-G12**: Budget check MUST run only at order submit, not at request submit. FundId and AppropriationId are required at submit time (ADR-001 D-3).

### Key Entities *(include if feature involves data)*

- **DisbursementRequest**: the initiator of the request-first workflow; records requester, beneficiary name (string only, ADR-001 D-1), requested amount, currency, purpose, financial year; status (7 states); dual-signature approvals (stored in ApprovalHistory, ADR-001 D-5); at most one linked order via PaymentOrder.DisbursementRequestId UNIQUE (ADR-001 D-2). No availability check at request level (ADR-001 D-3).
- **PaymentOrder**: the official disbursement decision — generated from an approved request (request-first) or created directly (order-first). Header (beneficiary name, fund, fiscal year, appropriation, currency, accountId) + deductions (7 types); status (8 states — single payment) + budgetCheckStatus (4 states) + treasury trace + journal link + optional issuing authority in ApprovalHistory. `vendorId` and `PaymentOrderLine` removed; `AccountId` added (ADR-001 D-4). `DisbursementRequestId` is UNIQUE (ADR-001 D-2).
- **Payment**: the single immutable execution record on an Approved order; Cash/Check; Completed/Failed; closes the request-order-payment triad. `PaymentOrderId` is UNIQUE (ADR-001 D-6); `DisbursementRequestId` derived from order.
- **BankAccount**: the bank reference (identity/branch/financial/controls); unique accountNumber; activate/deactivate; balance display fields fed by BANK-01.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-G01**: Every newly finalized approved request has exactly one linked order with matching approved gross amount, currency, beneficiary and issuing authority.
- **SC-G02**: Zero expenditures occur without passing the availability and budget checks (or a documented override/decision).
- **SC-G03**: 100% of displayed financial numbers are server-issued (verified by a client-computation audit).
- **SC-G04**: Every lifecycle decision (submit/approve/reject/cancel/send-to-treasury/void) is auditable from ApprovalHistory/DocumentStatusLog alone.
- **SC-G05**: Duplicate/concurrent approval and payment verification produces zero duplicate orders or completed payments.
- **SC-G06**: Every rejected request has a visible nonblank rejection reason and no generated order.
- **SC-G07**: Migration preserves existing financial records, links and audit history without fabricating historical facts.

## Assumptions

- The four sub-specs are implemented on the existing PaymentOrder/DisbursementRequest/Payment/BankAccount backend contract; schema changes are additive only (new columns/tables, no destructive edits to applied migrations).
- Permission codes marked GAP-ADD are added in the same feature; enforcement wiring follows the pending RBAC enforcement spec (open placeholder `RequireAssertion(_ => true)` remains until then).
- Two different users may share one qualified role for the dual signature — each signer must individually hold AccountsManager or AuthorizingOfficer (clarified 2026-09-08); verify against SEC-02 rules.
- The Transfer/InKind payment-method legal gap is tracked as a backend-contract request and is NOT invented in the UI.
- Ordering within the group: PAY-04 and PAY-01 (order-first path) can proceed in parallel first; PAY-02 (request-first) then PAY-03 follow the chain.
- The request-first workflow replaces the previous order-first PAY-02 creation flow. Existing order-first PAY-01 creation remains supported for backward compatibility.
- Mandatory reduction reason (when approved amount < requested amount) is a proposal, not yet confirmed by stakeholder — default: no mandatory reason required for reductions.
- Issuing authority on the generated order is resolved from the final signer's identity and declared capacity during planning — not silently assumed to be the last signer.
- Beneficiary is a display name string only — no Party linkage in v1 (ADR-001 D-1). Party linkage added later if needed via separate ADR.
- PaymentOrderLine is removed from target model; orders are header-only with AccountId (ADR-001 D-4). Multi-line support deferred to separate feature if needed.
- Availability check (HasWarning/Blocking) removed from request submit; budget check runs only at order submit (ADR-001 D-3).
