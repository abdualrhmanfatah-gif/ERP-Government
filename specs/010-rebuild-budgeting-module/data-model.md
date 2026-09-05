# Data Model: Rebuild Budgeting Module

**Feature**: `010-rebuild-budgeting-module` | **Date**: 2026-09-04

Conventions (all tables): PK `Id int identity`; `RowVersion rowversion`; `BaseAuditableEntity` audit (`Created`, `CreatedBy`, `LastModified`, `LastModifiedBy`); every FK `Restrict`; enums stored as `int`; money `decimal(23,2)`; dates `DateOnly` → `date`. No stored derived/duplicate columns anywhere in this module.

## 1. BudgetType

| Column | Type | Null | Notes |
|--------|------|------|-------|
| Id | int | PK | identity |
| Code | nvarchar(50) | NOT NULL | UNIQUE |
| Name | nvarchar(200) | NOT NULL | |
| Description | nvarchar(500) | NULL | |
| ControlMethod | int | NOT NULL | `BudgetControlMethod` |
| AllowOverrun | bit | NOT NULL | default false; root of inherit chain |
| IsActive | bit | NOT NULL | default true |
| RowVersion | rowversion | NOT NULL | |
| Audit | | | Created/CreatedBy/LastModified/LastModifiedBy |

Dropped vs current: `OverrunRequiresApproval`, `IsSystemType`.

## 2. Fund — unchanged

`FundNumber` UNIQUE, `FundName`, `FundType` (General/Special/Project), `FundCategory` (Operating/Capital), `FiscalYearId` int NULL FK→FiscalYears, `LegalAuthority`, `Description` NULL, `DefaultRevenueDebitAccountId` int NULL FK→Accounts, `IsActive`, `RowVersion`, audit.

## 3. BudgetClassification

| Column | Type | Null | Notes |
|--------|------|------|-------|
| Id | int | PK | identity |
| Code | nvarchar(50) | NOT NULL | UNIQUE |
| Name | nvarchar(200) | NOT NULL | |
| ParentId | int | NULL | FK→self Restrict; index |
| IsActive | bit | NOT NULL | default true |
| RowVersion / audit | | | |

Dropped vs current: stored `Level` (now computed `Level` in DTO).

## 4. Budget

| Column | Type | Null | Notes |
|--------|------|------|-------|
| Id | int | PK | identity |
| BudgetNumber | nvarchar(50) | NOT NULL | UNIQUE, BGT sequence |
| BudgetName | nvarchar(200) | NOT NULL | |
| BudgetTypeId | int | NOT NULL | FK→BudgetTypes, index |
| FiscalYearId | int | NOT NULL | FK→FiscalYears, index |
| FundId | int | NOT NULL | FK→Funds, index |
| TotalAmount | decimal(23,2) | NOT NULL | |
| Status | int | NOT NULL | `BudgetStatus` |
| AllowOverrun | bit | NULL | null = inherit BudgetType |
| EffectiveFrom | date | NOT NULL | |
| EffectiveTo | date | NULL | |
| Description | nvarchar(1000) | NULL | |
| RowVersion / audit | | | |

Dropped vs current: all snapshot amounts, `ControlMethod`, `OverrunRequiresApproval`, `ApprovedById/At`, `ClosedById/At`, `IsActive`.

## 5. BudgetItem

| Column | Type | Null | Notes |
|--------|------|------|-------|
| Id | int | PK | identity |
| ItemCode | nvarchar(50) | NOT NULL | UNIQUE per (BudgetId, ItemCode) |
| ItemName | nvarchar(200) | NOT NULL | |
| BudgetId | int | NOT NULL | FK→Budgets, index |
| ParentId | int | NULL | FK→self Restrict, index |
| AccountId | int | NULL | FK→Accounts, index |
| FundId | int | NULL | FK→Funds, index |
| CostCenterId | int | NULL | FK→CostCenters, index |
| BudgetClassificationId | int | NULL | FK→BudgetClassifications, index |
| AllowOverrun | bit | NULL | null = inherit Budget→BudgetType |
| IsActive | bit | NOT NULL | default true |
| RowVersion / audit | | | |

Dropped vs current: all amount columns, `Percentage`, `ControlLevel`, `IsMandatory`, `ProjectId`, `OrganizationUnitId`, stored level concept.

## 6. Appropriation

| Column | Type | Null | Notes |
|--------|------|------|-------|
| Id | int | PK | identity |
| AppropriationNumber | nvarchar(50) | NOT NULL | UNIQUE, APR sequence |
| BudgetId | int | NOT NULL | FK→Budgets, index |
| BudgetItemId | int | NOT NULL | FK→BudgetItems, index |
| AppropriationType | int | NOT NULL | `AppropriationType` |
| DocumentType | nvarchar(50) | NOT NULL | |
| DocumentId | int | NOT NULL | |
| Amount | decimal(23,2) | NOT NULL | signed for Adjustment; positive otherwise |
| Status | int | NOT NULL | `AppropriationStatus` (no Reversed) |
| RowVersion / audit | | | |

Dropped vs current: `FundId`, `FiscalYearId`, `CreatedById`, `ApprovedById/At`, `ReleasedAmount`, `EncumberedAmount`, `AvailableAmount`, `ControlStatus`, `IsReversed`, `ReversalOfId`, `ReversalReason`; `Transfer` type; `Reversed` status.

## 7. Encumbrance

| Column | Type | Null | Notes |
|--------|------|------|-------|
| Id | int | PK | identity |
| EncumbranceNumber | nvarchar(50) | NOT NULL | UNIQUE, ENC sequence |
| EncumbranceType | int | NOT NULL | `EncumbranceType` |
| AppropriationId | int | NOT NULL | FK→Appropriations, index (single budget-context source) |
| VendorId | int | NULL | FK→Suppliers, index |
| PurchaseOrderId | int | NULL | FK→PurchaseOrders, index |
| DocumentType | nvarchar(50) | NOT NULL | |
| DocumentId | int | NOT NULL | |
| Description | nvarchar(1000) | NULL | |
| EncumbranceDate | date | NOT NULL | |
| Amount | decimal(23,2) | NOT NULL | positive |
| Status | int | NOT NULL | `EncumbranceStatus` |
| ReversalOfId | int | NULL | FK→self Restrict, index; set only on reversal rows |
| ReversalReason | nvarchar(500) | NULL | reversal rows only |
| RowVersion / audit | | | |

Dropped vs current: `FundId`, `FiscalYearId`, `BudgetId`, `BudgetItemId`, `ReleasedAmount`, `AvailableAmount`, `ReleaseStatus`, stored `IsReversed`, `CreatedById`, `ApprovedById/At`.

## Enums (8, all `int`)

- `BudgetControlMethod`: None=0, Warning=1, Blocking=2
- `FundType`: General/Special/Project · `FundCategory`: Operating/Capital (unchanged)
- `BudgetStatus`: Draft, Submitted, Approved, Active, Suspended, Closed, Cancelled
- `AppropriationType`: Original, Supplement, Reduction, Adjustment (signed Amount)
- `AppropriationStatus`: Draft, PendingApproval, Approved, Active, Suspended, Closed, Cancelled
- `EncumbranceType`: Commitment, Obligational, Contractual, Advance, Adjustment
- `EncumbranceStatus`: Draft, PendingApproval, Approved, Active, PartiallyReleased, PartiallyLiquidated, FullyLiquidated, Closed, Cancelled, Reversed

Deleted enum concepts: `BudgetItemControlLevel`, `ReleaseStatus` usage in Budgeting, `Transfer`/`Reversal` appropriation types, `Liquidation*` (already removed by spec 009).

## Derived projections (never stored)

| DTO field | Computation |
|-----------|-------------|
| `BudgetClassificationDto.Level`, `BudgetItemDto.Level` | iterative parent-walk over `(Id, ParentId)` with cycle guard |
| `EncumbranceDto.IsReversed` | `Any(r => r.ReversalOfId == e.Id)` |
| Appropriation/Encumbrance `FundId/FiscalYearId/BudgetId/BudgetItemId` (+ names) | joins `Appropriation→Budget(→Fund, FiscalYear)`, `Appropriation→BudgetItem` |
| `ApprovalDecision` (latest) | `ApprovalHistory.Where(DocumentType, DocumentId).OrderByDescending(DecisionAt).FirstOrDefault()` |
| `CreatedByName` | audit `CreatedBy` |
| `AvailableForItem`, `AvailableForEncumbrance` | `IBudgetAvailabilityService` aggregates per FR-009 |

## State machines

- **Budget**: Draft→Submitted→Approved→Active→Suspended↔Active→Closed; Cancelled from Draft/Submitted/Suspended.
- **Appropriation**: Draft→PendingApproval→Approved→Active→Suspended/Closed/Cancelled; no Reversed; Update/Delete Draft-only; post-submission via Adjustment/Cancel.
- **Encumbrance**: Draft→PendingApproval→Approved→Active→PartiallyReleased→PartiallyLiquidated→FullyLiquidated→Closed; Cancelled from Draft/PendingApproval; Reversed via new reversal row (`ReversalOfId` + `ReversalReason`).

## Validation rules (per entity)

- Codes/numbers: required, max length, unique (Code: BudgetType/BudgetClassification; numbers: BGT/APR/ENC; `(BudgetId, ItemCode)`).
- Amounts: `> 0` except Adjustment (any non-zero signed); Reduction/Adjustment-decrease validated against computed availability under Blocking (explicit failure incl. available amount); Warning logs + allows.
- References: Budgets require Active BudgetType/Fund + existing FY; items require parent in same Budget (cycle + cross-budget-parent rejected); appropriations require Active Budget + Active item + open FY/period; encumbrances require Active appropriation + open FY/period + whole-item availability.
- Concurrency: every mutating command carries and verifies `RowVersion`.
