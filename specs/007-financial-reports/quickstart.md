# Quickstart Validation Guide: Financial Reports Module

**Feature**: 007-financial-reports | **Date**: 2026-09-03

## Prerequisites

- .NET 10 SDK installed
- SQL Server running (via Aspire or local)
- Node.js 20+ for frontend
- Seed data applied (run `ApplicationDbContextInitialiser`)

## Validation Scenarios

### Scenario 1: Balance Sheet Generation

**Setup**: Seed accounting data with posted journal entries containing Assets, Liabilities, and Revenue/Expense accounts.

```bash
# Run unit tests
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Reports"

# Run functional tests
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~BalanceSheet"
```

**Expected**: Tests pass with Assets = Liabilities + Equity assertion. Balanced=true.

**Manual API test**:
```bash
curl -H "Authorization: Bearer {token}" \
  "http://localhost:5000/api/Reports/balance-sheet?asOfDate=2026-01-31"
```

**Expected**: JSON response with Assets, Liabilities, Equity sections. Balanced=true.

---

### Scenario 2: Income Statement Generation

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~IncomeStatement"
```

**Expected**: Revenue - Expenses = NetIncome. Period crossing fiscal year handled.

**Manual API test**:
```bash
curl -H "Authorization: Bearer {token}" \
  "http://localhost:5000/api/Reports/income-statement?startDate=2026-01-01&endDate=2026-01-31"
```

**Expected**: JSON with Revenue sections, Expenses sections, NetIncome.

---

### Scenario 3: General Ledger with Pagination

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~GeneralLedger"
```

**Expected**: Lines ordered by date/entry number. Running balance computed per account. Pagination works.

**Manual API test**:
```bash
curl -H "Authorization: Bearer {token}" \
  "http://localhost:5000/api/Reports/general-ledger?startDate=2026-01-01&endDate=2026-01-31&page=1&pageSize=10"
```

**Expected**: JSON with 10 lines, TotalLines reflecting full count.

---

### Scenario 4: Cash Flow Statement Reconciliation

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~CashFlow"
```

**Expected**: OpeningCash + NetChange = ClosingCash. Reconciled=true when ClosingCash matches GL.

**Manual API test**:
```bash
curl -H "Authorization: Bearer {token}" \
  "http://localhost:5000/api/Reports/cash-flow?startDate=2026-01-01&endDate=2026-01-31"
```

**Expected**: JSON with Operating/Investing/Financing sections. Reconciled=true.

---

### Scenario 5: Multi-Format Export (Determinism)

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~ReportExport"
```

**Expected**: Screen, Excel, and PDF numeric values match to 4 decimal places.

**Manual API test**:
```bash
# Excel export
curl -H "Authorization: Bearer {token}" \
  "http://localhost:5000/api/Reports/balance-sheet/export?format=excel&asOfDate=2026-01-31" \
  -o balance-sheet.xlsx

# PDF export
curl -H "Authorization: Bearer {token}" \
  "http://localhost:5000/api/Reports/balance-sheet/export?format=pdf&asOfDate=2026-01-31" \
  -o balance-sheet.pdf
```

**Expected**: Files downloaded with correct headers. Numeric values identical.

---

### Scenario 6: Permission Enforcement

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~Report"
```

**Expected**: Anonymous → 401. Wrong permission → 403. Correct permission → 200.

---

### Scenario 7: Performance Target

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~ReportPerformance"
```

**Expected**: All 4 reports complete in <2 seconds with 100k MoveLines dataset.

---

### Scenario 8: Frontend Validation

```bash
cd src/Web/ClientApp
npm run dev
```

**Navigate to**:
- `/reports/balance-sheet` — verify page loads, filters work, export dropdown visible
- `/reports/income-statement` — verify date range picker, period shortcuts
- `/reports/general-ledger` — verify account filter, pagination controls
- `/reports/cash-flow` — verify Operating/Investing/Financing sections display

**Expected**: All pages render with RTL layout, loading skeletons, empty states, error boundaries.

---

### Scenario 9: Audit Trail Verification

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~Audit"
```

**Expected**: SecurityAuditLog contains entries for every report view, export, and print action.
