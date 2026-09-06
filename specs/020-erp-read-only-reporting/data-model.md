# Data Model: ERP Read-Only Reporting & Oversight

**Feature**: 020-erp-read-only-reporting
**Date**: 2026-09-06

## Overview

No new entity tables are created. Reports query over existing entities. This document describes the query-side DTOs (Data Transfer Objects) used to shape report output.

## Existing Entities Queried

### Budgeting Module

| Entity | Key Fields | Report Usage |
|--------|-----------|--------------|
| `Budget` | Id, BudgetNumber, FundId, FiscalYearId, Status | Budget execution scope |
| `BudgetItem` | Id, ItemCode, ItemName, BudgetId, ParentId, AccountId | Budget line grouping |
| `Appropriation` | Id, AppropriationNumber, BudgetItemId, Amount, Type, Status | Appropriated totals |
| `Encumbrance` | Id, EncumbranceNumber, AppropriationId, Amount, Status | Open encumbrances |
| `PaymentOrder` | Id, PaymentOrderNumber, FundId, AppropriationId, EncumbranceId, AmountGross, VendorId, Status, PaymentOrderDate | Executed payments |

### Revenue Module

| Entity | Key Fields | Report Usage |
|--------|-----------|--------------|
| `ReceiptVoucher` | Id, VoucherNumber, VoucherDate, PartyId, PaymentMethod, DepositSlipId, Status | Revenue collections |
| `ReceiptVoucherLine` | Id, ReceiptVoucherId, RevenueAccountId, Amount | Revenue account detail |
| `Check` | Id, ReceiptVoucherId, CheckNumber, Amount, Status, ClearedAt | Check clearing state |
| `DepositSlip` | Id, SlipNumber, SlipDate, Status, TotalAmount | Deposit status |
| `Party` | Id, Name | Payee/payer names |

### Payments Module

| Entity | Key Fields | Report Usage |
|--------|-----------|--------------|
| `PaymentOrder` | (same as above) | Disbursement register |
| `Payment` | Id, PaymentNumber, PaymentOrderId, Amount, PaidAt, Status | Payment execution |

### Accounting Module

| Entity | Key Fields | Report Usage |
|--------|-----------|--------------|
| `JournalEntry` | Id, EntryNumber, EntryDate, FiscalYearId, FiscalPeriodId, Status | Ledger movement |
| `JournalEntryLine` | Id, JournalEntryId, AccountId, Debit, Credit, FundId, ProjectId, BudgetItemId, CostCenterId, EncumbranceId, PaymentOrderId | Dimension joins, trial balance |
| `Account` | Id, AccountCode, AccountName, AccountType | Account listing |

### Dimensions

| Entity | Key Fields | Report Usage |
|--------|-----------|--------------|
| `Fund` | Id, FundCode, FundName | Fund filter |
| `Project` | Id, ProjectCode, ProjectName | Project filter |
| `FiscalYear` | Id, Year, Status | Period scoping |
| `FiscalPeriod` | Id, FiscalYearId, PeriodNumber, Status | Period filter |

## Query DTOs

### BudgetExecutionReportDto

```csharp
public record BudgetExecutionReportDto(
    int FiscalYearId,
    string FiscalYearName,
    List<BudgetExecutionLineDto> Lines,
    BudgetExecutionTotalDto Totals);

public record BudgetExecutionLineDto(
    int BudgetItemId,
    string ItemCode,
    string ItemName,
    int FundId,
    string FundCode,
    string FundName,
    int? ProgramId,
    string? ProgramCode,
    int? ProjectId,
    string? ProjectCode,
    decimal AppropriatedAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);

public record BudgetExecutionTotalDto(
    decimal AppropriatedAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);
```

### RevenueCollectionsReportDto

```csharp
public record RevenueCollectionsReportDto(
    int FiscalYearId,
    string FiscalYearName,
    List<RevenueCollectionsLineDto> Lines,
    RevenueCollectionsTotalDto Totals);

public record RevenueCollectionsLineDto(
    int ReceiptVoucherId,
    string VoucherNumber,
    DateOnly VoucherDate,
    int RevenueAccountId,
    string AccountCode,
    string AccountName,
    int PartyId,
    string PartyName,
    decimal Amount,
    string PaymentMethod,
    int? DepositSlipId,
    string? DepositSlipNumber,
    string? DepositSlipStatus,
    string CheckClearingStatus);

public record RevenueCollectionsTotalDto(
    decimal TotalAmount,
    decimal TotalCash,
    decimal TotalChecks,
    int PendingDeposits,
    int ClearedChecks,
    int BouncedChecks);
```

### DisbursementRegisterDto

```csharp
public record DisbursementRegisterDto(
    int FiscalYearId,
    string FiscalYearName,
    List<DisbursementRegisterLineDto> Lines,
    DisbursementRegisterTotalDto Totals);

public record DisbursementRegisterLineDto(
    int PaymentOrderId,
    string OrderNumber,
    DateOnly OrderDate,
    string PayeeName,
    decimal Amount,
    string Status,
    int FundId,
    string FundCode,
    string FundName,
    int? ApproverId,
    string? ApproverName,
    DateOnly? PaidAt);

public record DisbursementRegisterTotalDto(
    int TotalRequests,
    int DraftCount,
    int SubmittedCount,
    int ApprovedCount,
    int PaidCount,
    int RejectedCount,
    decimal TotalAmount,
    decimal PaidAmount);
```

### AvailabilitySnapshotDto

```csharp
public record AvailabilitySnapshotDto(
    int BudgetItemId,
    string ItemCode,
    string ItemName,
    int FiscalYearId,
    string FiscalYearName,
    string ControlState,
    List<AvailabilitySnapshotLineDto> Breakdown,
    AvailabilitySnapshotTotalDto Totals);

public record AvailabilitySnapshotLineDto(
    int FundId,
    string FundCode,
    string FundName,
    int? ProgramId,
    string? ProgramCode,
    int? ProjectId,
    string? ProjectCode,
    decimal AppropriationAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);

public record AvailabilitySnapshotTotalDto(
    decimal AppropriationAmount,
    decimal EncumberedAmount,
    decimal PaidAmount,
    decimal AvailableAmount);
```

### TrialBalanceReportDto

```csharp
public record TrialBalanceReportDto(
    int FiscalYearId,
    string FiscalYearName,
    int? FiscalPeriodId,
    string? FiscalPeriodName,
    List<TrialBalanceLineDto> Lines,
    TrialBalanceTotalDto Totals);

public record TrialBalanceLineDto(
    int AccountId,
    string AccountCode,
    string AccountName,
    string AccountType,
    decimal OpeningBalance,
    decimal DebitTotal,
    decimal CreditTotal,
    decimal ClosingBalance);

public record TrialBalanceTotalDto(
    decimal TotalDebits,
    decimal TotalCredits,
    decimal TotalOpeningBalance,
    decimal TotalClosingBalance);
```

### LedgerMovementDto

```csharp
public record LedgerMovementDto(
    int AccountId,
    string AccountCode,
    string AccountName,
    List<LedgerMovementLineDto> Entries,
    LedgerMovementTotalDto Totals);

public record LedgerMovementLineDto(
    int JournalEntryId,
    string EntryNumber,
    DateOnly EntryDate,
    string? Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);

public record LedgerMovementTotalDto(
    decimal TotalDebits,
    decimal TotalCredits,
    decimal FinalBalance);
```

## Filter DTO

```csharp
public record ReportFilterDto(
    int FiscalYearId,
    int? FiscalPeriodId,
    int? FundId,
    int? ProgramId,
    int? ProjectId,
    int? BudgetItemId,
    int? PartyId,
    string? PaymentMethod,
    string? Status,
    int? ApproverId);
```

## Validation Rules

- `FiscalYearId` is required on all reports.
- `FundId` is optional but recommended for performance.
- Date range filters (if used) must have StartDate <= EndDate.
- All monetary amounts are `decimal(23,2)` — no rounding at query level; display rounding handled by frontend.
