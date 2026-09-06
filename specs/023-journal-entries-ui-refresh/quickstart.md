# Quickstart Validation: Journal Entries UI Refresh

**Feature**: 023-journal-entries-ui-refresh
**Date**: 2026-09-06

## Prerequisites

- Backend running (`dotnet run --project src/Web`)
- Frontend dev server running (`cd src/Web/ClientApp && npm run dev`)
- At least one fiscal year, period, journal, account, fund, and project seeded
- Test user with `Accounting.JournalEntries.*` permissions

## Validation Scenarios

### V1: Journal Entries List Loads with Status Badges

1. Navigate to `/accounting/journal-entries`
2. **Expected**: List renders with columns: entry number, date, status badge, total debit, total credit, fund summary
3. **Expected**: Each entry shows a colored status badge matching its EntryStatus
4. **Expected**: Default sort is newest first (entry number descending)
5. **Expected**: No references to "Move" or "MoveLine" anywhere on the page

### V2: Filters Return Correct Results

1. On the journal entries list, filter by status = "Draft"
2. **Expected**: Only Draft entries shown
3. Filter by status = "Posted"
4. **Expected**: Only Posted entries shown
5. Filter by fund (select a specific fund)
6. **Expected**: Only entries with lines containing that fund are shown
7. Apply multiple filters simultaneously
8. **Expected**: Results satisfy all filter criteria (AND logic)

### V3: Create Journal Entry — Balanced Lines

1. Click "New Entry" button (navigates to `/accounting/journal-entries/create`)
2. Fill header: documentDate, journal, period, fiscalYear, currency
3. Add line 1: account = "1001-Cash", debit = 1000, credit = 0
4. Add line 2: account = "5001-Expense", debit = 0, credit = 1000
5. **Expected**: BalanceIndicator shows balanced (debits = credits)
6. Click Submit
7. **Expected**: Entry created with status Draft, redirected to detail view

### V4: Create Journal Entry — Unbalanced Blocked

1. On the create page, add line 1: debit = 1000, credit = 0
2. Add line 2: debit = 0, credit = 500
3. **Expected**: BalanceIndicator shows imbalance of 500
4. **Expected**: Submit button is disabled or shows error on click
5. **Expected**: Error message indicates "Debits (1000) do not equal Credits (500)"

### V5: Dimension Pickers Load Reference Data

1. On the create page, add a new line
2. Click the Fund dimension picker
3. **Expected**: Dropdown loads with available funds from the API
4. Select a fund
5. **Expected**: Fund is displayed on the line
6. Repeat for Project, Budget Item
7. **Expected**: All dimension pickers load their respective reference data

### V6: Lifecycle Actions — Submit → Approve → Post

1. Open a Draft entry detail view
2. Click "Submit"
3. **Expected**: Status changes to Submitted, status history panel records the transition
4. (Switch to approver user if needed)
5. Click "Approve"
6. **Expected**: Status changes to Approved, approval history panel records the decision
7. Click "Post"
8. **Expected**: Status changes to Posted, entry becomes immutable (no edit/delete buttons)

### V7: Reversal with Bidirectional Link

1. Open a Posted entry detail view
2. Click "Reverse"
3. Fill in reversal reason
4. **Expected**: New reversal entry created with status Draft (or Posted per spec)
5. **Expected**: Original entry status changes to Reversed
6. **Expected**: Original entry detail shows link to reversal entry
7. Navigate to reversal entry detail
8. **Expected**: Reversal entry shows link back to original entry

### V8: Empty State

1. On the journal entries list, filter by status = "Cancelled" (assuming no cancelled entries exist)
2. **Expected**: Empty state message displayed: "No results match the filters"

### V9: Concurrency Conflict

1. Open entry detail in two browser tabs
2. In tab 1, click Submit (Draft → Submitted)
3. In tab 2, attempt to Submit the same entry
4. **Expected**: Tab 2 receives a concurrency conflict error (409)
5. **Expected**: Tab 2 must refresh before retrying

### V10: RTL and Dark Mode

1. Switch UI to Arabic locale
2. **Expected**: All text right-to-left, layout mirrored
3. Switch to dark mode
4. **Expected**: All components render correctly in dark theme, status badges visible

## Run Commands

```bash
# Frontend tests
cd src/Web/ClientApp && npm run test

# Frontend lint
cd src/Web/ClientApp && npm run lint

# Frontend build
cd src/Web/ClientApp && npm run build
```
