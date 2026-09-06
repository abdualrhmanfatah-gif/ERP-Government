# 023 — Journal Entries UI Refresh — Converge Report

## Field Coverage Status

| Field | In Source | Status |
|-------|-----------|--------|
| `entryNumber` | ✅ List (col), Detail (header) | PASS |
| `ref` | ✅ List (search), Create (input), Detail (display) | PASS |
| `documentDate` | ✅ List (col), Create (input), Detail (display) | PASS |
| `postingDate` | ✅ Detail (display) | PASS |
| `entryType` | ✅ Create (select), Detail (display) | PASS |
| `journalId` | ✅ Create (select), List (filter + col) | PASS |
| `periodId` | ✅ Create (auto-resolve), Detail (display) | PASS |
| `fiscalYearId` | ✅ Create (auto-resolve), Detail (display) | PASS |
| `narration` | ✅ Create (textarea), Detail (display) | PASS |
| `sourceEventId` | ❌ Not in NSwag DTO | EXCLUDED — backend doesn't return this field |
| `isSystemGenerated` | ✅ List (badge), Detail (badge) | PASS |
| `rowVersion` | ✅ Optimistic concurrency in hooks | PASS |
| `reversalOfId` | ✅ Detail (reversal link) | PASS |
| `reversalReason` | ✅ Detail (reversal card) | PASS |
| `sequence` | ✅ Create (line table), Detail (line table) | PASS |
| `accountId` | ✅ Create (line editor), Detail (line table) | PASS |
| `description` | ✅ Create (line editor), Detail (line table) | PASS |
| `currencyId` | ✅ Create (line editor) | PASS |
| `exchangeRate` | ✅ Create (line editor) | PASS |
| `debit` | ✅ Create (line editor), Detail (line table), List (totalDebit) | PASS |
| `credit` | ✅ Create (line editor), Detail (line table), List (totalCredit) | PASS |
| `costCenterId` | ✅ Create (DimensionPickers) | PASS |
| `fundId` | ✅ Create (DimensionPickers), Detail (dimension display) | PASS |
| `projectId` | ✅ Create (DimensionPickers), Detail (dimension display) | PASS |
| `budgetItemId` | ✅ Create (DimensionPickers), Detail (dimension display) | PASS |
| `encumbranceId` | ✅ Create (DimensionPickers), Detail (dimension display) | PASS |
| `paymentOrderId` | ✅ Create (DimensionPickers), Detail (dimension display) | PASS |
| `lineId` (id) | ✅ Create (remove), Detail (key) | PASS |

## Design Lint

| Check | Status |
|-------|--------|
| All colors from tokens.ts | ✅ Uses var(--color-*) CSS vars |
| No hardcoded colors | ✅ |
| RTL dir="rtl" | ✅ All pages |
| Dark mode via CSS vars | ✅ |
| Focus ring from tokens | ✅ focus:ring-[var(--color-focus-ring)] |

## State Matrix

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| List | ✅ Skeleton | ✅ "لا توجد قيود" | ✅ Error toast | ✅ Permission guard | N/A | ✅ DataGrid |
| Create | N/A | N/A | ✅ Toast on error | ✅ Permission guard | N/A | ✅ Form |
| Detail | ✅ Spinner | N/A | ✅ Error card + back | ✅ Permission guard | ✅ Not found msg | ✅ Full detail |

## Converge Verdict

**PASS** — 27/28 fields present in source (sourceEventId excluded: not in NSwag DTO). All 3 pages have full state matrix. All tests exist. Design lint pass. RTL/Dark mode verified.
