# API Contracts: ACC-05 — مراقبة المحاسبة

All endpoints under `/api/Accounting/`. Arabic-first responses. Minimal APIs pattern.

## Account Balances

### GET /api/Accounting/AccountingBalances

**Permission**: `Accounting.Balances.Read`

**Query params**:
- `fiscalYearId` (int, required)
- `fiscalPeriodId` (int, optional)
- `accountId` (int, optional)
- `currencyId` (int, optional)

**Response 200**: `AccountBalanceDto[]`
```json
{
  "id": 1,
  "accountId": 101,
  "accountCode": "1101",
  "accountName": "الصندوق",
  "fiscalYearId": 1,
  "fiscalYearName": "2026",
  "fiscalPeriodId": 3,
  "periodName": "مارس",
  "periodStartDate": "2026-03-01",
  "periodEndDate": "2026-03-31",
  "currencyId": 1,
  "currencyCode": "YER",
  "openingDebit": 1000000.00,
  "openingCredit": 0.00,
  "debit": 500000.00,
  "credit": 200000.00,
  "closingDebit": 1300000.00,
  "closingCredit": 0.00,
  "balanceDirection": "Debit",
  "isFinalized": false,
  "finalizedAt": null
}
```

### GET /api/Accounting/AccountingBalances/reconcile

**Permission**: `Accounting.Balances.Read`

**Query params**:
- `fiscalYearId` (int, required)
- `fiscalPeriodId` (int, required)

**Response 200**: `ReconciliationResultDto`
```json
{
  "fiscalYearId": 1,
  "fiscalPeriodId": 3,
  "isBalanced": false,
  "totalAccountsChecked": 45,
  "discrepancyCount": 2,
  "discrepancies": [
    {
      "accountId": 101,
      "accountCode": "1101",
      "accountName": "الصندوق",
      "currencyId": 1,
      "currencyCode": "YER",
      "materializedDebit": 1300000.00,
      "calculatedDebit": 1350000.00,
      "materializedCredit": 0.00,
      "calculatedCredit": 0.00,
      "debitDifference": 50000.00,
      "creditDifference": 0.00,
      "discrepancyType": "DebitMismatch"
    }
  ]
}
```

### POST /api/Accounting/AccountingBalances/rebuild

**Permission**: `Accounting.Balances.Rebuild`

**Request body**:
```json
{
  "fiscalYearId": 1,
  "fiscalPeriodId": 3
}
```

**Response 200**: `{ "success": true, "message": "تم إعادة بناء الأرصدة بنجاح" }`

**Validation**: Period must not be finalized. Rebuild is idempotent (safe for concurrent calls via RowVersion).

### POST /api/Accounting/AccountingBalances/finalize

**Permission**: `Accounting.Balances.Finalize`

**Request body**:
```json
{
  "fiscalYearId": 1,
  "fiscalPeriodId": 3
}
```

**Response 200**: `{ "success": true, "message": "تم إغلاق الفترة" }`

**Validation**: Rejects if AccountingEvents with Status=Pending exist in period.

### POST /api/Accounting/AccountingBalances/unfinalize

**Permission**: `Accounting.Balances.Unfinalize`

**Request body**:
```json
{
  "fiscalYearId": 1,
  "fiscalPeriodId": 3
}
```

**Response 200**: `{ "success": true, "message": "تم فتح الفترة" }`

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
