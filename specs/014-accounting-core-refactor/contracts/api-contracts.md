# API Contracts: Accounting Core Refactor

**Feature**: 014-accounting-core-refactor
**Date**: 2026-09-05

## Journal Entries — `/api/journal-entries`

### POST /api/journal-entries
Create a new journal entry (Draft status).

**Request Body**:
```json
{
  "entryNumber": "JE-2026-001",
  "documentDate": "2026-09-05",
  "postingDate": "2026-09-05",
  "journalId": 1,
  "periodId": 9,
  "fiscalYearId": 2026,
  "narration": "Monthly accrual",
  "isSystemGenerated": false,
  "lines": [
    {
      "sequence": 1,
      "accountId": 101,
      "currencyId": 1,
      "debit": 1000.00,
      "credit": 0,
      "description": "Debit line",
      "fundId": 1,
      "projectId": null,
      "budgetItemId": null,
      "encumbranceId": null,
      "paymentOrderId": null
    },
    {
      "sequence": 2,
      "accountId": 201,
      "currencyId": 1,
      "debit": 0,
      "credit": 1000.00,
      "description": "Credit line"
    }
  ]
}
```

**Response**: `201 Created` — JournalEntry DTO with `entryStatus: "Draft"`

---

### GET /api/journal-entries
List journal entries with optional filters.

**Query Parameters**:
- `entryStatus` (Draft|Posted|Reversed)
- `journalId` (int)
- `periodId` (int)
- `fiscalYearId` (int)
- `fromDate`, `toDate` (DateOnly)
- `page`, `pageSize`

**Response**: `200 OK` — Paged list of JournalEntry DTOs

---

### GET /api/journal-entries/{id}
Get a single journal entry with lines.

**Response**: `200 OK` — JournalEntry DTO including `lines[]` with analytic dimensions

---

### PUT /api/journal-entries/{id}
Update a Draft journal entry. Returns `409 Conflict` if the entry was modified since the client last loaded it (optimistic concurrency via `If-Match` / RowVersion).

**Response**: `200 OK` or `409 Conflict`

---

### POST /api/journal-entries/{id}/post
Post a Draft journal entry. Changes status to Posted. Returns `409 Conflict` on concurrency mismatch.

**Response**: `200 OK` — Updated JournalEntry with `entryStatus: "Posted"`

---

### POST /api/journal-entries/{id}/reverse
Reverse a Posted journal entry. Creates a new reversal entry linked to the original. Original status changes to Reversed.

**Request Body**:
```json
{
  "reason": "Correction for incorrect amount"
}
```

**Response**: `201 Created` — New reversal JournalEntry DTO

---

## Accounting Events — `/api/accounting-events`

### GET /api/accounting-events
List accounting events with filters.

**Query Parameters**:
- `eventType` (ReceiptCollection|DepositClearing|PaymentExecution|Reversal)
- `eventCategory` (Revenue|Expenditure|Transfer|Adjustment|Other)
- `status` (Pending|Posted|Reversed)
- `page`, `pageSize`

**Response**: `200 OK` — Paged list of AccountingEvent DTOs

---

### GET /api/accounting-events/{id}
Get a single accounting event.

**Response**: `200 OK` — AccountingEvent DTO with `journalEntryId` (nullable)

---

### POST /api/accounting-events/{id}/post
Post an accounting event. Creates a linked JournalEntry. Returns `409 Conflict` if a posted event already exists for the same (EventType, SourceDocumentType, SourceDocumentId).

**Response**: `200 OK` — Updated AccountingEvent with `status: "Posted"` and `journalEntryId` set

---

## Payment Orders — `/api/payment-orders`

### GET /api/payment-orders
List payment orders.

**Response**: `200 OK` — Paged list of PaymentOrder DTOs (no aggregate fields)

---

### GET /api/payment-orders/{id}
Get a single payment order.

**Response**: `200 OK` — PaymentOrder DTO (no aggregate fields)

---

### GET /api/payment-orders/{id}/totals
Compute and return payment order totals.

**Response**: `200 OK`
```json
{
  "paymentOrderId": 1,
  "amountGross": 10000.00,
  "deductionAmount": 1500.00,
  "netAmount": 8500.00,
  "totalPaidAmount": 3000.00,
  "totalRemainingAmount": 5500.00,
  "isFullyPaid": false,
  "deductions": [
    { "id": 1, "amount": 1000.00, "deductionPercent": 10 },
    { "id": 2, "amount": 500.00, "deductionPercent": 5 }
  ]
}
```

---

### POST /api/payment-orders
Create a new payment order.

**Request Body**: PaymentOrder input DTO (entered fields only — no aggregates)

**Response**: `201 Created`

---

### PUT /api/payment-orders/{id}
Update a payment order.

**Response**: `200 OK`

---

## Shared DTO Patterns

### JournalEntry DTO
```json
{
  "id": 1,
  "entryNumber": "JE-2026-001",
  "documentDate": "2026-09-05",
  "postingDate": "2026-09-05",
  "entryStatus": "Draft",
  "journalId": 1,
  "narration": "Monthly accrual",
  "reversalOfId": null,
  "reversalReason": null,
  "postedAt": null,
  "lines": [ ... ]
}
```

### JournalEntryLine DTO
```json
{
  "id": 1001,
  "journalEntryId": 1,
  "sequence": 1,
  "accountId": 101,
  "description": "Debit line",
  "debit": 1000.00,
  "credit": 0,
  "fundId": 1,
  "projectId": null,
  "budgetItemId": null,
  "encumbranceId": null,
  "paymentOrderId": null
}
```

### AccountingEvent DTO
```json
{
  "id": 1,
  "eventType": "ReceiptCollection",
  "eventCategory": "Revenue",
  "sourceDocumentType": "ReceiptVoucher",
  "sourceDocumentId": 42,
  "status": "Posted",
  "journalEntryId": 1,
  "processedAt": "2026-09-05T10:30:00Z"
}
```

### PaymentOrder DTO
```json
{
  "id": 1,
  "paymentOrderNumber": "PO-2026-001",
  "paymentOrderDate": "2026-09-05",
  "vendorId": 10,
  "fundId": 1,
  "amountGross": 10000.00,
  "deductionAmount": 1500.00,
  "paymentMethod": "BankTransfer",
  "status": "Draft",
  "journalEntryId": null
}
```
