# API Contract: Journal Entries

**Source**: NSwag-generated `JournalEntriesClient` in `web-api-client.ts`
**Base path**: `/api/JournalEntries`

## Operations

| Method | Path | Request Body | Response |
|--------|------|-------------|----------|
| GET | `/` | — | `JournalEntryDto[]` (filtered by entryStatus, journalId, fromDate, toDate) |
| GET | `/{id}` | — | `JournalEntryDto` (header + lines) |
| POST | `/` | `CreateJournalEntryCommand` | `void` (returns 201 with location header) |
| PUT | `/{id}` | `UpdateJournalEntryCommand` | `void` |
| POST | `/{id}/submit` | `SubmitJournalEntryCommand` | `void` |
| POST | `/{id}/approve` | `ApproveJournalEntryCommand` | `void` |
| POST | `/{id}/post` | `PostJournalEntryCommand` | `void` |
| POST | `/{id}/reverse` | `ReverseJournalEntryCommand` | `void` |
| POST | `/{id}/cancel` | `CancelJournalEntryCommand` | `void` |
| POST | `/{id}/lines` | `CreateJournalEntryLineCommand` | `void` |
| PUT | `/{id}/lines/{lineId}` | `UpdateJournalEntryLineCommand` | `void` |
| DELETE | `/{id}/lines/{lineId}` | — | `void` |

## Error Responses

All endpoints use problem-details contract:
- `400` — validation failure (unbalanced entry, invalid transition, missing required fields)
- `401` — unauthenticated
- `403` — forbidden (permission check)
- `404` — entry or line not found
- `409` — concurrency conflict (stale RowVersion)

## Permissions

| Operation | Permission Code |
|---|---|
| List / Get | `Accounting.JournalEntries.Read` |
| Create / Update | `Accounting.JournalEntries.Create` |
| Submit | `Accounting.JournalEntries.Submit` |
| Approve | `Accounting.JournalEntries.Approve` |
| Post | `Accounting.JournalEntries.Post` |
| Reverse | `Accounting.JournalEntries.Reverse` |
| Cancel | `Accounting.JournalEntries.Cancel` |
| Update Lines | `Accounting.JournalEntries.UpdateLines` |
