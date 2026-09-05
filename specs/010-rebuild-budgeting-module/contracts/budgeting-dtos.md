# Contract: Budgeting DTOs (incl. computed projections)

**Feature**: `010-rebuild-budgeting-module`. Enum values serialize per existing JSON conventions (frontend `JsonStringEnumConverter` is registered; NSwag emits TS enums). Money: 2 decimals. Dates: `DateOnly` as date strings. Every mutating command echoes `rowVersion` (base64 `byte[]`).

## BudgetTypeDto
`{ id, code, name, description?, controlMethod, allowOverrun, isActive, rowVersion }`

## FundDto — unchanged shape
`{ id, fundNumber, fundName, fundType, fundCategory, fiscalYearId?, legalAuthority, description?, defaultRevenueDebitAccountId?, isActive, rowVersion }`

## BudgetClassificationDto / BudgetClassificationTreeDto
`{ id, code, name, parentId?, level /*computed*/, isActive, rowVersion, children? /*tree only*/ }`

## BudgetDto
`{ id, budgetNumber, budgetName, budgetTypeId, budgetTypeName, fiscalYearId, fundId, fundName, totalAmount, status, allowOverrun?, effectiveAllowOverrun /*resolved chain*/, effectiveFrom, effectiveTo?, description?, rowVersion }` — no snapshot amounts, no approval identity.

## BudgetItemDto (+ tree node)
`{ id, itemCode, itemName, budgetId, parentId?, accountId?, fundId?, costCenterId?, budgetClassificationId?, level /*computed*/, allowOverrun?, effectiveAllowOverrun /*resolved chain*/, isActive, rowVersion, children? }`

## AppropriationDto
Stored: `{ id, appropriationNumber, budgetId, budgetItemId, appropriationType, documentType, documentId, amount, status, rowVersion }`.
Projected: `{ budgetNumber, budgetName, fundId, fundName, fiscalYearId, availableForItem /*at query time*/, latestApproval { decision, decisionAt, approverUserId, reason? }?, createdBy }`.

## EncumbranceDto
Stored: `{ id, encumbranceNumber, encumbranceType, appropriationId, vendorId?, purchaseOrderId?, documentType, documentId, description?, encumbranceDate, amount, status, reversalOfId?, reversalReason?, rowVersion }`.
Projected: `{ isReversed /*EXISTS*/, budgetId, budgetNumber, budgetItemId, itemCode, fundId, fundName, fiscalYearId, availableForEncumbrance /*at query time*/, latestApproval {...}?, createdBy }`.

## Availability DTOs
- `ItemAvailabilityDto { budgetItemId, netAppropriated, totalSupplement, totalReduction, totalAdjustment, available, controlMethod, effectiveAllowOverrun, warning? }`
- `EncumbranceAvailabilityDto { appropriationId, budgetItemId, netAppropriated, totalEncumbered, available, controlMethod, effectiveAllowOverrun, warning? }`
- `warning` present only when ControlMethod=Warning and the operation exceeds availability.

## Approval history projection
`ApprovalDecisionDto { decision, decisionAt, approverUserId, requiredRole?, reason?, evaluationSnapshotJson? }` — latest-first ordering by `decisionAt DESC`.
