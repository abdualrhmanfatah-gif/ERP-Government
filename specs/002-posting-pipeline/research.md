# Research: Posting Pipeline

**Date**: 2026-09-02
**Spec**: SPEC-002

## Research Tasks

### 1. Orphan Recovery Pattern

**Question**: How should the OutboxProcessorService handle AccountingEvents stuck in Processing status after a crash?

**Decision**: Query AccountingEvents where Status=Processing AND LastModified < UtcNow.AddMinutes(-5), reset to Pending on startup.

**Rationale**:
- Simple query, no external dependencies
- 5-minute threshold balances responsiveness with avoiding false resets during normal processing
- Consistent with existing AccountingEventAuditor pattern (in-memory operations, no SaveChanges)

**Implementation**:
- Add `ResetStaleProcessingEventsAsync()` to AccountingEventAuditor
- Call from OutboxProcessorService `ExecuteAsync()` before main loop
- Log count of reset events

**Alternatives Considered**:
- Timeout-based polling: More complex, requires additional background task
- Manual intervention only: Unacceptable for financial events (events would be lost)

---

### 2. Structured Logging with Correlation IDs

**Question**: How to link OutboxMessage → AccountingEvent → Move in structured logs?

**Decision**: Pass correlation ID through handler chain via ILogger structured properties.

**Rationale**:
- Standard .NET logging pattern (message-template structured logging)
- Compatible with OpenTelemetry in ServiceDefaults
- No additional infrastructure required

**Implementation**:
- Use OutboxMessage.CorrelationId as root correlation ID
- Pass via ILogger structured properties: `{CorrelationId}`, `{AccountingEventId}`, `{MoveId}`
- Log at key lifecycle points: event received, rule matched, move created, completed, failed

**Alternatives Considered**:
- Custom correlation context (AsyncLocal): Overkill for in-process pipeline
- HTTP correlation headers: Not applicable (in-process events)

---

### 3. DeletePostingRule Guard

**Question**: How to prevent deletion of PostingRules that have pending AccountingEvents?

**Decision**: Check for Pending/Processing AccountingEvents referencing the rule before delete.

**Rationale**:
- Database-level integrity enforcement
- Consistent with Constitution Principle VI (Data Integrity)
- Simple query, no complex business rules

**Implementation**:
- New `DeletePostingRuleCommand` with handler
- Query: `context.AccountingEvents.AnyAsync(e => e.EventType == rule.EventType && (e.Status == Pending || e.Status == Processing))`
- Return failure if pending events exist

**Alternatives Considered**:
- Soft delete (IsActive=false): Adds complexity, not required by spec
- Cascade delete: Violates Constitution Principle VI (Restrict on all FKs)

---

### 4. Entry Number Format

**Question**: What format should auto-generated entry numbers use?

**Decision**: `AUTO-{year}-NNNNNN` (e.g., AUTO-2026-000001)

**Rationale**:
- Existing pattern in MoveGenerator.cs
- Year-scoped sequential numbers prevent collisions
- Clear distinction from manually created entries (JRN- prefix)

**Implementation**:
- Already implemented in MoveGenerator
- No changes needed

---

### 5. OutboxMessage Retention

**Question**: How long should processed OutboxMessages be retained?

**Decision**: 7 days

**Rationale**:
- Allows debugging of recent pipeline issues
- Consistent with OutboxOptions.RetentionDays = 7
- Prevents unbounded table growth

**Implementation**:
- Already implemented in OutboxProcessorService cleanup
- No changes needed
