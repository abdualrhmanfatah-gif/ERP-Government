# Feature Specification: Remove RecurringEntryExecutionLogs Table

**Feature Branch**: `040-remove-recurringentryexecutionlogs`
**Created**: 2026-09-08
**Decision Record**: DEP-026

## Context

`RecurringEntryExecutionLogs` is a per-run audit log (one row per execution attempt: status, timings, error, generated JournalEntry link). Sole consumer: RecurringEntryProcessor (via RecurringEntryExecutionLogService) — creates/updates logs + uses them for idempotency. Idempotency is already guaranteed structurally: due-entries selection filters `NextExecutionDate <= today` and success advances `NextExecutionDate`; failure leaves it due for retry. Log is redundant.

## Requirements

- **FR-1**: Migration drops RecurringEntryExecutionLogs (PK, 2 FKs, 4 indexes). Down recreates.
- **FR-2**: Deletion report [RecurringEntryExecutionLogDeletionReport] + SecurityAuditLog entry before drop. Status was stored as string (Created/Success/Failed) — converted to int on capture, back on restore.
- **FR-3**: Delete entity + config + enum RecurringEntryExecutionStatus + RecurringEntryExecutionLogService (whole file) + RecurringEntry.ExecutionLogs nav + HasMany config + both DbSets.
- **FR-4**: RecurringEntryProcessor: remove log-service instantiation, idempotency log check (documented NextExecutionDate guarantee), CreateLog/UpdateLogSuccess/UpdateLogFailed calls. Failure logging = application log only.
- **FR-5**: ProcessorTests: delete log-assertion test + FailedLog test, strip log lines from 4 other sites (compile fix; suite gate skipped per user instruction).
- **FR-6**: Docs: database-tables-complete.md (103 tables, Accounting 12).
