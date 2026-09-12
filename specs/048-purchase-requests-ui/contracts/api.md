# API Contracts: Purchase Requests (Frontend Consumer)

**Date**: 2026-09-11

## Endpoints Consumed

### GET /api/PurchaseRequests

**Query params**: status?, priority?, search?, page?, pageSize?

**Response** (200):
```json
[
  {
    "id": 1,
    "requestNumber": "PR-2026-000001",
    "requestDate": "2026-09-11",
    "requiredDate": "2026-09-15",
    "departmentId": 5,
    "costCenterId": 3,
    "priority": "Normal",
    "status": "Draft",
    "totalEstimatedCost": 15000.00,
    "notes": "مكتب جديد",
    "lineCount": 3,
    "created": "2026-09-11T10:30:00Z"
  }
]
```

### GET /api/PurchaseRequests/{id}

**Response** (200):
```json
{
  "id": 1,
  "requestNumber": "PR-2026-000001",
  "requestDate": "2026-09-11",
  "requiredDate": "2026-09-15",
  "departmentId": 5,
  "costCenterId": 3,
  "priority": "Normal",
  "status": "Draft",
  "totalEstimatedCost": 15000.00,
  "notes": "مكتب جديد",
  "lineCount": 3,
  "created": "2026-09-11T10:30:00Z",
  "lines": [
    {
      "id": 1,
      "itemId": 10,
      "unitId": 2,
      "requestedQuantity": 5,
      "approvedQuantity": null,
      "unitCostEstimate": 3000.00,
      "totalCostEstimate": 15000.00,
      "notes": null
    }
  ]
}
```

### POST /api/PurchaseRequests

**Request body**:
```json
{
  "requestDate": "2026-09-11",
  "requiredDate": "2026-09-15",
  "departmentId": 5,
  "costCenterId": 3,
  "priority": "Normal",
  "notes": "مكتب جديد",
  "lines": [
    {
      "itemId": 10,
      "unitId": 2,
      "requestedQuantity": 5,
      "unitCostEstimate": 3000.00,
      "notes": null
    }
  ]
}
```

**Response** (201): `{ "id": 1 }`

### PUT /api/PurchaseRequests/{id}

**Request body**: Same as POST (all fields)

**Response** (204): No content

### PATCH /api/PurchaseRequests/{id}/submit

**Response** (204): No content

### PATCH /api/PurchaseRequests/{id}/approve

**Response** (204): No content

### PATCH /api/PurchaseRequests/{id}/reject

**Request body**:
```json
{
  "reason": "الميزانية غير كافية"
}
```

**Response** (204): No content

### PATCH /api/PurchaseRequests/{id}/cancel

**Request body**:
```json
{
  "reason": "تم إلغاء الطلب"
}
```

**Response** (204): No content

### GET /api/Items

**Response** (200):
```json
[
  { "id": 10, "code": "ITM-001", "name": "حاسوب محمول" }
]
```

### GET /api/Units

**Response** (200):
```json
[
  { "id": 2, "code": "UNIT-PCS", "name": "قطعة" }
]
```

## Error Responses

### 4xx (Validation)
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "requestDate": ["التاريخ مطلوب"],
    "lines": ["يجب إضافة بند واحد على الأقل"]
  }
}
```

### 5xx (Server)
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "An error occurred.",
  "status": 500,
  "detail": "Internal server error"
}
```
