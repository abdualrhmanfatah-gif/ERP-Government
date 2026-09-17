# SPEC-002 — Posting Pipeline

> **RETIRED (DEP-026, spec 042, 2026-09-08)**: AccountingEvents staging entity removed. Posting pipeline now writes JournalEntries directly from domain events (PostingPipelineHandler + PostingRuleMatcher + JournalEntryGenerator retained; auditor/retry/queue removed). OutboxMessages shape retained.
>
> **FULLY SUPERSEDED (DEP-027, 2026-09-12)**: The retained engine pieces (PostingPipelineHandler, PostingRuleMatcher, JournalEntryGenerator/MoveGenerator, PostingRules/PostingRuleLines, PaymentOrderExecuted) are removed entirely. This spec is wholly historical. Posting is always native per-business handlers (CreateAccrualEntry / RecordPayment) → domain event → outbox → JournalEntry. Do not revive the rule engine.

**Feature Branch**: `002-posting-pipeline`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "SPEC-002"

## Clarifications

### Session 2026-09-02

- Q: What should happen to pending AccountingEvents if their PostingRule is deleted before processing? → A: Block deletion — reject delete if pending events reference this rule
- Q: Should the pipeline emit structured logs, metrics, or traces for operational monitoring? → A: Structured logging with correlation IDs
- Q: How should the pipeline handle AccountingEvents when the OutboxProcessorService crashes mid-processing (Status=Processing)? → A: Orphan recovery on startup
- Q: What is the explicit out-of-scope boundary for this spec beyond MoveLine generation (FEATURE-027)? → A: Explicit exclusion list
- Q: Should the pipeline support multiple PostingRules for the same EventType generating Moves to different journals (split posting)? → A: One Move per PostingRule

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Automatic Journal Entry from Domain Events (Priority: P1)

When a business event occurs (e.g., purchase order approved, payment executed), the system automatically creates a journal entry without manual intervention. The event is captured durably, matched to a posting rule, and a balanced journal entry is generated in the correct journal.

**Why this priority**: Core financial integration — every business transaction must reach the general ledger automatically. Manual posting defeats the purpose of an event-driven architecture.

**Independent Test**: Can be tested by raising a domain event (e.g., PurchaseOrderApproved) and verifying that a Move is created with correct JournalId, SourceEventId, and IsSystemGenerated=true.

**Acceptance Scenarios**:

1. **Given** a PurchaseOrderApproved event is raised, **When** the OutboxProcessorService processes it, **Then** a Move is created with JournalId matching the PostingRule for "PurchaseOrderApproved", EntryStatus="Draft", IsSystemGenerated=true
2. **Given** a PostingRule with Priority=10 and another with Priority=20 for the same EventType, **When** the pipeline processes the event, **Then** the Priority=10 rule is processed first
3. **Given** a PostingRule with IsActive=false, **When** the pipeline processes an event of that type, **Then** the inactive rule is excluded from matching

---

### User Story 2 - Durable Event Tracking with Retry (Priority: P1)

Events that fail to process are tracked with error details and retried automatically up to 3 times. After 3 failures, the event is marked for manual intervention. Failed events can be viewed and manually retried.

**Why this priority**: Financial events must not be lost. Automatic retry handles transient failures; manual retry handles persistent issues.

**Independent Test**: Can be tested by creating an AccountingEvent in Failed status with RetryCount=3 and verifying that ProcessAccountingEventCommand resets it to Pending.

**Acceptance Scenarios**:

1. **Given** a domain event fails during processing, **When** the PostingPipelineHandler catches the exception, **Then** AccountingEvent.Status = Failed, ErrorMessage is populated, RetryCount is incremented
2. **Given** a Failed AccountingEvent with RetryCount < 3, **When** the OutboxProcessorService polls, **Then** the event is automatically retried
3. **Given** a Failed AccountingEvent with RetryCount >= 3, **When** an administrator calls ProcessAccountingEventCommand, **Then** the event is reset to Pending and will be picked up on the next poll
4. **Given** a Failed AccountingEvent with RetryCount < 3, **When** an administrator calls RetryAccountingEventCommand, **Then** the command returns failure with "Automatic retry is still available"

---

### User Story 3 - Configurable Posting Rules (Priority: P2)

Finance administrators can configure which journal receives entries for each event type, with priority ordering for multiple rules per event type.

**Why this priority**: Different event types post to different journals; multiple rules per event type allow split postings (e.g., accrual + tax).

**Independent Test**: Can be tested by creating a PostingRule via CreatePostingRuleCommand and verifying it appears in GetPostingRulesListQuery results.

**Acceptance Scenarios**:

1. **Given** a PostingRule with EventType="PurchaseOrderApproved" and JournalId=2, **When** a PurchaseOrderApproved event is processed, **Then** the generated Move has JournalId=2
2. **Given** two PostingRules for the same EventType with different Priorities, **When** the pipeline processes the event, **Then** moves are generated for both rules, processed in Priority order
3. **Given** a PostingRule, **When** an administrator updates it via UpdatePostingRuleCommand, **Then** the change takes effect on the next event processing

---

### User Story 4 - Manual Event Processing and Monitoring (Priority: P2)

Finance administrators can view pending events, process failed events manually, and monitor the pipeline status through API endpoints.

**Why this priority**: Operational visibility and manual override capability for production support.

**Independent Test**: Can be tested by calling GET /accounting-events/pending and verifying the response returns pending events.

**Acceptance Scenarios**:

1. **Given** AccountingEvents in Pending status, **When** administrator calls GET /accounting-events/pending, **Then** the events are returned with EventType, SourceTable, SourceId, Status
2. **Given** a Failed AccountingEvent with RetryCount >= 3, **When** administrator calls POST /accounting-events/{id}/process, **Then** the event is reset to Pending
3. **Given** a PostingRule, **When** administrator calls GET /posting-rules/{id}, **Then** the rule details including EventType, JournalId, Priority, IsActive are returned

---

### Edge Cases

- What happens when no PostingRule matches the EventType? → AccountingEvent marked Failed with "No posting rule found" error
- What happens when the OutboxProcessorService fails to deserialize an event? → Message marked Failed, retried per backoff schedule
- What happens when two events for the same source entity are raised simultaneously? → Unique index on (SourceTable, SourceId, EventType) prevents duplicate AccountingEvents
- What happens when MoveGenerator generates an entry number that already exists? → DocumentSequenceService ensures atomicity; sequential format AUTO-{year}-NNNNNN prevents collisions
- What happens when the OutboxProcessorService is restarted? → Pending messages with NextRetryAt <= now are picked up on next poll
- What happens when a domain event does not implement IHasSourceEntity? → DomainEventHandler silently ignores it
- What happens when a PostingRule is deleted while AccountingEvents are pending for that rule? → Deletion is blocked; system rejects delete if pending events reference the rule
- What happens when multiple PostingRules match the same EventType? → Each matched rule generates a separate Move (one Move per PostingRule)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST capture all domain events implementing IHasSourceEntity as AccountingEvent records with Status=Pending
- **FR-002**: System MUST match AccountingEvents to PostingRules by EventType, ordered by Priority ascending
- **FR-003**: System MUST generate one Move per matched PostingRule with EntryStatus="Draft", IsSystemGenerated=true (multiple rules for same EventType produce multiple Moves)
- **FR-004**: System MUST generate sequential EntryNumbers in format AUTO-{year}-NNNNNN
- **FR-005**: System MUST set Move.SourceEventId to the AccountingEvent.Id
- **FR-006**: System MUST mark AccountingEvent as Completed after all matched rules are processed
- **FR-007**: System MUST mark AccountingEvent as Failed if any processing step fails, with ErrorMessage populated
- **FR-008**: System MUST retry Failed events automatically up to 3 times (RetryCount < 3)
- **FR-009**: System MUST require manual intervention after 3 failed attempts (RetryCount >= 3)
- **FR-010**: System MUST process AccountingEvents through the Outbox pattern with exponential backoff (1s, 5s, 30s, 2min, 10min)
- **FR-011**: System MUST clean up Processed OutboxMessages after 7 days retention
- **FR-012**: System MUST prevent duplicate AccountingEvents via unique index on (SourceTable, SourceId, EventType)
- **FR-013**: System MUST NOT call SaveChangesAsync in DomainEventHandler, PostingPipelineHandler, MoveGenerator, or AccountingEventAuditor to prevent infinite recursion
- **FR-014**: System MUST expose GET /accounting-events/pending endpoint to view pending events
- **FR-015**: System MUST expose GET /posting-rules/ endpoint to list posting rules
- **FR-016**: System MUST expose POST /posting-rules/ and PUT /posting-rules/{id} to manage posting rules
- **FR-017**: System MUST expose POST /accounting-events/{id}/process to reset Failed events for retry
- **FR-018**: System MUST expose POST /accounting-events/{id}/retry for manual retry after 3 failures
- **FR-019**: System MUST block deletion of PostingRules that have Pending or Processing AccountingEvents referencing them
- **FR-020**: System MUST emit structured logs for event processing lifecycle (start, completion, failure) with correlation IDs linking OutboxMessage, AccountingEvent, and Move
- **FR-021**: System MUST reset AccountingEvents stuck in Processing status for more than 5 minutes to Pending on OutboxProcessorService startup (orphan recovery)

### Key Entities

- **AccountingEvent**: Tracks domain events awaiting processing. Key attributes: EventType (event class name), SourceTable (entity type), SourceId (entity PK), Status (Pending/Processing/Completed/Failed), MoveId (generated journal entry), RetryCount, ErrorMessage
- **PostingRule**: Configuration mapping event types to journals. Key attributes: EventType, JournalId, Priority (execution order), IsActive
- **PostingRuleLine**: Line-level configuration for posting rules. Key attributes: AccountSource, FixedAccountId, DebitOrCredit, AmountSource, dimension requirements (Fund, CostCenter, Project)
- **OutboxMessage**: Durable message queue for domain events. Key attributes: TypeName, Payload (JSON), Status, RetryCount, NextRetryAt

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of domain events implementing IHasSourceEntity are captured as AccountingEvent records
- **SC-002**: Events are processed within 30 seconds of being raised under normal load
- **SC-003**: Automatic retry resolves 90% of transient failures without manual intervention
- **SC-004**: Failed events with RetryCount >= 3 are visible in pending events list within 1 minute
- **SC-005**: Posting rules can be configured without code changes
- **SC-006**: Pipeline processes 100 events per minute under normal load
- **SC-007**: Zero events lost due to processing failures (all events are tracked durably)

## Assumptions

- Domain events implementing IHasSourceEntity contain sufficient metadata (SourceEntityId, SourceEntityType, OccurredAt) for AccountingEvent creation
- PostingRules are pre-configured for all event types that require journal entry generation
- The OutboxProcessorService runs continuously as a background service
- MoveLine generation from PostingRuleLines is handled by a separate feature (FEATURE-027) and is not in scope for this spec
- The existing DispatchDomainEventsInterceptor correctly captures domain events to OutboxMessages
- Exchange rate resolution and base currency validation are handled by SPEC-001 (Journal Entry Lifecycle) when the Move is posted

## Out of Scope

- **MoveLine generation** — handled by FEATURE-027; this spec only creates the Move shell
- **Move posting** — handled by SPEC-001 (Journal Entry Lifecycle); auto-generated Moves start as Draft
- **PostingRuleLine resolution logic** — AccountSource and AmountSource resolution are deferred; PostingRuleLines are not consumed in this spec
- **User authentication/authorization** — handled by platform; this spec uses existing PermissionCodes
- **Frontend UI** — this spec covers backend pipeline only
- **Budget validation** — belongs to Budgeting module
- **Payment integration** — belongs to Payments module
