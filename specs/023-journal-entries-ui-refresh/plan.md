# Implementation Plan: Journal Entries UI Refresh

**Branch**: `023-journal-entries-ui-refresh` | **Date**: 2026-09-06 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/023-journal-entries-ui-refresh/spec.md`

## Summary

Replace the broken Move-based accounting screens (MovesListPage/MoveCreatePage/MoveDetailPage) with JournalEntry-based pages using the renamed `/api/JournalEntries` NSwag client. Three pages: list (status badges, filters), create (line editor + dimension pickers + balance guard), detail (lines + lifecycle + reversal link). Reuse shared ApprovalsPanel, StatusLogPanel from documents feature.

## Technical Context

**Language/Version**: TypeScript 5.x (React 19, Vite)

**Primary Dependencies**: React 19, react-router-dom, react-hook-form, zod, TanStack Query, NSwag-generated `JournalEntriesClient`

**Testing**: Vitest + React Testing Library

**Target Platform**: Web (SPA), Arabic-first RTL, dark mode

## FIELD-COVERAGE TABLE

Every field name from spec.md → screen → component → editable/read-only → formatter → permission.

### JournalEntry Header

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| `entryNumber` | List, Detail | DataGrid col, Header card | Read-only | Plain text + isSystemGenerated badge | Read |
| `ref` | List (search), Create, Detail | Input, Header card | Read/Write | Plain text | Read |
| `documentDate` | List, Create, Detail | DataGrid col, Input, Header card | Read/Write | `toLocaleDateString('ar-YE')` | Read |
| `postingDate` | Detail | Header card | Read-only | `toLocaleDateString('ar-YE')` | Read |
| `entryType` | Create, Detail | Select, Header card | Read/Write (create only) | Arabic label map | Read |
| `journalId` | List (filter), Create, Detail | Select, DataGrid col, Header card | Read/Write (create only) | journalName display | Read |
| `periodId` | List (filter), Detail | Header card | Read-only (auto-resolved) | periodName display | Read |
| `fiscalYearId` | List (filter), Detail | Header card | Read-only (auto-resolved) | fiscalYearName display | Read |
| `narration` | Create, Detail | Textarea, Header card | Read/Write | Plain text | Read |
| `sourceEventId` | Detail | Source badge | Read-only | Badge + link | Read |
| `isSystemGenerated` | List, Detail | Badge, Header card | Read-only | "نظام" badge | Read |
| `rowVersion` | Create, Detail | Hidden (optimistic) | Read-only | Conflict toast + refetch | Read |
| `reversalOfId` | Detail | Reversal link | Read-only | Link to original | Read |
| `reversalReason` | Detail | Reversal card | Read-only | Plain text | Read |

### JournalEntryLine

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| `sequence` | Create (table), Detail (table) | Table col # | Auto-order | Integer | Read |
| `accountId` | Create (editor), Detail (table) | Select, Table col | Read/Write (create only) | accountCode + accountName | Read |
| `description` | Create (editor), Detail (table) | Input, Table col | Read/Write (create only) | Plain text | Read |
| `currencyId` | Create (editor) | Select | Read/Write (create only) | code display | Read |
| `exchangeRate` | Create (editor) | Input | Read/Write (create only) | Number | Read |
| `debit` | Create (editor), Detail (table), List (aggregated) | Input, Table col | Read/Write (create only) | `toLocaleString('ar-YE')` | Read |
| `credit` | Create (editor), Detail (table), List (aggregated) | Input, Table col | Read/Write (create only) | `toLocaleString('ar-YE')` | Read |
| `costCenterId` | Create (editor), Detail (table) | DimensionPickers | Read/Write (create only) | costCenterName | Read |
| `fundId` | Create (editor), Detail (table) | DimensionPickers | Read/Write (create only) | fundName | Read |
| `projectId` | Create (editor), Detail (table) | DimensionPickers | Read/Write (create only) | projectName | Read |
| `budgetItemId` | Create (editor), Detail (table) | DimensionPickers | Read/Write (create only) | budgetItemCode | Read |
| `encumbranceId` | Create (editor), Detail (table) | DimensionPickers | Read/Write (create only) | encumbranceNumber | Read |
| `paymentOrderId` | Create (editor), Detail (table) | DimensionPickers | Read/Write (create only) | paymentOrderNumber | Read |
| `id` (lineId) | Create (remove), Detail | Hidden | Read-only | Long identifier | UpdateLines |

### List Aggregates

| Field | Screen | Component | Mode | Formatter | Permission |
|-------|--------|-----------|------|-----------|------------|
| `totalDebit` | List, Detail | DataGrid col, BalanceIndicator | Read-only (computed) | `toLocaleString('ar-YE')` | Read |
| `totalCredit` | List, Detail | DataGrid col, BalanceIndicator | Read-only (computed) | `toLocaleString('ar-YE')` | Read |

## DESIGN SECTION

**Design tokens source**: DESIGN.md + tokens.ts (read before any UI work).

### List Page (`JournalEntriesListPage.tsx`)

- **Layout**: `max-w-6xl mx-auto py-8 px-6`, RTL `dir="rtl"`
- **Header**: Title + subtitle + "New Entry" button
- **Filter bar**: Status chips (pill buttons with color toggle), search input
- **DataGrid**: Columns — entryNumber (mono font + isSystemGenerated badge), documentDate (locale date), entryStatus (StatusBadge), journalName, totalDebit, totalCredit
- **Tokens**: `var(--color-surface)`, `var(--color-primary)`, `var(--color-on-primary)`, `var(--color-outlineVariant)`, `var(--color-focus-ring)`
- **RTL**: All text naturally RTL via dir="rtl"; table headers use `text-right`/`text-left` for number alignment
- **Dark mode**: All via CSS vars — no hardcoded colors
- **Components reused**: DataGrid, StatusBadge, FilterBar (pattern)

### Create Page (`JournalEntryCreatePage.tsx`)

- **Layout**: `max-w-6xl mx-auto py-8 px-6`, RTL
- **Header form card**: documentDate, journalId, entryType, baseCurrencyId, ref, narration
- **Line editor card**: Table of lines + "Add Line" button + inline editing form
- **Balance card**: BalanceIndicator component
- **Dimension pickers**: DimensionPickers component (fund, project, budgetItem, encumbrance, paymentOrder)
- **Submit**: Bottom action bar with save + cancel buttons
- **Tokens**: Same as list; `var(--color-secondary-container)` for add line button
- **RTL**: Form labels above inputs, grid layout

### Detail Page (`JournalEntryDetailPage.tsx`)

- **Layout**: `max-w-6xl mx-auto py-8 px-6`, RTL
- **Header card**: entryNumber + isSystemGenerated badge + documentDate + StatusBadge
- **Info grid**: periodName, fiscalYearName, journalName, narration, ref, postedBy, cancelledBy
- **Lines table**: Same columns as create view
- **Actions card**: Lifecycle buttons (submit/approve/post/reverse/cancel) contextual to status
- **Reversal card**: Bidirectional link when applicable
- **Status log**: StatusLogPanel (from documents)
- **Approvals**: ApprovalsPanel (from documents, shown for Submitted/Approved)
- **Conflict toast**: 409 handling with refetch

### Components (existing/reused)

- `StatusBadge` — EntryStatus → colored badge (new, in accounting/components)
- `BalanceIndicator` — Running debit/credit totals (existing)
- `ReverseDialog` — Reverse confirmation with reason (existing, updated types)
- `DimensionPickers` — Fund/project/budgetItem/encumbrance/paymentOrder (new, in accounting/components)
- `ApprovalsPanel` — From documents feature (reused)
- `StatusLogPanel` — From documents feature (reused)

## STATE MATRIX

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| List | Skeleton rows | "لا توجد قيود" empty state | Error toast | Permission guard hides page | N/A | DataGrid with status chips, search, filters |
| Create | N/A | N/A | Toast on submit error | Permission guard hides "New Entry" button | N/A | Header form + line editor + balance indicator |
| Detail | Spinner "جاري تحميل القيد..." | N/A | Error card "خطأ في تحميل القيد" + back link | Permission guard hides action buttons | "القيد غير موجودة" + back link | Header card + lines table + actions + status log + approvals |

## TEST MAP

One Vitest behavior test per expected test item.

| Spec Test ID | File | Test Description | Asserts |
|-------------|------|-----------------|---------|
| T-023-001 | JournalEntriesListPage.test.tsx | Renders entries with status badges | StatusBadge rendered for each entry |
| T-023-002 | JournalEntriesListPage.test.tsx | Status filter returns matching entries | Filtered entries match status |
| T-023-003 | JournalEntriesListPage.test.tsx | Number search filters by entryNumber/ref | Search matches entryNumber |
| T-023-004 | JournalEntriesListPage.test.tsx | Empty state shows message | "لا توجد قيود" text present |
| T-023-005 | JournalEntriesListPage.test.tsx | Zero Move/MoveLine references | No "Move" or "MoveLine" in DOM |
| T-023-006 | JournalEntryCreatePage.test.tsx | Form renders header fields and line editor | Form fields present |
| T-023-007 | JournalEntryLinesEditor.test.tsx | Debit XOR credit validation | Both-set and both-zero blocked |
| T-023-008 | JournalEntryCreatePage.test.tsx | Balanced lines enable submit | Submit enabled when balanced |
| T-023-009 | JournalEntryCreatePage.test.tsx | Unbalanced lines block submit | Submit disabled when unbalanced |
| T-023-010 | DimensionPickers.test.tsx | Dimension pickers load reference data | Pickers render with data |
| T-023-011 | JournalEntryDetailPage.test.tsx | Header shows entry number/date/status/totals | Values rendered correctly |
| T-023-012 | JournalEntryDetailPage.test.tsx | Lines table renders with dimensions | Lines rows present |
| T-023-013 | JournalEntryDetailPage.test.tsx | Lifecycle buttons appear based on status | Correct buttons per status |
| T-023-014 | JournalEntryDetailPage.test.tsx | Posted entries show no edit/delete | No edit/delete buttons |
| T-023-015 | JournalEntryDetailPage.test.tsx | Status log panel shows transitions | StatusLogPanel rendered |
| T-023-016 | JournalEntryDetailPage.test.tsx | Reversed entry shows link to reversal | Reversal link present |
| T-023-017 | JournalEntryDetailPage.test.tsx | Reversal entry shows link to original | Original link present |
| T-023-018 | StatusBadge.test.tsx | Renders correct color per status | CSS class per status |

## COMPLETENESS GATE

Every field name from spec.md Field Contract must appear in implemented source. Converge verifies each:

```
entryNumber → JournalEntriesListPage.tsx (col), JournalEntryDetailPage.tsx (header)
ref → JournalEntriesListPage.tsx (search), JournalEntryCreatePage.tsx (input), JournalEntryDetailPage.tsx (display)
documentDate → JournalEntriesListPage.tsx (col), JournalEntryCreatePage.tsx (input), JournalEntryDetailPage.tsx (display)
postingDate → JournalEntryDetailPage.tsx (display)
entryType → JournalEntryCreatePage.tsx (select), JournalEntryDetailPage.tsx (display via entryType)
journalId → JournalEntryCreatePage.tsx (select), JournalEntriesListPage.tsx (filter + col)
periodId → JournalEntryCreatePage.tsx (auto-resolve), JournalEntryDetailPage.tsx (display)
fiscalYearId → JournalEntryCreatePage.tsx (auto-resolve), JournalEntryDetailPage.tsx (display)
narration → JournalEntryCreatePage.tsx (textarea), JournalEntryDetailPage.tsx (display)
sourceEventId → (badge + link in detail, if present)
isSystemGenerated → JournalEntriesListPage.tsx (badge), JournalEntryDetailPage.tsx (badge)
rowVersion → useJournalEntries.ts (optimistic concurrency)
reversalOfId → JournalEntryDetailPage.tsx (reversal link)
reversalReason → JournalEntryDetailPage.tsx (reversal card)
sequence → JournalEntryCreatePage.tsx (line table), JournalEntryDetailPage.tsx (line table)
accountId → JournalEntryCreatePage.tsx (line editor), JournalEntryDetailPage.tsx (line table)
description → JournalEntryCreatePage.tsx (line editor), JournalEntryDetailPage.tsx (line table)
currencyId → JournalEntryCreatePage.tsx (line editor)
exchangeRate → JournalEntryCreatePage.tsx (line editor)
debit → JournalEntryCreatePage.tsx (line editor), JournalEntryDetailPage.tsx (line table), JournalEntriesListPage.tsx (totalDebit)
credit → JournalEntryCreatePage.tsx (line editor), JournalEntryDetailPage.tsx (line table), JournalEntriesListPage.tsx (totalCredit)
costCenterId → JournalEntryCreatePage.tsx (DimensionPickers)
fundId → JournalEntryCreatePage.tsx (DimensionPickers), JournalEntryDetailPage.tsx (dimension display)
projectId → JournalEntryCreatePage.tsx (DimensionPickers), JournalEntryDetailPage.tsx (dimension display)
budgetItemId → JournalEntryCreatePage.tsx (DimensionPickers), JournalEntryDetailPage.tsx (dimension display)
encumbranceId → JournalEntryCreatePage.tsx (DimensionPickers), JournalEntryDetailPage.tsx (dimension display)
paymentOrderId → JournalEntryCreatePage.tsx (DimensionPickers), JournalEntryDetailPage.tsx (dimension display)
lineId (id) → JournalEntryCreatePage.tsx (remove by index), JournalEntryDetailPage.tsx (key)
```

## Constitution Check

| Principle | Status | Notes |
|---|---|---|
| I. Layered Architectural Integrity | ✅ PASS | Frontend consumes HTTP contract only |
| II. Bounded Contexts | ✅ PASS | Accounting feature self-contained |
| III. Server-Side Business-Rule Integrity | ✅ PASS | Balance/lifecycle enforced server-side |
| IV. Financial Integrity | ✅ PASS | Balance guard is UX only |
| VI. Data Integrity | ✅ PASS | Optimistic concurrency via RowVersion |
| VII. Authorization and SoD | ⚠️ PLACEHOLDER | Permission checks are UX only |
| VIII. Approval Workflows | ✅ PASS | Reuses ApprovalsPanel/StatusLogPanel |
| IX. API and Frontend Contract Integrity | ✅ PASS | Uses NSwag JournalEntriesClient |
| X. UI and Design System Consistency | ✅ PASS | Design tokens from central source |
| XI. Testing, Verification, and Evidence | ✅ PASS | TDD mandatory |
| XII. Controlled Architectural Change | ✅ PASS | No boundary changes |

**Gate result**: PASS
