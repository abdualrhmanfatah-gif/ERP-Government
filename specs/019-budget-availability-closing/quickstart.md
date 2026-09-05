# Quickstart Validation Guide: Financial Control Layer

**Feature**: 019-budget-availability-closing | **Date**: 2026-09-05

## Prerequisites

- Running application with test database
- Seed data: at least 1 fiscal year (Open status), 1 budget with budget items, 1 fund, 1 program
- Test user with permissions: `FinancialControl.LapseYear`, `FinancialControl.ApproveFinalAccount`, `BudgetItems.View`

## Validation Scenarios

### Scenario 1: Multi-Dimension Availability Query

**Goal**: Verify US1 — availability breakdown by fund, program, project, budget item.

**Setup**:
1. Create appropriations for budget item across 2 funds, 2 programs (total 4 combinations)
2. Create 1 encumbrance against one appropriation
3. Execute 1 payment against the encumbrance

**Steps**:
1. Call `GET /api/Availability/{budgetItemId}?fiscalYearId={id}`
2. Verify response contains 4 breakdown entries
3. Verify each entry shows correct appropriation, encumbered, paid, available amounts
4. Verify totals sum correctly across all dimensions

**Expected**: Availability breakdown matches expected values per dimension; totals are consistent.

---

### Scenario 2: Year-End Lapse Run

**Goal**: Verify US2 — lapse nullifies appropriations and encumbrances, blocks payments.

**Setup**:
1. Fiscal year with open appropriations (500,000) and open encumbrances (200,000)

**Steps**:
1. Call `POST /api/YearClosing/Lapse` with fiscal year ID
2. Verify response shows lapsed totals and status "Completed"
3. Attempt to create a payment against a budget item in the lapsed year
4. Verify payment is blocked with clear error message
5. Attempt to run lapse again for the same year
6. Verify duplicate is rejected (409 Conflict)

**Expected**: Lapse runs once, payments blocked, duplicate prevented.

---

### Scenario 3: Fiscal Year Reopening

**Goal**: Verify US2 — reopening restores amounts, blocks if payments exist.

**Setup**:
1. Fiscal year has been lapsed
2. No payments made against lapsed items

**Steps**:
1. Call `POST /api/YearClosing/Reopen` with fiscal year ID
2. Verify response shows restored totals and status "Completed"
3. Verify availability query now shows original amounts

**Expected**: Reopening restores amounts; availability reflects pre-lapse state.

---

### Scenario 4: Reopening Blocked by Payments

**Goal**: Verify US2 edge case — reopening blocked if payments exist against lapsed items.

**Setup**:
1. Fiscal year has been lapsed
2. A payment was made against a lapsed item during the closed period

**Steps**:
1. Call `POST /api/YearClosing/Reopen` with fiscal year ID
2. Verify 409 response with error message listing affected payments

**Expected**: Reopening is blocked with clear error.

---

### Scenario 5: Final Account Generation

**Goal**: Verify US3 — closing entries, final balances, budget-vs-actual.

**Setup**:
1. Fiscal year has been lapsed
2. Revenue and expense accounts have balances

**Steps**:
1. Call `POST /api/FinalAccounts/Generate` with fiscal year ID
2. Verify FinalAccount created with status "Draft"
3. Call `GET /api/FinalAccounts/{id}` to retrieve lines
4. Verify budget-vs-actual comparison shows: appropriations, encumbrances (separate), payments (actual), variance
5. Verify closing entries exist in ledger (check JournalEntries)

**Expected**: Final account generated without manual assembly; closing entries balanced.

---

### Scenario 6: Final Account Issuance

**Goal**: Verify US3 — issued final account is immutable.

**Setup**:
1. Final account exists in Draft status

**Steps**:
1. Call `POST /api/FinalAccounts/{id}/Issue`
2. Verify status transitions to "Issued"
3. Attempt to regenerate the final account for the same year
4. Verify rejection (year already has issued final account)
5. Attempt to reopen the fiscal year
6. Verify rejection (final account issued)

**Expected**: Issued final account is immutable; year cannot be reopened.

---

### Scenario 7: Year-Boundary Document Splitting

**Goal**: Verify edge case — encumbrance spanning two fiscal years.

**Setup**:
1. Encumbrance with date range spanning year boundary (e.g., Nov 2026 – Feb 2027)

**Steps**:
1. Run lapse for 2026
2. Verify encumbrance is split: portion in 2026 lapses, portion in 2027 remains open
3. Verify YearClosingRun log records the split

**Expected**: Proportional split; only 2026 portion lapses.

---

## Test Commands

```bash
# Run unit tests for the feature
dotnet test tests/Application.UnitTests --filter "LapseFiscalYear|ReopenFiscalYear|GenerateFinalAccount|AvailabilityBreakdown"

# Run functional tests
dotnet test tests/Application.FunctionalTests --filter "YearClosingLifecycle|FinalAccountGeneration"

# Run full suite (pre-merge gate)
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
```
