# Research: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-05

## R1: Dual-Signature Approval Pattern

**Decision**: Use existing ApprovalHistory entity with step-number sequencing. First approver must hold AccountsManager or AuthorizingOfficer role. Second approver must be a distinct user (any role). Status transitions from Draft → Pending Approval → Approved only when ≥2 approval records exist with the required role composition.

**Rationale**: The ApprovalHistory entity already supports step-numbered, multi-approver workflows. Adding a dual-signature check is a matter of querying ApprovalHistory for count/distinct/role — no new entity or workflow engine needed.

**Alternatives considered**:
- New DisbursementApproval entity: rejected — duplicates ApprovalHistory structure.
- IApprovalRuleEvaluationService with two-phase rules: rejected — that service evaluates document-type/amount-threshold rules, not dual-signature counting.

## R2: Budget Availability Check Integration

**Decision**: Call BudgetAvailabilityService.GetAvailabilitySummaryAsync(paymentOrder.AppropriationId → BudgetItemId) at request creation. Evaluate control method via EvaluateControlMethod. On Blocking+insufficient: reject with breakdown. On Warning+insufficient: allow with warning flag. On None: skip check.

**Rationale**: BudgetAvailabilityService already computes item-level availability (net appropriated − encumbered). The disbursement request consumes this without modification.

**Alternatives considered**:
- New DisbursementAvailabilityService: rejected — unnecessary abstraction over existing service.
- Check at approval time instead of creation: rejected — spec explicitly gates at creation.

## R3: Document Numbering Sequence

**Decision**: Use DocumentSequenceService with prefix "DSB" for DisbursementRequest and "PAY" for Payment. Number assigned at Draft creation (pre-printed-book style).

**Rationale**: Follows established pattern (BGT/APR/ENC prefixes for budgeting). Sequence service handles atomic increment.

**Alternatives considered**:
- Sequential int IDs as document numbers: rejected — not human-readable, no prefix for type identification.

## R4: Payment Order Status Transition

**Decision**: When a DisbursementRequest is created against an Approved PaymentOrder, the PaymentOrder status does NOT change (remains Approved). The 1:1 constraint via unique FK prevents duplicate requests. When payment is executed, PaymentOrder transitions to Paid directly.

**Rationale**: The spec says "the payment order's status transitions to indicate a disbursement is in progress" but the user input schema decisions show no intermediate status on PaymentOrder. The existing PaymentOrderStatus enum has no "DisbursementInProgress" value. Adding one would require changing the enum and all code that checks status. Instead, the 1:1 unique FK is sufficient guard — if a DisbursementRequest exists for a PaymentOrder, no second can be created.

**Alternatives considered**:
- Add "DisbursementInProgress" status to PaymentOrder: rejected — requires enum change, migration, and updating all existing status checks. The unique FK achieves the same guard without status pollution.

## R5: Ledger Posting for Disbursements

**Decision**: Payment execution raises a domain event. The AccountingEvent pipeline picks it up and creates a balanced JournalEntry: Dr Party/Liability (vendor payable), Cr Cash/Bank (payment account). Follows existing posting pattern from treasury spec (017) and budgeting spec (015).

**Rationale**: Constitution Principle II requires cross-module financial effects via domain events. The posting pipeline is the established integration pattern.

**Alternatives considered**:
- Direct GL write in Payment handler: rejected — violates Principle II and Registered Exception #2.

## R6: Cancellation Authorization

**Decision**: Any user with the disbursement permission can cancel an approved-but-unpaid DisbursementRequest. Cancellation is recorded in ApprovalHistory with Action=Cancel.

**Rationale**: User clarification during spec phase. Cancellation is less sensitive than approval — it stops money from moving, not starts it.

**Alternatives considered**:
- Only AccountsManager/AuthorizingOfficer: more restrictive but not what the user specified.
- Only the original requester: too restrictive for operational flexibility.

## R7: Concurrency Model

**Decision**: Point-in-time budget check with optimistic concurrency. No serialized reservation. The unique FK on PaymentOrderId prevents duplicate requests per payment order. Budget overruns from concurrent requests are caught by the Blocking control method.

**Rationale**: Matches existing pattern for Appropriations and Encumbrances. Optimistic concurrency via RowVersion is the standard across the codebase.

**Alternatives considered**:
- Distributed lock on budget item: rejected — over-engineered for government ERP transaction volume.
- Reservation pattern (decrement available on request creation): rejected — changes BudgetAvailabilityService semantics.
