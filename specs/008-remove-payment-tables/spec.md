# Feature Specification: Remove Payment Sub-Entity Tables and Convert PaymentMethod to Enum

**Feature Branch**: `008-remove-payment-tables`

**Created**: 2026-09-03

**Status**: Draft

**Input**: User description: "Delete 4 payment tables (AdvancePayments, PaymentAllocations, PaymentExecutions, PaymentMethods) and convert PaymentMethod to enum"

## Context

The Payments module currently contains four tables that add unnecessary complexity:

- **AdvancePayments** — a separate lifecycle for advance payments that duplicates what PaymentOrder + Moves already handle
- **PaymentExecutions** — a separate lifecycle for payment execution that maps directly to PaymentOrder states (Created → Approved → Sent → Completed)
- **PaymentAllocations** — transient allocations that have no lasting business meaning
- **PaymentMethods** — a lookup table for a fixed set of 6 values (Cash, BankTransfer, Check, CreditCard, WireTransfer, Other), creating CRUD overhead for what should be a simple enum

Both PaymentOrders and RevenueReceipts reference PaymentMethods via foreign key. The PaymentOrder state machine already covers the full document lifecycle. Accounting effects already flow through Moves via the PostingPipeline. These four tables add code, permissions, and schema surface area without proportional business value.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - System Simplification via Table Removal (Priority: P1)

As a system administrator, I want the four unnecessary payment tables removed so that the Payments module has fewer moving parts, fewer permissions to manage, and a simpler data model.

**Why this priority**: This is the core business goal. Removing these tables reduces schema complexity, eliminates orphaned CRUD endpoints, and simplifies authorization management. All other stories depend on this.

**Independent Test**: Can be verified by confirming the four tables no longer exist in the database schema, no endpoints reference them, and no code paths create or query them.

**Acceptance Scenarios**:

1. **Given** the system has the four tables (AdvancePayments, PaymentExecutions, PaymentAllocations, PaymentMethods), **When** the migration runs, **Then** all four tables are dropped from the database and no application code references them.
2. **Given** the system has 18 permissions related to these four tables, **When** the migration completes, **Then** all 18 permissions are removed from the security system and no code references them.
3. **Given** the system has DocumentSequence entries for PaymentExecution and AdvancePayment, **When** the migration completes, **Then** those entries are deleted while all other DocumentSequence rows remain intact.

---

### User Story 2 - PaymentMethod Enum Conversion (Priority: P1)

As a system user, I want PaymentMethod represented as a simple list of choices (Cash, BankTransfer, Check, CreditCard, WireTransfer, Other) rather than a database table, so that payment orders and revenue receipts reference a fixed set without lookup overhead.

**Why this priority**: The enum conversion is a prerequisite for removing the PaymentMethods table and directly impacts PaymentOrder and RevenueReceipt functionality.

**Independent Test**: Can be verified by confirming PaymentOrder and RevenueReceipt records store PaymentMethod as a named value (not a foreign key), and the frontend displays the enum name.

**Acceptance Scenarios**:

1. **Given** a PaymentOrder has a PaymentMethod, **When** the record is saved, **Then** the PaymentMethod is stored as a named enum value (Cash, BankTransfer, Check, CreditCard, WireTransfer, or Other) rather than a foreign key reference.
2. **Given** a RevenueReceipt has a PaymentMethod, **When** the record is saved, **Then** the PaymentMethod is stored as a named enum value.
3. **Given** existing PaymentOrder records reference PaymentMethods via foreign key, **When** the migration runs, **Then** each record's PaymentMethod is mapped to the correct enum value and no data is lost.
4. **Given** existing RevenueReceipt records reference PaymentMethods via foreign key, **When** the migration runs, **Then** each record's PaymentMethod is mapped to the correct enum value.
5. **Given** a PaymentOrder or RevenueReceipt has a PaymentMethodId pointing to a deactivated or unmapped PaymentMethod, **When** the migration runs, **Then** the system defaults to "Other" and records a warning in a migration report.

---

### User Story 3 - Data Preservation and Audit Trail (Priority: P2)

As a compliance officer, I want a deletion report generated for AdvancePayments and PaymentExecutions before the tables are dropped, so that we have an audit trail of what existed and its state at deletion time.

**Why this priority**: Financial systems require audit trails for schema changes. The Constitution Principle VI mandates reviewable migrations and Principle VIII requires audit logging.

**Independent Test**: Can be verified by confirming the report tables exist with the correct columns and contain the pre-deletion data, and that SecurityAuditLog entries exist for each table deletion.

**Acceptance Scenarios**:

1. **Given** AdvancePayments table contains records, **When** the migration drops the table, **Then** a report table [AdvancePaymentDeletionReport] is created containing Id, Number, Amount, Status, and CreatedAt for every deleted record.
2. **Given** PaymentExecutions table contains records, **When** the migration drops the table, **Then** a report table [PaymentExecutionDeletionReport] is created containing Id, Number, Status, Amount, and CreatedAt for every deleted record.
3. **Given** any table is dropped by this migration, **When** the deletion occurs, **Then** a SecurityAuditLog entry is written for each table: entity=System, action=SchemaDeletion, target=TableName, timestamp.

---

### User Story 4 - Existing Functionality Preserved (Priority: P1)

As a system user, I want all existing PaymentOrder and RevenueReceipt functionality to continue working without changes to their permissions or behavior, so that the simplification does not disrupt active workflows.

**Why this priority**: Any regression in the remaining payment or revenue functionality would undermine the simplification goal.

**Independent Test**: Can be verified by running the full existing test suite for PaymentOrders and RevenueReceipts and confirming all tests pass unchanged.

**Acceptance Scenarios**:

1. **Given** existing PaymentOrder permissions (PaymentOrdersView, PaymentOrdersCreate, etc.), **When** the migration completes, **Then** all these permissions remain active and functional.
2. **Given** existing RevenueReceipt permissions, **When** the migration completes, **Then** all these permissions remain active and functional.
3. **Given** existing PaymentOrder use cases (create, approve, cancel, etc.), **When** the system runs, **Then** all use cases function identically with the PaymentMethod enum instead of the FK reference.
4. **Given** existing RevenueReceipt use cases, **When** the system runs, **Then** all use cases function identically.

---

### User Story 5 - Reversibility (Priority: P3)

As a system administrator, I want the migration to be fully reversible so that if problems arise, the system can be restored to its prior state.

**Why this priority**: The Constitution requires reviewable migrations. Reversibility reduces rollback risk.

**Independent Test**: Can be verified by running the migration down and confirming all 4 tables are recreated with their original schema, permissions are restored, and enum columns are converted back to foreign keys.

**Acceptance Scenarios**:

1. **Given** the forward migration has completed, **When** the migration is reversed, **Then** all four tables are recreated with their original schema and data.
2. **Given** the forward migration has completed, **When** the migration is reversed, **Then** DocumentSequence entries for PaymentExecution and AdvancePayment are restored.
3. **Given** the forward migration has completed, **When** the migration is reversed, **Then** all 18 deleted permissions are restored.
4. **Given** the forward migration has completed, **When** the migration is reversed, **Then** PaymentOrder and RevenueReceipt PaymentMethod enum columns are converted back to integer foreign keys via the restored lookup table.

---

### Edge Cases

- What happens when a PaymentOrder's PaymentMethodId points to a deactivated PaymentMethod? → System maps to "Other" (enum value 5) and records a warning in the PaymentMethodMigrationReport.
- What happens when a RevenueReceipt has a null PaymentMethodId? → System defaults to "Other" and records a warning.
- What happens when cascade foreign keys from PaymentExecutions/PaymentAllocations/AdvancePayments to PaymentOrders or Moves prevent table drops? → Migration performs manual cleanup of dependent rows before dropping tables.
- What happens when existing tests reference the deleted entities? → Tests must be updated or removed before merge; compilation failure is the enforcement mechanism.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST replace the PaymentMethod database table with a fixed list of 6 choices: Cash, BankTransfer, Check, CreditCard, WireTransfer, Other — stored as a named value (not a foreign key) in PaymentOrders and RevenueReceipts.
- **FR-002**: System MUST drop the AdvancePayments table, its indexes, and its foreign keys. Before dropping, the system MUST generate a deletion report table containing every record's identifier, number, amount, status, and creation timestamp.
- **FR-003**: System MUST drop the PaymentExecutions table, its indexes, and its foreign keys. Before dropping, the system MUST generate a deletion report table containing every record's identifier, number, status, amount, and creation timestamp. The existing PaymentOrder state machine (Created → Approved → Sent → Completed) already covers the execution lifecycle.
- **FR-004**: System MUST drop the PaymentAllocations table, its indexes, and its foreign keys. No deletion report is needed for transient allocations.
- **FR-005**: System MUST drop the PaymentMethods table, its indexes, foreign keys, and seed data. Before dropping, the system MUST map each existing PaymentMethod foreign key reference to the correct enum value. Any unmapped reference MUST fail the migration with an explicit error list.
- **FR-006**: System MUST delete all security permissions whose code starts with "Payments.PaymentMethods", "Payments.PaymentExecutions", "Payments.PaymentAllocations", or "Payments.AdvancePayments". This affects 18 permission records. All associated role-permission links MUST be removed.
- **FR-007**: System MUST delete DocumentSequence entries where the document type is "PaymentExecution" or "AdvancePayment". All other DocumentSequence entries MUST remain unchanged.
- **FR-008**: System MUST log a SecurityAuditLog entry for each table deletion: entity type "System", action "SchemaDeletion", target table name, and timestamp.
- **FR-009**: System MUST be fully reversible. The reverse migration MUST recreate all four tables with their original schema, restore DocumentSequence entries, restore all permissions, and convert the enum columns back to integer foreign key references.
- **FR-010**: System MUST NOT change any existing PaymentOrder permissions or RevenueReceipt permissions. Existing use cases MUST continue to function unchanged.
- **FR-011**: All deleted code (entities, commands, queries, DTOs, endpoints, configurations, seed data, frontend components, tests) MUST be removed. The system MUST compile and pass all remaining tests with zero warnings.
- **FR-012**: Migration order MUST be: (1) add new enum columns as nullable, (2) backfill from existing foreign key values, (3) fail if any unmapped values remain, (4) drop old foreign key columns and add NOT NULL constraint, (5) drop tables, (6) clean up DocumentSequence and permissions.

### Key Entities

- **PaymentOrder**: The central payment document. Currently references PaymentMethod via integer foreign key. After this feature, references PaymentMethod as a named enum value. The state machine (Created → Approved → Sent → Completed) already covers the execution lifecycle.
- **RevenueReceipt**: A revenue receipt document. Currently references PaymentMethod via integer foreign key. After this feature, references PaymentMethod as a named enum value.
- **PaymentMethod** (enum): Replaces the PaymentMethod table. Values: Cash=0, BankTransfer=1, Check=2, CreditCard=3, WireTransfer=4, Other=5. Stored as an integer column, displayed as a named value.
- **AdvancePayments** (deleted): Former separate lifecycle table for advance payments. No replacement; functionality covered by PaymentOrder + Moves.
- **PaymentExecutions** (deleted): Former separate lifecycle table for payment execution. No replacement; functionality covered by PaymentOrder state machine.
- **PaymentAllocations** (deleted): Former transient allocation table. No replacement; allocation use cases are outside scope.
- **PaymentMethods** (deleted): Former lookup table for payment method choices. Replaced by the PaymentMethod enum.
- **AdvancePaymentDeletionReport** (migration artifact): Temporary report table generated during migration, containing pre-deletion data from AdvancePayments.
- **PaymentExecutionDeletionReport** (migration artifact): Temporary report table generated during migration, containing pre-deletion data from PaymentExecutions.
- **PaymentMethodMigrationReport** (migration artifact): Temporary report table generated during migration, recording PaymentOrder and RevenueReceipt rows where the PaymentMethod foreign key was deactivated or unmapped, with the default enum value assigned and a warning note.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The system compiles with zero warnings (warnings treated as errors per project constitution).
- **SC-002**: All remaining tests pass at 100% — no test failures or skipped tests.
- **SC-003**: The four tables (AdvancePayments, PaymentExecutions, PaymentAllocations, PaymentMethods) are absent from the database schema after migration.
- **SC-004**: No application code references the deleted entities, permissions, or tables — verified by comprehensive search returning zero matches.
- **SC-005**: PaymentOrder and RevenueReceipt records display PaymentMethod as a named enum value (e.g., "Cash", "BankTransfer") rather than a numeric ID or lookup reference.
- **SC-006**: The migration is fully reversible — running the down migration restores the original schema and all deleted data/permissions.
- **SC-007**: Deletion report tables exist with correct data for AdvancePayments and PaymentExecutions before the tables are dropped.

## Assumptions

- No production data exists in AdvancePayments, PaymentExecutions, or PaymentAllocations (or the deletion reports are acceptable as audit trail).
- The PaymentOrder state machine (Created → Approved → Sent → Completed) already covers the payment execution lifecycle; no new state transitions are needed.
- Accounting effects of payments already flow through Moves via the PostingPipeline; removing these tables does not change financial posting behavior.
- RevenueReceipts keeps PaymentMethod as a named enum value (not a foreign key to a lookup table).
- The 6 enum values (Cash, BankTransfer, Check, CreditCard, WireTransfer, Other) cover all current and foreseeable payment methods.
- Existing tests for PaymentOrders and RevenueReceipts will pass with minimal changes (PaymentMethod FK → enum).
- The Constitution's Principle VI (migrations only, reviewed), Principle VIII (audit immutability), and Principle VII (authorization) apply to this change.
