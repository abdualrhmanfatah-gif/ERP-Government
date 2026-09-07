# API Contracts: Recurring Entries (ACC-04)

Base path: `/api/RecurringEntries`

## GET /api/RecurringEntries

List recurring entry schedules with optional filters.

**Query Parameters**:
| Param | Type | Description |
|-------|------|-------------|
| IsActive | bool? | Filter by active status |
| JournalId | int? | Filter by journal |
| Frequency | RecurringFrequency? | Filter by frequency |
| Status | RecurringEntryStatus? | Filter by status |

**Response**: `200 OK`
```json
[
  {
    "id": 1,
    "entryNumber": "REC-000001",
    "templateId": 5,
    "templateName": "Monthly Depreciation",
    "journalId": 10,
    "journalName": "General Journal",
    "name": "Monthly Asset Depreciation",
    "frequency": "Monthly",
    "startDate": "2026-01-01",
    "endDate": "2026-12-31",
    "nextExecutionDate": "2026-10-01",
    "lastExecutedAt": "2026-09-01T00:05:12Z",
    "amount": 5000.00,
    "currencyId": 1,
    "fundId": null,
    "costCenterId": 3,
    "projectId": null,
    "descriptionTemplate": null,
    "status": "Active",
    "generatedJournalEntryId": 42,
    "isActive": true
  }
]
```

**Auth**: `Accounting.RecurringEntries.Read`

---

## GET /api/RecurringEntries/{id}

Get a single recurring entry schedule by ID.

**Response**: `200 OK` — single `RecurringEntryDto` object (same shape as list item)

**Response**: `404 Not Found`

**Auth**: `Accounting.RecurringEntries.Read`

---

## POST /api/RecurringEntries

Create a new recurring entry schedule.

**Request Body**:
```json
{
  "templateId": 5,
  "journalId": 10,
  "name": "Monthly Asset Depreciation",
  "frequency": "Monthly",
  "startDate": "2026-01-01",
  "endDate": "2026-12-31",
  "amount": 5000.00,
  "currencyId": 1,
  "fundId": null,
  "costCenterId": 3,
  "projectId": null,
  "descriptionTemplate": "Auto depreciation for {month}"
}
```

**Response**: `200 OK`

**Response**: `400 Bad Request` — validation errors array

**Validation Rules**:
- `journalId` > 0 (required)
- `name` not empty, max 200 chars
- `frequency` valid enum
- `startDate` not empty
- `endDate` >= `startDate` when present
- `amount` required when `templateId` is null

**Auth**: `Accounting.RecurringEntries.Create`

---

## POST /api/RecurringEntries/{id}/pause

Pause an active recurring entry schedule.

**Request Body**:
```json
{
  "id": 1,
  "reason": "Budget freeze pending Q2 review",
  "rowVersion": "AAAAAAABBBBBBBB"
}
```

**Response**: `204 No Content`

**Response**: `400 Bad Request` — "Only active recurring entries can be paused."

**Auth**: `Accounting.RecurringEntries.Pause`

---

## POST /api/RecurringEntries/{id}/resume

Resume a paused recurring entry schedule.

**Request Body**:
```json
{
  "id": 1,
  "rowVersion": "AAAAAAABBBBBBBB"
}
```

**Response**: `204 No Content`

**Response**: `400 Bad Request` — "Only paused recurring entries can be resumed."

**Auth**: `Accounting.RecurringEntries.Resume`

---

## POST /api/RecurringEntries/{id}/cancel

Cancel a recurring entry schedule (terminal action).

**Request Body**:
```json
{
  "id": 1,
  "reason": "Project completed early",
  "rowVersion": "AAAAAAABBBBBBBB"
}
```

**Response**: `204 No Content`

**Response**: `400 Bad Request` — "Completed recurring entries cannot be cancelled." or "Cancelled recurring entries cannot be cancelled."

**Auth**: `Accounting.RecurringEntries.Cancel`

---

## Status Transitions

| Current Status | Allowed Actions |
|---------------|-----------------|
| Active | Pause, Cancel |
| Paused | Resume, Cancel |
| Completed | (none — terminal) |
| Cancelled | (none — terminal) |

All lifecycle transitions are recorded in `DocumentStatusLog` with actor, timestamp, from/to status, and reason.
