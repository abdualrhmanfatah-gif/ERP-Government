# Quickstart: Budgeting Frontend Foundation #1 of 5

**Feature**: `011-budgeting-frontend-foundation` | **Date**: 2026-09-04

Validation guide for verifying the feature works end-to-end against the live backend.

## Prerequisites

1. Backend running with budgeting endpoints available at `/api/BudgetTypes`, `/api/Funds`, `/api/Encumbrances/availability`
2. Frontend dev server running (`npm run dev` in `src/Web/ClientApp/`)
3. Authenticated user with BudgetTypes.* and Funds.* permissions
4. Browser set to Arabic locale or application in RTL mode

## Validation Scenarios

### V1: Shared Types Integrity

**Setup**: Open browser DevTools console.

**Steps**:
1. Navigate to any budgeting page
2. Open DevTools console
3. Verify no TypeScript compilation errors in the build output

**Expected**: Zero type errors. Build completes successfully.

---

### V2: BudgetTypes List Page

**URL**: `/budgeting/budget-types`

**Steps**:
1. Navigate to `/budgeting/budget-types`
2. Verify the page loads with a DataGrid showing columns: Code, Name, ControlMethod (Arabic), AllowOverrun (badge), IsActive (toggle)
3. Verify loading skeleton appears briefly before data loads
4. Verify the "الموازena" sidebar group shows "أنواع الموازنة" link highlighted
5. Type in the search filter — grid filters instantly (client-side)
6. Select a ControlMethod filter — grid filters to matching records
7. Toggle the IsActive filter — grid shows active/inactive/both

**Expected**: All filters work client-side with instant feedback. Empty state shows if no records exist.

---

### V3: BudgetTypes Create

**Steps**:
1. Click the create button (visible only with BudgetTypes.Create permission)
2. Dialog opens with fields: Code, Name, Description, ControlMethod (select), AllowOverrun (switch)
3. Fill in Code = "TEST-TYPE", Name = "نوع تجريبي", ControlMethod = "تحذير", AllowOverrun = on
4. Click save
5. Dialog closes, new record appears in the list
6. Verify the ControlMethod column shows "تحذير" (Arabic label)

**Expected**: 201 response, list refreshes, new row visible.

---

### V4: BudgetTypes Edit

**Steps**:
1. Click the edit button on an existing row
2. Dialog opens in edit mode with pre-filled values
3. Change Name to "نوع معدّل"
4. Click save
5. Dialog closes, row updates in the list

**Expected**: 204 response, row reflects updated name.

---

### V5: BudgetTypes Toggle Active

**Steps**:
1. Click the IsActive toggle switch on a row
2. Confirmation dialog appears ("هل أنت متأكد؟")
3. Confirm the toggle
4. IsActive state changes in the grid

**Expected**: 204 response, toggle state reflects new value.

---

### V6: BudgetTypes Permission Gating

**Steps**:
1. Open the page without BudgetTypes.Create permission (or verify the stub grants all)
2. If permission is denied: create button is hidden
3. Verify edit buttons are hidden without BudgetTypes.Update permission

**Expected**: Actions are hidden based on permissions. No errors thrown.

---

### V7: Funds List Page

**URL**: `/budgeting/funds`

**Steps**:
1. Navigate to `/budgeting/funds`
2. Verify DataGrid shows: FundNumber, FundName, FundType, FundCategory, FiscalYear, DefaultRevenueDebitAccount, IsActive
3. Apply search filter — instant client-side filtering
4. Apply FundType filter — filters to matching funds
5. Apply FundCategory filter — filters to matching funds

**Expected**: All filters work client-side. Empty state shows if no funds exist.

---

### V8: Funds Create

**Steps**:
1. Click the create button
2. Dialog opens with fields: FundNumber, FundName, FundType (select), FundCategory (select), FiscalYear (combobox), LegalAuthority, Description, DefaultRevenueDebitAccount (combobox)
3. If no accounts exist: Account combobox is disabled with hint
4. If no fiscal years exist: FiscalYear combobox is disabled with hint
5. Fill in required fields and save

**Expected**: 201 response, new fund appears in list.

---

### V9: Funds Detail View

**Steps**:
1. Click a fund row in the list
2. Navigate to `/budgeting/funds/:id`
3. Verify detail page shows all fund fields
4. Use browser back button — returns to list

**Expected**: Detail page loads with all fields. Browser navigation works.

---

### V10: Funds Edit

**Steps**:
1. Return to `/budgeting/funds`
2. Click edit button on a row
3. Dialog opens in edit mode with pre-filled values
4. Change FundName and save

**Expected**: 204 response, list and detail view reflect changes.

---

### V11: RowVersion Conflict

**Steps**:
1. Open two browser tabs on the same BudgetTypes list
2. Edit a record in tab 1 and save
3. Edit the same record in tab 2 (with stale rowVersion) and save
4. Verify toast notification appears: "تعارض البيانات"

**Expected**: 409 response, toast shown, list refetched with current data.

---

### V12: Dark Mode

**Steps**:
1. Toggle dark mode
2. Navigate to `/budgeting/budget-types`
3. Verify all colors, backgrounds, borders, and text use dark mode tokens
4. Navigate to `/budgeting/funds`
5. Verify same dark mode compliance
6. Open a create dialog — verify dialog renders in dark mode

**Expected**: No hardcoded colors visible. All values from design tokens.

---

### V13: RTL Layout

**Steps**:
1. Verify `<html dir="rtl">` is set
2. Navigate to `/budgeting/budget-types`
3. Verify sidebar appears on the right side
4. Verify text alignment is right-to-left
5. Verify filter bar and grid columns are RTL-aligned
6. Open a dialog — verify form fields are RTL-aligned

**Expected**: Full RTL layout using logical CSS properties.

---

### V14: AvailabilityIndicator

**Note**: This component is consumed by specs #3/#4. Validation requires appropriation data.

**Steps**:
1. If appropriation data exists: navigate to a page that uses AvailabilityIndicator
2. Verify green indicator when available amount is positive
3. Verify amber when overrun + Warning control
4. Verify red when overrun + Blocking control
5. Verify green when ControlMethod is None
6. Verify zero-state message when no appropriations exist
7. Verify loading skeleton during fetch

**Expected**: All four visual states render correctly.

---

### V15: Sidebar Progressive Activation

**Steps**:
1. Verify sidebar "الموازena" group shows only Budget Types and Funds
2. Verify no links for Classifications, Budgets, Appropriations, Encumbrances (specs #2-#5 not yet implemented)

**Expected**: Only implemented features appear in the sidebar.

---

## Build Validation

```bash
# From src/Web/ClientApp/
npm run build
```

**Expected**: Zero errors, zero type-checking failures.

## Test Execution

```bash
# From src/Web/ClientApp/
npx vitest run
```

**Expected**: All tests pass (if tests are written for this feature).
