# Quickstart Validation Guide

**Branch**: `016-unified-party-document` | **Date**: 2026-09-05

## Prerequisites

- .NET 9 SDK installed
- SQL Server running (local or container)
- Repository cloned and building (`dotnet build`)

## Validation Scenarios

### V1: Party CRUD and Sequence Generation

```bash
# Create a new Party
curl -X POST http://localhost:5000/api/Parties \
  -H "Content-Type: application/json" \
  -d '{"partyType":0,"nameAr":"شركة اختبار","taxNumber":"TEST001"}'

# Expected: 201 with partyCode "PTY-000001"

# List parties
curl http://localhost:5000/api/Parties

# Expected: 200 with array containing the created party

# Get by ID
curl http://localhost:5000/api/Parties/1

# Expected: 200 with party details

# Toggle active
curl -X PATCH http://localhost:5000/api/Parties/1/toggle-active \
  -H "Content-Type: application/json" \
  -d '{"rowVersion":"<from-create-response>"}'

# Expected: 200 with isActive=false
```

### V2: Supplier Migration

```bash
# Run migration
dotnet ef database update --project src/Infrastructure

# Verify: no Suppliers table
# Verify: Parties table contains all former Supplier records
# Verify: PartyType=0 (Supplier) for all migrated records
# Verify: PartyCode assigned to each record
# Verify: Zero "Supplier" references in src/ (grep -r "Supplier" src/ should return no entity references)
```

### V3: ApprovalHistory Backfill

```bash
# Verify backfill results
# - All existing "Approved" rows have Action=1 (Approve)
# - All "Draft -> Submitted" rows have Action=0 (Submit)
# - All "* -> Cancelled" rows have Action=4 (Cancel)
# - Original Decision string preserved in Reason when Reason was empty
# - ApprovalStep defaulted to 1 for all existing rows
```

### V4: PaymentOrder Approval (Single SaveChanges)

```bash
# Submit a payment order, then approve it
# Verify: single SaveChanges call (check logs or trace)
# Verify: ApprovalHistory record created with Action=1, ApprovalStep=1, real RequiredRole
# Verify: DocumentStatusLog record created with FromStatus="Submitted", ToStatus="Approved"
```

### V5: Attachment Gate

```bash
# Create a DocumentAttachmentRequirement for Budget with IsMandatory=true
curl -X POST http://localhost:5000/api/DocumentAttachmentRequirements \
  -H "Content-Type: application/json" \
  -d '{"documentType":"Budget","attachmentTypeCode":"BOQ","titleAr":"جدول الكميات","isMandatory":true}'

# Attempt to approve a Budget without the mandatory attachment
# Expected: 400 with message indicating missing mandatory attachment "BOQ"

# Upload the required attachment
curl -X POST http://localhost:5000/api/Documents/budgets/42/attachments \
  -F "file=@boq.xlsx" \
  -F "attachmentTypeCode=BOQ" \
  -F "documentType=Budget"

# Attempt approval again
# Expected: Approval succeeds (200)
```

### V6: Document Status Log (Append-Only)

```bash
# Perform a state transition (e.g., submit a budget)
# Verify: DocumentStatusLog row created

# Attempt to update or delete the status log row via API
# Expected: 405 Method Not Allowed (no PUT/PATCH/DELETE endpoints)

# Verify: status log row is unchanged
```

### V7: Concurrency Conflict

```bash
# Fetch a Party, note its rowVersion
# Update the Party via PUT
# Attempt to update the same Party with the OLD rowVersion
# Expected: 409 Conflict with problem-details body
```

### V8: Document Sequence Prefixes

```bash
# Create entities for each new prefix:
# - Party → PTY-000001
# - ReceiptVoucher → RCV-000001 (when entity exists)
# - DepositSlip → DSL-000001 (when entity exists)
# - DisbursementRequest → DSB-000001 (when entity exists)
# - Payment → PAY-000001 (when entity exists)

# Verify: each prefix generates unique, sequential numbers
# Verify: concurrent requests produce no duplicates
```

## Test Commands

```bash
# Unit tests
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests

# Functional tests (requires test database)
dotnet test tests/Application.FunctionalTests

# Integration tests (requires Testcontainers)
dotnet test tests/Infrastructure.IntegrationTests
```

## Expected Outcomes

| Scenario | Pass Criteria |
|----------|---------------|
| V1 | Party created with PTY code, CRUD works, toggle flips isActive |
| V2 | Zero Supplier references, all data migrated, PartyCode assigned |
| V3 | All ApprovalHistory rows have Action enum, no data loss |
| V4 | Single SaveChanges, correct ApprovalHistory + DocumentStatusLog |
| V5 | Missing attachment blocks approval; present attachment allows it |
| V6 | Status log created on transition; no update/delete possible |
| V7 | Concurrent update returns 409 |
| V8 | All 5 prefixes generate unique sequential numbers |
