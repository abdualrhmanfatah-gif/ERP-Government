# Data Model: Reports Group (RPT-01..06)

**Date**: 2026-09-08

## Overview

No new entities are created. All DTOs are server-owned and consumed by the frontend via NSwag-generated clients. This document references the existing DTO shapes as the binding contract.

## DTO Reference

### RPT-01 — Budget Execution

| DTO | Fields | Source |
|---|---|---|
| `BudgetExecutionReportDto` | fiscalYearId, fiscalYearName, lines[], totals | `src/Application/Reporting/BudgetExecution/GetBudgetExecutionReport/BudgetExecutionReportDto.cs` |
| `BudgetExecutionLineDto` | budgetItemId, itemCode, itemName, fundId, fundNumber, fundName, programId?, programCode?, projectId?, projectCode?, appropriatedAmount, encumberedAmount, paidAmount, availableAmount | Same file |
| `BudgetExecutionTotalDto` | appropriatedAmount, encumberedAmount, paidAmount, availableAmount | Same file |
| `BudgetExecutionDetailDto` | budgetItemId, itemCode, itemName, fundId, fundNumber, encumbrances: EncumbranceDetailDto[], payments: PaymentDetailDto[] | `src/Application/Reporting/BudgetExecution/GetBudgetExecutionDetail/BudgetExecutionDetailDto.cs` |

**Invariant**: `availableAmount = appropriatedAmount - encumberedAmount - paidAmount` (server-computed).

### RPT-02 — Revenue Collections

| DTO | Fields | Source |
|---|---|---|
| `RevenueCollectionsReportDto` | fiscalYearId, fiscalYearName, lines[], totals | `src/Application/Reporting/RevenueCollections/GetRevenueCollectionsReport/RevenueCollectionsReportDto.cs` |
| `RevenueCollectionsDetailDto` | receiptVoucherId, voucherNumber, voucherDate, partyName, totalAmount, paymentMethod, depositSlipNumber?, depositSlipStatus?, lines[], checks[] | `src/Application/Reporting/RevenueCollections/GetRevenueCollectionsDetail/RevenueCollectionsDetailDto.cs` |
| `RevenueCollectionsLineDetailDto` | revenueAccountId, accountCode, accountName, amount | Same file |

### RPT-03 — Disbursement Register

| DTO | Fields | Source |
|---|---|---|
| `DisbursementRegisterDto` | fiscalYearId, fiscalYearName, lines[], totals | `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/DisbursementRegisterDto.cs` |
| `DisbursementRegisterLineDto` | paymentOrderId, orderNumber, orderDate, payeeName, amount, status, fundId, fundCode, fundName, approverId?, approverName?, paidAt? | Same file |
| `DisbursementRegisterTotalDto` | totalRequests, draftCount, submittedCount, approvedCount, paidCount, rejectedCount, totalAmount, paidAmount | Same file |
| `DisbursementRegisterDetailDto` | paymentOrderId, orderNumber, orderDate, payeeName, amount, status, fundCode, approverName?, paidAt?, payments[] | `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterDetail/DisbursementRegisterDetailDto.cs` |

### RPT-04 — Availability Snapshot

| DTO | Fields | Source |
|---|---|---|
| `AvailabilitySnapshotDto` | budgetItemId, itemCode, itemName, fiscalYearId, fiscalYearName, controlState, breakdown[], totals | `src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotQuery/AvailabilitySnapshotDto.cs` |
| `AvailabilitySnapshotDetailDto` | budgetItemId, itemCode, itemName, appropriations[], encumbrances[], payments[] | `src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotDetail/AvailabilitySnapshotDetailDto.cs` |

### RPT-05 — Trial Balance

| DTO | Fields | Source |
|---|---|---|
| `TrialBalanceReportDto` | fiscalYearId, fiscalYearName, fiscalPeriodId?, fiscalPeriodName?, lines[], totals | `src/Application/Reporting/TrialBalance/GetTrialBalanceReport/TrialBalanceReportDto.cs` |
| `TrialBalanceLineDto` | accountId, accountCode, accountName, accountType, openingBalance, debitTotal, creditTotal, closingBalance | Same file |
| `TrialBalanceTotalDto` | totalDebits, totalCredits, totalOpeningBalance, totalClosingBalance | Same file |
| `LedgerMovementDto` | accountId, accountCode, accountName, entries[], totals | `src/Application/Reporting/TrialBalance/GetLedgerMovement/LedgerMovementDto.cs` |

**Display-only**: Balance indicator = ΣtotalDebits == ΣtotalCredits (computed by frontend, labeled "عرض").

### RPT-06 — Financial Statements

| DTO | Fields | Source |
|---|---|---|
| `BalanceSheetDto` | asOfDate, currency, assets/liabilities/equity: BalanceSheetGroup, liabilitiesAndEquity, balanced, generatedAt | `src/Application/Accounting/Reports/BalanceSheet/BalanceSheetDto.cs` |
| `BalanceSheetGroup` | title, titleAr, items, total | Same file |
| `IncomeStatementDto` | startDate, endDate, currency, revenue/expenses: IncomeStatementGroup, netIncome, generatedAt | `src/Application/Accounting/Reports/IncomeStatement/IncomeStatementDto.cs` |
| `GeneralLedgerDto` | currency, totalLines, page, pageSize, lines[], totals, generatedAt | `src/Application/Accounting/Reports/GeneralLedger/GeneralLedgerDto.cs` |
| `GeneralLedgerLine` | documentDate, entryNumber, reference?, narration?, accountCode, accountName, debit, credit, runningBalance | Same file |
| `CashFlowStatementDto` | sections: CashFlowSection[] {title, titleAr, items: CashFlowLineItem[] {description, amount}, total} | `src/Application/Accounting/Reports/CashFlowStatement/CashFlowStatementDto.cs` |
| `TrialBalanceDto` (legacy) | fiscalYearId, fiscalYearName, fiscalPeriodId, periodName, sections[], totalDebit, totalCredit, isBalanced, currency, generatedAt | `src/Application/Accounting/Reports/TrialBalance/TrialBalanceDto.cs` |
