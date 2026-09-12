# API Contracts: RFQ & Quotation Management

**Feature**: 050-rfq-quotation-ui
**Date**: 2026-09-11

All endpoints return `Result<T>`. Authentication via JWT Bearer. Authorization via `PermissionCodes`.

## RFQ Endpoints

### GET /api/RequestForQuotations

**Permission**: `RFQView`

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| status | RFQStatus? | null | Filter by status |
| search | string? | null | Search by RFQ number or notes |
| page | int | 1 | |
| pageSize | int | 20 | |

**Response**: `Result<PaginatedList<RFQListItem>>`

```json
{
  "items": [
    {
      "id": 1,
      "rfqNumber": "RFQ-000001",
      "rfqDate": "2026-09-11",
      "purchaseRequestId": 5,
      "status": 0,
      "deadlineDate": "2026-09-20",
      "currencyCode": "YER",
      "supplierCount": 3,
      "created": "2026-09-11T10:00:00Z"
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

### GET /api/RequestForQuotations/{id}

**Permission**: `RFQView`

**Response**: `Result<RFQDetailResponse>`

```json
{
  "id": 1,
  "rfqNumber": "RFQ-000001",
  "rfqDate": "2026-09-11",
  "purchaseRequestId": 5,
  "deadlineDate": "2026-09-20",
  "currencyCode": "YER",
  "termsAndConditions": "...",
  "status": 0,
  "notes": "...",
  "suppliers": [
    {
      "id": 1,
      "supplierPartyId": 10,
      "supplierName": "شركة الأمل",
      "invitationDate": "2026-09-11",
      "responseDate": null,
      "status": 0,
      "notes": null,
      "quotationId": null
    }
  ]
}
```

### POST /api/RequestForQuotations

**Permission**: `RFQCreate`

**Request Body**:
```json
{
  "purchaseRequestId": 5,
  "deadlineDate": "2026-09-20",
  "currencyCode": "YER",
  "termsAndConditions": "...",
  "notes": "...",
  "supplierPartyIds": [10, 11, 12]
}
```

**Response**: `Result<int>` (created RFQ id)

### PUT /api/RequestForQuotations/{id} (NEW)

**Permission**: `RFQCreate`

**Request Body**: Same as POST

**Response**: `Result`

### PATCH /api/RequestForQuotations/{id}/publish

**Permission**: `RFQPublish`

**Response**: `Result`

### PATCH /api/RequestForQuotations/{id}/close

**Permission**: `RFQComplete`

**Response**: `Result`

### PATCH /api/RequestForQuotations/{id}/cancel

**Permission**: `RFQCancel`

**Response**: `Result`

### PATCH /api/RequestForQuotations/suppliers/{rfqSupplierId}/response (NEW)

**Permission**: `RFQCreate`

**Request Body**:
```json
{
  "responseDate": "2026-09-15",
  "notes": "..."
}
```

**Response**: `Result`

## Quotation Endpoints

### GET /api/Quotations (NEW route)

**Permission**: `QuotationsView`

**Query Parameters**:
| Param | Type | Default | Notes |
|-------|------|---------|-------|
| rfqId | int? | null | Filter by RFQ |
| supplierPartyId | int? | null | Filter by supplier |
| status | QuotationStatus? | null | Filter by status |
| search | string? | null | Search by quotation number |
| page | int | 1 | |
| pageSize | int | 20 | |

**Response**: `Result<PaginatedList<QuotationListItem>>`

```json
{
  "items": [
    {
      "id": 1,
      "quotationNumber": "QT-000001",
      "rfqId": 1,
      "supplierPartyId": 10,
      "quotationDate": "2026-09-12",
      "validUntil": "2026-10-12",
      "status": 0,
      "grandTotal": 1500000.00,
      "created": "2026-09-12T08:00:00Z"
    }
  ],
  "totalCount": 15,
  "page": 1,
  "pageSize": 20
}
```

### GET /api/Quotations/{id} (FIX stub)

**Permission**: `QuotationsView`

**Response**: `Result<QuotationDetailResponse>` (full entity with lines)

### POST /api/Quotations

**Permission**: `QuotationsCreate`

**Request Body**:
```json
{
  "rfqId": 1,
  "rfqSupplierId": 1,
  "supplierPartyId": 10,
  "quotationDate": "2026-09-12",
  "validUntil": "2026-10-12",
  "currencyCode": "YER",
  "exchangeRate": 1.0,
  "shippingCost": 0,
  "otherCharges": 0,
  "paymentTerms": "...",
  "deliveryTerms": "...",
  "leadTimeDays": 14,
  "warrantyPeriodMonths": 12,
  "notes": "...",
  "lines": [
    {
      "purchaseRequestDetailId": 1,
      "itemId": 5,
      "unitId": 2,
      "quantity": 100,
      "unitPrice": 15000.00,
      "discountPercent": 5,
      "taxPercent": 15,
      "notes": null
    }
  ]
}
```

**Response**: `Result<int>` (created quotation id)

### PUT /api/Quotations/{id} (NEW)

**Permission**: `QuotationsCreate`

**Request Body**: Same as POST (only for Draft status)

**Response**: `Result`

### PATCH /api/Quotations/{id}/submit (NEW route)

**Permission**: `QuotationsCreate`

**Response**: `Result`

### PATCH /api/Quotations/{id}/start-evaluation

**Permission**: `QuotationsEvaluate`

**Response**: `Result`

### PATCH /api/Quotations/{id}/complete-evaluation

**Permission**: `QuotationsEvaluate`

**Request Body**:
```json
{
  "technicalScore": 85.5,
  "financialScore": 92.0,
  "rejectionReason": null
}
```

**Response**: `Result`

### PATCH /api/Quotations/{id}/select

**Permission**: `QuotationsSelect`

**Request Body**:
```json
{
  "selectionReason": "Best price and delivery terms"
}
```

**Response**: `Result`

### PATCH /api/Quotations/{id}/award

**Permission**: `QuotationsAward`

**Response**: `Result`

### PATCH /api/Quotations/{id}/reject (NEW route)

**Permission**: `QuotationsReject`

**Request Body**:
```json
{
  "rejectionReason": "Price too high"
}
```

**Response**: `Result`

## Error Response Format

All errors follow the standard `Result<T>` pattern:

```json
{
  "succeeded": false,
  "errors": ["Error message in Arabic"]
}
```

4xx errors: inline near form fields.
5xx errors: toast notification via `result-to-ui.ts`.
