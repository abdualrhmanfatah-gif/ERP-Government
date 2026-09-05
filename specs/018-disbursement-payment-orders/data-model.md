# Data Model: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-05

## New Enums

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

### PaymentStatus

```csharp
public enum PaymentStatus
{
    Completed = 0,
    Failed = 1
}
```

## New Entities

### DisbursementRequest

Inherits `BaseAuditableEntity` (int PK, audit fields, RowVersion).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Inherited |
| RequestNumber | string | UNIQUE, required, max 20 | DSB-{D6} assigned at Draft creation |
| PaymentOrderId | int | FK UNIQUE, required | 1:1 enforced by unique index |
| RequestedById | int | FK required | User who created the request |
| RequestDate | DateOnly | required | Date request was created |
| Status | DisbursementRequestStatus | required, default Draft | Lifecycle state |
| HasWarning | bool | required, default false | True when Warning control allowed creation with insufficient funds |
| Notes | string? | optional, max 500 | Free-text notes |
| RowVersion | byte[] | required | Optimistic concurrency token |

**Indexes**:
- `IX_DisbursementRequests_PaymentOrderId` UNIQUE — enforces 1:1 constraint
- `IX_DisbursementRequests_RequestNumber` UNIQUE — document number uniqueness
- `IX_DisbursementRequests_Status` — filtered queries

**Relationships**:
- → PaymentOrder (1:1, required)
- ← ApprovalHistory (0..n, via DocumentType="DisbursementRequest")
- ← Payment (0..1)

### Payment

Inherits `BaseAuditableEntity` (int PK, audit fields, RowVersion).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Inherited |
| PaymentNumber | string | UNIQUE, required, max 20 | PAY-{D6} assigned at execution |
| DisbursementRequestId | int | FK UNIQUE, required | 1:1 with DisbursementRequest |
| PaymentOrderId | int | FK required | Mirrors from request for query convenience |
| PaymentMethod | PaymentMethod | required | Cash/Check/Transfer/InKind (existing enum) |
| Amount | decimal(23,2) | required | Net total snapshot of payment order at execution |
| PaidById | int | FK required | User who executed the payment |
| PaidAt | DateTimeOffset | required | Execution timestamp |
| ReferenceNumber | string? | optional, max 100 | External reference (check number, transfer ref) |
| Notes | string? | optional, max 500 | Free-text notes |
| Status | PaymentStatus | required, default Completed | Payment outcome |
| RowVersion | byte[] | required | Optimistic concurrency token |

**Indexes**:
- `IX_Payments_PaymentNumber` UNIQUE
- `IX_Payments_DisbursementRequestId` UNIQUE — 1:1 enforcement
- `IX_Payments_PaymentOrderId` — query by payment order

**Relationships**:
- → DisbursementRequest (1:1, required)
- → PaymentOrder (1:1, required, via FK)
- ← AccountingEvent (0..1, via posting pipeline)

## Modified Entities

### PaymentOrder

No schema changes. The existing `Status` field transitions to `Paid` when a Payment is recorded. The 1:1 constraint is enforced by `DisbursementRequest.PaymentOrderId` unique index — no column added to PaymentOrder.

## Status Transitions

### DisbursementRequest Lifecycle

```
Draft ──(submit)──→ PendingApproval
PendingApproval ──(1st approve)──→ PendingApproval (step 1 recorded)
PendingApproval ──(2nd approve, distinct user)──→ Approved
PendingApproval ──(reject)──→ Rejected
PendingApproval ──(cancel)──→ Cancelled
Approved ──(execute payment)──→ Disbursed
Approved ──(cancel)──→ Cancelled
Draft ──(cancel)──→ Cancelled
Any ──(PO amended)──→ Invalidated
```

### Payment Lifecycle

```
 Completed (terminal)
 Failed (terminal)
```

### PaymentOrder Status Changes

```
Approved ──(payment executed)──→ Paid
```

## Validation Rules

| Rule | Source | Enforcement |
|------|--------|-------------|
| PaymentOrderId must be unique | FR-001, 1:1 constraint | DB unique index + application check |
| PaymentOrder must be Approved | US1 scenario 4 | Handler validation |
| PaymentOrder net total > 0 | FR-017 | Handler validation |
| Budget availability check at creation | FR-002, FR-003, FR-004, FR-005 | Handler calls BudgetAvailabilityService |
| ≥2 distinct approvers for approval | FR-006, FR-008 | Handler queries ApprovalHistory |
| ≥1 approver with AccountsManager/AuthorizingOfficer | FR-006 | Handler checks role via IIdentityService |
| Request number assigned at creation | FR-013 | Handler calls DocumentSequenceService |
| Payment amount = payment order net total | FR-009 | Handler snapshots (AmountGross - DeductionAmount) |
| Payment posted to ledger atomically | FR-010, SC-005 | Domain event in same transaction |
