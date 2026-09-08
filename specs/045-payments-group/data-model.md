# Data Model: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-08

Entities verified against `src/Domain/Payments/Entities/` (as-built). No schema changes required by this feature except the idempotent permission seed for `PaymentOrders.OverrideBudgetCheck`. All entities inherit `BaseAuditableEntity` (audit + `RowVersion`).

## PaymentOrder

Official disbursement decision. `src/Domain/Payments/Entities/PaymentOrder.cs`.

| Field | Type | Notes |
|---|---|---|
| Id | int PK | |
| PaymentOrderNumber | string | `PO-{D6}`, unique, server-issued at Draft creation via DocumentSequence |
| PaymentOrderDate / DueDate | DateOnly / DateOnly? | |
| PaymentOrderType | string | server values |
| VendorId (+ VendorPartyId?) | int / int? | Party of type Vendor |
| FundId / FiscalYearId / AppropriationId | int | budget chain — AppropriationId required |
| BudgetClassificationId / CostCenterId / ProjectId | int? | dimensions |
| PurchaseOrderId / EncumbranceId | int? | procurement/encumbrance links |
| CurrencyId | int | 024 |
| ExchangeRate | decimal? | required when currency ≠ base (research D8) |
| AmountGross / DeductionAmount | decimal(23,2) | server-computed; lines sum to net, deductions sum to DeductionAmount (±0.01) |
| PaymentMethod | PaymentMethod? | Cash/Check |
| BankAccountId | int? | PAY-04, active only |
| BeneficiaryName (+Iban/AccountNumber/BankName?) | string | |
| Status | PaymentOrderStatus | 9 states below |
| BudgetCheckStatus | BudgetCheckStatus | 4 states below |
| TreasuryStatus / TreasuryReference / TreasurySentAt | string? / string? / DateTimeOffset? | set by SendToTreasury ("Sent", required reference, now) |
| PaidAt | DateTimeOffset? | set by RecordPayment |
| JournalEntryId / AccountingEventId | int? | posting links |
| Notes | string? | |
| RowVersion | byte[] | concurrency token |

**Collections**: `PaymentOrderLine` (lineNumber, lineType enum 5, accountId, amount, taxAmount?, dimension optionals), `PaymentOrderDeduction` (lineNumber, deductionType enum 7, accountId, amount, deductionPercent?, isMandatory, isTaxDeduction, taxAuthorityId?, referenceNumber?).

**Status transitions (PaymentOrderStatus)**:

| From | Event | To | Guard |
|---|---|---|---|
| — | create | Draft | appropriationId required; lines net = gross − deductions; deductions sum = DeductionAmount |
| Draft | submit | Submitted | ≥1 line; budget check runs (D1) → Passed or Failed (Failed still submits? NO — submit fails with Failed status persisted) |
| Draft/Submitted | cancel | Cancelled | reason required; linked non-terminal requests → Invalidated (D4) |
| Submitted | approve | Approved | budgetCheck ≠ Failed (Failed needs override: flag + `PaymentOrders.OverrideBudgetCheck` → Overridden, recorded — D2); approval rules evaluated; approval history + status log written |
| Submitted | reject | Rejected | reason required |
| Approved | send-to-treasury | SentToTreasury | TreasuryReference required |
| Approved/SentToTreasury | void | Voided | unpaid only (research D3); linked non-terminal requests → Invalidated (D4) |
| SentToTreasury | RecordPayment (PAY-03) | Paid | server |

**BudgetCheckStatus**: Pending (at Draft) → Passed/Failed (at submit) → Overridden (at approve-with-override).

## DisbursementRequest

Execution trigger, 1:1 with order. `src/Domain/Payments/Entities/DisbursementRequest.cs`.

| Field | Type | Notes |
|---|---|---|
| Id | int PK | |
| RequestNumber | string | `DSB-{D6}`, unique, server-issued |
| PaymentOrderId | int | 1:1 unique (0 after release on cancel — as-built release mechanism) |
| RequestedById | int | |
| RequestDate | DateOnly | |
| Status | DisbursementRequestStatus | 7 states below |
| HasWarning | bool | availability Warning (not Blocking) at create/submit |
| Notes | string? | |
| RowVersion | byte[] | |

**Status transitions (DisbursementRequestStatus)**:

| From | Event | To | Guard |
|---|---|---|---|
| — | create (on Approved order without request) | Draft | 1:1 unique; requestedAmount = order netTotal snapshot; availability Warning → HasWarning |
| Draft | submit | PendingApproval | availability Blocking ⇒ refused (BudgetAvailabilityService) |
| PendingApproval | approve (step 1) | PendingApproval | qualified role + history row; different user enforced at step 2 |
| PendingApproval | approve (step 2) | Approved | different user + qualified role (D5); approvalDate set |
| PendingApproval | reject | Rejected | reason required |
| Draft/PendingApproval | cancel | Cancelled | reason required; releases order (PaymentOrderId = 0) |
| Approved | cancel | Cancelled | unpaid only (payment existence check); releases order |
| Approved | RecordPayment | Disbursed | server; paymentDate set |
| Draft/PendingApproval/(Approved unpaid) | order cancel/void | Invalidated | automatic (D4) |

**Approvals**: rendered from `ApprovalHistory` (DocumentType = "DisbursementRequest") — step, approverUserId, approverName, role, decision, decisionAt. No inline columns.

## Payment

Single immutable execution record. `src/Domain/Payments/Entities/Payment.cs`.

| Field | Type | Notes |
|---|---|---|
| Id | int PK | |
| PaymentNumber | string | `PAY-{D6}`, unique, server-issued in-transaction |
| DisbursementRequestId / PaymentOrderId | int | |
| PaymentMethod | PaymentMethod | Cash/Check only |
| Amount | decimal(23,2) | server snapshot = order netTotal (research D7) |
| PaidById / PaidAt | int / DateTimeOffset | server |
| ReferenceNumber / Notes | string? | optional |
| Status | PaymentStatus | Completed (Failed path = Result failure, research D10) |
| RowVersion | byte[] | |

**Rules**: precondition = request Approved; uniqueness = one payment per request (query guard + DB unique index on DisbursementRequestId); immutable (no PUT/DELETE); success raises `PaymentRecordedEvent` → posting pipeline; order → Paid + PaidAt.

## BankAccount

`src/Domain/Payments/Entities/BankAccount.cs`. 17 contract fields (identity/branch/financial/controls) — see spec. Key rules: AccountNumber unique; IsDefault single flag with auto-switch in the same transaction (D6); OpeningBalance entered at create; CurrentBalance/LastReconciliationDate display-only (BANK-01 feeds); IsActive via activate/deactivate commands (both `{id, rowVersion}`); deactivated accounts excluded from pickers.

## Referenced entities (read-only, other modules)

Party (Vendor), Fund, FiscalYear, BudgetItem/Appropriation, BudgetClassification, CostCenter, Project, PurchaseOrder, Encumbrance, Currency (024), ExchangeRate (024), BankAccount (PAY-04), TaxAuthority, Account (GL), ApprovalHistory (Security), DocumentSequence (FinancialSettings).

## Validation rules summary (enforced server-side)

- Lines sum to net (±0.01); deductions sum to DeductionAmount (±0.01); amountGross > 0; deductionAmount ≥ 0; deduction > gross refused.
- Draft-only edit (UpdatePaymentOrder BR-2 — as-built).
- Mandatory deduction not deletable.
- Every lifecycle action re-validated in handler + RowVersion verified.
- All numbering via DocumentSequence in the same transaction.
