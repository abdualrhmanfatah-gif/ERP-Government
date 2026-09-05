# Report API Contracts

**Feature**: 007-financial-reports | **Date**: 2026-09-03

All endpoints are under `/api/Reports`. Authentication required. Each endpoint declares its own permission via `RequireAuthorization`.

## GET /api/Reports/balance-sheet

**Permission**: `accounting.reports.balance-sheet`

**Query Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| asOfDate | date (YYYY-MM-DD) | Yes | Must be <= today |
| fiscalPeriodId | int | No | Defaults to period containing asOfDate |

**Response**: `200 OK` → `BalanceSheetDto` (JSON)

**Error Responses**:
- `400 Bad Request` — ProblemDetails: asOfDate in future, invalid fiscalPeriodId
- `401 Unauthorized` — anonymous access
- `403 Forbidden` — missing `accounting.reports.balance-sheet` permission

**ETag**: `W/"{hash of asOfDate + fiscalPeriodId + data version}"` — client sends `If-None-Match`; returns `304 Not Modified` if unchanged.

---

## GET /api/Reports/income-statement

**Permission**: `accounting.reports.income-statement`

**Query Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| startDate | date (YYYY-MM-DD) | Yes | Must be <= endDate |
| endDate | date (YYYY-MM-DD) | Yes | Must be <= today |

**Response**: `200 OK` → `IncomeStatementDto` (JSON)

**Error Responses**:
- `400 Bad Request` — ProblemDetails: startDate > endDate, endDate in future
- `401 Unauthorized`
- `403 Forbidden`

**ETag**: Same pattern as Balance Sheet.

---

## GET /api/Reports/general-ledger

**Permission**: `accounting.reports.general-ledger`

**Query Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| accountId | int | No | Filter by account ID |
| accountCode | string | No | Filter by account code (alternative to accountId) |
| fiscalPeriodId | int | No | Filter by fiscal period |
| startDate | date | No | Default: start of current fiscal year |
| endDate | date | No | Default: today |
| page | int | No | Default: 1 |
| pageSize | int | No | Default: 500, max: 1000 |

**Response**: `200 OK` → `GeneralLedgerDto` (JSON, paginated)

**Error Responses**:
- `400 Bad Request` — ProblemDetails: invalid accountId/accountCode, startDate > endDate
- `401 Unauthorized`
- `403 Forbidden`

**ETag**: Includes page/pageSize in hash.

---

## GET /api/Reports/cash-flow

**Permission**: `accounting.reports.cash-flow`

**Query Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| startDate | date (YYYY-MM-DD) | Yes | Must be <= endDate |
| endDate | date (YYYY-MM-DD) | Yes | Must be <= today |

**Response**: `200 OK` → `CashFlowStatementDto` (JSON)

**Error Responses**:
- `400 Bad Request` — ProblemDetails: startDate > endDate, endDate in future
- `401 Unauthorized`
- `403 Forbidden`

**ETag**: Same pattern.

---

## GET /api/Reports/{name}/export

**Permission**: `accounting.reports.export` (gates Excel/PDF download)

**Path Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| name | string | Yes | One of: `balance-sheet`, `income-statement`, `general-ledger`, `cash-flow` |

**Query Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| format | string | Yes | `excel` or `pdf` |
| (report params) | varies | Yes | Same params as the corresponding GET endpoint |

**Response**:
- `200 OK` with `Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` (Excel) or `application/pdf` (PDF)
- `Content-Disposition: attachment; filename="{report-name}-{date}.{ext}"`

**Error Responses**:
- `400 Bad Request` — invalid report name, format, or params
- `401 Unauthorized`
- `403 Forbidden` — missing `accounting.reports.export` permission

**Note**: No ETag on export endpoints (file generation is idempotent but not cached).

---

## Audit Trail

Every successful or failed report generation (view, export, print) writes to `SecurityAuditLog`:

```json
{
  "userId": "string",
  "action": "ReportGenerate | ReportExport | ReportPrint",
  "entityType": "BalanceSheet | IncomeStatement | GeneralLedger | CashFlowStatement",
  "entityId": "0",
  "newValues": {
    "params": { "asOfDate": "2026-01-31", "fiscalPeriodId": 1 },
    "format": "Screen | Excel | PDF | Print",
    "success": true,
    "failureReason": null,
    "generatedAt": "2026-09-03T10:30:00Z"
  }
}
```
