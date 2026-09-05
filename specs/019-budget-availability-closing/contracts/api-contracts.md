# API Contracts: Financial Control Layer

**Feature**: 019-budget-availability-closing | **Date**: 2026-09-05

## Endpoints

### 1. GET /api/Availability/{budgetItemId}

**Purpose**: Retrieve multi-dimensional budget availability breakdown for a budget line.

**Authorization**: `BudgetItems.View`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| fiscalYearId | int | Yes | Fiscal year to query |

**Response 200 OK**:
```json
{
  "budgetItemId": 42,
  "budgetItemCode": "511-001",
  "fiscalYearId": 5,
  "fiscalYearName": "2026",
  "breakdown": [
    {
      "fundId": 1,
      "fundCode": "GF",
      "fundName": "General Fund",
      "programId": 3,
      "programCode": "EDU",
      "programName": "Education",
      "projectId": null,
      "projectCode": null,
      "projectName": null,
      "budgetItemId": 42,
      "itemCode": "511-001",
      "appropriationAmount": 500000.00,
      "encumberedAmount": 150000.00,
      "paidAmount": 100000.00,
      "availableAmount": 250000.00
    }
  ],
  "totals": {
    "appropriationAmount": 500000.00,
    "encumberedAmount": 150000.00,
    "paidAmount": 100000.00,
    "availableAmount": 250000.00
  }
}
```

**Response 404 Not Found**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Budget item not found",
  "status": 404,
  "detail": "Budget item with ID 42 does not exist."
}
```

---

### 2. POST /api/YearClosing/Lapse

**Purpose**: Execute the fiscal year lapse run.

**Authorization**: `FinancialControl.LapseYear`

**Request Body**:
```json
{
  "fiscalYearId": 5
}
```

**Response 200 OK**:
```json
{
  "yearClosingRunId": 1,
  "fiscalYearId": 5,
  "fiscalYearName": "2026",
  "runType": "Lapse",
  "runAt": "2026-12-31T23:59:59Z",
  "lapsedAppropriationTotal": 1250000.00,
  "lapsedEncumbranceTotal": 300000.00,
  "status": "Completed",
  "affectedBudgetItems": 15,
  "affectedAppropriations": 42,
  "affectedEncumbrances": 8
}
```

**Response 409 Conflict**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Fiscal year already closed",
  "status": 409,
  "detail": "Fiscal year 2026 has already been lapsed. Run ID: 1."
}
```

---

### 3. POST /api/YearClosing/Reopen

**Purpose**: Reopen a lapsed fiscal year (before final account issuance).

**Authorization**: `FinancialControl.LapseYear`

**Request Body**:
```json
{
  "fiscalYearId": 5
}
```

**Response 200 OK**:
```json
{
  "yearClosingRunId": 2,
  "fiscalYearId": 5,
  "runType": "Reopen",
  "status": "Completed",
  "restoredAppropriationTotal": 1250000.00,
  "restoredEncumbranceTotal": 300000.00
}
```

**Response 409 Conflict** — payments against lapsed items exist:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Cannot reopen fiscal year",
  "status": 409,
  "detail": "Cannot reopen fiscal year 2026 — 3 payment(s) were made against lapsed items. Resolve these payments before reopening."
}
```

**Response 409 Conflict** — final account issued:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Cannot reopen fiscal year",
  "status": 409,
  "detail": "Cannot reopen fiscal year 2026 — final account has been issued."
}
```

---

### 4. POST /api/FinalAccounts/Generate

**Purpose**: Generate the final account for a closed fiscal year.

**Authorization**: `FinancialControl.ApproveFinalAccount`

**Request Body**:
```json
{
  "fiscalYearId": 5
}
```

**Response 200 OK**:
```json
{
  "finalAccountId": 1,
  "fiscalYearId": 5,
  "fiscalYearName": "2026",
  "status": "Draft",
  "generatedAt": "2027-01-15T10:00:00Z",
  "lineCount": 24,
  "closingEntryCount": 2
}
```

**Response 409 Conflict** — year not lapsed:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Fiscal year not closed",
  "status": 409,
  "detail": "Fiscal year 2026 must be closed before generating the final account."
}
```

---

### 5. POST /api/FinalAccounts/{id}/Issue

**Purpose**: Approve and issue the final account (makes it immutable).

**Authorization**: `FinancialControl.ApproveFinalAccount`

**Response 200 OK**:
```json
{
  "finalAccountId": 1,
  "status": "Issued",
  "issuedAt": "2027-01-20T14:30:00Z"
}
```

---

### 6. GET /api/FinalAccounts/{id}

**Purpose**: Retrieve the final account with all lines.

**Authorization**: `FinancialControl.ApproveFinalAccount` (or read-only permission)

**Response 200 OK**:
```json
{
  "finalAccountId": 1,
  "fiscalYearId": 5,
  "fiscalYearName": "2026",
  "status": "Issued",
  "generatedAt": "2027-01-15T10:00:00Z",
  "issuedAt": "2027-01-20T14:30:00Z",
  "lines": [
    {
      "dimension": "Fund",
      "dimensionId": 1,
      "dimensionCode": "GF",
      "dimensionName": "General Fund",
      "budgetedAmount": 5000000.00,
      "actualAmount": 4200000.00,
      "variance": 800000.00
    },
    {
      "dimension": "Program",
      "dimensionId": 3,
      "dimensionCode": "EDU",
      "dimensionName": "Education",
      "budgetedAmount": 2000000.00,
      "actualAmount": 1800000.00,
      "variance": 200000.00
    }
  ]
}
```

---

### 7. GET /api/Statements/Collection

**Purpose**: Generate yearly collection statement.

**Authorization**: `RevenueReceipts.View`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| fiscalYearId | int | Yes | Fiscal year |

**Response 200 OK**:
```json
{
  "fiscalYearId": 5,
  "fiscalYearName": "2026",
  "collections": [
    {
      "fundCode": "GF",
      "fundName": "General Fund",
      "programCode": "EDU",
      "programName": "Education",
      "date": "2026-03-15",
      "amount": 50000.00,
      "source": "Tax Revenue"
    }
  ],
  "subtotals": {
    "GF": 3000000.00,
    "HRF": 1500000.00
  },
  "grandTotal": 4500000.00,
  "isClosed": true
}
```

---

### 8. GET /api/Statements/Disbursement

**Purpose**: Generate yearly disbursement statement.

**Authorization**: `Payments.View`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| fiscalYearId | int | Yes | Fiscal year |

**Response 200 OK**:
```json
{
  "fiscalYearId": 5,
  "fiscalYearName": "2026",
  "disbursements": [
    {
      "fundCode": "GF",
      "fundName": "General Fund",
      "programCode": "EDU",
      "programName": "Education",
      "date": "2026-04-01",
      "amount": 120000.00,
      "payee": "Ministry of Education"
    }
  ],
  "subtotals": {
    "GF": 2500000.00,
    "HRF": 800000.00
  },
  "grandTotal": 3300000.00,
  "isClosed": true
}
```

---

## Error Response Contract

All error responses follow the RFC 7807 Problem Details format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Error title",
  "status": 400,
  "detail": "Detailed error message.",
  "errors": {
    "fieldName": ["Validation error message"]
  }
}
```
