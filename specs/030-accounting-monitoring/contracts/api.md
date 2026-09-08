# API Contracts: ACC-05 — مراقبة المحاسبة

All endpoints under `/api/Accounting/`. Arabic-first responses. Minimal APIs pattern.

> **RETIRED (DEP-026, spec 041)**: Account Balances endpoints (list/reconcile/rebuild/finalize/unfinalize) removed. Balances computed live from JournalEntryLines.

## Accounting Events

### GET /api/Accounting/AccountingEvents/pending

**Permission**: `Accounting.AccountingEvents.Read`

**Query params**:
- `status` (string, optional: "Pending", "Failed", "Processing")
- `eventType` (string, optional)
- `page` (int, optional, default 1)
- `pageSize` (int, optional, default 25)

**Response 200**: `AccountingEventDto[]`
```json
[
  {
    "id": 1,
    "eventType": "PaymentExecution",
    "sourceTable": "PaymentOrder",
    "sourceId": 42,
    "status": "Failed",
    "journalEntryId": null,
    "errorMessage": "Account 1101 is not postable",
    "processedAt": null,
    "retryCount": 3
  }
]
```

**Read-only**: No POST/PUT/DELETE endpoints.

## Posting Rules

### GET /api/Accounting/PostingRules

**Permission**: `Accounting.PostingRules.Read`

**Response 200**: `PostingRuleDto[]`
```json
[
  {
    "id": 1,
    "name": "تحصيل الايرادات",
    "eventType": "ReceiptCollection",
    "journalId": 5,
    "journalName": "يومية التحصيل",
    "priority": 10,
    "isActive": true,
    "lines": [
      {
        "id": 1,
        "sequence": 1,
        "accountSource": "FixedAccount",
        "fixedAccountId": 101,
        "debitOrCredit": "Debit",
        "amountSource": "EventAmount",
        "fundDimensionRequired": false,
        "costCenterDimensionRequired": false,
        "projectDimensionRequired": false
      }
    ]
  }
]
```

### POST /api/Accounting/PostingRules

**Permission**: `Accounting.PostingRules.Create`

**Request body**: `{ name, eventType, journalId, priority, isActive, lines: [...] }`

**Response 201**: `{ "id": 1 }`

**Validation**: At least one line required. If AccountSource=FixedAccount, FixedAccountId required.

### PUT /api/Accounting/PostingRules/{id}

**Permission**: `Accounting.PostingRules.Edit`

**Request body**: Same as POST. Lines replaced atomically.

**Response 200**: `{ "success": true }`

### DELETE /api/Accounting/PostingRules/{id}

**Permission**: `Accounting.PostingRules.Delete`

**Response 200**: `{ "success": true }`

**Validation**: Rejects if rule is referenced by AccountingEvents with Status=Processing or Status=Posted.

## Error Responses

All errors use ProblemDetails contract:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "fiscalPeriodId": ["فترة مالية مطلوبة"]
  }
}
```
