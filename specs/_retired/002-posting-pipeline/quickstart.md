# Quickstart Validation: Posting Pipeline

**Date**: 2026-09-02
**Spec**: SPEC-002

## Prerequisites

- .NET 10.0 SDK installed
- SQL Server instance available
- Database migrated to latest version

## Validation Scenarios

### Scenario 1: Automatic Journal Entry from Domain Event

**Objective**: Verify that a domain event creates a Move via the pipeline

**Steps**:
1. Start the application
2. Create a test PostingRule for "PurchaseOrderApproved" event type
3. Raise a PurchaseOrderApproved domain event (via test or API)
4. Wait for OutboxProcessorService to process (up to 5 seconds)
5. Verify AccountingEvent status changed to Completed
6. Verify Move was created with correct JournalId and IsSystemGenerated=true

**Expected Outcome**:
- AccountingEvent.Status = Completed
- Move.EntryStatus = Draft
- Move.IsSystemGenerated = true
- Move.SourceEventId = AccountingEvent.Id
- Move.JournalId matches PostingRule.JournalId

---

### Scenario 2: Multiple PostingRules Generate Multiple Moves

**Objective**: Verify split posting behavior

**Steps**:
1. Create two PostingRules for "PurchaseOrderApproved" with different JournalIds and Priorities
2. Raise a PurchaseOrderApproved domain event
3. Wait for processing
4. Verify two Moves were created, one per rule

**Expected Outcome**:
- Two AccountingEvent records (one per rule) OR one AccountingEvent with two Moves
- Move 1 JournalId = Rule 1.JournalId
- Move 2 JournalId = Rule 2.JournalId
- Moves created in Priority order

---

### Scenario 3: Automatic Retry on Failure

**Objective**: Verify retry behavior for transient failures

**Steps**:
1. Create a PostingRule for a valid EventType
2. Simulate a failure (e.g., invalid JournalId that causes FK violation)
3. Wait for processing
4. Verify AccountingEvent.Status = Failed, RetryCount = 1
5. Wait for next OutboxProcessorService poll (up to 5 seconds)
6. Verify event was retried

**Expected Outcome**:
- AccountingEvent.Status cycles through Failed → Pending → Processing
- RetryCount increments on each failure
- After 3 failures, RetryCount = 3 and no more automatic retries

---

### Scenario 4: Manual Retry After 3 Failures

**Objective**: Verify manual retry capability

**Steps**:
1. Create an AccountingEvent with Status=Failed and RetryCount=3
2. Call POST /accounting-events/{id}/process
3. Verify event was reset to Pending
4. Wait for processing
5. Verify event was processed

**Expected Outcome**:
- AccountingEvent.Status = Pending (after reset)
- AccountingEvent.RetryCount = 0 (after reset)
- Event picked up on next poll

---

### Scenario 5: Delete PostingRule with Pending Events

**Objective**: Verify delete guard (FR-019)

**Steps**:
1. Create a PostingRule
2. Create an AccountingEvent with Status=Pending referencing the rule's EventType
3. Call DELETE /posting-rules/{id}
4. Verify 400 Bad Request response

**Expected Outcome**:
- HTTP 400 with error message about pending events
- PostingRule still exists

---

### Scenario 6: Orphan Recovery on Startup

**Objective**: Verify stale Processing events are reset (FR-021)

**Steps**:
1. Create an AccountingEvent with Status=Processing and LastModified > 5 minutes ago
2. Restart the OutboxProcessorService (or application)
3. Verify event was reset to Pending

**Expected Outcome**:
- AccountingEvent.Status = Pending
- AccountingEvent.LastModified updated
- Structured log entry with correlation ID

---

### Scenario 7: Structured Logging

**Objective**: Verify structured logs with correlation IDs (FR-020)

**Steps**:
1. Enable logging at Information level
2. Process any domain event
3. Check logs for structured entries

**Expected Outcome**:
- Log entries contain: CorrelationId, AccountingEventId, MoveId (if created)
- Log entries follow message-template format (no string interpolation)
- Key lifecycle points logged: received, matched, created, completed/failed

---

## Test Commands

```bash
# Run unit tests
dotnet test tests/Application.UnitTests --filter "EventHandlers"

# Run functional tests
dotnet test tests/Application.FunctionalTests --filter "PostingPipeline"

# Run all tests
dotnet test
```

## Success Criteria Validation

| Criterion | Validation Method |
|-----------|------------------|
| SC-001: 100% event capture | Scenario 1 — verify AccountingEvent created |
| SC-002: <30s processing | Scenario 1 — measure time from event to Completed |
| SC-003: 90% auto-retry | Scenario 3 — verify retry behavior |
| SC-004: <1min visibility | Scenario 4 — verify manual retry works |
| SC-005: Configurable rules | Scenario 2 — verify multiple rules work |
| SC-006: 100 events/min | Load test — process 100 events in parallel |
| SC-007: Zero event loss | Scenario 3 — verify all events tracked durably |
