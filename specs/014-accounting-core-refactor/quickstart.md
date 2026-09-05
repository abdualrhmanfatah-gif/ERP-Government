# Quickstart Validation Guide: Accounting Core Refactor

**Feature**: 014-accounting-core-refactor
**Date**: 2026-09-05

## Prerequisites

- .NET 10 SDK installed
- SQL Server accessible (local or Aspire-managed)
- Application database migrated to latest

## Validation Scenarios

### V1: JournalEntry Rename — Zero Stale References

**Goal**: Confirm all "Move"/"MoveLine" references are eliminated.

1. Run global search for word-boundary `Move` and `MoveLine` across the entire solution
2. Verify zero matches in: `src/Domain/`, `src/Application/`, `src/Infrastructure/`, `src/Web/`, `tests/`
3. Verify API routes use `/api/journal-entries` (not `/api/moves`)
4. Verify database tables are `JournalEntries` and `JournalEntryLines` (not `Moves`, `MoveLines`)

**Expected**: Zero stale references. All naming consistent.

---

### V2: EntryStatus Enum Enforcement

**Goal**: Confirm EntryStatus is an enum with Draft/Posted/Reversed only.

1. Query `JournalEntries` table — verify `EntryStatus` column is int-backed (not string)
2. Create a journal entry — verify status defaults to `Draft` (0)
3. Post the entry — verify status changes to `Posted` (1)
4. Reverse the entry — verify status changes to `Reversed` (2)
5. Attempt to set an invalid status value — verify it is rejected

**Expected**: Only Draft(0), Posted(1), Reversed(2) exist in the database.

---

### V3: JournalEntryLine XOR Constraint

**Goal**: Confirm exactly one of Debit/Credit > 0 per line.

1. Create a journal entry line with Debit=100, Credit=0 — verify it succeeds
2. Create a line with Debit=0, Credit=100 — verify it succeeds
3. Create a line with Debit=100, Credit=100 — verify it is rejected with validation error
4. Create a line with Debit=0, Credit=0 — verify it is rejected (at least one must be > 0)

**Expected**: Database CHECK constraint and FluentValidation both enforce the XOR rule.

---

### V4: Analytic Dimensions on JournalEntryLine

**Goal**: Confirm five dimension FKs are queryable.

1. Create a journal entry line with FundId=1, ProjectId=2, BudgetItemId=3
2. Query the line — verify all three dimension values are returned
3. Create a line with no dimensions — verify all dimension fields are null
4. Query the availability engine — verify it can filter by Fund, Project, BudgetItem

**Expected**: All dimensions stored and queryable.

---

### V5: PaymentOrder Aggregate Strip

**Goal**: Confirm no stored aggregates or inline approval columns.

1. Load a PaymentOrder entity — verify fields AmountNet, TotalPaidAmount, TotalRemainingAmount, IsFullyPaid do NOT exist
2. Verify ApprovedById, RejectedById, CancelledById, VoidedById do NOT exist
3. Call `GET /api/payment-orders/{id}/totals` — verify computed values are returned
4. Verify AmountGross and DeductionAmount are still present

**Expected**: Only entered fields stored; totals computed on demand.

---

### V6: PaymentOrderLine / PaymentOrderDeduction Strip

**Goal**: Confirm computed fields removed from line items.

1. Load a PaymentOrderLine — verify Amount and TaxAmount exist; BaseAmount, AllocatedAmount, RemainingAmount, NetAmount do NOT exist
2. Load a PaymentOrderDeduction — verify Amount and DeductionPercent exist; BaseAmount does NOT exist

**Expected**: Only entered fields stored.

---

### V7: AccountingEvent Journal Link

**Goal**: Confirm AccountingEvent gains JournalEntryId and structured enums.

1. Create an AccountingEvent with EventType=ReceiptCollection, EventCategory=Revenue, Status=Pending
2. Post the event — verify JournalEntryId is populated
3. Attempt to post a second event with same (EventType, SourceDocumentType, SourceDocumentId) — verify it is rejected as duplicate
4. Reverse the linked JournalEntry — verify event Status changes to Reversed

**Expected**: Journal link established; unique constraint prevents double-posting.

---

### V8: PaymentMethod InKind

**Goal**: Confirm InKind added to PaymentMethod enum.

1. Create a payment with method=InKind — verify it saves successfully
2. Query the payment — verify method is stored as InKind(5)
3. Verify Other is renumbered to 6

**Expected**: All 7 payment method values accepted.

---

### V9: Optimistic Concurrency on JournalEntry Post

**Goal**: Confirm concurrent modification is detected.

1. User A loads a Draft journal entry
2. User B modifies and posts the same entry
3. User A attempts to post — verify `409 Conflict` response
4. User A refreshes, sees Posted status, cannot modify

**Expected**: Concurrency conflict detected and reported.

---

### V10: Reversal Pattern

**Goal**: Confirm posted entries are corrected only via reversal.

1. Post a journal entry
2. Attempt to edit the posted entry — verify it is rejected (immutable)
3. Reverse the entry — verify new reversal entry is created with ReversalOfId pointing to original
4. Verify original status is Reversed

**Expected**: Corrections only via reversal entries.

---

## Test Commands

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests

# Run integration tests (requires database)
dotnet test tests/Infrastructure.IntegrationTests

# Run functional tests (requires running application)
dotnet test tests/Application.FunctionalTests

# Run acceptance tests (requires running application + browser)
dotnet test tests/Web.AcceptanceTests
```

## Success Verification

After implementing all scenarios, verify:
- `dotnet build` succeeds with zero errors
- All existing tests pass (no regressions)
- New tests cover: rename, enum conversion, XOR constraint, analytic dimensions, aggregate stripping, unique constraint, concurrency, reversal
