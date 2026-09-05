# Data Model: Rebuild Budgeting Module Backend

**Date**: 2026-09-05
**Feature**: 013-budgeting-backend-rebuild
**Spec**: [spec.md](spec.md)

## Enums

### BudgetControlMethod
| Value | Name |
|-------|------|
| 0 | None |
| 1 | Warning |
| 2 | Blocking |

### BudgetStatus
| Value | Name |
|-------|------|
| 0 | Draft |
| 1 | Submitted |
| 2 | Approved |
| 3 | Active |
| 4 | Suspended |
| 5 | Closed |
| 6 | Cancelled |

### FundType
| Value | Name |
|-------|------|
| 0 | General |
| 1 | Special |
| 2 | Project |

### FundCategory
| Value | Name |
|-------|------|
| 0 | Operating |
| 1 | Capital |

### AppropriationType
| Value | Name |
|-------|------|
| 0 | Original |
| 1 | Supplement |
| 2 | Reduction |
| 3 | Transfer |
| 4 | Adjustment |

### AppropriationStatus
| Value | Name |
|-------|------|
| 0 | Draft |
| 1 | PendingApproval |
| 2 | Approved |
| 3 | Active |
| 4 | Suspended |
| 5 | Closed |
| 6 | Cancelled |
| 7 | Reversed |

### EncumbranceType
| Value | Name |
|-------|------|
| 0 | Commitment |
| 1 | Obligational |
| 2 | Contractual |
| 3 | Advance |
| 4 | Adjustment |

### EncumbranceStatus
| Value | Name |
|-------|------|
| 0 | Draft |
| 1 | PendingApproval |
| 2 | Approved |
| 3 | Active |
| 4 | PartiallyReleased |
| 5 | PartiallyLiquidated |
| 6 | FullyLiquidated |
| 7 | Closed |
| 8 | Cancelled |
| 9 | Reversed |

## Entities

### BudgetType
| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, Identity | |
| Code | string(50) | UNIQUE, NOT NULL | |
| Name | string(200) | NOT NULL | |
| Description | string(500) | Nullable | |
| ControlMethod | BudgetControlMethod | NOT NULL, default None | Enum: None/Warning/Blocking |
| AllowOverrun | bool | NOT NULL, default false | Root of override chain |
| IsActive | bool | NOT NULL, default true | |
| RowVersion | byte[] | ROWVERSION | Optimistic concurrency |
| Created | DateTime | NOT NULL | Audit |
| CreatedBy | string | NOT NULL | Audit |
| LastModified | DateTime? | Nullable | Audit |
| LastModifiedBy | string? | Nullable | Audit |

**Indexes**: IX_BudgetType_Code (unique)

### Fund
| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, Identity | |
| FundNumber | string | UNIQUE, NOT NULL | |
| FundName | string | NOT NULL | |
| FundType | FundType | NOT NULL | Enum: General/Special/Project |
| FundCategory | FundCategory | NOT NULL | Enum: Operating/Capital |
| FiscalYearId | int? | Nullable FK -> FiscalYears | |
| LegalAuthority | string | NOT NULL | |
| Description | string? | Nullable | |
| DefaultRevenueDebitAccountId | int? | Nullable FK -> Accounts | |
| IsActive | bool | NOT NULL, default true | |
| RowVersion | byte[] | ROWVERSION | |
| Created/CreatedBy/LastModified/LastModifiedBy | | Audit fields | |

**Indexes**: IX_Fund_FundNumber (unique), IX_Fund_FiscalYearId (FK)

### BudgetClassification
| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, Identity | |
| Code | string | UNIQUE, NOT NULL | |
| Name | string | NOT NULL | |
| ParentId | int? | Nullable FK -> self, Restrict | |
| IsActive | bool | NOT NULL, default true | |
| RowVersion | byte[] | ROWVERSION | |
| Created/CreatedBy/LastModified/LastModifiedBy | | Audit fields | |

**Indexes**: IX_BudgetClassification_Code (unique), IX_BudgetClassification_ParentId (FK)
**Note**: Level is NOT stored — computed at query time via recursive CTE

### Budget
| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, Identity | |
| BudgetNumber | string | UNIQUE, NOT NULL | BGT sequence |
| BudgetName | string | NOT NULL | |
| BudgetTypeId | int | FK -> BudgetTypes, NOT NULL | |
| FiscalYearId | int | FK -> FiscalYears, NOT NULL | |
| FundId | int | FK -> Funds, NOT NULL | |
| TotalAmount | decimal(23,2) | NOT NULL | |
| Status | BudgetStatus | NOT NULL, default Draft | |
| AllowOverrun | bool? | Nullable | null = inherit BudgetType.AllowOverrun |
| EffectiveFrom | DateOnly | NOT NULL | |
| EffectiveTo | DateOnly? | Nullable | |
| Description | string? | Nullable | |
| RowVersion | byte[] | ROWVERSION | |
| Created/CreatedBy/LastModified/LastModifiedBy | | Audit fields | |

**Indexes**: IX_Budget_BudgetNumber (unique), IX_Budget_BudgetTypeId (FK), IX_Budget_FiscalYearId (FK), IX_Budget_FundId (FK)

### BudgetItem
| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, Identity | |
| ItemCode | string | NOT NULL | |
| ItemName | string | NOT NULL | |
| BudgetId | int | FK -> Budgets, NOT NULL | |
| ParentId | int? | Nullable FK -> self, Restrict | |
| AccountId | int? | Nullable FK -> Accounts | |
| FundId | int? | Nullable FK -> Funds | |
| CostCenterId | int? | Nullable FK -> CostCenters | |
| BudgetClassificationId | int? | Nullable FK -> BudgetClassifications | |
| AllowOverrun | bool? | Nullable | null = inherit Budget.AllowOverrun ?? BudgetType.AllowOverrun |
| IsActive | bool | NOT NULL, default true | |
| RowVersion | byte[] | ROWVERSION | |
| Created/CreatedBy/LastModified/LastModifiedBy | | Audit fields | |

**Constraints**: UNIQUE(BudgetId, ItemCode)
**Indexes**: IX_BudgetItem_BudgetId (FK), IX_BudgetItem_ParentId (FK), IX_BudgetItem_AccountId (FK), IX_BudgetItem_FundId (FK), IX_BudgetItem_CostCenterId (FK), IX_BudgetItem_BudgetClassificationId (FK)
**Note**: Level is NOT stored — computed at query time via recursive hierarchy

### Appropriation
| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, Identity | |
| AppropriationNumber | string | UNIQUE, NOT NULL | APR sequence |
| BudgetId | int | FK -> Budgets, NOT NULL | |
| BudgetItemId | int | FK -> BudgetItems, NOT NULL | |
| AppropriationType | AppropriationType | NOT NULL | Transfer restricted to Draft only |
| DocumentType | string(50) | NOT NULL | |
| DocumentId | int | NOT NULL | |
| Amount | decimal(23,2) | NOT NULL | |
| Status | AppropriationStatus | NOT NULL, default Draft | |
| RowVersion | byte[] | ROWVERSION | |
| Created/CreatedBy/LastModified/LastModifiedBy | | Audit fields | |

**Indexes**: IX_Appropriation_AppropriationNumber (unique), IX_Appropriation_BudgetId (FK), IX_Appropriation_BudgetItemId (FK)
**Note**: FundId, FiscalYearId NOT stored — joined via Budget. IsReversed, ReversalOfId NOT stored (Appropriation reversal = new Adjustment row with negative Amount)

### Encumbrance
| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | int | PK, Identity | |
| EncumbranceNumber | string | UNIQUE, NOT NULL | ENC sequence |
| EncumbranceType | EncumbranceType | NOT NULL | |
| AppropriationId | int | FK -> Appropriations, NOT NULL | Single source of budget context |
| VendorId | int? | Nullable FK -> Suppliers | |
| PurchaseOrderId | int? | Nullable FK -> PurchaseOrders | |
| DocumentType | string(50) | NOT NULL | |
| DocumentId | int | NOT NULL | |
| Description | string? | Nullable | |
| EncumbranceDate | DateOnly | NOT NULL | |
| Amount | decimal(23,2) | NOT NULL | |
| Status | EncumbranceStatus | NOT NULL, default Draft | |
| ReversalOfId | int? | Nullable FK -> self, Restrict | Set on reversal row only |
| ReversalReason | string(500) | Nullable | Stored on reversal row only |
| RowVersion | byte[] | ROWVERSION | |
| Created/CreatedBy/LastModified/LastModifiedBy | | Audit fields | |

**Indexes**: IX_Encumbrance_EncumbranceNumber (unique), IX_Encumbrance_AppropriationId (FK), IX_Encumbrance_ReversalOfId (FK)
**Note**: FundId, FiscalYearId, BudgetId, BudgetItemId NOT stored — joined via Appropriation->Budget. IsReversed NOT stored — computed via existence check on ReversalOfId

## Relationships

```
BudgetType 1──* Budget 1──* BudgetItem 1──* Appropriation 1──* Encumbrance
                  │                       │
Fund 1────────────┘                       │
                                          │
BudgetClassification 1──* BudgetItem      │
                                          │
BudgetItem self-ref (ParentId)            │
BudgetClassification self-ref (ParentId)  │
Encumbrance self-ref (ReversalOfId)───────┘
```

## Derived Value Projections (Not Stored)

| Derived Value | Computation | Where |
|---------------|-------------|-------|
| BudgetClassification.Level | Recursive CTE from ParentId chain | DTO projection |
| BudgetItem.Level | Recursive CTE from ParentId chain | DTO projection |
| Encumbrance.IsReversed | `EXISTS(SELECT 1 FROM Encumbrances WHERE ReversalOfId = this.Id)` | DTO projection |
| Encumbrance.Budget context | JOIN through Appropriation -> Budget | DTO projection |
| Encumbrance.Fund/FiscalYear context | JOIN through Appropriation -> Budget -> Fund/FiscalYear | DTO projection |
| Appropriation.Fund/FiscalYear context | JOIN through Budget -> Fund/FiscalYear | DTO projection |
| Latest approval decision | `SELECT TOP 1 FROM ApprovalHistory WHERE DocumentType=@t AND DocumentId=@id ORDER BY DecisionAt DESC` | DTO projection |
| Creator identity | Audit CreatedBy field | DTO projection |

## Availability Formulas

```
AvailableForAppropriation(BudgetItemId):
  SUM(Amount WHERE BudgetItemId=@id AND Status=Active AND Type IN (Original, Supplement, Transfer-In))
  - SUM(Amount WHERE BudgetItemId=@id AND Status=Active AND Type IN (Reduction, Transfer-Out))

AvailableForEncumbrance(AppropriationId):
  NetAppropriated (from BudgetItem)
  - SUM(Amount WHERE AppropriationId=@id AND Status IN (Active, PartiallyReleased, PartiallyLiquidated) AND ReversalOfId IS NULL)
```

## State Machines

### Budget FSM
```
Draft -> Submitted -> Approved -> Active -> Suspended -> Active -> Closed
                   Draft -> Cancelled
              Submitted -> Cancelled
              Suspended -> Cancelled
```

### Appropriation FSM
```
Draft -> PendingApproval -> Approved -> Active -> Suspended -> Closed
                                            -> Cancelled
                                            -> Reversed (new Adjustment row)
```

### Encumbrance FSM
```
Draft -> PendingApproval -> Approved -> Active -> PartiallyReleased -> PartiallyLiquidated -> FullyLiquidated -> Closed
                      Draft -> Cancelled
              PendingApproval -> Cancelled
              Active -> Reversed (new row with ReversalOfId)
              PartiallyReleased -> Reversed
              PartiallyLiquidated -> Reversed
```
