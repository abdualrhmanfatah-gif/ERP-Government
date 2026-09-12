# Feature Specification: Remove AccountingEvents Table (Final)

**Feature Branch**: `042-remove-accountingevents`
**Created**: 2026-09-08
**Decision Record**: DEP-026
**Blast radius**: largest of DEP-026 program

## Context

`AccountingEvents` is a staging entity between domain events and JournalEntries: DomainEventHandler creates a row per event; PostingPipelineHandler consumes it (auditor status transitions), matches PostingRules, generates JournalEntry shells. RetryPolicy + manual retry commands + events queue page support manual recovery. Staging hop adds schema, queries, guards, and a UI queue with no transform benefit — pipeline writes JournalEntries directly (outbox shape retained via OutboxMessages).

## Requirements

- **FR-1**: Migration drops AccountingEvents + JournalEntries.SourceEventId (FK first, then index, then column). Down recreates both.
- **FR-2**: Deletion report [AccountingEventDeletionReport] + SecurityAuditLog entry before drop. Status/EventCategory stored as string — converted to int on capture, back on restore (EventStatus: Pending=0, Processing=1, Posted=2, Reversed=3, Failed=4; EventCategory: Revenue=0, Expenditure=1, Transfer=2, Adjustment=3, Other=4).
- **FR-3**: Delete entity + config + EventStatus enum + DomainEventHandler + AccountingEventAuditor + RetryPolicy + ProcessEvent/RetryEvent commands + GetPendingEvents query + AccountingEventDto + endpoint + Application DI registrations.
- **FR-4**: PostingPipelineHandler slimmed: no auditor/retry — match rules → generate JournalEntries directly; no-rules = warning skip; exception = logged and swallowed (document save completes; observable via application logs).
- **FR-5**: JournalEntryGenerator (MoveGenerator.cs): drop AccountingEvent param, SourceEventId link; Narration from EventType.
- **FR-6**: JournalEntry.SourceEventId property + nav + config index/FK removed.
- **FR-7**: DeletePostingRuleCommand: pending-events guard removed (deletion unconditional — DEP-026 option A).
- **FR-8**: OutboxProcessorService: stale-processing orphan recovery method removed (AccountingEvents gone).
- **FR-9**: PermissionCodes.AccountingEventsRead + orphaned Balances.* codes (missed by spec 041) + policy registrations + seed entries removed. Frontend permissions constants + usePermission union updated.
- **FR-10**: Frontend: EventsQueuePage + usePendingEvents + accountingEventsClient + AccountingEventDto type + route + nav removed. NSwag regen (0 stale refs) + build green.
- **FR-11**: Tests: EventHandlers test dir (6 files) deleted; DeletePostingRuleTests: 2 guard tests deleted, 2 kept.
- **FR-12**: specs/002-posting-pipeline → specs/_retired/002-posting-pipeline (retired banner). Docs: database-tables-complete.md (101 tables, Accounting 10); feature-architecture-map rows updated.

## Behavior changes (accepted)

- Failed posting-rule generation no longer retries — error logged only. Manual retry queue removed.
- DeletePostingRule no longer rejects on pending events.
- Suite gate skipped per user instruction (all DEP-026 specs).
