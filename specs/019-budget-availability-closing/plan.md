# Implementation Plan: Financial Control Layer — Multi-Dimension Budget Availability, Year-End Operations, and Final Account

**Branch**: `019-budget-availability-closing` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/019-budget-availability-closing/spec.md`

## Summary

Extend the existing Budgeting module with multi-dimensional budget availability (fund, program, project, budget item), fiscal year closing operations (lapse run with append-only log), and final account generation (closing entries, final balances, budget-versus-actual comparison). Also adds yearly collection/disbursement statements and a constitution amendment (last task).

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: EF Core, MediatR, FluentValidation, Minimal APIs
**Storage**: SQL Server
**Testing**: xUnit, FluentAssertions, Testcontainers (existing test projects)
**Target Platform**: Linux server (Docker)
**Project Type**: Web service (backend only; frontend out of scope)
**Performance Goals**: SC-001 <5s availability query, SC-003 <60s lapse run, SC-006 <30s final account generation
**Constraints**: Single base currency, decimal(23,2), optimistic concurrency via RowVersion
**Scale/Scope**: Government ERP, moderate transaction volume

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | PASS | Domain → Application → Infrastructure → Web. No cross-layer violations. New entities in Domain/Budgeting, new commands/queries in Application/Budgeting. |
| II. Bounded Contexts | PASS | YearClosingRun, FinalAccount, FinalAccountLine live in Budgeting module. Closing entries route through AccountingEvent pipeline. No cross-module writes. |
| III. Server-Side Business Rules | PASS | Lapse idempotency, payment blocking, final account immutability — all enforced server-side in handlers. |
| IV. Financial Integrity | PASS | Closing entries generated as balanced JournalEntries via AccountingEvent pipeline. No stored computed columns — FinalAccountLine materialized at generation time. |
| V. Budget Control Before Expenditure | PASS (extended) | Multi-dimensional availability check becomes the standard gate. Full chain: appropriations → encumbrances → payments. Constitution amendment (US4) documents this. |
| VI. Data Integrity | PASS | Migrations, Restrict FK, RowVersion, explicit decimal precision. YearClosingRun append-only. FinalAccount immutable after Issued. |
| VII. Authorization & SoD | PASS | Dedicated permission codes: FinancialControl.LapseYear, FinancialControl.ApproveFinalAccount. |
| VIII. Approval Workflows | PASS | FinalAccount approval recorded via ApprovalHistory (if approval workflow exists) or direct status transition with audit. |
| IX. API Contract | PASS | OpenAPI via NSwag. ProblemDetails error contract. |
| X. UI/Design System | N/A | Backend only. |
| XI. Testing | PASS | TDD mandatory. Tests for dimension breakdown math, lapse idempotency + payment block, final account totals vs journal, year-boundary document handling. |
| XII. Controlled Change | PASS | Constitution amendment (US4) is a registered change to Principle V with version bump. |

## Project Structure

### Documentation (this feature)

```text
specs/019-budget-availability-closing/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── api-contracts.md # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Budgeting/
│   ├── Entities/
│   │   ├── YearClosingRun.cs              # NEW — append-only fiscal year closing log
│   │   ├── FinalAccount.cs                # NEW — authoritative financial statement
│   │   └── FinalAccountLine.cs            # NEW — line item in final account
│   └── Enums/
│       ├── YearClosingRunStatus.cs        # NEW — Completed, Reversed
│       └── FinalAccountStatus.cs          # NEW — Draft, Issued
├── Application/Budgeting/
│   ├── Commands/FinancialControl/
│   │   ├── LapseFiscalYear/
│   │   │   ├── LapseFiscalYearCommand.cs
│   │   │   ├── LapseFiscalYearCommandHandler.cs
│   │   │   └── LapseFiscalYearCommandValidator.cs
│   │   ├── ReopenFiscalYear/
│   │   │   ├── ReopenFiscalYearCommand.cs
│   │   │   ├── ReopenFiscalYearCommandHandler.cs
│   │   │   └── ReopenFiscalYearCommandValidator.cs
│   │   ├── GenerateFinalAccount/
│   │   │   ├── GenerateFinalAccountCommand.cs
│   │   │   ├── GenerateFinalAccountCommandHandler.cs
│   │   │   └── GenerateFinalAccountCommandValidator.cs
│   │   └── IssueFinalAccount/
│   │       ├── IssueFinalAccountCommand.cs
│   │       ├── IssueFinalAccountCommandHandler.cs
│   │       └── IssueFinalAccountCommandValidator.cs
│   ├── Queries/FinancialControl/
│   │   ├── GetAvailabilityBreakdown/
│   │   │   └── GetAvailabilityBreakdownQuery.cs
│   │   ├── GetYearClosingRuns/
│   │   │   └── GetYearClosingRunsQuery.cs
│   │   ├── GetFinalAccount/
│   │   │   └── GetFinalAccountQuery.cs
│   │   ├── GetCollectionStatement/
│   │   │   └── GetCollectionStatementQuery.cs
│   │   └── GetDisbursementStatement/
│   │       └── GetDisbursementStatementQuery.cs
│   └── Common/
│       ├── BudgetAvailabilityService.cs   # EXTEND — add dimension breakdown method
│       └── AvailabilityBreakdownDto.cs    # NEW — dimension breakdown DTO
├── Web/Endpoints/FinancialControl/
│   ├── Availability.cs                    # IEndpointGroup — availability query
│   ├── YearClosing.cs                     # IEndpointGroup — lapse/reopen
│   └── FinalAccounts.cs                   # IEndpointGroup — final account CRUD
├── Infrastructure/Data/
│   ├── ApplicationDbContext.cs            # Add DbSets for new entities
│   └── Configurations/
│       ├── YearClosingRunConfiguration.cs  # NEW
│       ├── FinalAccountConfiguration.cs    # NEW
│       └── FinalAccountLineConfiguration.cs # NEW
└── Application/Common/Security/
    └── PermissionCodes.cs                 # ADD FinancialControl.* codes

tests/
├── Application.UnitTests/Budgeting/
│   ├── LapseFiscalYearTests.cs            # NEW
│   ├── ReopenFiscalYearTests.cs           # NEW
│   ├── GenerateFinalAccountTests.cs       # NEW
│   └── AvailabilityBreakdownTests.cs      # NEW
└── Application.FunctionalTests/Budgeting/
    ├── YearClosingLifecycleTests.cs       # NEW
    └── FinalAccountGenerationTests.cs     # NEW
```

**Structure Decision**: Extends existing Budgeting module. New entities in Domain/Budgeting/Entities. Commands/queries follow the one-folder-per-action pattern under Application/Budgeting/Commands/FinancialControl and Queries/FinancialControl. Availability service extension in Common/. Endpoints in Web/Endpoints/FinancialControl/.

**As-built conventions (from user)**:
- Same conventions as previous specs.
- Extended availability builds on BudgetAvailabilityService (015) + MoveLine dims (014) + budget structures (015: Fund/Program/Project/Item).
- Zero stored computed — final account lines materialized into FinalAccountLine at generation time (document snapshot, not cache).
- Lapse: open encumbrances → Cancelled(lapsed), remaining appropriation balances → lapsed; block payments/requests referencing lapsed items; idempotent per fiscal year.
- Closing entries: generated JournalEntries via AccountingEvent (revenue/expense closing to final-account account).
- Constitution (LAST TASK): .specify/memory/constitution.md v1.2.0 → v1.3.0 — Principle V chain becomes appropriations → encumbrances → payments; gates enforce from the NEXT spec onward.

## Key Design Decisions

### 1. Multi-Dimension Availability

**Decision**: Extend `BudgetAvailabilityService` with a new method `GetAvailabilityBreakdownAsync(int budgetItemId, int fiscalYearId)` that returns a list of dimension-grouped availability records.

**Rationale**: The existing service computes aggregate availability at the budget-item level. The new method adds a GROUP BY on fund, program, project dimensions while reusing the same appropriation/encumbrance/payment query logic. No new tables needed — the dimensions already exist on BudgetItem (FundId) and Appropriation/Encumbrance (link to BudgetItem).

**Alternative considered**: Create a separate `MultiDimensionAvailabilityService` — rejected because it would duplicate query logic and violate the single-service pattern.

### 2. YearClosingRun as Append-Only Log

**Decision**: `YearClosingRun` is INSERT-ONLY. Reopening creates a new `YearClosingRun` record with `Status = Reversed` that references the original run.

**Rationale**: Constitution Principle VIII (Audit Immutability) requires audit trails to be INSERT-ONLY. The append-only pattern is consistent with `ApprovalHistory` and `DocumentStatusLog`.

**Alternative considered**: Allow editing YearClosingRun — rejected because it violates audit immutability.

### 3. FinalAccount Lines Materialized at Generation

**Decision**: `FinalAccountLine` records are created (materialized) when the final account is generated. They are a snapshot of the budget-vs-actual state at generation time, not a live cache.

**Rationale**: User requirement: "Zero stored computed — final account lines materialized into FinalAccountLine at generation time (document snapshot, not cache)." This ensures the final account is immutable once generated.

**Alternative considered**: Compute lines on-the-fly at query time — rejected because it would not guarantee immutability and could change if underlying data changes.

### 4. Lapse Idempotency

**Decision**: The lapse command checks for an existing `YearClosingRun` with `Status = Completed` for the same fiscal year and rejects duplicate runs. Reopening creates a new `YearClosingRun` with `Status = Reversed`.

**Rationale**: User requirement: "idempotent per fiscal year." The entity enforces uniqueness on (FiscalYearId, Status = Completed).

### 5. Closing Entries via AccountingEvent Pipeline

**Decision**: Closing entries (revenue/expense zeroing) are generated as `JournalEntry` records via the existing `AccountingEvent` pipeline. The lapse command raises an `AccountingEvent` of type `ClosingEntry` which the posting rules process into balanced journal entries.

**Rationale**: Constitution Principle II (Event-Carried Integration) requires cross-module side effects to travel through domain events. The Accounting module processes the event into a balanced journal entry.

### 6. Permission Codes

**Decision**: Add two new permission codes:
- `FinancialControl.LapseYear` — for lapse and reopen operations
- `FinancialControl.ApproveFinalAccount` — for final account approval

**Rationale**: Follows the existing `{Module}.{Action}` pattern in PermissionCodes.cs. These are new module-level codes for the financial control operations.

### 7. Constitution Amendment

**Decision**: Amendment is the LAST task. Principle V is updated to explicitly state the full chain: appropriations → encumbrances → payments. Version bumps from 1.2.0 to 1.3.0 (MINOR for expanded principle).

**Rationale**: User requirement: "Constitution (LAST TASK): .specify/memory/constitution.md v1.2.0 → v1.3.0 — Principle V chain becomes appropriations → encumbrances → payments; gates enforce from the NEXT spec onward."

## Migration Strategy

1. New entities (YearClosingRun, FinalAccount, FinalAccountLine) — single migration adding three tables.
2. Extend BudgetAvailabilityService — no schema change, code-only extension.
3. Add permission codes — seed data migration.
4. Add closing entry posting rules — seed data migration.

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Lapse run performance on large datasets | Medium | Batch processing, indexed queries on FiscalYearId + Status |
| Concurrent availability queries during lapse | Medium | Application-level locking on FiscalYearId during lapse (FR-018) |
| Year-boundary document splitting complexity | Low | Proportional allocation formula defined in research phase |
| Final account generation time | Low | Materialized lines, not on-the-fly computation |
