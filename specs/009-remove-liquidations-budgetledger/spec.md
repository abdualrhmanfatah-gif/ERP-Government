# Feature Specification: Remove Liquidations and BudgetLedgerEntries Tables

**Feature Branch**: `009-remove-liquidations-budgetledger`

**Created**: 2026-09-04

**Status**: Draft

**Input**: User description: "Delete Liquidations and BudgetLedgerEntries tables with ALL relationships, counters, permissions, and code - no exceptions"

## Context

The budget module currently contains two tables that are being removed from the budget cycle:

- **Liquidations** — a lifecycle table for liquidating encumbrances (BF-004). The Constitution Principle V chain is being simplified from Budget → Appropriation → Encumbrance → Liquidation → Payment to Budget → Appropriation → Encumbrance → Payment. This supersedes spec 008 which kept Liquidation.
- **BudgetLedgerEntries** — a standalone write-only ledger with no consumers. No code references this table beyond its own entity and configuration.

Both tables add schema complexity, permissions, and code surface area without proportional business value. The Liquidation feature (BF-004) is being removed from the feature registry entirely (12 → 11 business features). PaymentOrders, Encumbrances, Budgets, BudgetItems, and Appropriations tables remain.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Drop Liquidations Table (Priority: P1)

As a system administrator, I want the Liquidations table removed along with all foreign keys, counter fields, and associated code so that the budget module has a simpler data model without legacy Liquidation references.

**Why this priority**: This is the core business goal. Removing the Liquidations table eliminates the BF-004 feature entirely and simplifies the Constitution Principle V chain. All other stories depend on this.

**Independent Test**: Can be verified by confirming the Liquidations table no longer exists in the database schema, no application code references it, and the deletion report contains pre-deletion data.

**Acceptance Scenarios**:

1. **Given** the system has the Liquidations table with FKs to Appropriations, BudgetItems, Budgets, Currencies, Encumbrances, FiscalYears, Funds, Moves, Suppliers, PurchaseOrders, and Users, **When** the migration runs, **Then** the table is dropped and a [LiquidationDeletionReport] table is created containing Id, LiquidationNumber, EncumbranceId, Amount, Status, CreatedAt, and DeletedAt for every deleted record.
2. **Given** the Liquidations table has indexes (IX_Liquidations_LiquidationNumber, FK indexes), **When** the migration runs, **Then** all indexes are dropped with the table.
3. **Given** the Liquidations table has a self-FK ReversalOfId, **When** the migration runs, **Then** the self-referential relationship is dropped with the table.
4. **Given** the Liquidations table has a self-FK ReversalOfId, **When** the migration runs, **Then** the self-referential relationship is dropped with the table.

---

### User Story 2 - Drop BudgetLedgerEntries Table (Priority: P1)

As a system administrator, I want the BudgetLedgerEntries table removed since it is a standalone write-only ledger with no consumers.

**Why this priority**: BudgetLedgerEntry has no code references beyond its own entity. Removing it is a clean deletion with minimal risk.

**Independent Test**: Can be verified by confirming the BudgetLedgerEntries table no longer exists and no code references it.

**Acceptance Scenarios**:

1. **Given** the system has the BudgetLedgerEntries table with FKs to Appropriations, BudgetItems, Budgets, Encumbrances, FiscalYears, and Funds, **When** the migration runs, **Then** the table is dropped and a [BudgetLedgerEntryDeletionReport] table is created containing every deleted record's data.
2. **Given** the BudgetLedgerEntries table has indexes and a PK, **When** the migration runs, **Then** all indexes are dropped with the table.

---

### User Story 3 - Remove PaymentOrders.LiquidationId Column (Priority: P1)

As a system administrator, I want the LiquidationId column removed from the PaymentOrders table since Liquidations no longer exist.

**Why this priority**: This FK column references a deleted table and must be removed to maintain schema integrity.

**Independent Test**: Can be verified by confirming PaymentOrders has no LiquidationId column and no code references it.

**Acceptance Scenarios**:

1. **Given** the system has PaymentOrders with a LiquidationId column and IX_PaymentOrders_LiquidationId index, **When** the migration runs, **Then** the column and index are dropped.
2. **Given** PaymentOrder rows reference Liquidations via LiquidationId, **When** the migration runs, **Then** those rows are captured in the [LiquidationDeletionReport] before the column is dropped.
3. **Given** the PaymentOrder domain entity has a LiquidationId property and Liquidation navigation, **When** the code is updated, **Then** both are removed.

---

### User Story 4 - Remove Counter Fields (Priority: P1)

As a system administrator, I want all Liquidation-related counter fields removed from Encumbrances, Budgets, BudgetItems, and Appropriations since they become dead-zero fields without the Liquidation feature.

**Why this priority**: These fields track liquidation progress which no longer exists. Full removal is cleaner than leaving zero-value columns.

**Independent Test**: Can be verified by confirming no LiquidatedAmount or LiquidationStatus columns exist in the affected tables.

**Acceptance Scenarios**:

1. **Given** Encumbrances has LiquidatedAmount and LiquidationStatus columns, **When** the migration runs, **Then** both columns are dropped.
2. **Given** Budgets has a LiquidatedAmount column, **When** the migration runs, **Then** the column is dropped.
3. **Given** BudgetItems has a LiquidatedAmount column, **When** the migration runs, **Then** the column is dropped.
4. **Given** Appropriations has a LiquidatedAmount column, **When** the migration runs, **Then** the column is dropped.

---

### User Story 5 - Domain, Application, and Infrastructure Cleanup (Priority: P1)

As a developer, I want all Liquidation and BudgetLedgerEntry code removed — entities, enums, events, commands, queries, DTOs, endpoints, configurations, permissions, seeds, tests, and documentation — so that no dead code remains.

**Why this priority**: Leaving dead code creates maintenance burden and confusion. The "no exceptions" scope demands complete removal.

**Independent Test**: Can be verified by grep returning zero matches for Liquidation|BudgetLedgerEntry in src/ and tests/ (excluding historical migrations and deletion report artifacts).

**Acceptance Scenarios**:

1. **Given** the system has Liquidation.cs, BudgetLedgerEntry.cs, LiquidationStatus.cs, LiquidationType enum, LiquidationCreated.cs, LiquidationPosted.cs, **When** cleanup runs, **Then** all are deleted.
2. **Given** the system has 5 Liquidation commands, 2 Liquidation queries, 1 BudgetLedgerEntry query, and their DTOs, **When** cleanup runs, **Then** all are deleted.
3. **Given** the system has Liquidations.cs and BudgetLedgerEntries.cs endpoint files, **When** cleanup runs, **Then** all endpoints are deleted.
4. **Given** the system has LiquidationConfiguration.cs and BudgetLedgerEntryConfiguration.cs, **When** cleanup runs, **Then** all configurations are deleted.
5. **Given** the system has permissions for LiquidationsView, LiquidationsCreate, LiquidationsApprove, LiquidationsMarkPaid, LiquidationsReverse, and BudgetLedgerEntriesView, **When** cleanup runs, **Then** all permissions, policy registrations, and seed data are removed.

---

### User Story 6 - Constitution Amendment and Decision Record (Priority: P2)

As a compliance officer, I want the Constitution Principle V chain amended to reflect Budget → Appropriation → Encumbrance → Payment (Liquidation removed), with a decision record per Principle XII.

**Why this priority**: The Constitution governs the system architecture. Amending it documents the rationale and maintains governance integrity.

**Independent Test**: Can be verified by confirming Principle V describes the new chain and a decision record exists.

**Acceptance Scenarios**:

1. **Given** Principle V currently describes Budget → Appropriation → Encumbrance → Liquidation → Payment, **When** the amendment is applied, **Then** it reads Budget → Appropriation → Encumbrance → Payment.
2. **Given** Principle XII requires decision records for architectural changes, **When** the amendment is applied, **Then** a decision record documents rationale, scope, migration/remediation plan, and BF-004 removal (12 → 11 business features).

---

### User Story 7 - Audit Trail (Priority: P2)

As a compliance officer, I want deletion reports generated for both tables before they are dropped, and SecurityAuditLog entries written for each table deletion, so that we have a permanent audit trail.

**Why this priority**: Financial systems require audit trails for schema changes. The Constitution Principle VIII mandates audit logging.

**Independent Test**: Can be verified by confirming deletion report tables exist with correct data and SecurityAuditLog entries exist.

**Acceptance Scenarios**:

1. **Given** Liquidations table contains records, **When** the migration drops the table, **Then** [LiquidationDeletionReport] is created with pre-deletion data and is INSERT-ONLY (kept permanently).
2. **Given** BudgetLedgerEntries table contains records, **When** the migration drops the table, **Then** [BudgetLedgerEntryDeletionReport] is created with pre-deletion data and is INSERT-ONLY (kept permanently).
3. **Given** any table is dropped by this migration, **When** the deletion occurs, **Then** a SecurityAuditLog entry is written: entity=System, action=SchemaDeletion, target=TableName, timestamp.

---

### User Story 8 - Reversibility (Priority: P3)

As a system administrator, I want the migration to be fully reversible so that if problems arise, the system can be restored to its prior state.

**Why this priority**: The Constitution requires reviewable migrations. Reversibility reduces rollback risk.

**Independent Test**: Can be verified by running the migration down and confirming all tables, columns, and code are restored.

**Acceptance Scenarios**:

1. **Given** the forward migration has completed, **When** the migration is reversed, **Then** the Liquidations table is recreated with its original schema and data.
2. **Given** the forward migration has completed, **When** the migration is reversed, **Then** the BudgetLedgerEntries table is recreated with its original schema and data.
3. **Given** the forward migration has completed, **When** the migration is reversed, **Then** the PaymentOrders.LiquidationId column and index are restored.
4. **Given** the forward migration has completed, **When** the migration is reversed, **Then** all counter fields (LiquidatedAmount, LiquidationStatus) are restored.

---

### Edge Cases

- PaymentOrder rows with non-null LiquidationId: captured in deletion report before column drop.
- Encumbrance rows with LiquidationStatus != None: captured in deletion report; column drops regardless.
- Historical migration files (InitialCreate etc.) remain untouched — EF migration history immutable; new migration + updated model snapshot only.
- CancelEncumbrance after change: no liquidation guard — relies on existing EncumbranceStatus/ReleaseStatus checks only.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST drop the Liquidations table including all FKs, indexes, PK, and self-FK ReversalOfId. Before dropping, the system MUST generate a [LiquidationDeletionReport] table (Id, LiquidationNumber, EncumbranceId, Amount, Status, CreatedAt, DeletedAt) for audit. Down migration MUST recreate the full schema.
- **FR-002**: System MUST drop the BudgetLedgerEntries table including all FKs, indexes, and PK. Before dropping, the system MUST generate a [BudgetLedgerEntryDeletionReport] table for audit. Down migration MUST recreate the full schema.
- **FR-003**: System MUST drop the PaymentOrders.LiquidationId column and index IX_PaymentOrders_LiquidationId. PaymentOrders referencing deleted Liquidations MUST be captured in [LiquidationDeletionReport] before column drop. Domain PaymentOrder.cs removes LiquidationId property and Liquidation navigation. PaymentOrderConfiguration removes index and relationship mapping.
- **FR-004**: System MUST drop columns: Encumbrances.LiquidatedAmount, Encumbrances.LiquidationStatus, Budgets.LiquidatedAmount, BudgetItems.LiquidatedAmount, Appropriations.LiquidatedAmount. Domain entities remove these properties. Configurations remove property mappings.
- **FR-005**: System MUST delete Domain/Budgeting/Entities/Liquidation.cs and BudgetLedgerEntry.cs, Domain/Budgeting/Enums/LiquidationStatus.cs and LiquidationType enum, Domain/Events/Budgeting/LiquidationCreated.cs and LiquidationPosted.cs. ReleaseStatus.cs comment updated (now only used by Encumbrances.ReleaseStatus).
- **FR-006**: System MUST delete Application/Budgeting/Commands/Liquidations/ (5 commands), Application/Budgeting/Queries/Liquidations/ (2 queries), Application/Budgeting/Queries/BudgetLedgerEntries/ (1 query), LiquidationDto.cs, BudgetLedgerEntryDto.cs. Update DTOs removing liquidation fields. Update GetBudgetSummaryQuery, CreateEncumbranceCommand, CancelEncumbranceCommand, CreateAppropriationCommand removing liquidation-related logic.
- **FR-007**: System MUST delete PermissionCodes: LiquidationsView, LiquidationsCreate, LiquidationsApprove, LiquidationsMarkPaid, LiquidationsReverse, BudgetLedgerEntriesView. Delete AddPolicy lines in DependencyInjection.cs. Delete SecurityPermission seed rows. Update RolePermissionSeedData.cs removing StartsWith filters and Make() entries.
- **FR-008**: System MUST delete Web/Endpoints/Budgeting/Liquidations.cs and BudgetLedgerEntries.cs plus route group registrations. OpenAPI document MUST be regenerated.
- **FR-009**: System MUST delete DocumentSequence rows where DocumentType='Liquidation'. Remove 'Liquidation'->'LIQ' prefix mapping from DocumentSequenceService prefix map. Update DocumentSequenceServiceTests.
- **FR-010**: System MUST delete Configurations/Budgeting/LiquidationConfiguration.cs and BudgetLedgerEntryConfiguration.cs. Remove DbSet<Liquidation> and DbSet<BudgetLedgerEntry> from ApplicationDbContext.cs and IApplicationDbContext.cs. Update configurations per FR-003/FR-004.
- **FR-011**: System MUST delete LiquidationTests class in Domain UnitTests EntityTests.cs, any Liquidation command handler tests, and update DocumentSequenceServiceTests and Encumbrance/Appropriation/Budget tests referencing removed fields. Compilation MUST pass.
- **FR-012**: Constitution Principle V chain MUST be amended to: Budget → Appropriation → Encumbrance → Payment. Decision record MUST be created per Principle XII documenting rationale, scope, migration/remediation plan, BF-004 removal from feature registry (12 → 11 business features). docs/database-schema.md tables 49-50 removed + LiquidatedAmount columns removed from tables 45-48. Feature registry + architecture map updated.
- **FR-013**: Schema deletion MUST log SecurityAuditLog entries per table. Deletion reports are INSERT-ONLY audit artifacts kept permanently.

### Key Entities

- **Liquidations** (deleted): Former lifecycle table for liquidating encumbrances. No replacement; feature BF-004 removed entirely.
- **BudgetLedgerEntries** (deleted): Former write-only ledger with no consumers. No replacement.
- **PaymentOrder**: Retains all functionality. Loses LiquidationId FK column and Liquidation navigation.
- **Encumbrance**: Retains all functionality. Loses LiquidatedAmount and LiquidationStatus counter fields. ReleaseStatus field and ReleaseStatus enum remain (used for encumbrance release tracking).
- **Budget**: Retains all functionality. Loses LiquidatedAmount counter field.
- **BudgetItem**: Retains all functionality. Loses LiquidatedAmount counter field.
- **Appropriation**: Retains all functionality. Loses LiquidatedAmount counter field.
- **LiquidationDeletionReport** (migration artifact): Permanent audit table containing pre-deletion data from Liquidations and PaymentOrders with non-null LiquidationId.
- **BudgetLedgerEntryDeletionReport** (migration artifact): Permanent audit table containing pre-deletion data from BudgetLedgerEntries.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The system compiles with zero warnings (warnings treated as errors per project constitution).
- **SC-002**: All remaining tests pass at 100% — no test failures or skipped tests.
- **SC-003**: Grep for 'Liquidation|BudgetLedgerEntry' in src/ and tests/ returns zero matches except Infrastructure/Migrations historical files and deletion report artifacts.
- **SC-004**: Liquidations and BudgetLedgerEntries tables are absent from the model snapshot; migration applies cleanly on fresh DB.
- **SC-005**: PaymentOrders table has no LiquidationId column.
- **SC-006**: OpenAPI v1.json contains no Liquidation or BudgetLedgerEntry schemas or endpoints.
- **SC-007**: Constitution v1.1.0 with amended Principle V and decision record committed.

## Assumptions

- No production financial data depends on Liquidations (deletion report covers audit).
- BudgetLedgerEntry was write-only ledger with no consumers (verified).
- Frontend has zero references to Liquidations or BudgetLedgerEntries (verified).
- Encumbrance.ReleaseStatus field and ReleaseStatus enum remain (used for encumbrance release tracking).
- Historical migration files remain untouched (EF migration history immutable).
- The Constitution's Principle VI (migrations only, reviewed), Principle VIII (audit immutability), and Principle VII (authorization) apply to this change.
- The deletion report tables are permanent INSERT-ONLY audit artifacts, not temporary migration tables.
