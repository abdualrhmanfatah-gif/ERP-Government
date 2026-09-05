# Data Model: Budgeting Frontend Foundation #1 of 5

**Feature**: `011-budgeting-frontend-foundation` | **Date**: 2026-09-04

This document defines the frontend TypeScript interfaces that mirror the backend DTOs exactly. All types are consumed by the shared layer (`features/budgeting/shared/types.ts`) and are the single source of truth for all 5 budgeting frontend specs.

Conventions: Enums serialize as `number` (backend `JsonStringEnumConverter`). Money fields are `number` (2 decimal places). Dates are `string` (ISO date format from `DateOnly`). `rowVersion` is `string` (base64-encoded `byte[]`). Optional fields use `?` suffix.

## Enums

### BudgetControlMethod
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | None | لا يوجد |
| 1 | Warning | تحذير |
| 2 | Blocking | حجب |

### FundType
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | General | عام |
| 1 | Special | خاص |
| 2 | Project | مشروع |

### FundCategory
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | Operating | تشغيلي |
| 1 | Capital | رأسمالي |

### BudgetStatus
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | Draft | مسودة |
| 1 | Submitted | مقدم |
| 2 | Approved | معتمد |
| 3 | Active | نشط |
| 4 | Suspended | معلق |
| 5 | Closed | مغلق |
| 6 | Cancelled | ملغي |

### AppropriationType
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | Original | أصلي |
| 1 | Supplement | تكميلي |
| 2 | Reduction | تخفيض |
| 3 | Adjustment | تعديل |

### AppropriationStatus
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | Draft | مسودة |
| 1 | PendingApproval | بانتظار الاعتماد |
| 2 | Approved | معتمد |
| 3 | Active | نشط |
| 4 | Suspended | معلق |
| 5 | Closed | مغلق |
| 6 | Cancelled | ملغي |

### EncumbranceType
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | Commitment | التزام |
| 1 | Obligational | التزامي |
| 2 | Contractual | تعاقدي |
| 3 | Advance | سلفة |
| 4 | Adjustment | تعديل |

### EncumbranceStatus
| Value | Label | Arabic |
|-------|-------|--------|
| 0 | Draft | مسودة |
| 1 | PendingApproval | بانتظار الاعتماد |
| 2 | Approved | معتمد |
| 3 | Active | نشط |
| 4 | PartialReleased | تحرير جزئي |
| 5 | PartialLiquidated | تسوية جزئية |
| 6 | FullyLiquidated | تسوية كاملة |
| 7 | Closed | مغلق |
| 8 | Cancelled | ملغي |
| 9 | Reversed | معكوس |

## DTOs

### BudgetTypeDto (used in spec #1)
```ts
interface BudgetTypeDto {
  id: number;
  code: string;
  name: string;
  description?: string;
  controlMethod: BudgetControlMethod;
  allowOverrun: boolean;
  isActive: boolean;
  rowVersion: string;
}
```

### FundDto (used in spec #1)
```ts
interface FundDto {
  id: number;
  fundNumber: string;
  fundName: string;
  fundType: FundType;
  fundCategory: FundCategory;
  fiscalYearId?: number;
  legalAuthority: string;
  description?: string;
  defaultRevenueDebitAccountId?: number;
  isActive: boolean;
  rowVersion: string;
}
```

### BudgetClassificationDto (shared type for specs #2-#5)
```ts
interface BudgetClassificationDto {
  id: number;
  code: string;
  name: string;
  parentId?: number;
  level: number; // computed, not stored
  isActive: boolean;
  rowVersion: string;
}
```

### BudgetClassificationTreeDto
```ts
interface BudgetClassificationTreeDto extends BudgetClassificationDto {
  children?: BudgetClassificationTreeDto[];
}
```

### BudgetDto (shared type for spec #2)
```ts
interface BudgetDto {
  id: number;
  budgetNumber: string;
  budgetName: string;
  budgetTypeId: number;
  budgetTypeName: string;
  fiscalYearId: number;
  fundId: number;
  fundName: string;
  totalAmount: number;
  status: BudgetStatus;
  allowOverrun?: boolean;
  effectiveAllowOverrun: boolean;
  effectiveFrom: string;
  effectiveTo?: string;
  description?: string;
  rowVersion: string;
}
```

### BudgetItemDto (shared type for spec #2)
```ts
interface BudgetItemDto {
  id: number;
  itemCode: string;
  itemName: string;
  budgetId: number;
  parentId?: number;
  accountId?: number;
  fundId?: number;
  costCenterId?: number;
  budgetClassificationId?: number;
  level: number; // computed
  allowOverrun?: boolean;
  effectiveAllowOverrun: boolean;
  isActive: boolean;
  rowVersion: string;
}
```

### AppropriationDto (shared type for spec #3)
```ts
interface AppropriationDto {
  // Stored
  id: number;
  appropriationNumber: string;
  budgetId: number;
  budgetItemId: number;
  appropriationType: AppropriationType;
  documentType: string;
  documentId: number;
  amount: number;
  status: AppropriationStatus;
  rowVersion: string;
  // Projected
  budgetNumber: string;
  budgetName: string;
  fundId: number;
  fundName: string;
  fiscalYearId: number;
  availableForItem: number;
  latestApproval?: ApprovalDecisionDto;
  createdBy: string;
}
```

### EncumbranceDto (shared type for spec #4)
```ts
interface EncumbranceDto {
  // Stored
  id: number;
  encumbranceNumber: string;
  encumbranceType: EncumbranceType;
  appropriationId: number;
  vendorId?: number;
  purchaseOrderId?: number;
  documentType: string;
  documentId: number;
  description?: string;
  encumbranceDate: string;
  amount: number;
  status: EncumbranceStatus;
  reversalOfId?: number;
  reversalReason?: string;
  rowVersion: string;
  // Projected
  isReversed: boolean;
  budgetId: number;
  budgetNumber: string;
  budgetItemId: number;
  itemCode: string;
  fundId: number;
  fundName: string;
  fiscalYearId: number;
  availableForEncumbrance: number;
  latestApproval?: ApprovalDecisionDto;
  createdBy: string;
}
```

### AvailabilityDto (used in spec #1 AvailabilityIndicator)
```ts
interface ItemAvailabilityDto {
  budgetItemId: number;
  netAppropriated: number;
  totalSupplement: number;
  totalReduction: number;
  totalAdjustment: number;
  available: number;
  controlMethod: BudgetControlMethod;
  effectiveAllowOverrun: boolean;
  warning?: string;
}

interface EncumbranceAvailabilityDto {
  appropriationId: number;
  budgetItemId: number;
  netAppropriated: number;
  totalEncumbered: number;
  available: number;
  controlMethod: BudgetControlMethod;
  effectiveAllowOverrun: boolean;
  warning?: string;
}
```

### ApprovalDecisionDto (shared projection type)
```ts
interface ApprovalDecisionDto {
  decision: string;
  decisionAt: string;
  approverUserId: string;
  requiredRole?: string;
  reason?: string;
  evaluationSnapshotJson?: string;
}
```

## Create/Update Command Types

### CreateBudgetTypeCommand
```ts
interface CreateBudgetTypeCommand {
  code: string;
  name: string;
  description?: string;
  controlMethod: BudgetControlMethod;
  allowOverrun: boolean;
}
```

### UpdateBudgetTypeCommand
```ts
interface UpdateBudgetTypeCommand {
  id: number;
  rowVersion: string;
  code: string;
  name: string;
  description?: string;
  controlMethod: BudgetControlMethod;
  allowOverrun: boolean;
}
```

### ToggleBudgetTypeActiveCommand
```ts
interface ToggleBudgetTypeActiveCommand {
  id: number;
  rowVersion: string;
  isActive: boolean;
}
```

### CreateFundCommand
```ts
interface CreateFundCommand {
  fundNumber: string;
  fundName: string;
  fundType: FundType;
  fundCategory: FundCategory;
  fiscalYearId?: number;
  legalAuthority: string;
  description?: string;
  defaultRevenueDebitAccountId?: number;
}
```

### UpdateFundCommand
```ts
interface UpdateFundCommand {
  id: number;
  rowVersion: string;
  fundNumber: string;
  fundName: string;
  fundType: FundType;
  fundCategory: FundCategory;
  fiscalYearId?: number;
  legalAuthority: string;
  description?: string;
  defaultRevenueDebitAccountId?: number;
}
```

## Arabic Label Maps

Each enum has a corresponding `Record<number, string>` map for Arabic display:

```ts
const budgetControlMethodLabels: Record<BudgetControlMethod, string> = {
  [BudgetControlMethod.None]: 'لا يوجد',
  [BudgetControlMethod.Warning]: 'تحذير',
  [BudgetControlMethod.Blocking]: 'حجب',
};

const fundTypeLabels: Record<FundType, string> = {
  [FundType.General]: 'عام',
  [FundType.Special]: 'خاص',
  [FundType.Project]: 'مشروع',
};

const fundCategoryLabels: Record<FundCategory, string> = {
  [FundCategory.Operating]: 'تشغيليلي',
  [FundCategory.Capital]: 'رأسمالي',
};

// ... same pattern for BudgetStatus, AppropriationType, AppropriationStatus,
//     EncumbranceType, EncumbranceStatus
```

Unknown enum value fallback: `labels[value] ?? String(value)` — displays the raw numeric value as a string rather than crashing.

## File Structure

```text
features/budgeting/shared/
├── types.ts          # All interfaces, enums, label maps, command types
├── client.ts         # Typed API functions + cache key factory
├── index.ts          # Barrel re-exports
└── components/
    └── AvailabilityIndicator.tsx
```
