# Research: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-09

## R1: Dual-Signature Approval Pattern

**Decision**: Use existing ApprovalHistory entity with step-number sequencing. Both approvers must hold AccountsManager or AuthorizingOfficer role. Second approver must be a distinct user. Both must authorize the same approved amount. Status transitions from Draft → PendingApproval → Approved only when ≥2 approval records exist with the required role composition and matching amounts.

**Rationale**: The ApprovalHistory entity already supports step-numbered, multi-approver workflows. Adding dual-signature checks (count, distinct users, role validation, amount matching) is handler logic — no new entity or workflow engine needed.

**Alternatives considered**:
- New DisbursementApproval entity: rejected — duplicates ApprovalHistory structure.
- IApprovalRuleEvaluationService with two-phase rules: rejected — that service evaluates document-type/amount-threshold rules, not dual-signature counting.

## R2: Request-First Workflow

**Decision**: DisbursementRequest initiates the workflow. PaymentOrder is auto-generated atomically upon second approval. The order starts as a Draft with FundId=0 and AppropriationId=null. A user with PaymentOrdersUpdate permission prepares the order (Fund, Appropriation, account, deductions) before submission.

**Rationale**: Matches the as-built codebase. The request-first flow separates "authorization to disburse" (request approval) from "financial preparation" (order setup). Deferred FundId allows batch preparation of orders.

**Alternatives considered**:
- Order-first (create order → disburse): rejected — original spec 018 design; superseded by request-first amendment in spec 045.

## R3: Amount Freeze After First Approval

**Decision**: Once the first approval is recorded (PendingApproval with ≥1 approval), the requested amount is frozen. Only notes and purpose may be updated. The first approved amount becomes the binding amount for the second signature. Edits to requested amount are rejected.

**Rationale**: Prevents silent amount drift between signatures. If the first approver authorizes amount X, the second approver must also authorize amount X. Editing the requested amount between signatures would create a mismatch between what was agreed upon.

**Alternatives considered**:
- Allow amount edits between approvals: rejected — second approver might see different amount than first authorized.
- Require re-approval from first approver if amount changes: rejected — adds complexity; amount-freeze is simpler and more secure.

## R4: FundId/AppropriationId Deferred to Order Preparation

**Decision**: PaymentOrder auto-generated from approval starts with FundId=0 and AppropriationId=null. These are set during order preparation (UpdatePaymentOrder) before submission. Budget check runs at submission after FundId/AppropriationId are set.

**Rationale**: Matches existing code behavior (ApproveDisbursementRequestCommand sets FundId=0). Allows accountants to prepare orders in batches — create multiple approved requests, then prepare orders later.

**Alternatives considered**:
- Copy FundId from request: rejected — request doesn't have FundId (it's a standalone request, not linked to a specific fund until order preparation).
- Require FundId at request creation: rejected — request is a generic disbursement intent; fund assignment is an order-level decision.

## R5: Edit Permission for Draft Requests

**Decision**: Any user with DisbursementRequestsUpdate permission can edit any draft request. Ownership is not a constraint. The permission is role-based, matching the existing RBAC model and PaymentOrder editing behavior (any PaymentOrdersUpdate holder can edit any draft order).

**Rationale**: Consistent with existing permission model. No ownership-based restrictions on draft editing anywhere in the codebase. Simpler to implement and maintain.

**Alternatives considered**:
- Only original requester: rejected — too restrictive; someone else may need to correct a draft.
- DisbursementRequestsCreate permission: rejected — conflates creation and editing; separate permissions provide finer-grained control.

## R6: Cancel Permission

**Decision**: DisbursementRequestsCancel — the dedicated cancel permission code, already defined in PermissionCodes.cs and bound to the cancel endpoint.

**Rationale**: Clean separation of duties. Each lifecycle operation (view, create, update, submit, approve, reject, cancel) has its own permission code.

**Alternatives considered**:
- DisbursementRequestsReject: rejected — reject and cancel are different operations (reject stops a pending request; cancel stops any non-paid request).
- DisbursementRequestsUpdate: rejected — too broad; cancel is a destructive action that deserves its own permission.

## R7: Document Numbering Sequence

**Decision**: Use DocumentSequenceService with prefix "DSB" for DisbursementRequest. PaymentOrder uses "PO". Payment uses "PAY". Numbers assigned at entity creation (pre-printed-book style).

**Rationale**: Follows established pattern (BGT/APR/ENC prefixes for budgeting). Sequence service handles atomic increment.

**Alternatives considered**:
- Sequential int IDs as document numbers: rejected — not human-readable, no prefix for type identification.

## R8: Budget Availability Check Integration

**Decision**: Budget check runs at order submission (not request creation). BudgetAvailabilityService evaluates the budget item's control method (None/Warning/Blocking) and available funds. On Blocking+insufficient: reject with breakdown. On Warning+insufficient: allow with warning flag. On None: skip check.

**Rationale**: Request approval gates creation; budget check gates order approval. This separation allows the request to proceed without budget dimensions (Fund/Appropriation are deferred).

**Alternatives considered**:
- Check at request creation: rejected — request doesn't have FundId/AppropriationId yet; budget check requires these dimensions.
- Check at approval time: rejected — spec explicitly gates at order submission.

## R9: Ledger Posting for Disbursements

**Decision**: Payment execution raises a domain event (PaymentRecordedEvent). The AccountingEvent pipeline picks it up and creates a balanced JournalEntry: Dr Party/Liability (vendor payable), Cr Cash/Bank (payment account). Follows existing posting pattern.

**Rationale**: Constitution Principle II requires cross-module financial effects via domain events. The posting pipeline is the established integration pattern.

**Alternatives considered**:
- Direct GL write in Payment handler: rejected — violates Principle II and Registered Exception #2.

## R10: Concurrency Model

**Decision**: Point-in-time budget check with optimistic concurrency (RowVersion). No serialized reservation. The unique FK on DisbursementRequestId prevents duplicate orders per request. Budget overruns from concurrent orders are caught by the Blocking control method.

**Rationale**: Matches existing pattern for Appropriations and Encumbrances. Optimistic concurrency via RowVersion is the standard across the codebase.

**Alternatives considered**:
- Distributed lock on budget item: rejected — over-engineered for government ERP transaction volume.
- Reservation pattern (decrement available on request creation): rejected — changes BudgetAvailabilityService semantics.

## R11: Role Check on Second Approval

**Decision**: Both the first AND second approver must hold AccountsManager or AuthorizingOfficer role. The role check applies to every approval step, not just the first.

**Rationale**: Clarified during spec session 2026-09-08 (PAY-02 contract alignment). Both signatures represent authorization; both signers must be qualified. The current code only checks role on step 1 — step 2 must add the role check.

**Alternatives considered**:
- Only first approver needs role: rejected — per spec 045 clarification, both steps require qualified role.
- Any user can be second approver: rejected — same reasoning; both signatures represent authorization.
