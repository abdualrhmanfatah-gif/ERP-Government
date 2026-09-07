# Quickstart Validation: ACC-05 — مراقبة المحاسبة

## Prerequisites

- .NET 10 SDK installed
- SQL Server running (local or container)
- Node.js 18+ (for frontend)
- Existing accounting data: at least 1 FiscalYear, 1 FiscalPeriod, 5+ Accounts, 3+ JournalEntryLines posted

## Backend Validation

### 1. Build

```bash
dotnet build src/Web/Web.csproj
```

Expected: Build succeeded, 0 errors.

### 2. Run Unit Tests

```bash
dotnet test tests/Application.UnitTests --filter "Accounting"
```

Expected: All handler tests pass (Finalize, Unfinalize, Rebuild, Reconcile, PostingRule CRUD).

### 3. Run Functional Tests

```bash
dotnet test tests/Application.FunctionalTests --filter "AccountingMonitoring"
```

Expected: Tests covering:
- Period close with pending events → rejected
- Period close with no pending events → finalized
- Period reopen → unfinalized
- Reconciliation with matching balances → isBalanced=true
- Reconciliation with mismatched balances → discrepancies returned
- Rebuild recalculates from journal entry lines
- Posting rule create/update/delete with lines

### 4. Manual API Testing

```bash
# Get balances for fiscal year 1, period 3
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/Accounting/AccountingBalances?fiscalYearId=1&fiscalPeriodId=3"

# Reconcile
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/Accounting/AccountingBalances/reconcile?fiscalYearId=1&fiscalPeriodId=3"

# Rebuild
curl -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"fiscalYearId":1,"fiscalPeriodId":3}' \
  "http://localhost:5000/api/Accounting/AccountingBalances/rebuild"

# Finalize
curl -X POST -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"fiscalYearId":1,"fiscalPeriodId":3}' \
  "http://localhost:5000/api/Accounting/AccountingBalances/finalize"

# Get pending events
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/Accounting/AccountingEvents/pending"

# Get posting rules
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/Accounting/PostingRules"
```

## Frontend Validation

### 1. Generate API Client

```bash
cd src/Web/ClientApp
npm run generate-api
```

Expected: New typed clients for AccountingBalances, AccountingEvents, PostingRules.

### 2. Run Frontend Tests

```bash
npm run test
```

Expected: All accounting-monitoring tests pass.

### 3. Build Frontend

```bash
npm run build
```

Expected: Build succeeded, 0 errors.

### 4. Manual UI Testing

Navigate to:
- `/accounting/balances` — balances list with 6 columns, finalize/unfinalize buttons
- `/accounting/events` — event queue (read-only, filters by status/type)
- `/accounting/posting-rules` — posting rules CRUD with nested lines

## Expected Outcomes

| Scenario | Expected |
|----------|----------|
| View balances for period | 6 columns displayed, isFinalized badge visible |
| Finalize period (no pending events) | Success, isFinalized=true |
| Finalize period (has pending events) | Error: "هناك أحداث معلقة، أكمل الترحيل أولاً" |
| Reconcile balanced period | "متوازن" message |
| Reconcile imbalanced period | Discrepancy table with per-currency rows |
| Rebuild balances | Recalculated from journal entry lines |
| Create posting rule | Saved with lines, appears in list |
| Delete posting rule (unused) | Deleted successfully |
| Delete posting rule (in use) | Rejected with error |
| View failed event | errorMessage + retryCount displayed |
