# Data Model: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-09 (amended per ADR-001)

Entities verified against `src/Domain/Payments/Entities/` (as-built). Schema changes per ADR-001: column drops, column adds, unique index creation. No destructive edits to applied migrations. All entities inherit `BaseAuditableEntity` (audit + `RowVersion`).

## PaymentOrder

Official disbursement decision. `src/Domain/Payments/Entities/PaymentOrder.cs`.

| Field | Type | Notes |
|---|---|---|
| Id | int PK | |
| PaymentOrderNumber | string | `PO-{D6}`, unique, server-issued at Draft creation via DocumentSequence |
| PaymentOrderDate / DueDate | DateOnly / DateOnly? | |
| PaymentOrderType | string | server values |
| FundId / FiscalYearId / AppropriationId | int? / int / int? | budget chain — FundId and AppropriationId nullable during Draft on request-first orders; required at submit |
| BudgetClassificationId / CostCenterId / ProjectId | int? | dimensions |
| AccountId | int? | GL account link — optional in Draft, mandatory at Submit (ADR-001 D-4) |
| PurchaseOrderId / EncumbranceId | int? | procurement/encumbrance links |
| CurrencyId | int | 024 |
| ExchangeRate | decimal? | required when currency ≠ base (research D8) |
| AmountGross / DeductionAmount | decimal(23,2) | server-computed; deductions sum to DeductionAmount |
| PaymentMethod | PaymentMethod? | Cash/Check |
| BankAccountId | int? | PAY-04, active only |
| BeneficiaryName | string | required — copied from request (request-first) or supplier (procurement). BeneficiaryPartyId removed (ADR-001 D-1) |
| BeneficiaryIban / BeneficiaryAccountNumber / BeneficiaryBankName | string? | optional |
| Status | PaymentOrderStatus | 8 states below (PartiallyPaid removed — single payment per order) |
| BudgetCheckStatus | BudgetCheckStatus | 4 states below |
| TreasuryStatus / TreasuryReference / TreasurySentAt | string? / string? / DateTimeOffset? | set by SendToTreasury ("Sent", required reference, now) |
| PaidAt | DateTimeOffset? | set by RecordPayment |
| JournalEntryId / AccountingEventId | int? | posting links |
| Notes | string? | |
| IssuingAuthorityName | string? | officeholder name (set on request-first orders; stored in ApprovalHistory, ADR-001 D-5) |
| IssuingAuthorityCapacity | string? | General Manager or Finance Director (set on request-first orders; stored in ApprovalHistory, ADR-001 D-5) |
| DisbursementRequestId | int? | UNIQUE link to source request (request-first path); null on order-first (ADR-001 D-2) |
| RowVersion | byte[] | concurrency token |

**Removed**: `VendorId` (replaced by BeneficiaryName), `BeneficiaryPartyId` (ADR-001 D-1), `PaymentOrderLine` collection (ADR-001 D-4).

**Collections**: `PaymentOrderDeduction` (lineNumber, deductionType enum 7, accountId, amount, deductionPercent?, isMandatory, isTaxDeduction, taxAuthorityId?, referenceNumber?) — retained.

**Status transitions (PaymentOrderStatus)** — 8 states (PartiallyPaid removed):

| From | Event | To | Guard |
|---|---|---|---|
| — | create (order-first) | Draft | appropriationId required; deductions present; accountId optional |
| — | create (request-first) | Draft | generated from approved request; amountGross = approved amount; no lines; issuing authority in ApprovalHistory; fundId/appropriationId null (deferred) |
| Draft | submit | Submitted | fundId + appropriationId + accountId required; budget check runs |
| Draft | cancel | Cancelled | reason required; linked non-terminal requests → Invalidated |
| Submitted | approve | Approved | budgetCheck ≠ Failed (Failed needs override: flag + `PaymentOrders.OverrideBudgetCheck` → Overridden, recorded); approval history + status log written |
| Submitted | reject | Rejected | reason required |
| Approved | send-to-treasury | SentToTreasury | TreasuryReference required |
| Approved/SentToTreasury | void | Voided | unpaid only; linked non-terminal requests → Invalidated |
| SentToTreasury | RecordPayment (PAY-03) | Paid | server (single payment) |

**BudgetCheckStatus**: Pending (at Draft) → Passed/Failed (at submit) → Overridden (at approve-with-override).

## DisbursementRequest

Request-first initiator. `src/Domain/Payments/Entities/DisbursementRequest.cs`.

| Field | Type | Notes |
|---|---|---|
| Id | int PK | |
| RequestNumber | string | `DSB-{D6}`, unique, server-issued |
| RequestedById | int | requester identity |
| BeneficiaryName | string | required — BeneficiaryId removed (ADR-001 D-1) |
| RequestedAmount | decimal(23,2) | original amount requested (positive) |
| CurrencyId | int | 024 |
| Purpose | string | reason for the disbursement |
| FinancialYearId | int | fiscal year |
| RequestDate | DateOnly | |
| Status | DisbursementRequestStatus | 7 states below |
| Notes | string? | |
| PaymentDate | DateTimeOffset? | set by PAY-03 payment completion |
| RowVersion | byte[] | |

**Removed** (per ADR-001):
- `BeneficiaryId` — use BeneficiaryName string only (D-1)
- `HasWarning` — availability check moved to order submit (D-3)
- `PaymentOrderId` / `PaymentOrderNumber` — link derived from PaymentOrder.DisbursementRequestId (D-2)
- `ApprovedAmount` — stored in ApprovalHistory (D-5)
- `IssuingAuthorityName` / `IssuingAuthorityCapacity` — stored in ApprovalHistory (D-5)
- `ApprovalDate` — stored in ApprovalHistory (D-5)

**Status transitions (DisbursementRequestStatus)**:

| From | Event | To | Guard |
|---|---|---|---|
| — | create (independent) | Draft | no order required; positive requestedAmount; beneficiaryName + currencyId + purpose + financialYearId |
| Draft | submit | PendingApproval | pure status transition — no availability check (ADR-001 D-3) |
| PendingApproval | approve (step 1) | PendingApproval | qualified role + approvedAmount ≤ requestedAmount + issuing authority in ApprovalHistory; different user enforced at step 2 |
| PendingApproval | approve (step 2) | Approved + order generated | different user + qualified role + same amount as step 1; atomic: order created (DisbursementRequestId UNIQUE), status log + approval history written |
| PendingApproval | reject | Rejected | nonblank reason required; no order created |
| Draft/PendingApproval | cancel | Cancelled | reason required |
| Approved | cancel | Cancelled | unpaid only (payment existence check); releases order |
| Approved | RecordPayment | Disbursed | server; paymentDate set |
| Draft/PendingApproval/(Approved unpaid) | order cancel/void | Invalidated | automatic |

**Approvals**: rendered from `ApprovalHistory` (DocumentType = "DisbursementRequest") — step, approverUserId, approverName, role, decision, decisionAt, approvedAmount, issuingAuthorityName, issuingAuthorityCapacity. No inline columns on DisbursementRequest (ADR-001 D-5).

## Payment

Single immutable execution record. `src/Domain/Payments/Entities/Payment.cs`.

| Field | Type | Notes |
|---|---|---|
| Id | int PK | |
| PaymentNumber | string | `PAY-{D6}`, unique, server-issued in-transaction |
| PaymentOrderId | int | UNIQUE — one payment per order (ADR-001 D-6) |
| DisbursementRequestId | int | derived from PaymentOrder.DisbursementRequestId |
| PaymentMethod | PaymentMethod | Cash/Check only |
| Amount | decimal(23,2) | server snapshot = order netAmount |
| PaidById / PaidAt | int / DateTimeOffset | server |
| ReferenceNumber / Notes | string? | optional |
| Status | PaymentStatus | Completed (Failed path = Result failure, research D10) |
| RowVersion | byte[] | |

**Rules**: precondition = order Approved; uniqueness = one payment per order (PaymentOrderId UNIQUE index, ADR-001 D-6); immutable (no PUT/DELETE); success raises `PaymentRecordedEvent` → posting pipeline; order → Paid + PaidAt. DisbursementRequestId derived from PaymentOrder — no separate storage needed.

## BankAccount

`src/Domain/Payments/Entities/BankAccount.cs`. 17 contract fields (identity/branch/financial/controls) — see spec. Key rules: AccountNumber unique; IsDefault single flag with auto-switch in the same transaction (D6); OpeningBalance entered at create; CurrentBalance/LastReconciliationDate display-only (BANK-01 feeds); IsActive via activate/deactivate commands (both `{id, rowVersion}`); deactivated accounts excluded from pickers.

## Referenced entities (read-only, other modules)

Party (Vendor), Fund, FiscalYear, BudgetItem/Appropriation, BudgetClassification, CostCenter, Project, PurchaseOrder, Encumbrance, Currency (024), ExchangeRate (024), BankAccount (PAY-04), TaxAuthority, Account (GL), ApprovalHistory (Security), DocumentSequence (FinancialSettings).

## Validation rules summary (enforced server-side)

- Deductions sum to DeductionAmount (±0.01); amountGross > 0; deductionAmount ≥ 0; deduction > gross refused.
- Draft-only edit (UpdatePaymentOrder BR-2 — as-built).
- Mandatory deduction not deletable.
- Every lifecycle action re-validated in handler + RowVersion verified.
- All numbering via DocumentSequence in the same transaction.
- Request-first: requestedAmount > 0; approvedAmount ≤ requestedAmount; approvedAmount > 0.
- Request-first: final approval + order creation atomic (no partial commit).
- Single payment per order: PaymentOrderId UNIQUE enforced (ADR-001 D-6).
- DisbursementRequestId UNIQUE on PaymentOrder (ADR-001 D-2).
- FundId + AppropriationId + AccountId required at submit (ADR-001 D-3/D-4).
- Approval data (amount, issuing authority) in ApprovalHistory only — no duplicate columns (ADR-001 D-5).
