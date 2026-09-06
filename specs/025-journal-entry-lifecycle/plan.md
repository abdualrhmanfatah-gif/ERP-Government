# Implementation Plan: Journal Entry Lifecycle Screens

**Branch**: `025-journal-entry-lifecycle` | **Date**: 2026-09-06 | **Spec**: [spec.md](spec.md)

## Summary

Rebuild journal entry lifecycle screens for the six-state machine. 3 pages (list/create/detail), 1 dialog (reverse). POST-style lifecycle actions. Frontend-only.

## FIELD-COVERAGE TABLE

| Field | Screen | Component | Mode | Permission |
|-------|--------|-----------|------|------------|
| `entryNumber` | List, Detail | DataGrid col, Header | Read-only | Read |
| `entryStatus` | List, Detail | StatusBadge | Read-only | Read |
| `documentDate` | Create, Detail | Input, Header | R/W (create) | Read/Create |
| `postingDate` | Detail | Header | Read-only | Read |
| `entryType` | Create, Detail | Select, Header | R/W (create) | Read/Create |
| `journalId` | Create, Detail | Select, Header | R/W (create) | Read/Create |
| `periodId` | Detail | Header | Read-only | Read |
| `fiscalYearId` | Detail | Header | Read-only | Read |
| `narration` | Create, Detail | Textarea, Header | R/W | Read/Create |
| `ref` | Create, Detail | Input, Header | R/W | Read/Create |
| `reversalOfId` | Detail | Reversal link | Read-only | Read |
| `reversalReason` | Detail | Reversal card | Read-only | Read |
| `postedById/At` | Detail | Header | Read-only | Read |
| `cancelledById/At` | Detail | Header | Read-only | Read |
| `isSystemGenerated` | List, Detail | Badge, Header | Read-only | Read |
| `rowVersion` | All mutations | Hidden | Optimistic | Read |
| `sourceEventId` | Detail | Source badge | Read-only | Read |
| `sequence` | Create, Detail | Table col | Auto | Read |
| `accountId` | Create, Detail | Select, Table | R/W (Draft) | UpdateLines |
| `description` | Create, Detail | Input, Table | R/W (Draft) | UpdateLines |
| `currencyId` | Create | Select | R/W (Draft) | UpdateLines |
| `exchangeRate` | Create | Input | R/W (Draft) | UpdateLines |
| `debit` | Create, Detail | Input, Table | R/W (Draft) | UpdateLines |
| `credit` | Create, Detail | Input, Table | R/W (Draft) | UpdateLines |
| `fundId` | Create, Detail | DimensionPickers | R/W (Draft) | UpdateLines |
| `projectId` | Create, Detail | DimensionPickers | R/W (Draft) | UpdateLines |
| `budgetItemId` | Create, Detail | DimensionPickers | R/W (Draft) | UpdateLines |
| `encumbranceId` | Create, Detail | DimensionPickers | R/W (Draft) | UpdateLines |
| `paymentOrderId` | Create, Detail | DimensionPickers | R/W (Draft) | UpdateLines |
| `costCenterId` | Create, Detail | DimensionPickers | R/W (Draft) | UpdateLines |

## DESIGN SECTION

### List Page
- Status filter chips (6 clickable pills, not dropdown)
- Search by entry number/ref
- DataGrid: entryNumber (mono + system badge), documentDate, StatusBadge, journalName, totalDebit, totalCredit

### Create Page
- Header form: documentDate (triggers FiscalYearIndicator), journal, entryType, narration, ref
- Line editor: table + inline editing, XOR validation, DimensionPickers, running balance
- Submit button disabled when unbalanced or zero lines

### Detail Page
- Header card: entryNumber + system badge + documentDate + StatusBadge
- Info grid: period, fiscalYear, journal, narration, ref, postedBy, cancelledBy
- Lines table with dimensions
- EntryLifecycleActions bar (POST-style)
- Reversal card (bidirectional link)
- StatusLogPanel + ApprovalsPanel

## STATE MATRIX

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| List | Skeleton | "لا توجد قيود" | Toast | Guard | N/A | DataGrid + chips |
| Create | N/A | N/A | Toast | Guard | N/A | Form + lines + balance |
| Detail | Spinner | N/A | Error card | Guard (actions) | Not found msg | Full detail + lifecycle |

## TEST MAP

| Test ID | File | Description |
|---------|------|-------------|
| T-025-001 | JournalEntriesListPage.test.tsx | 6 status chips |
| T-025-002 | JournalEntriesListPage.test.tsx | Search by entry number |
| T-025-003 | JournalEntriesListPage.test.tsx | System badge |
| T-025-004 | JournalEntryCreatePage.test.tsx | Header form |
| T-025-005 | JournalEntryLinesEditor.test.tsx | XOR validation |
| T-025-006 | JournalEntryCreatePage.test.tsx | Balance live update |
| T-025-007 | JournalEntryCreatePage.test.tsx | Submit blocked unbalanced |
| T-025-008 | JournalEntryDetailPage.test.tsx | Lifecycle buttons per state |
| T-025-009 | JournalEntryDetailPage.test.tsx | Posted read-only |
| T-025-010 | JournalEntryDetailPage.test.tsx | Reversal linkage |
| T-025-011 | JournalEntryDetailPage.test.tsx | Conflict toast 409 |
| T-025-012 | JournalEntryDetailPage.test.tsx | Status log panel |
| T-025-013 | JournalEntryDetailPage.test.tsx | Approvals panel |
| T-025-014 | StatusBadge.test.tsx | 6 states |
| T-025-015 | ReverseDialog.test.tsx | Reason required |

## COMPLETENESS GATE

```
entryNumber → List (col), Detail (header)
entryStatus → List (StatusBadge), Detail (StatusBadge)
documentDate → Create (input), Detail (header)
postingDate → Detail (header)
entryType → Create (select), Detail (header)
journalId → Create (select), Detail (header)
periodId → Detail (header)
fiscalYearId → Detail (header)
narration → Create (textarea), Detail (header)
ref → Create (input), Detail (header)
reversalOfId → Detail (reversal link)
reversalReason → Detail (reversal card)
postedById/At → Detail (header)
cancelledById/At → Detail (header)
isSystemGenerated → List (badge), Detail (badge)
rowVersion → hooks (optimistic)
sourceEventId → Detail (source badge)
sequence → Create (line table), Detail (line table)
accountId → Create (line editor), Detail (line table)
description → Create (line editor), Detail (line table)
debit → Create (line editor), Detail (line table)
credit → Create (line editor), Detail (line table)
fundId → Create (DimensionPickers), Detail (dimension display)
projectId → Create (DimensionPickers), Detail (dimension display)
budgetItemId → Create (DimensionPickers), Detail (dimension display)
encumbranceId → Create (DimensionPickers), Detail (dimension display)
paymentOrderId → Create (DimensionPickers), Detail (dimension display)
costCenterId → Create (DimensionPickers)
```

## Constitution Check

| Principle | Status |
|-----------|--------|
| I. Layered Architectural Integrity | ✅ PASS |
| II. Bounded Contexts | ✅ PASS |
| III. Server-Side Business-Rule Integrity | ✅ PASS |
| IV. Financial Integrity | ✅ PASS |
| VII. Authorization | ⚠️ PLACEHOLDER |
| VIII. Approval Workflows | ✅ PASS |
| IX. API and Frontend Contract Integrity | ✅ PASS |
| X. UI and Design System Consistency | ✅ PASS |
| XI. Testing, Verification, and Evidence | ✅ PASS |
| XII. Controlled Architectural Change | ✅ PASS |

**Gate**: PASS
