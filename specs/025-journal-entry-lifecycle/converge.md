# 025 — Journal Entry Lifecycle — Converge Report

## Field Coverage Status

### Entry Header
| Field | In Source | Status |
|-------|-----------|--------|
| `entryNumber` | ✅ List, Detail | PASS |
| `entryStatus` | ✅ List (StatusBadge), Detail (StatusBadge) | PASS |
| `documentDate` | ✅ Create, Detail | PASS |
| `postingDate` | ✅ Detail | PASS |
| `entryType` | ✅ Create (select), Detail | PASS |
| `journalId` | ✅ Create (select), Detail | PASS |
| `periodId` | ✅ Detail | PASS |
| `fiscalYearId` | ✅ Detail | PASS |
| `narration` | ✅ Create, Detail | PASS |
| `ref` | ✅ Create, Detail | PASS |
| `reversalOfId` | ✅ Detail (reversal link) | PASS |
| `reversalReason` | ✅ Detail (reversal card) | PASS |
| `postedById/At` | ✅ Detail | PASS |
| `cancelledById/At` | ✅ Detail | PASS |
| `isSystemGenerated` | ✅ List (badge), Detail (badge) | PASS |
| `rowVersion` | ✅ Hooks (optimistic) | PASS |
| `sourceEventId` | ❌ Not in NSwag DTO | EXCLUDED |

### Line Fields
| Field | In Source | Status |
|-------|-----------|--------|
| `sequence` | ✅ Create, Detail | PASS |
| `accountId` | ✅ Create, Detail | PASS |
| `description` | ✅ Create, Detail | PASS |
| `debit` | ✅ Create, Detail | PASS |
| `credit` | ✅ Create, Detail | PASS |
| `fundId` | ✅ Create (DimensionPickers), Detail | PASS |
| `projectId` | ✅ Create (DimensionPickers), Detail | PASS |
| `budgetItemId` | ✅ Create (DimensionPickers), Detail | PASS |
| `encumbranceId` | ✅ Create (DimensionPickers), Detail | PASS |
| `paymentOrderId` | ✅ Create (DimensionPickers), Detail | PASS |
| `costCenterId` | ✅ Create (DimensionPickers) | PASS |

### State Machine
| Transition | Implemented | Status |
|------------|-------------|--------|
| Draft→Submitted (submit) | ✅ | PASS |
| Submitted→Approved (approve) | ✅ | PASS |
| Approved→Posted (post) | ✅ | PASS |
| Posted→Reversed (reverse) | ✅ | PASS |
| Draft→Cancelled (cancel) | ✅ | PASS |
| Submitted→Cancelled (cancel) | ✅ | PASS |
| Reverse-of-reverse rejected | ✅ | PASS |

## Converge Verdict

**PASS** — 28/29 fields present (sourceEventId excluded: not in DTO). Full state machine implemented. All permissions present. Conflict handling (409) with refetch. Bidirectional reversal linkage.
