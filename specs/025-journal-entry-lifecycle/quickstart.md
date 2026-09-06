# Quickstart Validation: Journal Entry Lifecycle Screens

**Date**: 2026-09-06
**Feature**: 025-journal-entry-lifecycle

## Prerequisites

- Running dev server (backend + frontend)
- Authenticated user with Accounting.JournalEntries permissions
- At least one active fiscal year with an open period
- At least one active journal
- At least one active currency (base currency configured)

## V1: Create and Edit a Draft

1. Navigate to `/accounting/journal-entries/create`
2. Select document date within an open fiscal period
3. Verify fiscal year and period auto-resolve (FiscalYearIndicator shows correct values)
4. Select a journal, entry type (Standard), enter narration
5. Click "Save as Draft" — entry is created with entryNumber displayed
6. On detail page, add a line: select account, currency, enter debit amount (100)
7. Add second line: same account, enter credit amount (100)
8. Verify balance footer shows balanced state (green, diff = 0)
9. Edit a line description — verify it saves
10. Remove a line — verify balance updates

## V2: Submit and Approve

1. From V1, verify Submit button is visible (Draft state)
2. Click Submit — entry transitions to Submitted
3. Verify Submit/Cancel buttons disappear, Approve/Cancel buttons appear
4. Click Approve — entry transitions to Approved
5. Verify Approve button disappears, Post button appears

## V3: Post with Fiscal Period Guards

1. From V2, click Post — entry transitions to Posted
2. Verify posting date and poster name are displayed
3. Verify only Reverse button is visible

**Fiscal period lock test**: Attempt to post an entry with a locked period — verify "fiscal period is locked for posting" error message appears verbatim.

**Date out of range test**: Attempt to post with document date outside period — verify range error message appears.

## V4: Reverse a Posted Entry

1. From V3, click Reverse — ReverseDialog opens
2. Verify reason textarea is present and required
3. Enter reversal reason
4. Verify counter-line preview shows mirrored amounts (debit↔credit)
5. Confirm — entry transitions to Reversed, counter-entry is Posted
6. Verify reversal linkage: original shows reversal link + reason, counter-entry shows origin link

**Double reversal test**: Open the Reversed entry — verify Reverse button is NOT visible.

**Locked period test**: Attempt reversal on a locked-period entry — verify fiscal control rejection.

## V5: Cancel

1. Create a new draft entry
2. Click Cancel — confirmation dialog appears
3. Confirm — entry transitions to Cancelled
4. Verify CancelledBy/CancelledAt are displayed
5. Verify no lifecycle actions are visible

**Approved entry test**: Open an Approved entry — verify Cancel button is NOT visible.

## V6: Browse and Audit

1. Navigate to `/accounting/journal-entries`
2. Verify status filter chips for all six states (Draft, Submitted, Approved, Posted, Reversed, Cancelled)
3. Click a status chip — list filters to that status
4. Filter by fiscal year — verify filtering works
5. Search by entry number — verify search works
6. Open any entry — verify approvals panel renders
7. Verify status log panel renders with transition history
8. Verify system badge appears on system-generated entries
9. Verify reversal linkage displays correctly (both directions)

## V7: Conflict Handling

1. Open an entry in two browser tabs
2. Edit and save in tab 1
3. Attempt to submit in tab 2 — verify 409 conflict toast appears
4. Verify refetch offer is displayed
5. Click refetch — entry reloads with current data
