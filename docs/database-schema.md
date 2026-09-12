# Database Schema: ERP Government

**Last Updated**: 2026-09-10 (Spec 046 — BudgetItemAllocations Model)

## Budgeting Module

### Budgets

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| BudgetNumber | nvarchar(50) | no | Unique, BGT-NNNNNN |
| BudgetName | nvarchar(200) | no | |
| BudgetTypeId | int FK | no | → BudgetTypes |
| FiscalYearId | int FK | no | → FiscalYears |
| FundId | int FK | no | → Funds |
| Status | int | no | enum: Draft=0, Submitted=1, Approved=2, Active=3, Suspended=4, Closed=5, Cancelled=6 |
| EffectiveFrom | date | no | |
| EffectiveTo | date | yes | |
| AllowOverrun | bit | yes | |
| Description | nvarchar(1000) | yes | |
| ApprovedAt | datetimeoffset | yes | |
| ApprovedBy | nvarchar(max) | yes | |
| ClosedAt | datetimeoffset | yes | |
| ClosedBy | nvarchar(max) | yes | |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### BudgetItems

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| ItemCode | nvarchar(50) | no | |
| ItemName | nvarchar(200) | no | |
| BudgetId | int FK | no | → Budgets |
| ParentId | int? FK | yes | Self-referencing |
| AccountId | int? FK | yes | → Accounts |
| CostCenterId | int? FK | yes | → CostCenters |
| BudgetClassificationId | int? FK | yes | → BudgetClassifications |
| ControlMethod | int? | yes | enum: None=0, Warning=1, Blocking=2 |
| AllowOverrun | bit | yes | Override per item |
| IsActive | bit | no | default true |
| Remarks | nvarchar(1000) | yes | |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### BudgetItemAllocations (NEW)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| BudgetId | int FK | no | → Budgets |
| BudgetItemId | int FK | no | → BudgetItems |
| ProposedAmount | decimal(23,2) | no | Editable in Draft |
| ApprovedAmount | decimal(23,2) | yes | Set on approval, frozen after |
| Remarks | nvarchar(1000) | yes | |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

**UNIQUE**: (BudgetId, BudgetItemId) — prevents duplicate items per budget.
**Lifecycle**: Managed through Budget status (Draft editable, Submitted read-only, Approved frozen, Active for transactions).

### BudgetTransactions (replaces Appropriation)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| TransactionNumber | nvarchar(50) | no | Unique, BTR-NNNNNN |
| BudgetId | int FK | no | → Budgets |
| BudgetItemAllocationId | int FK | no | → BudgetItemAllocations |
| TransactionType | int | no | enum: InitialAppropriation=0, Supplement=1, Reduction=2, Transfer=3, CarryForward=4, Adjustment=5, Lapse=6, Reversal=7 |
| TransactionDate | date | no | |
| Amount | decimal(23,2) | no | Per-allocation amount |
| Direction | int | no | enum: Increase=0, Decrease=1 |
| DocumentType | nvarchar(100) | yes | |
| DocumentId | int? | yes | |
| Description | nvarchar(1000) | yes | |
| Status | int | no | enum: Draft=0, Submitted=1, Approved=2, Posted=3, Reversed=4, Rejected=5 |
| ApprovedAt | datetimeoffset | yes | |
| ApprovedBy | nvarchar(max) | yes | |
| PostedAt | datetimeoffset | yes | |
| PostedBy | nvarchar(max) | yes | |
| ReversalOfId | int? FK | yes | Self-referencing |
| ReversalReason | nvarchar(1000) | yes | |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

**Lifecycle**: Draft → Submitted → Approved → Posted | Submitted → Rejected | Posted → Reversed

### BudgetTransactionLines (REMOVED)

> **Removed in Spec 046**: BudgetTransactionLines table has been removed. Each BudgetTransaction now targets exactly one BudgetItemAllocation via BudgetItemAllocationId. Existing data was migrated to BudgetTransaction records.

### BudgetClassifications

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| Code | nvarchar(50) | no | |
| Name | nvarchar(200) | no | |
| ParentId | int? FK | yes | Self-referencing |
| ClassificationLevel | int | yes | |
| IsActive | bit | no | default true |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### BudgetTypes

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| Code | nvarchar(50) | no | |
| Name | nvarchar(200) | no | |
| Description | nvarchar(1000) | yes | |
| ControlMethod | int | no | enum: None=0, Warning=1, Blocking=2 |
| AllowOverrun | bit | no | default false |
| IsActive | bit | no | default true |
| RowVersion | rowversion | no | Concurrency |

### Funds

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| FundNumber | nvarchar(50) | no | Unique |
| FundName | nvarchar(200) | no | |
| FundType | int | no | enum: General=0, Special=1, Project=2 |
| FundCategory | int | no | enum: Operating=0, Capital=1 |
| LegalAuthority | nvarchar(500) | no | |
| Description | nvarchar(1000) | yes | |
| DefaultRevenueAccountId | int? FK | yes | → Accounts |
| CurrencyId | int? FK | yes | → Currencies |
| IsActive | bit | no | default true |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### Encumbrances

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| EncumbranceNumber | nvarchar(50) | no | Unique, ENC-NNNNNN |
| EncumbranceType | int | no | enum: Commitment=0, Obligational=1, Contractual=2, Advance=3, Adjustment=4 |
| VendorPartyId | int? FK | yes | → Parties |
| PurchaseOrderId | int? FK | yes | |
| DocumentType | nvarchar(100) | yes | |
| DocumentId | int? | yes | |
| EncumbranceDate | date | no | |
| Description | nvarchar(1000) | yes | |
| TotalAmount | decimal(23,2) | no | Computed from lines |
| Status | int | no | enum: Draft=0, PendingApproval=1, Approved=2, Active=3, PartiallyReleased=4, PartiallyLiquidated=5, FullyLiquidated=6, Released=7, Cancelled=8, Reversed=9, Suspended=10 |
| ReversalOfId | int? FK | yes | Self-referencing |
| ReversalReason | nvarchar(1000) | yes | |
| PostedAt | datetimeoffset | yes | |
| PostedBy | nvarchar(max) | yes | |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### EncumbranceLines (NEW)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| EncumbranceId | int FK | no | → Encumbrances |
| BudgetItemId | int FK | no | → BudgetItems |
| Amount | decimal(23,2) | no | |
| LiquidatedAmount | decimal(23,2) | no | default 0 |
| CancelledAmount | decimal(23,2) | no | default 0 |
| Description | nvarchar(1000) | yes | |
| RowVersion | rowversion | no | Concurrency |

**Outstanding**: Amount - LiquidatedAmount - CancelledAmount

### BudgetItemMonthlyPlans

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| BudgetItemId | int FK | no | → BudgetItems |
| FiscalPeriodId | int FK | no | → FiscalPeriods |
| PlannedAmount | decimal(23,2) | no | |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### YearClosingRuns

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| FiscalYearId | int FK | no | → FiscalYears |
| FundId | int? FK | yes | → Funds |
| RunType | int | no | enum: Lapse=0, Reopen=1 |
| StartedAt | datetimeoffset | no | |
| CompletedAt | datetimeoffset | yes | |
| RunById | int FK | no | → Users |
| Status | int | no | enum: Pending=0, Running=1, Completed=2, Failed=3, Reversed=4 |
| LapsedAppropriationTotal | decimal(23,2) | no | |
| LapsedEncumbranceTotal | decimal(23,2) | no | |
| ReversalRunId | int? FK | yes | Self-referencing |
| ReversalReason | nvarchar(1000) | yes | |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### FinalAccounts

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| FiscalYearId | int FK | no | → FiscalYears |
| FundId | int FK | no | → Funds |
| YearClosingRunId | int FK | no | → YearClosingRuns |
| Status | int | no | enum: Draft=0, Issued=1 |
| GeneratedAt | datetimeoffset | no | |
| GeneratedById | int FK | no | → Users |
| ReviewedAt | datetimeoffset | yes | |
| ReviewedById | int? FK | yes | → Users |
| IssuedAt | datetimeoffset | yes | |
| IssuedById | int? FK | yes | → Users |
| RowVersion | rowversion | no | Concurrency |
| CreatedAt | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModifiedAt | datetimeoffset | yes | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### FinalAccountLines

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| FinalAccountId | int FK | no | → FinalAccounts |
| Dimension | int | no | enum: Fund=0, Program=1, Project=2, Item=3 |
| DimensionId | int? | yes | |
| DimensionCode | nvarchar(50) | no | |
| DimensionName | nvarchar(200) | no | |
| OriginalBudgetAmount | decimal(23,2) | no | |
| RevisedBudgetAmount | decimal(23,2) | no | |
| EncumberedAmount | decimal(23,2) | no | |
| ActualAmount | decimal(23,2) | no | |
| VarianceAmount | decimal(23,2) | no | |
| RowVersion | rowversion | no | Concurrency |

## Payments Module (Modified)

### PurchaseRequests

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | no | Identity |
| RequestNumber | nvarchar(20) | no | Unique, PR-NNNNNN |
| RequestDate | datetime2 | no | |
| RequiredDate | date | yes | |
| DepartmentId | int? FK | yes | → Departments |
| CostCenterId | int? FK | yes | → CostCenters |
| RequesterId | int? | yes | |
| RequesterName | nvarchar(200) | no | مقدم الطلب |
| Priority | int | no | enum: Low=0, Normal=1, High=2, Urgent=3 |
| Status | int | no | enum: Draft=0, Submitted=1, Approved=2, UnderProcurement=3, Rejected=4, Cancelled=5, Expired=6 |
| TotalEstimatedCost | decimal(23,2) | yes | Computed from lines |
| Notes | nvarchar(2000) | yes | |
| RowVersion | rowversion | no | Concurrency |
| Created | datetimeoffset | no | Audit |
| CreatedBy | nvarchar(max) | yes | Audit |
| LastModified | datetimeoffset | no | Audit |
| LastModifiedBy | nvarchar(max) | yes | Audit |

### PaymentOrders

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| BudgetItemId | int? FK | yes | → BudgetItems (backward compatibility) |
| BudgetItemAllocationId | int? FK | yes | → BudgetItemAllocations (primary budget control link) |
| EncumbranceLineId | int? FK | yes | → EncumbranceLines |

## Document Sequences

| Type | Prefix | Format |
|------|--------|--------|
| BudgetTransaction | BTR | BTR-NNNNNN |
| Encumbrance | ENC | ENC-NNNNNN |
| Budget | BGT | BGT-NNNNNN |
| PaymentOrder | PO | PO-NNNNNN |

## Availability Formulas

```
ActualExpenditure(BudgetItemAllocation) =
    SUM(JournalEntryLine.Debit) - SUM(JournalEntryLine.Credit)
    WHERE JournalEntryLine.AccountId = BudgetItem.AccountId
    AND JournalEntry.FiscalYearId = Budget.FiscalYearId
    AND JournalEntry.EntryStatus = Posted
    AND JournalEntry.ReversalOfId IS NULL

RemainingAmount(BudgetItemAllocation) =
    ApprovedAmount - ActualExpenditure

OutstandingEncumbrance(BudgetItemId) =
    SUM(EncumbranceLine.Amount - LiquidatedAmount - CancelledAmount)
    WHERE EncumbranceLine.BudgetItemId = @budgetItemId
    AND Encumbrance.Status in Active/PartiallyReleased/PartiallyLiquidated
    AND Encumbrance.ReversalOfId IS NULL

AvailableAmount(BudgetItemAllocation) =
    RemainingAmount - OutstandingEncumbrance

Budget Control: Evaluated against AvailableAmount
- None → always allowed
- Warning → allowed with warning log if exceeded
- Blocking → rejected if exceeded

AllowOverrun chain: BudgetItem.AllowOverrun → Budget.AllowOverrun → BudgetType.AllowOverrun
```
