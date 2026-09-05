# Financial Module Design Rules

## Override Authority

This file contains page-specific overrides for the Financial module.
Base rules: `design-system/erp-government/MASTER.md`

## Currency Management

### Currency List Page

```text
Structure:
  PageShell
    → PageHeader (title + create button)
    → FilterBar (search, status filter)
    → DataGrid
      → Columns: code, name, symbol, decimalPlaces, isBase, status
      → Actions: edit, activate/deactivate
      → Status: StatusBadge (active/inactive)
```

### Currency Form

```text
Fields:
  - code (text, required, LTR, disabled on edit)
  - name (text, required)
  - symbol (text, required, LTR, max 5 chars)
  - decimalPlaces (number, 0-6)
  - roundingPrecision (number, positive)
  - isBase (switch, only on create)

Validation:
  - code: required, max 10 chars
  - name: required, max 100 chars
  - symbol: required, max 5 chars
  - decimalPlaces: integer, 0-6
  - roundingPrecision: positive
```

### Currency Display

```text
Format: {symbol} {amount}
Example: $ 1,234.56

Rules:
  - LTR direction for all currency values
  - Tabular numerics for alignment
  - 2 decimal places (configurable)
  - Negative: red color + minus sign
  - Zero: muted color
```

## Exchange Rates

### Exchange Rate List Page

```text
Structure:
  PageShell
    → PageHeader (title + create button)
    → FilterBar (base currency, target currency, date range)
    → DataGrid
      → Columns: baseCurrency, targetCurrency, rate, rateType, rateDate
      → Actions: edit, view history
      → Expandable row: rate history
```

### Exchange Rate Form

```text
Fields:
  - baseCurrencyId (select, required)
  - currencyId (select, required)
  - rateDate (date, required)
  - rateType (select: Official/Market)
  - rate (number, required, LTR, step 0.000001)

Validation:
  - baseCurrencyId: required, must differ from currencyId
  - rate: positive, max 6 decimal places
```

### Exchange Rate Display

```text
Format: {baseCurrency} → {targetCurrency}: {rate}
Example: USD → SAR: 3.75

Rules:
  - LTR direction for rate values
  - 6 decimal places for precision
  - Color: on-surface for active rates
  - Gray: for historical rates
```

## Fiscal Years

### Fiscal Year List Page

```text
Structure:
  PageShell
    → PageHeader (title + create button)
    → DataGrid
      → Columns: name, yearNumber, startDate, endDate, status
      → Actions: view, open, close
      → Status: StatusBadge (open/closed)
```

### Fiscal Year Detail Page

```text
Structure:
  PageShell
    → PageHeader (title + actions)
    → InfoCard (year details)
    → FiscalPeriodsList (embedded)
    → ClosingEntriesList (embedded)

Sections:
  - Year info: name, number, dates, status
  - Periods: table with lock/unlock actions
  - Closing entries: list with view action
```

### Fiscal Period Form

```text
Fields:
  - name (text, required)
  - periodNumber (number, required, disabled on edit)
  - startDate (date, required)
  - endDate (date, required)

Validation:
  - endDate must be after startDate
  - periodNumber: 1-12
```

## Document Sequences

### Document Sequence List Page

```text
Structure:
  PageShell
    → PageHeader (title + create button)
    → FilterBar (document type filter)
    → DataGrid
      → Columns: name, documentType, resetPolicy, lastNumber
      → Actions: edit
```

### Document Sequence Form

```text
Fields:
  - name (text, required)
  - documentType (select, required)
  - resetPolicy (select: Yearly/Never)

Document Types:
  - Appropriation (ال Allocation)
  - Encumbrance (التحفظ)
  - Liquidation (التصفية)
  - PaymentOrder (أمر الدفع)
  - PaymentExecution (تنفيذ الدفع)
  - AdvancePayment (الدفع المقدم)
  - JournalEntry (القيد اليومي)
  - VendorBill (فاتورة المورد)
  - PurchaseOrder (أمر الشراء)
  - RevenueReceipt (إيراد)
```

## Color Usage

### Financial Status Colors

| Status | BG | FG | Usage |
|--------|----|----|-------|
| draft | status-draft-bg | status-draft-fg | Draft documents |
| pending | status-pending-bg | status-pending-fg | Awaiting approval |
| approved | status-approved-bg | status-approved-fg | Approved items |
| posted | success-bg | success | Posted entries |
| locked | info-bg | info | Locked periods |
| cancelled | status-closed-bg | status-closed-fg | Cancelled documents |

### Currency Colors

| Element | Color | Usage |
|---------|-------|-------|
| Base currency badge | primary-container | Base currency indicator |
| Foreign currency | on-surface | Standard currency |
| Negative amount | error | Overdraft/deficit |
| Zero amount | on-surface-variant | Zero balance |

## Accessibility

### Keyboard Navigation

- Tab order: Page header → Filter bar → Data grid → Actions
- Enter/Space: Activate buttons, open links
- Arrow keys: Navigate within grid cells
- Escape: Close dialogs, cancel editing

### Screen Reader

- Currency values: "{currency} {amount}" format
- Status badges: Announce status text
- Grid actions: "تعديل {currency name}"
- Form errors: Announce with field focus

### Focus Management

- Form fields: Auto-focus first field on open
- Dialogs: Trap focus within dialog
- Grid: Focus row on selection
- Actions: Visible focus ring on all interactive elements
