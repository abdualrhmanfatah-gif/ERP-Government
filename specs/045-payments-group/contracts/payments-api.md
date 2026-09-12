# HTTP Contract Deltas: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-09 (amended per ADR-001)

The backend-generated OpenAPI document is the SSOT (Constitution IX). This file documents **behavior deltas** against the as-built endpoints — including breaking changes per ADR-001. Frontend consumes via the NSwag client regenerated after implementation.

## Breaking Changes (ADR-001)

| Change | ADR Section | Impact |
|---|---|---|
| BeneficiaryPartyId removed from PaymentOrder | D-1 | Response schema: remove `beneficiaryPartyId` |
| BeneficiaryId removed from DisbursementRequest | D-1 | Request/response schema: remove `beneficiaryId` |
| PaymentOrderId removed from DisbursementRequest | D-2 | Response schema: remove `paymentOrderId`, `paymentOrderNumber` |
| DisbursementRequestId UNIQUE on PaymentOrder | D-2 | Response schema: `disbursementRequestId` is UNIQUE |
| HasWarning removed from DisbursementRequest | D-3 | Response schema: remove `hasWarning` |
| PaymentOrderLine removed | D-4 | Response schema: remove `lines[]` |
| AccountId added to PaymentOrder | D-4 | Request/response schema: add `accountId` |
| ApprovedAmount removed from DisbursementRequest | D-5 | Response schema: remove `approvedAmount` |
| IssuingAuthority removed from DisbursementRequest | D-5 | Response schema: remove `issuingAuthorityName`, `issuingAuthorityCapacity` |
| ApprovalDate removed from DisbursementRequest | D-5 | Response schema: remove `approvalDate` |
| PaymentOrderId UNIQUE on Payment | D-6 | Response schema: `paymentOrderId` is UNIQUE |
| RecordPaymentRequest: paymentOrderId instead of disbursementRequestId | D-6 | Request schema change |

## As-built endpoints (unchanged routes)

### /api/PaymentOrders

| Method | Path | Delta |
|---|---|---|
| GET | / | **breaking**: filters change — remove `vendor`; response loses `beneficiaryPartyId`, gains `accountId` |
| GET | /{id} | **breaking**: response loses `beneficiaryPartyId`, `lines[]`; gains `accountId` |
| GET | /{id}/totals | unchanged route; `paidAmount`/`remainingAmount`/`isFullyPaid` reflect single-payment state |
| POST | / | **breaking**: request gains `accountId`; response loses `beneficiaryPartyId`; `fundId`/`appropriationId` nullable during Draft on request-first path |
| POST | /{id}/submit | **behavior**: budget check via `BudgetAvailabilityService` (appropriation-level); Failed persists `BudgetCheckStatus=Failed`; **validation**: `fundId` + `appropriationId` + `accountId` required at submit |
| POST | /{id}/approve | **behavior + request field**: new optional `overrideFailedBudgetCheck: boolean` |
| POST | /{id}/reject | unchanged (reason required) |
| POST | /{id}/cancel | **behavior**: cancels + auto-invalidates linked non-terminal requests |
| POST | /{id}/send-to-treasury | unchanged (`TreasuryReference` required) |
| POST | /{id}/void | **behavior**: guard = Approved/SentToTreasury + unpaid; auto-invalidates linked requests |
| PUT | /{id} | unchanged (Draft-only) |

### /api/DisbursementRequests

| Method | Path | Delta |
|---|---|---|
| GET | / | **breaking**: response loses `beneficiaryId`, `hasWarning`, `paymentOrderId`, `paymentOrderNumber`, `approvedAmount`, `issuingAuthorityName`, `issuingAuthorityCapacity`, `approvalDate` |
| GET | /{id} | **breaking**: same field removals; `approvals[]` from ApprovalHistory retains approvedAmount/issuingAuthority per step |
| POST | / | **breaking**: request loses `beneficiaryId`; uses `beneficiaryName` only |
| PATCH | /{id}/submit | **behavior**: pure status transition — no availability check (ADR-001 D-3) |
| PATCH | /{id}/approve | **behavior**: approval data stored in ApprovalHistory only (ADR-001 D-5) |
| PATCH | /{id}/reject | unchanged (reason required) |
| PATCH | /{id}/cancel | unchanged (reason required; releases order) |

### /api/Payments

| Method | Path | Delta |
|---|---|---|
| GET | / | **breaking**: response loses `disbursementRequestId` as independent field; gains `paymentOrderId` UNIQUE |
| GET | /{id} | **breaking**: same |
| POST | / | **breaking**: request uses `paymentOrderId!` instead of `disbursementRequestId!`; `disbursementRequestId` derived from order |

### /api/BankAccounts

| Method | Path | Delta |
|---|---|---|
| GET | / | unchanged |
| GET | /{id} | unchanged |
| POST | / | **behavior**: IsDefault=true auto-demotes previous default in same transaction |
| PUT | /{id} | **behavior**: same auto-switch |
| POST | /{id}/activate | unchanged |
| POST | /{id}/deactivate | unchanged |

## Contract detail — approve-with-override

`POST /api/PaymentOrders/{id}/approve`

```jsonc
// Request (extended — additive, non-breaking)
{
  "id": 1,
  "reason": "string?",
  "rowVersion": "base64",
  "overrideFailedBudgetCheck": false   // NEW, optional, default false
}
```

- `BudgetCheckStatus == Failed` && `overrideFailedBudgetCheck == false` → 400 problem-details, verbatim message "Cannot approve payment order with failed budget check."
- `Failed` + flag + caller lacks `PaymentOrders.OverrideBudgetCheck` → 403 problem-details.
- `Failed` + flag + permission held → approved; `BudgetCheckStatus = Overridden`; ApprovalHistory + DocumentStatusLog rows written with snapshot (actor, decision, reason, check detail).

## Contract detail — RecordPaymentRequest (ADR-001 D-6)

`POST /api/Payments/Payments`

```jsonc
// Request (breaking — disbursementRequestId replaced by paymentOrderId)
{
  "paymentOrderId": 1,          // REQUIRED — was disbursementRequestId
  "paymentMethod": "Cash",      // enum: Cash/Check
  "referenceNumber": "string?",  // optional
  "notes": "string?"             // optional
  // amount is server-computed snapshot — no field
}
```

- `disbursementRequestId` is derived from `PaymentOrder.DisbursementRequestId` — not in the request.

## Contract detail — error semantics (Constitution IX)

400 validation / 401 unauthenticated / 403 forbidden / 404 not found / concurrency conflict surfaced distinctly (RowVersion). All business-rule refusals are `Result.Failure` problem-details with the verbatim server message (CC-2).

## New permission code

`PaymentOrders.OverrideBudgetCheck` — added to `PermissionCodes`, policy registration, idempotent permission seed. Frontend navigation entry for the override affordance carries the same identifier (Constitution X).

## Regeneration

After implementation: `cd src/Web/ClientApp && npm run generate-api` (NSwag client) — required because of breaking schema changes per ADR-001.
