# Data Model: Budget Officer Workspace

**Feature**: 021-budget-officer-workspace
**Date**: 2026-09-06

## Overview

This feature is UI-only. No new database entities. All data comes from existing backend APIs. The only client-side data structure is the monthly plan stored in localStorage.

## Backend DTOs (Read-Only — consumed by UI)

### BudgetDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | PK |
| budgetNumber | string | BGT-{seq} |
| budgetName | string | |
| budgetTypeId | number | FK → BudgetType |
| budgetTypeName | string | joined |
| fiscalYearId | number | FK → FiscalYear |
| fundId | number | FK → Fund |
| fundName | string | joined |
| totalAmount | number | decimal(23,2) |
| status | BudgetStatus | enum: Draft/Submitted/Approved/Active/Suspended/Closed/Cancelled |
| allowOverrun | boolean? | nullable, inherits from BudgetType |
| effectiveAllowOverrun | boolean | computed |
| effectiveFrom | string | DateOnly |
| effectiveTo | string? | nullable DateOnly |
| description | string? | |
| rowVersion | string | concurrency token |

### BudgetItemDto (Tree)

| Field | Type | Notes |
|-------|------|-------|
| id | number | PK |
| itemCode | string | unique per budget |
| itemName | string | |
| budgetId | number | FK → Budget |
| parentId | number? | self-ref FK, nullable |
| accountId | number? | FK → Account |
| fundId | number? | FK → Fund |
| costCenterId | number? | FK → CostCenter |
| budgetClassificationId | number? | FK → BudgetClassification |
| level | number | computed from hierarchy |
| allowOverrun | boolean? | nullable, inherits |
| effectiveAllowOverrun | boolean | computed |
| isActive | boolean | |
| rowVersion | string | |

### AppropriationDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | PK |
| appropriationNumber | string | APR-{seq} |
| budgetId | number | FK → Budget |
| budgetItemId | number | FK → BudgetItem |
| appropriationType | AppropriationType | enum: Original/Supplement/Reduction/Adjustment |
| documentType | string | |
| documentId | number | |
| amount | number | decimal(23,2) |
| status | AppropriationStatus | enum |
| rowVersion | string | |
| budgetNumber | string | joined |
| budgetName | string | joined |
| fundId | number | joined via Budget |
| fundName | string | joined |
| fiscalYearId | number | joined |
| availableForItem | number | computed |
| latestApproval | ApprovalDecisionDto? | |
| createdBy | string | |

### EncumbranceDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | PK |
| encumbranceNumber | string | ENC-{seq} |
| encumbranceType | EncumbranceType | enum |
| appropriationId | number | FK → Appropriation |
| vendorId | number? | FK → Suppliers |
| purchaseOrderId | number? | FK → PurchaseOrders |
| documentType | string | |
| documentId | number | |
| description | string? | |
| encumbranceDate | string | DateOnly |
| amount | number | decimal(23,2) |
| status | EncumbranceStatus | enum |
| reversalOfId | number? | self-ref FK |
| reversalReason | string? | |
| rowVersion | string | |
| isReversed | boolean | computed |
| budgetId | number | joined via Appropriation |
| budgetNumber | string | joined |
| budgetItemId | number | joined |
| itemCode | string | joined |
| fundId | number | joined |
| fundName | string | joined |
| fiscalYearId | number | joined |
| availableForEncumbrance | number | computed |
| latestApproval | ApprovalDecisionDto? | |
| createdBy | string | |

### ItemAvailabilityDto

| Field | Type | Notes |
|-------|------|-------|
| budgetItemId | number | |
| netAppropriated | number | |
| totalSupplement | number | |
| totalReduction | number | |
| totalAdjustment | number | |
| available | number | |
| controlMethod | BudgetControlMethod | enum: None/Warning/Blocking |
| effectiveAllowOverrun | boolean | |
| warning | string? | |

### EncumbranceAvailabilityDto

| Field | Type | Notes |
|-------|------|-------|
| appropriationId | number | |
| budgetItemId | number | |
| netAppropriated | number | |
| totalEncumbered | number | |
| available | number | |
| controlMethod | BudgetControlMethod | |
| effectiveAllowOverrun | boolean | |
| warning | string? | |

## Client-Side Data Structures

### MonthlyPlan (localStorage)

Key: `monthly-plan-{budgetItemId}`

```typescript
interface MonthlyPlan {
  budgetItemId: number;
  months: [
    number, // January
    number, // February
    number, // March
    number, // April
    number, // May
    number, // June
    number, // July
    number, // August
    number, // September
    number, // October
    number, // November
    number, // December
  ];
  updatedAt: string; // ISO timestamp
}
```

## Enums (already defined in shared/types.ts)

- `BudgetStatus`: Draft=0, Submitted=1, Approved=2, Active=3, Suspended=4, Closed=5, Cancelled=6
- `AppropriationType`: Original=0, Supplement=1, Reduction=2, Adjustment=3
- `AppropriationStatus`: Draft=0, PendingApproval=1, Approved=2, Active=3, Suspended=4, Closed=5, Cancelled=6
- `EncumbranceType`: Commitment=0, Obligational=1, Contractual=2, Advance=3, Adjustment=4
- `EncumbranceStatus`: Draft=0, PendingApproval=1, Approved=2, Active=3, PartialReleased=4, PartialLiquidated=5, FullyLiquidated=6, Closed=7, Cancelled=8, Reversed=9
- `BudgetControlMethod`: None=0, Warning=1, Blocking=2
