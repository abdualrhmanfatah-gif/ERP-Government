# Data Model: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-09

## Enums (existing)

### DisbursementRequestStatus

```csharp
public enum DisbursementRequestStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4,
    Disbursed = 5,
    Invalidated = 6
}
```

### PaymentOrderStatus

```csharp
public enum PaymentOrderStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    SentToTreasury = 3,
    Paid = 4,
    Cancelled = 5,
    Rejected = 6,
    Voided = 7
}
```

### PaymentStatus

```csharp
public enum PaymentStatus
{
    Completed = 0,
    Failed = 1
}
```

### BudgetCheckStatus

```csharp
public enum BudgetCheckStatus
{
    Pending = 0,
    Passed = 1,
    Failed = 2,
    Overridden = 3
}
```

## Entities

### DisbursementRequest

Inherits `BaseAuditableEntity` (int PK, audit fields, RowVersion).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Inherited |
| RequestNumber | string | UNIQUE, required, max 20 | DSB-{D6} assigned at Draft creation |
| RequestedById | int | FK required | User who created the request |
| RequestedByName | string | required, max 200 | Snapshot of requester login |
| BeneficiaryName | string | required, max 200 | Payee name |
| RequestedAmount | decimal(23,2) | required, > 0 | Amount requested |
| CurrencyId | int | FK required | References Currency |
| Purpose | string | required, max 500 | Purpose of disbursement |
| FinancialYearId | int | FK required | References FiscalYear |
| RequestDate | DateOnly | required | Date request was created |
| Status | DisbursementRequestStatus | required, default Draft | Lifecycle state |
| Notes | string? | optional, max 500 | Free-text notes |
| PaymentDate | DateTimeOffset? | nullable | Set when payment is executed |
| RowVersion | byte[] | required | Optimistic concurrency token |

**Note**: Per ADR-001 D-2, DisbursementRequest does NOT have a PaymentOrderId. The link is one-directional: PaymentOrder.DisbursementRequestId UNIQUE points to the request.

**Indexes**:
- `IX_DisbursementRequests_RequestNumber` UNIQUE — document number uniqueness
- `IX_DisbursementRequests_Status` — filtered queries
- `IX_DisbursementRequests_RequestedById` — query by requester
- `IX_DisbursementRequests_CurrencyId` — query by currency
- `IX_DisbursementRequests_FinancialYearId` — query by fiscal year

**Relationships**:
- ← PaymentOrder (0..1, via PaymentOrder.DisbursementRequestId UNIQUE)
- ← ApprovalHistory (0..n, via DocumentType="DisbursementRequest")
- ← Payment (0..1, via Payment.DisbursementRequestId)

### PaymentOrder

Inherits `BaseAuditableEntity` (int PK, audit fields, RowVersion).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Inherited |
| PaymentOrderNumber | string | UNIQUE, required, max 20 | PO-{D6} assigned at creation |
| PaymentOrderDate | DateOnly | required | Date order was created |
| DueDate | DateOnly? | optional | Payment due date |
| PaymentOrderType | string | required, max 50 | "Disbursement" for request-first |
| FundId | int | required | Set during preparation (0 at auto-gen) |
| FiscalYearId | int | FK required | Copied from request |
| AppropriationId | int? | nullable | Set during preparation |
| BudgetClassificationId | int? | nullable | Set during preparation |
| CostCenterId | int? | nullable | Set during preparation |
| ProjectId | int? | nullable | Set during preparation |
| AccountId | int? | nullable | Set during preparation |
| PurchaseOrderId | int? | nullable | Optional source reference |
| EncumbranceId | int? | nullable | Optional encumbrance link |
| CurrencyId | int | FK required | Copied from request |
| ExchangeRate | decimal? | nullable | Exchange rate if foreign currency |
| AmountGross | decimal(23,2) | required | Approved amount from request |
| DeductionAmount | decimal(23,2) | required, default 0 | Sum of deduction line items |
| PaymentMethod | PaymentMethod? | nullable | Cash or Check |
| BankAccountId | int? | nullable | Bank account for payment |
| BeneficiaryName | string | required, max 200 | Copied from request |
| BeneficiaryIban | string? | optional, max 50 | IBAN |
| BeneficiaryAccountNumber | string? | optional, max 50 | Account number |
| BeneficiaryBankName | string? | optional, max 100 | Bank name |
| Status | PaymentOrderStatus | required, default Draft | Lifecycle state |
| BudgetCheckStatus | BudgetCheckStatus | required, default Pending | Budget check result |
| TreasuryStatus | string? | optional | Treasury processing status |
| TreasuryReference | string? | optional, max 100 | Treasury reference number |
| TreasurySentAt | DateTimeOffset? | nullable | When sent to treasury |
| PaidAt | DateTimeOffset? | nullable | When payment executed |
| JournalEntryId | int? | nullable | Posted journal entry |
| AccountingEventId | int? | nullable | Source accounting event |
| Notes | string? | optional, max 500 | Free-text notes |
| DisbursementRequestId | int? | nullable, UNIQUE | FK to DisbursementRequest (1:1) |
| IssuingAuthorityName | string? | optional, max 200 | Officeholder name |
| IssuingAuthorityCapacity | string? | optional, max 100 | Officeholder title |
| RowVersion | byte[] | required | Optimistic concurrency token |

**Indexes**:
- `IX_PaymentOrders_PaymentOrderNumber` UNIQUE
- `IX_PaymentOrders_DisbursementRequestId` UNIQUE — enforces 1:1 constraint
- `IX_PaymentOrders_Status` — filtered queries
- `IX_PaymentOrders_FundId` — query by fund
- `IX_PaymentOrders_FiscalYearId` — query by fiscal year

**Relationships**:
- → DisbursementRequest (0..1, optional FK)
- ← PaymentOrderDeduction (0..n)
- ← Payment (0..1)

### Payment

Inherits `BaseAuditableEntity` (int PK, audit fields, RowVersion).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Inherited |
| PaymentNumber | string | UNIQUE, required, max 20 | PAY-{D6} assigned at execution |
| PaymentOrderId | int | FK UNIQUE, required | 1:1 with PaymentOrder |
| DisbursementRequestId | int | FK required | Copied from PaymentOrder |
| PaymentMethod | PaymentMethod | required | Cash or Check |
| Amount | decimal(23,2) | required | Server-computed: AmountGross - DeductionAmount |
| PaidById | int | FK required | User who executed the payment |
| PaidByName | string | required, max 200 | Snapshot of executor login |
| PaidAt | DateTimeOffset | required | Execution timestamp |
| ReferenceNumber | string? | optional, max 100 | External reference |
| Notes | string? | optional, max 500 | Free-text notes |
| Status | PaymentStatus | required, default Completed | Payment outcome |
| PayeeName | string? | optional, max 200 | Payee (denormalized from order) |
| RowVersion | byte[] | required | Optimistic concurrency token |

**Indexes**:
- `IX_Payments_PaymentNumber` UNIQUE
- `IX_Payments_PaymentOrderId` UNIQUE — 1:1 enforcement
- `IX_Payments_DisbursementRequestId` — query by request

**Relationships**:
- → PaymentOrder (1:1, required)
- ← AccountingEvent (0..1, via posting pipeline)

### PaymentOrderDeduction

Inherits `BaseAuditableEntity` (int PK, audit fields, RowVersion).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Inherited |
| PaymentOrderId | int | FK required | References PaymentOrder |
| LineNumber | int | required, unique per order | Sequential line number |
| DeductionType | DeductionType | required | Tax, WithholdingTax, Insurance, etc. |
| DeductionCode | string? | optional, max 50 | Deduction code |
| Description | string? | optional, max 500 | Description |
| AccountId | int | FK required | GL account for deduction |
| Amount | decimal(23,2) | required, ≥ 0 | Deduction amount |
| DeductionPercent | decimal(5,2)? | nullable | Optional percentage |
| IsMandatory | bool | required | Cannot be removed on update |
| IsTaxDeduction | bool | required | If true, TaxAuthorityId required |
| TaxAuthorityId | int? | nullable | Required when IsTaxDeduction |
| ReferenceNumber | string? | optional, max 100 | Reference number |
| RowVersion | byte[] | required | Optimistic concurrency token |

**Indexes**:
- `IX_PaymentOrderDeductions_PaymentOrderId_LineNumber` UNIQUE — line uniqueness per order
- `IX_PaymentOrderDeductions_AccountId` — query by account

**Relationships**:
- → PaymentOrder (many-to-1, Restrict)

## Status Transitions

### DisbursementRequest Lifecycle

```
Draft ──(submit)──→ PendingApproval
PendingApproval ──(1st approve, role-qualified)──→ PendingApproval (step 1, amount frozen)
PendingApproval ──(2nd approve, distinct role-qualified, same amount)──→ Approved + auto-generate PaymentOrder
PendingApproval ──(reject with reason)──→ Rejected
Draft/PendingApproval/Approved(unpaid) ──(cancel with reason, DisbursementRequestsCancel)──→ Cancelled
Approved ──(execute payment)──→ Disbursed
Any ──(PO cancelled/voided)──→ Invalidated
```

### PaymentOrder Lifecycle (request-first path)

```
Draft (auto-generated, FundId=0, AppropriationId=null)
  ──(prepare: set Fund, Appropriation, account, deductions)──→ Draft (ready)
  ──(submit with FundId/AppropriationId)──→ Submitted (budget check runs)
  ──(approve)──→ Approved
  ──(reject)──→ Rejected
  ──(cancel)──→ Cancelled → invalidates linked DisbursementRequest
Approved ──(send to treasury)──→ SentToTreasury
Approved/SentToTreasury ──(void, no payment)──→ Voided → invalidates linked DisbursementRequest
Approved/SentToTreasury ──(record payment)──→ Paid
```

### Payment Lifecycle

```
Created as Completed (terminal)
Failed (terminal)
```

## Validation Rules

| Rule | Source | Enforcement |
|------|--------|-------------|
| RequestedAmount > 0 | FR-002 | Handler + FluentValidation |
| BeneficiaryName not blank | FR-002 | Handler + FluentValidation |
| Purpose not blank | FR-002 | Handler + FluentValidation |
| Valid CurrencyId | FR-002 | Handler (FK check) |
| Valid FinancialYearId | FR-002 | Handler (FK check) |
| Amount freeze after first approval | FR-003 | Handler: rejects amount edit if ≥1 approval exists |
| Draft-only edits on request | FR-004 | Handler: status must be Draft for full edit |
| ≥2 distinct approvers | FR-005, FR-007 | Handler queries ApprovalHistory |
| ≥1 approver with AccountsManager/AuthorizingOfficer | FR-005 | Handler checks role via IIdentityService (both steps) |
| Approved amount ≤ requested amount | FR-008 | Handler validation |
| Same amount across both signatures | FR-009 | Handler compares step 1 and step 2 amounts |
| FundId/AppropriationId required at submit | FR-012 | SubmitPaymentOrderCommand validation |
| Budget check at submission | FR-012 | Handler calls BudgetAvailabilityService |
| Blocked approval on failed budget | FR-013 | ApprovePaymentOrderCommand checks BudgetCheckStatus |
| Draft-only edits on order | FR-014 | UpdatePaymentOrderCommand: status must be Draft |
| Deduction sum = DeductionAmount | FR-016 | CreatePaymentOrder + UpdatePaymentOrder handlers |
| Mandatory deductions cannot be removed | FR-017 | UpdatePaymentOrderCommand: BR-4 check |
| Tax deduction requires TaxAuthorityId | FR-018 | CreatePaymentOrder + UpdatePaymentOrder handlers |
| DeductionAmount ≤ AmountGross | FR-019 | Handler validation |
| One payment per order | FR-021 | RecordPaymentCommand: checks for existing completed payment |
| Payment only from Approved/SentToTreasury | FR-022 | RecordPaymentCommand status check |
| Cancel only if order not paid | FR-027 | CancelDisbursementRequestCommand: checks PaymentOrder.PaidAt |
