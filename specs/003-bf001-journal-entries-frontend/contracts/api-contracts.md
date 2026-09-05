# API Contracts: BF-001 Journal Entries Frontend

**Date**: 2026-09-02
**Feature**: BF-001 Journal Entries Frontend
**Note**: Frontend consumes these endpoints. No backend changes.

## Journal Entry Endpoints

### List Journal Entries

```
GET /api/Moves
```

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| EntryStatus | string | NO | Filter by status (Draft/Submitted/Approved/Posted/Reversed/Cancelled) |
| JournalId | number | NO | Filter by journal ID |
| FromDate | string | NO | Filter from date (ISO date) |
| ToDate | string | NO | Filter to date (ISO date) |

**Response**: `200 OK` — `MoveDto[]`

### Get Journal Entry

```
GET /api/Moves/{id}
```

**Response**: `200 OK` — `MoveDto` (includes Lines collection)
**Response**: `404 Not Found`

### Create Journal Entry

```
POST /api/Moves
```

**Request Body**:
```json
{
  "documentDate": "2026-09-02",
  "journalId": 1,
  "entryType": 0,
  "periodId": 1,
  "fiscalYearId": 1,
  "narration": "Entry description",
  "ref": "Optional reference"
}
```

**Response**: `200 OK` — `{ "id": 123 }`
**Response**: `400 Bad Request` — Validation errors

### Update Journal Entry

```
PUT /api/Moves/{id}
```

**Request Body**:
```json
{
  "narration": "Updated description",
  "ref": "Updated reference",
  "rowVersion": "..."
}
```

**Response**: `204 No Content`
**Response**: `400 Bad Request` — Validation errors
**Response**: `409 Conflict` — RowVersion mismatch

### Submit Journal Entry

```
POST /api/Moves/{id}/submit
```

**Request Body**: `{ "rowVersion": "..." }`
**Response**: `204 No Content`
**Response**: `400 Bad Request` — "Only Draft entries can be submitted"

### Approve Journal Entry

```
POST /api/Moves/{id}/approve
```

**Request Body**: `{ "rowVersion": "..." }`
**Response**: `204 No Content`
**Response**: `400 Bad Request` — "Only Submitted entries can be approved"

### Post Journal Entry

```
POST /api/Moves/{id}/post
```

**Request Body**: `{ "rowVersion": "..." }`
**Response**: `204 No Content`
**Response**: `400 Bad Request` — "Only Approved entries can be posted" / "Period is closed" / "Lines do not balance"

### Cancel Journal Entry

```
POST /api/Moves/{id}/cancel
```

**Request Body**: `{ "rowVersion": "..." }`
**Response**: `204 No Content`
**Response**: `400 Bad Request` — "Only Draft or Submitted entries can be cancelled"

### Reverse Journal Entry

```
POST /api/Moves/{id}/reverse
```

**Request Body**:
```json
{
  "reversalReason": "Reason for reversal",
  "rowVersion": "..."
}
```

**Response**: `200 OK` — `{ "reversalId": 456 }`
**Response**: `400 Bad Request` — "Only Posted entries can be reversed" / "Already reversed"

## Move Line Endpoints

### Add Line

```
POST /api/Moves/{id}/lines
```

**Request Body**:
```json
{
  "accountId": 101,
  "description": "Line description",
  "currencyId": 1,
  "exchangeRate": 1.0,
  "debit": 1000.00,
  "credit": 0,
  "costCenterId": null
}
```

**Response**: `200 OK` — `{ "lineId": 789 }`
**Response**: `400 Bad Request` — "Account must be postable" / "Debit or Credit required"

### Update Line

```
PUT /api/Moves/{id}/lines/{lineId}
```

**Request Body**:
```json
{
  "id": 789,
  "moveId": 123,
  "accountId": 101,
  "description": "Updated description",
  "currencyId": 1,
  "exchangeRate": 1.0,
  "debit": 1500.00,
  "credit": 0,
  "rowVersion": "..."
}
```

**Response**: `204 No Content`
**Response**: `409 Conflict` — RowVersion mismatch

### Remove Line

```
DELETE /api/Moves/{id}/lines/{lineId}
```

**Response**: `204 No Content`

## Reference Data Endpoints

### List Journals

```
GET /api/Journals?IsActive=true
```

**Response**: `200 OK` — `JournalDto[]`

### List Accounts (Postable)

```
GET /api/Accounts?IsActive=true
```

**Response**: `200 OK` — `AccountDto[]`

### List Currencies

```
GET /api/Currencies?IsActive=true
```

**Response**: `200 OK` — `CurrencyDto[]`

### Lookup Exchange Rate

```
GET /api/ExchangeRates/lookup?BaseCurrencyId=1&CurrencyId=2&Date=2026-09-02&RateType=Official
```

**Response**: `200 OK` — `ExchangeRateLookupDto`
**Response**: `404 Not Found` — No rate found

### Get Fiscal Year by Date

```
GET /api/FiscalYears/by-date?date=2026-09-02
```

**Response**: `200 OK` — `FiscalYearPeriodResult`
**Response**: `404 Not Found` — No open period for date

### List Cost Centers

```
GET /api/CostCenters
```

**Response**: `200 OK` — `CostCenterDto[]`

## Error Response Format

All error responses follow the problem-details contract:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "EntryStatus": ["The EntryStatus field is not valid."]
  }
}
```

Frontend extracts Arabic error messages from `errors` object or `title` field.
