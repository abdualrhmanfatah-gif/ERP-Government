# Quickstart Validation: ERP Read-Only Reporting & Oversight

**Feature**: 020-erp-read-only-reporting
**Date**: 2026-09-06

## Prerequisites

- SQL Server running with seed data (at least one fiscal year with appropriations, encumbrances, payments, receipt vouchers, and journal entries)
- Backend running (`dotnet run --project src/Web`)
- Auth token with `Reporting.*` permissions

## Validation Scenarios

### V1: Budget Execution Report

**Setup**: Seed data with FY2026, Fund "General", BudgetItem "Office Supplies" with:
- Appropriation: 100,000
- Encumbrance: 30,000
- Payment: 40,000

**Steps**:
```bash
# 1. Get budget execution report
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/BudgetExecutionReports?FiscalYearId=1&FundId=1"

# 2. Verify response contains:
#    - AppropriatedAmount: 100000
#    - EncumberedAmount: 30000
#    - PaidAmount: 40000
#    - AvailableAmount: 30000

# 3. Drill-down into budget item
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/BudgetExecutionReports/1/detail?FiscalYearId=1&FundId=1"

# 4. Export to Excel
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/BudgetExecutionReports/export?FiscalYearId=1&FundId=1&format=excel" \
  -o budget-execution.xlsx
```

**Expected**: Excel file opens with correct totals matching the JSON response.

---

### V2: Revenue Collections Report

**Setup**: Seed data with ReceiptVoucher for Party "Acme Corp", RevenueAccount "Fees", Amount 50,000, DepositSlip attached, Check cleared.

**Steps**:
```bash
# 1. Get revenue collections report
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/RevenueCollectionsReports?FiscalYearId=1&FundId=1"

# 2. Verify response contains:
#    - ReceiptVoucher with VoucherNumber, PartyName, Amount
#    - DepositSlipNumber and DepositSlipStatus populated
#    - CheckClearingStatus: "Cleared"

# 3. Drill-down
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/RevenueCollectionsReports/1/detail"
```

**Expected**: Detail shows individual receipt voucher lines and check details.

---

### V3: Disbursement Register

**Setup**: Seed data with PaymentOrders in various statuses (Draft, Submitted, Approved, Paid).

**Steps**:
```bash
# 1. Get disbursement register
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/DisbursementRegisterReports?FiscalYearId=1&FundId=1"

# 2. Verify Totals contain correct counts per status

# 3. Filter by status
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/DisbursementRegisterReports?FiscalYearId=1&Status=Paid"
```

**Expected**: Only paid disbursements returned when filtered.

---

### V4: Availability Snapshot

**Setup**: Reuse V1 seed data (Appropriation 100k, Encumbrance 30k, Payment 40k).

**Steps**:
```bash
# 1. Get availability snapshot
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/AvailabilitySnapshotReports?FiscalYearId=1&BudgetItemId=1"

# 2. Verify Totals:
#    - AppropriationAmount: 100000
#    - EncumberedAmount: 30000
#    - PaidAmount: 40000
#    - AvailableAmount: 30000

# 3. Verify ControlState is displayed

# 4. Drill-down
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/AvailabilitySnapshotReports/1/detail?FiscalYearId=1"
```

**Expected**: Breakdown by fund/program/project dimensions shown.

---

### V5: Trial Balance

**Setup**: Seed journal entries for FY2026 with balanced debits/credits across accounts.

**Steps**:
```bash
# 1. Get trial balance
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/TrialBalanceReports?FiscalYearId=1"

# 2. Verify: TotalDebits == TotalCredits (to the last riyal)

# 3. Get ledger movement for specific account
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/TrialBalanceReports/1/ledger-movement?FiscalYearId=1"

# 4. Verify: entries listed with RunningBalance calculated correctly
```

**Expected**: Debits equal credits. Running balance is cumulative and correct.

---

### V6: Reconciliation Test

**Purpose**: Verify report numbers reconcile with ledger to the last riyal.

**Steps**:
```bash
# 1. Get budget execution report
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/BudgetExecutionReports?FiscalYearId=1" > report.json

# 2. Get raw ledger totals (trial balance)
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/TrialBalanceReports?FiscalYearId=1" > trial-balance.json

# 3. Compare: Report totals must match ledger balances
#    - Appropriated total = sum of appropriation journal entries
#    - Paid total = sum of payment journal entries
```

**Expected**: Zero discrepancy between report totals and ledger.

---

### V7: Authorization Test

**Steps**:
```bash
# 1. Request without auth token
curl "http://localhost:5000/api/BudgetExecutionReports?FiscalYearId=1"
# Expected: 401 Unauthorized

# 2. Request with insufficient permissions
curl -H "Authorization: Bearer $TOKEN_NO_REPORT_PERM" \
  "http://localhost:5000/api/BudgetExecutionReports?FiscalYearId=1"
# Expected: 403 Forbidden
```

---

### V8: Performance Test

**Steps**:
```bash
# 1. Time a full fiscal year report
time curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5000/api/BudgetExecutionReports?FiscalYearId=1" > /dev/null

# Expected: Wall clock < 2 seconds
```

---

## Running Tests

```bash
# Unit tests
dotnet test tests/Application.UnitTests --filter "Reporting"

# Functional tests (against real database)
dotnet test tests/Application.FunctionalTests --filter "Reporting"

# Full suite (pre-merge gate)
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
dotnet test tests/Infrastructure.IntegrationTests
dotnet test tests/Web.AcceptanceTests
```

## Troubleshooting

| Issue | Resolution |
|-------|------------|
| Report returns empty | Verify seed data exists for the fiscal year |
| Export fails | Check ClosedXML/QuestPDF packages are installed |
| 403 on all endpoints | Verify permission codes are registered in DI |
| Slow query (>2s) | Check covering indexes on (FundId, FiscalYearId, BudgetItemId) |
