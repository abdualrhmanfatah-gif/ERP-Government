# Quickstart Validation: Recurring Entries (ACC-04)

**Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

## Prerequisites

- .NET 10 SDK installed
- SQL Server running (local or container)
- Database migrated to latest
- Valid JWT token with `Accounting.RecurringEntries.*` permissions
- A Journal entity exists (for `journalId`)
- A JournalEntryTemplate entity exists (optional, for `templateId`)

## Validation Scenarios

### V1: Create Schedule — Happy Path

```bash
# Create with template
curl -X POST http://localhost:5000/api/RecurringEntries \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "templateId": 1,
    "journalId": 1,
    "name": "Monthly Rent",
    "frequency": "Monthly",
    "startDate": "2026-10-01",
    "amount": 5000
  }'
# Expected: 200 OK
# Verify: entryNumber starts with "REC-", nextExecutionDate = startDate, status = "Active"
```

### V2: Create Without Template (Amount Required)

```bash
curl -X POST http://localhost:5000/api/RecurringEntries \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "journalId": 1,
    "name": "Ad-hoc Monthly",
    "frequency": "Monthly",
    "startDate": "2026-10-01",
    "amount": 2500
  }'
# Expected: 200 OK
```

### V3: Create Without Amount or Template → Reject

```bash
curl -X POST http://localhost:5000/api/RecurringEntries \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "journalId": 1,
    "name": "No Amount Schedule",
    "frequency": "Monthly",
    "startDate": "2026-10-01"
  }'
# Expected: 400 Bad Request — "Amount is required when no template is specified."
```

### V4: Create With EndDate Before StartDate → Reject

```bash
curl -X POST http://localhost:5000/api/RecurringEntries \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "journalId": 1,
    "name": "Bad Dates",
    "frequency": "Monthly",
    "startDate": "2026-12-01",
    "endDate": "2026-01-01",
    "amount": 1000
  }'
# Expected: 400 Bad Request — "End date must not be before start date."
```

### V5: Pause → Resume

```bash
# Pause
curl -X POST http://localhost:5000/api/RecurringEntries/1/pause \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "reason": "Budget review", "rowVersion": "..."}'
# Expected: 204 No Content

# Verify status = Paused via GET
curl http://localhost:5000/api/RecurringEntries/1 \
  -H "Authorization: Bearer $TOKEN"
# Expected: status = "Paused"

# Resume
curl -X POST http://localhost:5000/api/RecurringEntries/1/resume \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "rowVersion": "..."}'
# Expected: 204 No Content

# Verify status = Active, nextExecutionDate preserved
```

### V6: Cancel (Terminal)

```bash
curl -X POST http://localhost:5000/api/RecurringEntries/1/cancel \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "reason": "Project ended", "rowVersion": "..."}'
# Expected: 204 No Content

# Verify status = "Cancelled"
# Verify Resume button hidden in UI (no resume action available)
```

### V7: Reject Invalid Transitions

```bash
# Pause on Paused → 400
curl -X POST http://localhost:5000/api/RecurringEntries/1/pause \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "rowVersion": "..."}'
# Expected: 400 — "Only active recurring entries can be paused."

# Resume on Active → 400
curl -X POST http://localhost:5000/api/RecurringEntries/1/resume \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "rowVersion": "..."}'
# Expected: 400 — "Only paused recurring entries can be resumed."

# Cancel on Cancelled → 400
curl -X POST http://localhost:5000/api/RecurringEntries/1/cancel \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "rowVersion": "..."}'
# Expected: 400 — "Cancelled recurring entries cannot be cancelled."
```

### V8: DocumentStatusLog Recorded

```sql
-- After pause + cancel, verify audit trail
SELECT EntityName, DocumentId, FromStatus, ToStatus, Reason, ChangedAt
FROM DocumentStatusLogs
WHERE EntityName = 'RecurringEntry' AND DocumentId = 1
ORDER BY ChangedAt;
-- Expected: 2 rows (pause + cancel) with reasons and timestamps
```

### V9: Frontend — List Page

1. Navigate to Recurring Entries list
2. Verify all schedules displayed with status badges
3. Filter by Status = Active → only active shown
4. Filter by Frequency = Monthly → only monthly shown
5. Verify nextExecutionDate displayed for each row

### V10: Frontend — Create Page

1. Open create form
2. Select journal from dropdown
3. Optionally select template
4. Fill name, frequency, start date
5. Submit → redirect to detail page
6. Verify entry number auto-generated

### V11: Frontend — Detail Page with Lifecycle Actions

1. Open an Active schedule detail
2. Verify Pause and Cancel buttons visible, Resume hidden
3. Click Pause → enter reason → confirm → status updates to Paused
4. Verify Resume button now visible, Pause hidden
5. Click Resume → confirm → status returns to Active
6. Click Cancel → enter reason → confirm → status becomes Cancelled
7. Verify all lifecycle action buttons hidden for Cancelled

## Expected Test Results

```bash
# Unit tests
dotnet test tests/Application.UnitTests --filter "RecurringEntry"
# Expected: All pass — validation rules, lifecycle guards

# Integration tests
dotnet test tests/Infrastructure.IntegrationTests --filter "RecurringEntry"
# Expected: All pass — DB operations, sequence numbering

# Frontend tests
cd src/Web/ClientApp && npm run test -- --filter recurring-entries
# Expected: All pass — component rendering, form validation
```
