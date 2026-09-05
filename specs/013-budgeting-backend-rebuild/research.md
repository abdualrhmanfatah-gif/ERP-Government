# Research: Rebuild Budgeting Module Backend

**Date**: 2026-09-05
**Feature**: 013-budgeting-backend-rebuild

## Decision Log

### 1. Migration Strategy — Wipe + Recreate

**Decision**: Single EF Core migration that drops all 7 existing Budgeting tables, recreates them per new schema, preserves DocumentSequence rows for BGT/APR/ENC, and reseeds reference data idempotently.

**Rationale**: The spec explicitly requires data wipe ("all existing Budgeting data wiped — fresh start"). A clean migration avoids complex ALTER TABLE chains for column renames/removals. DocumentSequence rows must be preserved to maintain numbering continuity.

**Alternatives considered**:
- Incremental ALTER TABLE migrations: Rejected — too many column drops/renames across 7 tables, high risk of data corruption during migration, and spec mandates wipe anyway.
- Separate SQL scripts: Rejected — inconsistent with EF Core migration pattern used throughout the project.

### 2. Availability Engine — Computed at Transaction Time

**Decision**: Availability computed via LINQ/SQL queries against live Appropriation and Encumbrance rows within the same DbContext transaction. No stored computed columns, no background jobs, no caching.

**Rationale**: Constitution Principles III and V mandate computation from current data at transaction time. This ensures consistency — two concurrent transactions see the same availability until one commits.

**Alternatives considered**:
- Stored computed columns: Rejected — violates "no derived data stored" principle.
- Background cache/snapshot: Rejected — violates "computed at transaction time" principle.
- Separate availability microservice: Rejected — over-engineering for a module-level concern.

### 3. AllowOverrun Inheritance Chain

**Decision**: Three-level null-coalesce chain: `BudgetItem.AllowOverrun ?? Budget.AllowOverrun ?? BudgetType.AllowOverrun`. Evaluated in application code at validation time, not in the database.

**Rationale**: The nullable bool pattern (`bool?`) with null-means-inherit is the spec's explicit design. BudgetType provides the concrete default (never null). This avoids stored denormalization while keeping the inheritance flexible.

**Alternatives considered**:
- Database computed column: Rejected — would require JOIN at row level, violating the "no stored derived values" principle.
- Flattened copy on BudgetItem: Rejected — would create stale data if BudgetType changes.

### 4. Level Computation — Recursive Hierarchy

**Decision**: Compute Level at query time using recursive CTE (SQL Server) or iterative CTE via EF Core raw SQL / `FromSqlRaw`. For in-memory scenarios (unit tests), use iterative parent-walking.

**Rationale**: SQL Server supports recursive CTEs natively. EF Core's `FromSqlRaw` allows raw SQL for performance-critical hierarchy queries. For DTOs, Level is an integer computed during projection.

**Alternatives considered**:
- Materialized path (store path string): Rejected — violates "no Level stored" principle.
- Closure table: Rejected — additional table not in spec, adds complexity.
- Application-side recursion only: Rejected — N+1 query risk for deep hierarchies; CTE is more efficient.

### 5. IsReversed Computation

**Decision**: `IsReversed` computed as `dbContext.Encumbrances.Any(e => e.ReversalOfId == encumbrance.Id)` in the DTO projection. Not stored on the entity.

**Rationale**: Simple existence check. SQL Server optimizes this via the ReversalOfId index. Avoids storing a derived boolean that could become stale.

**Alternatives considered**:
- Stored IsReversed column: Rejected — violates "no derived data stored" principle. Would need trigger/update on every reversal, adding complexity.
- Count-based (ReversedCount > 0): Rejected — equivalent to existence check, no benefit.

### 6. Contextual FK Projections (Fund/FiscalYear on Appropriation/Encumbrance)

**Decision**: Expose Fund, FiscalYear, BudgetItem context via LINQ joins through `Appropriation -> Budget -> Fund/FiscalYear` in DTO projections. Never stored on the child entity.

**Rationale**: The spec explicitly forbids denormalized FKs on Appropriation (FundId, FiscalYearId) and Encumbrance (FundId, FiscalYearId, BudgetId, BudgetItemId). Join projections keep the schema clean while providing full context in DTOs.

**Alternatives considered**:
- Database views: Rejected — adds maintenance overhead, not aligned with EF Core entity patterns.
- Stored denormalized columns: Rejected — violates spec explicitly.

### 7. Permission Seeding

**Decision**: Seed 5 fixed roles (Admin, BudgetOfficer, Approver, ProcurementOfficer, Analyst) with explicit permission sets in the migration seed. Use existing `PermissionCodes` constants.

**Rationale**: Clarified in Session 2026-09-05 — fixed mapping with deterministic permissions enables reproducible SC-007 tests (401/403 checks).

**Alternatives considered**:
- Dynamic role configuration: Rejected — makes acceptance tests non-deterministic.
- Permission codes only without roles: Rejected — would leave role assignment to deployment, creating test ambiguity.

### 8. Transfer AppropriationType — Draft Only

**Decision**: Transfer type is valid only while the Appropriation is in Draft status. Once submitted, all fund movement uses Adjustment type (signed Amount: positive adds, negative subtracts).

**Rationale**: Clarified in Session 2026-09-05 — simplifies availability logic (Transfer-in/out only applies in Draft) and avoids dual-path availability computation.

**Alternatives considered**:
- Transfer usable at Active state: Rejected — would require Transfer-in/Transfer-out semantics in the availability formula, adding complexity.
- Remove Transfer type entirely: Rejected — still needed for initial fund allocation between BudgetItems in Draft.

### 9. Warning Control Method — Audit Log

**Decision**: When BudgetType ControlMethod is Warning and over-availability occurs, log the warning to the existing audit log infrastructure (same as authorization decisions). No new table.

**Rationale**: Clarified in Session 2026-09-05 — aligns with Constitution Principle VII (audited decisions), avoids new table, gives analysts a unified event stream.

**Alternatives considered**:
- Dedicated BudgetWarning table: Rejected — adds schema complexity for a secondary concern.
- Application logger only (no persistence): Rejected — warnings would not be queryable by analysts.

### 10. Encumbrance Reversal Pattern

**Decision**: Reversal creates a new Encumbrance row with `ReversalOfId` pointing to the original. The original row is unchanged. `IsReversed` computed via existence check on the reversal row. Availability restored by excluding reversed encumbrances.

**Rationale**: Immutable original row pattern — reversal is a new transactional event, not a mutation. The `ReversalOfId` FK (Restrict on delete) preserves referential integrity.

**Alternatives considered**:
- Mutate original row (set IsReversed=true, Status=Reversed): Rejected — violates immutable audit trail principle, and the spec explicitly stores ReversalOfId on the reversal row only.
- Soft delete original: Rejected — would lose the original amount in availability computations.
