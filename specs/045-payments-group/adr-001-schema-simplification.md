# ADR-001: Payments Group Schema Simplification

**Branch**: `045-payments-group` | **Date**: 2026-09-09 | **Status**: Accepted

> **Partial supersession (2026-09-09)**: D-1 and its name-only consequences below are historical, superseded by [ADR-002](adr-002-beneficiary-party-link.md) after explicit stakeholder clarification. The current target requires `BeneficiaryName` plus optional `BeneficiaryPartyId`. D-2 through D-6 remain in force.

## Context

The Payments Group spec (045) accumulated six structural conflicts during iterative refinement. These conflicts arise from over-engineering the data model relative to the actual workflow:

1. BeneficiaryPartyId on both DisbursementRequest and PaymentOrder adds complexity without value — the beneficiary is always a name string; Party linkage is optional and independent.
2. PaymentOrderId stored on DisbursementRequest creates a bidirectional dependency; the natural link is DisbursementRequestId on PaymentOrder.
3. Budget check and HasWarning on DisbursementRequest premature — the request captures intent; budget controls apply at order submit.
4. PaymentOrderLine on the target model contradicts the header-only request-first model and the single-account design.
5. Approval columns (ApprovedAmount, IssuingAuthorityName, IssuingAuthorityCapacity, ApprovalDate) duplicated on DisbursementRequest and ApprovalHistory violates CC-3.
6. Payment links to DisbursementRequest via PaymentOrderId — the request link should be derived, not stored.

## Decision

### D-1: BeneficiaryName only (no Party ID)

- **DisbursementRequest**: remove `BeneficiaryId`. Keep `BeneficiaryName` (string, required).
- **PaymentOrder**: remove `BeneficiaryPartyId`. Keep `BeneficiaryName` (string, required).
- Beneficiary is always a display name. Party linkage (if needed later) is a separate concern.

### D-2: DisbursementRequestId UNIQUE on PaymentOrder

- **PaymentOrder**: `DisbursementRequestId` becomes `int?` with a UNIQUE constraint. Set on request-first orders; null on order-first orders.
- **DisbursementRequest**: remove `PaymentOrderId` and `PaymentOrderNumber`. The link is derived from PaymentOrder.DisbursementRequestId.
- One request → at most one order (enforced by UNIQUE). One order → at most one request (nullable FK).

### D-3: Budget check at order submit only

- **DisbursementRequest**: remove `HasWarning` field. Remove availability check from request submit lifecycle.
- Budget check runs only at `SubmitPaymentOrderCommand` (order submit), after FundId and AppropriationId are filled.
- Request submit becomes a pure status transition (Draft → PendingApproval) without budget validation.

### D-4: Remove PaymentOrderLine, add AccountId

- **PaymentOrder**: remove `PaymentOrderLine` collection from the target model. Add `AccountId` (int?, GL account link).
- `AccountId` is optional during Draft, mandatory at Submit.
- Orders from request-first path are header-only (no lines). Order-first path with multi-line support is deferred to a separate feature if needed.
- `PaymentOrderDeduction` collection is retained (deductions are header-level, not line-level).

### D-5: Approval data in ApprovalHistory only

- **DisbursementRequest**: remove `ApprovedAmount`, `IssuingAuthorityName`, `IssuingAuthorityCapacity`, `ApprovalDate`.
- All approval decisions, amounts, and issuing authority are stored in `ApprovalHistory` rows (DocumentType = "DisbursementRequest").
- `PaymentDate` remains on DisbursementRequest (set by PAY-03 payment completion).

### D-6: Payment links via PaymentOrderId UNIQUE

- **Payment**: `PaymentOrderId` is the primary link to the order. Add UNIQUE constraint on PaymentOrderId (one payment per order).
- **Payment**: `DisbursementRequestId` is derived from `PaymentOrder.DisbursementRequestId` (no separate storage needed, or kept as a denormalized read field).
- The triad closure: Payment records against PaymentOrder; PaymentOrder traces back to DisbursementRequest.

## Consequences

- **Breaking changes**: PaymentOrderDto loses BeneficiaryPartyId, gains AccountId. DisbursementRequestDto loses BeneficiaryId, HasWarning, PaymentOrderId, PaymentOrderNumber, ApprovedAmount, IssuingAuthorityName, IssuingAuthorityCapacity, ApprovalDate. PaymentDto gains PaymentOrderId UNIQUE.
- **Migration**: EF migration required — column drops, column adds, unique index creation.
- **OpenAPI regeneration**: required after all changes (`npm run generate-api`).
- **Constitution IX**: breaking changes require this ADR (decision record before implementation).
- **Constitution XII**: controlled change — this ADR documents the rationale, scope, and migration.

## Alternatives Considered

- Keeping BeneficiaryPartyId for future Party linkage (rejected — YAGNI; add when needed with a separate ADR).
- Storing PaymentOrderId on DisbursementRequest (rejected — creates bidirectional dependency; the natural owner is PaymentOrder).
- Moving availability check to request submit (rejected — premature; budget controls belong at order level per Constitution V).
- Keeping PaymentOrderLine for order-first path (rejected — over-engineering; header-only model is sufficient for the target workflow).
