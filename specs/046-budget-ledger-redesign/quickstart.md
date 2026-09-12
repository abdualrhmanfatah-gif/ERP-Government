# Quickstart Validation Guide: Budget Preparation — BudgetItemAllocations

**Date**: 2026-09-10

## Prerequisites

- SQL Server running with the Budgeting schema migrated
- Test user with Budgets.Create, Budgets.Submit, Budgets.Approve, Budgets.Activate permissions
- At least one BudgetType, Fund, FiscalYear, BudgetClassification, and BudgetItem with AccountId in the database

## Scenario 1: Create Budget with Allocations (US1)

**Setup**: Create a Budget in Draft status with BudgetItems "ITM-001" (AccountId=100) and "ITM-002" (AccountId=101).

**Steps**:
1. POST /api/BudgetItemAllocations — `{budgetId: 1, budgetItemId: 1, proposedAmount: 500000}`
2. POST /api/BudgetItemAllocations — `{budgetId: 1, budgetItemId: 2, proposedAmount: 300000}`
3. POST /api/BudgetItemAllocations — `{budgetId: 1, budgetItemId: 1, proposedAmount: 100000}` → expect 400 duplicate error
4. GET /api/BudgetItemAllocations?budgetId=1 → expect 2 rows, approvedAmount=null
5. PUT /api/BudgetItemAllocations/1 — `{proposedAmount: 550000}` → expect 200
6. POST /api/Budgets/1/submit → expect status=Submitted
7. GET /api/BudgetItemAllocations?budgetId=1 → expect read-only (proposedAmount=550000, approvedAmount=null)
8. POST /api/Budgets/1/approve → expect status=Approved, allocationsUpdated=2
9. GET /api/BudgetItemAllocations?budgetId=1 → expect approvedAmount = proposedAmount for both

**Expected**: Allocations created, duplicate rejected, amounts frozen at approval.

## Scenario 2: Budget Transaction Per Allocation (US4)

**Setup**: Budget in Active status with allocation A (ApprovedAmount=550000).

**Steps**:
1. POST /api/BudgetTransactions — `{budgetItemAllocationId: 1, transactionType: 1, amount: 100000, direction: 0}` (Supplement, Increase)
2. POST /api/BudgetTransactions/{id}/submit → expect Submitted
3. POST /api/BudgetTransactions/{id}/approve → expect Approved
4. POST /api/BudgetTransactions/{id}/post → expect Posted, allocation.approvedAmount = 650000
5. POST /api/BudgetTransactions — `{budgetItemAllocationId: 1, transactionType: 2, amount: 200000, direction: 1}` (Reduction, Decrease)
6. POST → submit → approve → post → expect allocation.approvedAmount = 450000
7. POST /api/BudgetTransactions — `{budgetItemAllocationId: 1, transactionType: 2, amount: 500000, direction: 1}` → post → expect 400 "المبلغ المعتمد لا يمكن أن يصبح سالبًا"

**Expected**: ApprovedAmount updates atomically on post. Negative floor enforced.

## Scenario 3: Actual Expenditure from Journal Entries (US3)

**Setup**: Allocation A with ApprovedAmount=450000, BudgetItem.AccountId=100, Budget.FiscalYearId=1.

**Steps**:
1. Create a JournalEntry with debit 200000 on AccountId=100, FiscalYearId=1, status=Posted
2. GET /api/BudgetItemAllocations/1 → expect remainingAmount=250000, actualExpenditure=200000
3. Create a JournalEntry with credit 50000 on AccountId=100, FiscalYearId=1, status=Posted
4. GET /api/BudgetItemAllocations/1 → expect remainingAmount=300000, actualExpenditure=150000
5. Reverse the first JournalEntry → GET → expect remainingAmount=350000, actualExpenditure=100000

**Expected**: Expenditure computed from net posted JournalEntryLines. Reversals excluded.

## Scenario 4: Shared Account Prevention (FR-025)

**Setup**: BudgetItem "ITM-001" with AccountId=100, BudgetItem "ITM-002" with AccountId=100, same Budget.

**Steps**:
1. POST /api/BudgetItemAllocations — `{budgetId: 1, budgetItemId: 1, proposedAmount: 500000}` → expect 201
2. POST /api/BudgetItemAllocations — `{budgetId: 1, budgetItemId: 2, proposedAmount: 300000}` → expect 400 "الحساب مرتبط ببند آخر في هذه الموازنة"

**Expected**: Second allocation with same AccountId rejected.

## Scenario 5: Budget Close vs Cancel (Edge Cases)

**Setup**: Budget with allocations, some Posted transactions, some outstanding encumbrances.

**Steps**:
1. POST /api/Budgets/1/cancel → expect 400 "لا يمكن إلغاء الموازنة لوجود حركات مرحّلة أو التزامات قائمة"
2. POST /api/Budgets/1/close → expect 200, status=Closed
3. POST /api/BudgetTransactions — `{budgetItemAllocationId: 1, ...}` → expect 400 "الموازنة غير مفعلة"
4. GET /api/BudgetItemAllocations?budgetId=1 → expect allocations still visible (historical)

**Expected**: Cancel blocked by transactions. Close preserves data, blocks new operations.

## Scenario 6: Reversal (US5)

**Setup**: Posted BudgetTransaction BTR-000001 with Amount=100000 Increase on allocation A.

**Steps**:
1. POST /api/BudgetTransactions/1/reverse — `{reason: "تصحيح"}`
2. Expect new transaction BTR-000002 with type=Reversal, amount=100000, direction=Decrease, reversalOfId=1
3. POST BTR-000002 through lifecycle to Posted
4. GET allocation A → expect approvedAmount restored to pre-transaction value
5. POST /api/BudgetTransactions/1/reverse → expect 400 (already Reversed)

**Expected**: Reversal inverts effect. Original never modified.

## Scenario 7: PaymentOrder Linkage (US2)

**Setup**: Active budget with allocation A (ApprovedAmount=450000, remaining=300000).

**Steps**:
1. POST /api/Payments/PaymentOrders — `{budgetItemAllocationId: 1, ...}` → expect 201
2. GET /api/Payments/PaymentOrders/{id} → expect allocationRemainingAmount=300000
3. Complete payment → journal entry posted → GET allocation → expect remainingAmount decreased

**Expected**: PaymentOrder linked to allocation. Remaining amount decreases after payment.

## Scenario 8: Allocations After Active (FR-007a)

**Setup**: Active budget with existing allocations.

**Steps**:
1. POST /api/BudgetItemAllocations — `{budgetId: 1, budgetItemId: 3, proposedAmount: 200000}` → expect 400 (budget not in Draft)
2. POST /api/BudgetTransactions — `{budgetItemAllocationId: NEW, transactionType: 1, amount: 200000, direction: 0}` → expect 201
3. Post the Supplement transaction
4. GET /api/BudgetItemAllocations?budgetId=1 → expect new allocation with approvedAmount=200000

**Expected**: New allocations added via Supplement transaction after Active.
