# API Contract: Journal Entry Lifecycle

**Date**: 2026-09-06
**Feature**: 025-journal-entry-lifecycle

## Endpoints

All endpoints are under `/api/JournalEntries`. Permission codes from `PERMISSIONS.Accounting.JournalEntries`.

### List Entries
```
GET /api/JournalEntries?entryStatus={status}&journalId={id}&fromDate={date}&toDate={date}
Permission: Accounting.JournalEntries.Read
Response: JournalEntryDto[]
```

### Get Entry
```
GET /api/JournalEntries/{id}
Permission: Accounting.JournalEntries.Read
Response: JournalEntryDto
```

### Create Entry
```
POST /api/JournalEntries
Permission: Accounting.JournalEntries.Create
Body: CreateJournalEntryCommand { documentDate, journalId, periodId, fiscalYearId, narration, ref, entryType }
Response: 201 Created
```

### Update Entry
```
PUT /api/JournalEntries/{id}
Permission: Accounting.JournalEntries.UpdateLines (Draft only)
Body: UpdateJournalEntryCommand { narration, ref, rowVersion }
Response: 204 No Content
```

### Submit Entry
```
POST /api/JournalEntries/{id}/submit
Permission: Accounting.JournalEntries.Submit
Body: SubmitJournalEntryCommand { id, rowVersion }
Response: 204 No Content
Errors: 400 "must have at least one line", 400 "entry is not balanced"
```

### Approve Entry
```
POST /api/JournalEntries/{id}/approve
Permission: Accounting.JournalEntries.Approve
Body: ApproveJournalEntryCommand { id, rowVersion }
Response: 204 No Content
Errors: 400 "entry is not in Submitted status"
```

### Post Entry
```
POST /api/JournalEntries/{id}/post
Permission: Accounting.JournalEntries.Post
Body: PostJournalEntryCommand { id, rowVersion }
Response: 204 No Content
Errors: 400 "fiscal period is locked for posting", 400 "document date outside period range"
```

### Reverse Entry
```
POST /api/JournalEntries/{id}/reverse
Permission: Accounting.JournalEntries.Reverse
Body: ReverseJournalEntryCommand { id, reversalReason, rowVersion }
Response: 204 No Content
Errors: 400 "fiscal period is locked", 400 "year is not open", 400 "reversals cannot be reversed"
```

### Cancel Entry
```
POST /api/JournalEntries/{id}/cancel
Permission: Accounting.JournalEntries.Cancel
Body: CancelJournalEntryCommand { id, rowVersion }
Response: 204 No Content
Errors: 400 "only Draft or Submitted entries can be cancelled"
```

### Add Line
```
POST /api/JournalEntries/{id}/lines
Permission: Accounting.JournalEntries.UpdateLines (Draft only)
Body: CreateJournalEntryLineCommand { accountId, description, currencyId, exchangeRate, debit, credit, costCenterId }
Response: 204 No Content
```

### Update Line
```
PUT /api/JournalEntries/{id}/lines/{lineId}
Permission: Accounting.JournalEntries.UpdateLines (Draft only)
Body: UpdateJournalEntryLineCommand { id, accountId, description, currencyId, exchangeRate, debit, credit, costCenterId, rowVersion }
Response: 204 No Content
```

### Delete Line
```
DELETE /api/JournalEntries/{id}/lines/{lineId}
Permission: Accounting.JournalEntries.UpdateLines (Draft only)
Response: 204 No Content
```

## Error Contract

All errors use the standard problem-details format:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "fiscal period is locked for posting"
}
```

Concurrent edit conflicts return 409 with RowVersion mismatch detail.
