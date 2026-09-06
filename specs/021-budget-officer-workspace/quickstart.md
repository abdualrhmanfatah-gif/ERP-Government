# Quickstart Validation: Budget Officer Workspace

**Feature**: 021-budget-officer-workspace
**Date**: 2026-09-06

## Prerequisites

- Backend running (dotnet run from src/Web)
- Frontend dev server running (npm run dev from src/Web/ClientApp)
- At least one BudgetType, Fund, FiscalYear, and BudgetClassification seeded
- User logged in with Budgets.Create, Budgets.Submit, Budgets.Approve, Budgets.Activate permissions

## Validation Scenarios

### VS1: Full Budget Lifecycle (US1)

1. Navigate to `الموازنة → الموازنات` in sidebar
2. Click `موازنة جديدة` → fill BudgetName, BudgetType, FiscalYear, Fund, EffectiveFrom → Save
3. Verify budget appears in list with status `مسودة`
4. Click budget row → detail page loads with item tree (empty)
5. Click `+ بند رئيسي` → add ItemCode "1000", ItemName "Salaries" → Save
6. Click `+ فرعي` on "1000" → add ItemCode "1001", ItemName "Basic Salary" → Save
7. Verify tree shows 2 nodes with correct levels
8. Click `تقديم` (Submit) → status changes to `مقدم`
9. Click `اعتماد` (Approve) → status changes to `معتمد`
10. Click `تفعيل` (Activate) → status changes to `نشط`
11. Verify: lifecycle buttons update at each status change

**Expected**: Budget progresses through Draft → Submitted → Approved → Active. Tree renders correctly. All transitions recorded.

### VS2: Appropriation with Availability (US2)

1. Open an Active budget → select item "1001"
2. Click `تسجيل تخصيص` → select type `أصلي` (Original), enter Amount 500000
3. Verify availability indicator shows 500000 (green)
4. Save → appropriation created in Draft
5. Submit → Approve → Activate
6. Create Supplement of 100000 → verify availability shows 600000
7. Create Reduction of 200000 → verify availability shows 400000
8. Attempt Reduction of 500000 → verify blocking error (if Blocking control)

**Expected**: Availability updates correctly for each appropriation type.

### VS3: Transfer Between Items (US2)

1. In same budget, add item "1002" with child "1003"
2. Activate item "1002"
3. Create Transfer appropriation on item "1001" → target selector shows "1002" and "1003"
4. Select "1002" as target, Amount 50000 → Save
5. Verify: source item availability decreases, target item gets 50000

**Expected**: Transfer correctly moves funds between items.

### VS4: Encumbrance with Live Availability (US3)

1. Open an Active appropriation on item "1001" (net appropriated 400000)
2. Create encumbrance → enter Amount 100000 → verify availability shows 300000 (green)
3. Change amount to 350000 → verify availability shows 50000 (green)
4. Change amount to 450000 → verify availability shows -50000 (red for Blocking)
5. Save encumbrance → it's created in Draft
6. Reverse the encumbrance → enter reason → verify original shows as reversed

**Expected**: Availability indicator updates live. Reversal works with reason.

### VS5: Monthly Plan Editor (US4)

1. Open budget item "1001" → click `الخطة الشهرية`
2. Enter amounts for 6 months (e.g., 80000 each)
3. Click Save → verify plan stored
4. Navigate away → return to same item → verify plan restored
5. Verify variance badge shows (total of 480000 vs 400000 appropriated)
6. Click Copy → paste into spreadsheet → verify tab-separated format

**Expected**: Plan persists across reloads. Variance indicator works. Copy produces TSV.

### VS6: Budget Execution Drill-Down (US5)

1. Open Active budget → verify summary panel shows Total Appropriated, Total Encumbered, Total Available
2. Click item "1001" → expands to show its appropriations
3. Click an appropriation → expands to show its encumbrances
4. Verify totals at each level match the sum of child records

**Expected**: Drill-down shows correct totals at all levels.

### VS7: RTL and Accessibility

1. All screens render Arabic text correctly (no Latin text overflow)
2. Tab through all interactive elements → verify focus order is logical
3. Verify ARIA labels on availability indicators and tree nodes
4. Verify keyboard Enter/Space activates buttons and tree nodes

**Expected**: Full RTL support. Keyboard navigation works. Screen reader labels present.

## Test Commands

```bash
# Frontend unit tests
cd src/Web/ClientApp
npm run test -- --run features/budgeting/__tests__/

# Full frontend lint
npm run lint

# Frontend build
npm run build
```

## Common Issues

- **Availability shows "—"**: Check that the appropriation/budget item exists and is Active
- **Lifecycle buttons missing**: Verify user has the required permission (even if placeholder)
- **Monthly plan not persisting**: Check localStorage in browser DevTools → Application → Local Storage
- **Transfer target dropdown empty**: Verify the budget has items in Active status
