# Report DTOs

**Feature**: 007-financial-reports | **Date**: 2026-09-03

All DTOs are classes with `init` properties, following the existing `AccountBalanceDto` pattern. Currency field is always set to base currency code for v1.

## Common Types

### ReportSection

```csharp
public class ReportSection
{
    public string Title { get; init; } = string.Empty;       // Arabic section name
    public string TitleEn { get; init; } = string.Empty;     // English fallback
    public List<ReportLine> Lines { get; init; } = [];
    public decimal Total { get; init; }
}
```

### ReportLine

```csharp
public class ReportLine
{
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal Balance { get; init; }                     // Debit - Credit (or as appropriate)
}
```

## BalanceSheetDto

```csharp
public class BalanceSheetDto
{
    public DateOnly AsOfDate { get; init; }
    public string Currency { get; init; } = string.Empty;     // Base currency code
    
    public BalanceSheetGroup Assets { get; init; } = new();
    public BalanceSheetGroup Liabilities { get; init; } = new();
    public BalanceSheetGroup Equity { get; init; } = new();
    
    public decimal LiabilitiesAndEquity { get; init; }       // Liabilities.Total + Equity.Total
    public bool Balanced { get; init; }                       // Assets == LiabilitiesAndEquity
    public DateTimeOffset GeneratedAt { get; init; }
}

public class BalanceSheetGroup
{
    public List<ReportSection> Sections { get; init; } = [];
    public decimal Total { get; init; }
}
```

**Sections** (grouped by AccountGroup within each type):
- Assets: Current Assets, Non-Current Assets
- Liabilities: Current Liabilities, Non-Current Liabilities
- Equity: Equity (capital, reserves, retained earnings) + NetIncome line

## IncomeStatementDto

```csharp
public class IncomeStatementDto
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string Currency { get; init; } = string.Empty;
    
    public IncomeStatementGroup Revenue { get; init; } = new();
    public IncomeStatementGroup Expenses { get; init; } = new();
    
    public decimal NetIncome { get; init; }                   // Revenue.Total - Expenses.Total
    public DateTimeOffset GeneratedAt { get; init; }
}

public class IncomeStatementGroup
{
    public List<ReportSection> Sections { get; init; } = [];
    public decimal Total { get; init; }
}
```

**Sections** (grouped by AccountGroup within Revenue or Expense type):
- Revenue: Sales Revenue, Other Revenue, etc.
- Expenses: Cost of Sales, Operating Expenses, Financial Expenses, etc.

## GeneralLedgerDto

```csharp
public class GeneralLedgerDto
{
    public string Currency { get; init; } = string.Empty;
    public int TotalLines { get; init; }                      // For pagination
    public int Page { get; init; }
    public int PageSize { get; init; }
    public List<GeneralLedgerLine> Lines { get; init; } = [];
    public GeneralLedgerTotals Totals { get; init; } = new();
    public DateTimeOffset GeneratedAt { get; init; }
}

public class GeneralLedgerLine
{
    public DateOnly DocumentDate { get; init; }
    public string EntryNumber { get; init; } = string.Empty;
    public string Reference { get; init; } = string.Empty;
    public string Narration { get; init; } = string.Empty;
    public string AccountCode { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal RunningBalance { get; init; }              // Cumulative per account
}

public class GeneralLedgerTotals
{
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
}
```

## CashFlowStatementDto

```csharp
public class CashFlowStatementDto
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string Currency { get; init; } = string.Empty;
    
    public CashFlowSection Operating { get; init; } = new();
    public CashFlowSection Investing { get; init; } = new();
    public CashFlowSection Financing { get; init; } = new();
    
    public decimal NetChange { get; init; }                   // Op + Inv + Fin totals
    public decimal OpeningCash { get; init; }                 // GL cash balance at startDate-1
    public decimal ClosingCash { get; init; }                 // OpeningCash + NetChange
    public bool Reconciled { get; init; }                     // |ClosingCash - GL_Cash| <= 0.0001
    public string? Warning { get; init; }                     // e.g., "No cash accounts configured"
    public DateTimeOffset GeneratedAt { get; init; }
}

public class CashFlowSection
{
    public string Title { get; init; } = string.Empty;        // "Operating Activities" etc.
    public string TitleAr { get; init; } = string.Empty;      // Arabic title
    public List<CashFlowLineItem> Items { get; init; } = [];
    public decimal Total { get; init; }
}

public class CashFlowLineItem
{
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}
```
