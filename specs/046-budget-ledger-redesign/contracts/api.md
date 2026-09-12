# API Contracts: Budget Preparation — BudgetItemAllocations

**Date**: 2026-09-10

## BudgetItemAllocations

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/BudgetItemAllocations?budgetId={id} | List allocations for a budget |
| GET | /api/BudgetItemAllocations/{id} | Get allocation by ID with remaining/available |
| POST | /api/BudgetItemAllocations | Create allocation (Budget in Draft only) |
| PUT | /api/BudgetItemAllocations/{id} | Update allocation (Budget in Draft only) |
| DELETE | /api/BudgetItemAllocations/{id} | Delete allocation (Draft, no linked transactions) |

### POST /api/BudgetItemAllocations

**Request**:
```json
{
  "budgetId": 1,
  "budgetItemId": 101,
  "proposedAmount": 500000.00,
  "remarks": "مخصص الرواتب"
}
```

**Response 201**:
```json
{
  "id": 1,
  "budgetId": 1,
  "budgetItemId": 101,
  "budgetItemCode": "ITM-001",
  "budgetItemName": "الرواتب",
  "proposedAmount": 500000.00,
  "approvedAmount": null,
  "remarks": "مخصص الرواتب",
  "remainingAmount": null,
  "availableAmount": null,
  "rowVersion": "AAAAAA=="
}
```

**Errors**:
- 400: "هذا البند مسجل بالفعل في الموازنة" (duplicate)
- 400: "البند غير نشط" (inactive item)
- 400: "الحساب مرتبط ببند آخر في هذه الموازنة" (shared account)
- 403: missing Budgets.Create permission

### GET /api/BudgetItemAllocations/{id}

**Response 200**:
```json
{
  "id": 1,
  "budgetId": 1,
  "budgetItemId": 101,
  "budgetItemCode": "ITM-001",
  "budgetItemName": "الرواتب",
  "proposedAmount": 500000.00,
  "approvedAmount": 500000.00,
  "remarks": "مخصص الرواتب",
  "remainingAmount": 300000.00,
  "availableAmount": 250000.00,
  "actualExpenditure": 200000.00,
  "outstandingEncumbrance": 50000.00,
  "status": "Active",
  "rowVersion": "AAAAAA=="
}
```

## BudgetTransactions

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/BudgetTransactions?budgetItemAllocationId={id} | List transactions for an allocation |
| GET | /api/BudgetTransactions/{id} | Get transaction by ID |
| POST | /api/BudgetTransactions | Create transaction |
| POST | /api/BudgetTransactions/{id}/submit | Submit for review |
| POST | /api/BudgetTransactions/{id}/approve | Approve |
| POST | /api/BudgetTransactions/{id}/reject | Reject with reason |
| POST | /api/BudgetTransactions/{id}/post | Post (update allocation atomically) |
| POST | /api/BudgetTransactions/{id}/reverse | Reverse a posted transaction |

### POST /api/BudgetTransactions

**Request**:
```json
{
  "budgetItemAllocationId": 1,
  "transactionType": 1,
  "transactionDate": "2026-09-10",
  "amount": 100000.00,
  "direction": 0,
  "description": "إضافة مخصص"
}
```

**Response 201**:
```json
{
  "id": 1,
  "transactionNumber": "BTR-000001",
  "budgetItemAllocationId": 1,
  "transactionType": 1,
  "transactionTypeName": "Supplement",
  "transactionDate": "2026-09-10",
  "amount": 100000.00,
  "direction": 0,
  "directionName": "Increase",
  "status": 0,
  "statusName": "Draft",
  "rowVersion": "AAAAAA=="
}
```

### POST /api/BudgetTransactions/{id}/post

**Response 200**:
```json
{
  "id": 1,
  "transactionNumber": "BTR-000001",
  "status": 3,
  "statusName": "Posted",
  "postedAt": "2026-09-10T14:30:00Z",
  "allocationApprovedAmount": 600000.00,
  "rowVersion": "AAAAAA=="
}
```

**Errors**:
- 400: "المبلغ المعتمد لا يمكن أن يصبح سالبًا" (decrease would make ApprovedAmount negative)
- 400: "الموازنة غير مفعلة" (budget not Active)
- 400: "المعاملة مرحّلة بالفعل" (already posted)

### POST /api/BudgetTransactions/{id}/reverse

**Request**:
```json
{
  "reason": "تصحيح خطأ في المبلغ"
}
```

**Response 200**:
```json
{
  "id": 2,
  "transactionNumber": "BTR-000002",
  "transactionType": 7,
  "transactionTypeName": "Reversal",
  "reversalOfId": 1,
  "amount": 100000.00,
  "direction": 1,
  "directionName": "Decrease",
  "status": 0,
  "statusName": "Draft",
  "rowVersion": "AAAAAA=="
}
```

## Budgets (Lifecycle Actions)

| Method | Path | Description |
|--------|------|-------------|
| POST | /api/Budgets/{id}/submit | Submit for review |
| POST | /api/Budgets/{id}/approve | Approve (freeze allocations) |
| POST | /api/Budgets/{id}/reject | Reject with notes |
| POST | /api/Budgets/{id}/activate | Activate (post) |
| POST | /api/Budgets/{id}/close | Close budget |
| POST | /api/Budgets/{id}/cancel | Cancel budget |

### POST /api/Budgets/{id}/approve

**Response 200**:
```json
{
  "id": 1,
  "status": 2,
  "statusName": "Approved",
  "allocationsUpdated": 5,
  "rowVersion": "AAAAAA=="
}
```

### POST /api/Budgets/{id}/reject

**Request**:
```json
{
  "reason": "المبالغ غير متوافقة مع الخطط التشغيلية",
  "notes": "يرجى مراجعة مخصصات القسم الإداري"
}
```

### POST /api/Budgets/{id}/cancel

**Errors**:
- 400: "لا يمكن إلغاء الموازنة لوجود حركات مرحّلة أو التزامات قائمة"

## PaymentOrders (BudgetItemAllocationId)

PaymentOrder gains `budgetItemAllocationId` (nullable int) on create/update. Existing `budgetItemId` retained for backward compatibility.

### GET /api/Payments/PaymentOrders/{id}

**Response** (budget fields only):
```json
{
  "budgetItemAllocationId": 1,
  "budgetItemId": 101,
  "allocationRemainingAmount": 300000.00,
  "allocationAvailableAmount": 250000.00
}
```
