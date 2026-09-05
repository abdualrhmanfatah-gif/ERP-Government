# Quickstart Validation: BF-001 Journal Entries Frontend

**Date**: 2026-09-02
**Feature**: BF-001 Journal Entries Frontend
**Prerequisites**: Backend running on `http://localhost:5226`, frontend dev server running on `http://localhost:5173`

## Scenario 1: List Journal Entries with Filters

**Steps**:
1. Navigate to `http://localhost:5173/accounting/journal-entries`
2. Verify table loads with columns: Entry Number, Date, Journal, System-generated, Status
3. Select status filter "مسودة" (Draft)
4. Verify only Draft entries are shown
5. Clear filters
6. Enter date range (from: 2026-01-01, to: 2026-12-31)
7. Verify entries within date range are shown
8. Click an entry number
9. Verify navigation to detail page

**Expected**: Table loads within 2 seconds. Filters work correctly. URL updates with filter params. Back button restores filters.

## Scenario 2: Create Journal Entry with Balanced Lines

**Steps**:
1. Navigate to `http://localhost:5173/accounting/journal-entries/create`
2. Set Document Date to today
3. Verify Fiscal Year and Period are auto-detected and displayed
4. Select a Journal (optional)
5. Add Line 1: Account = "1101" (Cash), Debit = 1000, Credit = 0, Currency = base
6. Add Line 2: Account = "4101" (Revenue), Debit = 0, Credit = 1000, Currency = base
7. Verify Balance Indicator shows: Total Debit = 1000, Total Credit = 1000, Difference = 0
8. Click "إنشاء القيد"
9. Verify success toast
10. Verify redirect to detail page

**Expected**: Entry created with 2 lines. Balance indicator shows balanced. Redirect to detail page within 2 seconds.

## Scenario 3: Create Journal Entry with Unbalanced Lines (Blocked)

**Steps**:
1. Navigate to create page
2. Add Line 1: Debit = 1000, Credit = 0
3. Add Line 2: Debit = 0, Credit = 500
4. Verify Balance Indicator shows: Difference = 500 (red)
5. Verify submit button is disabled
6. Click submit button (should not respond)

**Expected**: Submit blocked when lines unbalanced. Visual indicator shows difference.

## Scenario 4: Workflow Transition — Draft to Posted

**Steps**:
1. Navigate to a Draft entry detail page
2. Click "إرسال للاعتماد"
3. Verify status changes to "Submitted" and success toast
4. Click "اعتماد"
5. Verify status changes to "Approved" and success toast
6. Click "ترحيل"
7. Verify confirmation dialog appears
8. Confirm
9. Verify status changes to "Posted" and success toast
10. Verify audit info "رحّل بواسطة: ..." appears

**Expected**: All transitions complete within 2 seconds. Status badge updates. Toast notifications in Arabic.

## Scenario 5: Reverse Posted Entry

**Steps**:
1. Navigate to a Posted entry detail page
2. Click "عكس القيد"
3. Verify ReverseDialog opens
4. Enter reversal reason (required)
5. Confirm
6. Verify new reversal entry is created
7. Verify redirect to new reversal entry
8. Verify original entry status is "Reversed"

**Expected**: Reversal creates new entry with swapped debit/credit. Original marked as Reversed.

## Scenario 6: Cancel Draft Entry

**Steps**:
1. Navigate to a Draft entry detail page
2. Click "إلغاء"
3. Verify confirmation dialog appears
4. Confirm
5. Verify status changes to "Cancelled" and success toast
6. Verify no workflow buttons remain (read-only)

**Expected**: Cancel completes. Entry becomes read-only.

## Scenario 7: Concurrency Conflict Handling

**Steps**:
1. Open same Draft entry in two browser tabs
2. In Tab 1: Update narration and save
3. In Tab 2: Update narration and save
4. Verify Tab 2 shows warning toast "تم تعديل القيد من مستخدم آخر"
5. Verify Tab 2 entry data is refreshed

**Expected**: Conflict detected. User informed. Data refreshed.

## Scenario 8: Empty State

**Steps**:
1. Navigate to journal entries list
2. Apply filters that match no entries
3. Verify empty state message "لا توجد قيود" is displayed
4. Verify link to create new entry is present

**Expected**: Empty state with call-to-action.

## Scenario 9: Error State

**Steps**:
1. Stop the backend server
2. Navigate to journal entries list
3. Verify error message is displayed
4. Verify retry button is present
5. Click retry
6. Verify error persists (backend still down)

**Expected**: Error state with retry functionality.

## Scenario 10: Inline Edit Lines (Draft)

**Steps**:
1. Navigate to a Draft entry detail page
2. Click on a line row
3. Verify inline edit form appears
4. Change the debit amount
5. Click save
6. Verify line is updated
7. Click "حذف" on another line
8. Verify line is removed

**Expected**: Inline editing works for Draft entries. Add/edit/remove lines functional.
