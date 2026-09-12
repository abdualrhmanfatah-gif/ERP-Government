# API Contracts: Posting Pipeline

**Date**: 2026-09-02
**Spec**: SPEC-002

## Endpoints

### GET /api/Accounting/AccountingEvents/pending

**Purpose**: List pending AccountingEvents

**Auth**: AccountingEventsRead

**Query Parameters**:
- EventType (string, optional) — filter by event type
- SourceTable (string, optional) — filter by source table

**Response**: 200 OK
```json
[
  {
    "id": 1,
    "eventType": "PurchaseOrderApproved",
    "sourceTable": "PurchaseOrder",
    "sourceId": 42,
    "status": "Pending",
    "retryCount": 0,
    "createdAt": "2026-09-02T10:00:00Z"
  }
]
```

---

### POST /api/Accounting/AccountingEvents/{id}/process

**Purpose**: Reset Failed event for automatic retry

**Auth**: AccountingEventsRead

**Path Parameters**:
- id (int, required) — AccountingEvent.Id

**Request Body**: None

**Response**:
- 204 No Content — success
- 400 Bad Request — event not found or not in Failed status

---

### POST /api/Accounting/AccountingEvents/{id}/retry

**Purpose**: Manual retry after 3 failed attempts

**Auth**: AccountingEventsRead

**Path Parameters**:
- id (int, required) — AccountingEvent.Id

**Request Body**: None

**Response**:
- 204 No Content — success
- 400 Bad Request — event not found, not Failed, or automatic retry still available

---

### GET /api/Accounting/PostingRules

**Purpose**: List PostingRules

**Auth**: PostingRulesRead

**Query Parameters**:
- IsActive (bool, optional) — filter by active status
- EventType (string, optional) — filter by event type
- JournalId (int, optional) — filter by journal

**Response**: 200 OK
```json
[
  {
    "id": 1,
    "name": "Purchase Order Accrual",
    "eventType": "PurchaseOrderApproved",
    "journalId": 2,
    "priority": 10,
    "isActive": true
  }
]
```

---

### GET /api/Accounting/PostingRules/{id}

**Purpose**: Get PostingRule by ID

**Auth**: PostingRulesRead

**Path Parameters**:
- id (int, required) — PostingRule.Id

**Response**:
- 200 OK — rule details
- 404 Not Found — rule not found

---

### POST /api/Accounting/PostingRules

**Purpose**: Create PostingRule

**Auth**: PostingRulesCreate

**Request Body**:
```json
{
  "name": "Purchase Order Accrual",
  "eventType": "PurchaseOrderApproved",
  "journalId": 2,
  "priority": 10
}
```

**Response**:
- 200 OK — `{ "id": 1 }`
- 400 Bad Request — validation error

---

### PUT /api/Accounting/PostingRules/{id}

**Purpose**: Update PostingRule

**Auth**: PostingRulesUpdate

**Path Parameters**:
- id (int, required) — PostingRule.Id

**Request Body**:
```json
{
  "name": "Purchase Order Accrual (Updated)",
  "priority": 20,
  "rowVersion": "AAAAAAAAB"
}
```

**Response**:
- 204 No Content — success
- 400 Bad Request — validation error or concurrency conflict

---

### DELETE /api/Accounting/PostingRules/{id}

**Purpose**: Delete PostingRule (NEW — FR-019)

**Auth**: PostingRulesDelete

**Path Parameters**:
- id (int, required) — PostingRule.Id

**Request Body**: None

**Response**:
- 204 No Content — success
- 400 Bad Request — rule not found or has pending events
- 409 Conflict — pending AccountingEvents reference this rule

**Error Response**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "DeletePostingRule": ["Cannot delete PostingRule with pending AccountingEvents. Process or reset events first."]
  }
}
```

---

## Error Codes

| Code | Description |
|------|-------------|
| AccountingEventNotFound | AccountingEvent with given Id does not exist |
| EventNotFailed | AccountingEvent is not in Failed status |
| AutomaticRetryAvailable | AccountingEvent has RetryCount < 3; use Process instead |
| PostingRuleNotFound | PostingRule with given Id does not exist |
| PostingRuleHasPendingEvents | PostingRule cannot be deleted; pending events exist |
