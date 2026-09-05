# Data Model: Financial Reports Module

**Feature**: 007-financial-reports | **Date**: 2026-09-03

## New Entities

### CashFlowMappingRule

Maps AccountGroups to Cash Flow Statement sections (Operating, Investing, Financing).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, identity | Inherited from BaseAuditableEntity |
| AccountGroupId | int | FK → AccountGroup, NOT NULL | Which account group to classify |
| Section | CashFlowSectionType | NOT NULL | Operating, Investing, or Financing |
| Description | string(200) | nullable | Explanation of classification rule |
| IsActive | bool | NOT NULL, default true | Soft disable without deletion |
| RowVersion | byte[] | concurrency token | Per Principle VI |

**Relationships**:
- Many-to-one: CashFlowMappingRule → AccountGroup (AccountGroupId)
- A single AccountGroup maps to exactly one Cash Flow section
- Multiple AccountGroups can map to the same section

**Validation Rules** (from FR-007):
- AccountGroupId must reference an active AccountGroup
- Section must be a valid CashFlowSectionType enum value
- Each AccountGroup should have at most one active mapping (unique constraint on AccountGroupId where IsActive=true)

**Index**: IX_CashFlowMappingRule_AccountGroupId (unique filtered: IsActive=true)

### CashFlowSectionType (Enum)

| Value | Name | Arabic | Description |
|-------|------|--------|-------------|
| 0 | Operating | تشغيلي | Revenue, expenses, working capital changes |
| 1 | Investing | استثماري | Fixed asset acquisitions/disposals |
| 2 | Financing | تمويلي | Equity, long-term debt, dividends |

## Existing Entities (Referenced, Not Modified)

### Account (existing)

Key fields for reports:
- `Code` (string) — account number/code
- `Name` (string) — account name (Arabic)
- `AccountGroupId` (FK → AccountGroup) — determines section grouping
- `IsPostable` (bool) — whether account accepts postings
- `IsReconcilable` (bool) — bank/cash accounts for Cash Flow
- `IsActive` (bool) — active accounts only
- `NormalBalance` (NormalBalanceType enum) — Debit or Credit

### AccountGroup (existing)

Key fields for reports:
- `Code` (string)
- `Name` (string)
- `Type` (AccountGroupType enum) — Asset, Liability, Equity, Revenue, Expense
- `ParentId` (nullable FK → self) — hierarchical grouping
- `Level` (int) — depth in hierarchy
- `IsActive` (bool)

### AccountGroupType (existing enum)

Values: Asset=0, Liability=1, Equity=2, Revenue=3, Expense=4

### AccountBalance (existing, materialized)

Key fields for Balance Sheet:
- `AccountId` (FK → Account)
- `FiscalYearId` (FK → FiscalYear)
- `FiscalPeriodId` (FK → FiscalPeriod)
- `OpeningDebit`, `OpeningCredit` — period opening balances
- `Debit`, `Credit` — period activity
- `ClosingDebit`, `ClosingCredit` — computed closing balances
- `IsFinalized` (bool) — whether period is closed

### Move (existing)

Key fields for reports:
- `EntryStatus` (string) — Draft/Pending/Posted/Reversed/Cancelled
- `DocumentDate` (DateTime) — transaction date
- `EntryNumber` (string) — unique entry number
- `ReversalOfMoveId` (nullable FK → self) — links reversal to original
- `PostedAt` (DateTimeOffset?) — when posted
- `FiscalYearId`, `PeriodId` — period association

### MoveLine (existing)

Key fields for reports:
- `MoveId` (FK → Move)
- `Sequence` (int) — line order within move
- `AccountId` (FK → Account) — which account
- `Debit` (decimal) — debit amount
- `Credit` (decimal) — credit amount
- `CurrencyId` (FK → Currency)
- `ExchangeRate` (decimal) — for base currency conversion

### SecurityAuditLog (existing)

Key fields for report audit:
- `UserId` (string) — who generated the report
- `Action` (string) — will use "ReportGenerate", "ReportExport", "ReportPrint"
- `EntityType` (string) — will use report name ("BalanceSheet", "IncomeStatement", etc.)
- `NewValues` (JSON) — will store report params, format, success/failure

## Query Patterns (No New Tables)

### Balance Sheet Query Pattern

```
Input: asOfDate, optional fiscalPeriodId
1. Resolve fiscalPeriodId from asOfDate if not provided
2. Query AccountBalances WHERE Account.Type=Asset, grouped by AccountGroup → Assets sections
3. Query AccountBalances WHERE Account.Type=Liability, grouped by AccountGroup → Liabilities sections
4. Query AccountBalances WHERE Account.Type=Equity, grouped by AccountGroup → Equity sections
5. Compute NetIncome = Revenue total - Expense total for current period
6. Add NetIncome to Equity total
7. Assert: Assets.Total = Liabilities.Total + Equity.Total
```

### Income Statement Query Pattern

```
Input: startDate, endDate
1. Query MoveLines WHERE Account.Type=Revenue, Move.EntryStatus='Posted',
   Move.DocumentDate BETWEEN startDate AND endDate
   GROUP BY AccountGroup → Revenue sections
2. Query MoveLines WHERE Account.Type=Expense, same filters
   GROUP BY AccountGroup → Expense sections
3. NetIncome = Revenue.Total - Expenses.Total
```

### General Ledger Query Pattern

```
Input: optional accountId/accountCode, optional fiscalPeriodId, optional startDate/endDate, page, pageSize
1. Build base query: MoveLines JOIN Moves WHERE Move.EntryStatus='Posted'
2. Apply filters (accountId, date range, fiscal period)
3. ORDER BY Move.DocumentDate ASC, Move.EntryNumber, MoveLine.Sequence
4. Apply pagination (SKIP/TAKE)
5. Compute running balance per account (cumulative sum of Debit - Credit)
6. Return page + TotalLines count
```

### Cash Flow Statement Query Pattern

```
Input: startDate, endDate
1. Compute OpeningCash: SUM(Cash account balances at startDate - 1 day)
2. Query MoveLines WHERE Account.Type IN (Revenue, Expense), posted in period
   → Operating section (indirect method: start with NetIncome, add non-cash adjustments)
3. Query MoveLines WHERE AccountGroup mapped to Investing section, posted in period
   → Investing section
4. Query MoveLines WHERE AccountGroup mapped to Financing section, posted in period
   → Financing section
5. NetChange = Operating.Total + Investing.Total + Financing.Total
6. ClosingCash = OpeningCash + NetChange
7. Reconciled = |ClosingCash - GL_Cash_Balance| <= 0.0001
```
