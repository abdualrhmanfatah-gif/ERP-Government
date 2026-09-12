# Data Model: Purchase Requests UI

**Date**: 2026-09-11

## Entities (Frontend Types)

### PurchaseRequest

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | number | yes | Primary key |
| requestNumber | string | yes | LTR display |
| requestDate | string | yes | ISO date |
| requiredDate | string | no | ISO date |
| departmentId | number | no | Optional |
| costCenterId | number | no | Optional |
| priority | PurchaseRequestPriority | yes | Enum: Low, Normal, High, Urgent |
| status | PurchaseRequestStatus | yes | Enum |
| totalEstimatedCost | number | no | Computed from lines |
| notes | string | no | Free text |
| lineCount | number | yes | Computed |
| created | string | yes | ISO datetime |

### PurchaseRequestDetail (extends PurchaseRequest)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| lines | PurchaseRequestDetailLine[] | yes | Line items |

### PurchaseRequestDetailLine

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | number | yes | Primary key |
| itemId | number | yes | FK to Item |
| unitId | number | yes | FK to Unit |
| requestedQuantity | number | yes | > 0 |
| approvedQuantity | number | no | Set during approval |
| unitCostEstimate | number | no | Per-unit cost |
| totalCostEstimate | number | no | Computed: qty × unitCost |
| notes | string | no | Free text |

### Catalog (Item)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | number | yes | Primary key |
| code | string | yes | Display in combobox |
| name | string | yes | Display in combobox |

### Catalog (Unit)

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| id | number | yes | Primary key |
| code | string | yes | Display in combobox |
| name | string | yes | Display in combobox |

## Enums

### PurchaseRequestStatus

```
Draft → Submitted → Approved → UnderProcurement
                ↘ Rejected
Approved → Cancelled
```

### PurchaseRequestPriority

```
Low | Normal | High | Urgent
```

## Form Schemas (Zod)

### createPurchaseRequestSchema

- requestDate: string, min(1), "التاريخ مطلوب"
- requiredDate: string, optional
- departmentId: number, optional
- costCenterId: number, optional
- priority: enum, required, "الأولوية مطلوبة"
- notes: string, optional
- lines: array, min(1), "يجب إضافة بند واحد على الأقل"
  - itemId: number, min(1), "البند مطلوب"
  - unitId: number, min(1), "الوحدة مطلوبة"
  - requestedQuantity: number, min(0.01), "الكمية يجب أن تكون > 0"
  - unitCostEstimate: number, optional
  - notes: string, optional

### updatePurchaseRequestSchema

Same as create, all fields nullable (for partial updates).

## Relationships

```
PurchaseRequest 1──* PurchaseRequestDetail
PurchaseRequestDetail *──1 Item (via itemId)
PurchaseRequestDetail *──1 Unit (via unitId)
```
