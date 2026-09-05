# Implementation Plan: Remove Liquidations and BudgetLedgerEntries

**Branch**: `009-remove-liquidations-budgetledger` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/009-remove-liquidations-budgetledger/spec.md`

## Summary

Drop the Liquidations and BudgetLedgerEntries tables with all relationships, counter fields, permissions, enums, events, commands, queries, DTOs, endpoints, configurations, seeds, tests, and documentation. Remove LiquidationId from PaymentOrders. Remove LiquidatedAmount/LiquidationStatus from Encumbrances, Budgets, BudgetItems, and Appropriations. Amend Constitution Principle V chain (remove Liquidation). Create deletion report tables for audit. Single EF Core migration with full reversibility.

**Scope**: ~48 files affected (18 deleted, 30 modified). Zero frontend changes. No replacement tables.

## Technical Context

**Language/Version**: C# / .NET 10.0 (SDK 10.0.201)

**Primary Dependencies**: MediatR 14.1.0 (CQRS), FluentValidation 12.1.1, AutoMapper 16.1.1, Entity Framework Core 10.0.5, SQL Server (via Aspire), NUnit 4.5.1, Moq 4.20.72, Shouldly 4.3.0

**Storage**: SQL Server via EF Core. Single DbContext (ApplicationDbContext) with configuration-per-entity pattern. All FKs use ReferentialAction.Restrict. Optimistic concurrency via rowversion.

**Testing**: NUnit 4.5.1 with Moq (unit), EF Core InMemory (functional), Respawn (DB reset), Playwright + Reqnroll BDD (acceptance). Test projects: Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests.

**Target Platform**: Linux server (containerized via .NET Aspire)

**Project Type**: Web application (Clean Architecture: Domain → Application → Infrastructure → Web)

**Performance Goals**: No specific performance targets for this deletion feature

**Constraints**: Warnings treated as errors. Migrations only (no auto-schema). All FKs Restrict. Financial audit immutability (Principle VIII).

**Scale/Scope**: ~48 files, 2 tables dropped, 7 columns dropped, 2 tables created (audit), 5 enums deleted, 6 commands modified, 1 query modified, 6 permissions deleted, 2 endpoint files deleted, ~15 test assertions updated.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | Deletions respect layer boundaries. No cross-layer references created. |
| II. Bounded Contexts | ✅ PASS | Liquidation/BudgetLedgerEntry are Budgeting module internals. |
| III. Server-Side Rules | ✅ PASS | CancelEncumbrance guard removal is justified (R2). State machine guards remain. |
| IV. Financial Integrity | ⚠️ REQUIRES DECISION RECORD | BudgetLedgerEntry removal affects financial audit trail. Covered by Principle XII decision record + SecurityAuditLog. |
| V. Budget Control Chain | ⚠️ AMENDMENT REQUIRED | Chain simplified: Budget → Appropriation → Encumbrance → Payment. Decision record per Principle XII. |
| VI. Data Integrity | ✅ PASS | Migration is versioned, reviewable. Deletion reports are INSERT-ONLY audit artifacts. |
| VII. Authorization | ✅ PASS | 6 permissions removed cleanly. No anonymous access introduced. |
| VIII. Audit Immutability | ✅ PASS | Deletion reports are INSERT-ONLY. SecurityAuditLog entries written per table deletion. |
| IX. API Contract | ✅ PASS | OpenAPI regenerated. Removed endpoints/schemas disappear. |
| X. UI Consistency | ✅ PASS | Frontend has zero references (verified). No UI changes needed. |
| XI. Testing | ✅ PASS | Tests updated/deleted. Compilation and test pass are success criteria. |
| XII. Controlled Change | ⚠️ DECISION RECORD REQUIRED | Principles IV and V amendments require decision record. |

**Gate result**: PASS with conditions — decision record required for Principles IV and V amendments before implementation.

## Project Structure

### Documentation (this feature)

```text
specs/009-remove-liquidations-budgetledger/
├── plan.md              # This file
├── research.md          # Phase 0: Research decisions (R1-R9)
├── data-model.md        # Phase 1: Entity/table changes
├── quickstart.md        # Phase 1: Validation scenarios
└── tasks.md             # Phase 2: Task breakdown (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/Budgeting/
│   ├── Entities/        # DELETE: Liquidation.cs, BudgetLedgerEntry.cs
│   │                  # MODIFY: Encumbrance.cs, Budget.cs, BudgetItem.cs, Appropriation.cs, PaymentOrder.cs
│   ├── Enums/           # DELETE: LiquidationStatus.cs, LiquidationType.cs, BudgetLedgerEntryType.cs,
│   │                  #         BudgetLedgerDirection.cs, BudgetLedgerStatus.cs
│   │                  # MODIFY: ReleaseStatus.cs (comment)
│   └── Events/Budgeting/ # DELETE: LiquidationCreated.cs, LiquidationPosted.cs
├── Application/Budgeting/
│   ├── Commands/
│   │   ├── Liquidations/  # DELETE: entire directory (5 commands)
│   │   ├── Encumbrances/  # MODIFY: CreateEncumbrance, CancelEncumbrance, ApproveEncumbrance,
│   │   │                  #         ReleaseEncumbrance, ReverseEncumbrance
│   │   └── Appropriations/ # MODIFY: CreateAppropriation, ApproveAppropriation, ReverseAppropriation
│   ├── Queries/
│   │   ├── Liquidations/   # DELETE: entire directory (2 queries)
│   │   ├── BudgetLedgerEntries/ # DELETE: entire directory (1 query)
│   │   └── Budgets/       # MODIFY: GetBudgetSummaryQuery
│   └── Common/            # DELETE: LiquidationDto.cs, BudgetLedgerEntryDto.cs
│                        # MODIFY: EncumbranceDto.cs, BudgetDto.cs, BudgetItemDto.cs,
│                        #         AppropriationDto.cs, BudgetSummaryDto.cs
├── Application/Common/
│   ├── Security/          # MODIFY: PermissionCodes.cs (remove 6 constants)
│   └── Interfaces/        # MODIFY: IApplicationDbContext.cs (remove 2 DbSets)
├── Application/FinancialSettings/Common/Services/
│                        # MODIFY: DocumentSequenceService.cs (remove Liquidation prefix)
├── Infrastructure/Data/
│   ├── Configurations/Budgeting/ # DELETE: LiquidationConfiguration.cs, BudgetLedgerEntryConfiguration.cs
│   │                          # MODIFY: EncumbranceConfiguration.cs, BudgetConfiguration.cs,
│   │                          #         BudgetItemConfiguration.cs, AppropriationConfiguration.cs
│   ├── Configurations/Payments/  # MODIFY: PaymentOrderConfiguration.cs (remove LiquidationId index)
│   ├── ApplicationDbContext.cs    # MODIFY: remove 2 DbSets
│   └── Migrations/               # CREATE: new migration + updated snapshot
├── Web/Endpoints/Budgeting/      # DELETE: Liquidations.cs, BudgetLedgerEntries.cs
├── Web/DependencyInjection.cs    # MODIFY: remove 6 policy registrations
├── Web/wwwroot/openapi/          # REGENERATE: v1.json
└── Domain/Events/Budgeting/      # DELETE: LiquidationCreated.cs, LiquidationPosted.cs

tests/
├── Domain.UnitTests/Budgeting/   # MODIFY: EntityTests.cs (remove LiquidationTests, update EncumbranceTests)
├── Application.UnitTests/Budgeting/ # MODIFY: QueryTests.cs (remove LiquidatedAmount from test data)
└── Application.UnitTests/FinancialSettings/ # MODIFY: DocumentSequenceServiceTests.cs (remove Liquidation assertions)

docs/
├── database-schema.md            # MODIFY: remove tables 49-50, remove LiquidatedAmount columns from 45-48
└── final-business-feature-registry.md # MODIFY: remove BF-004, renumber, update counts

.specify/memory/
└── constitution.md               # MODIFY: amend Principle V, update version
```

**Structure Decision**: Clean Architecture monolith with Domain/Application/Infrastructure/Web layers. Feature affects all 4 layers symmetrically — deletions propagate inward from Web endpoints through Application commands/queries to Domain entities, with Infrastructure handling persistence.

## Complexity Tracking

> No Constitution violations that require justification — all changes are deletions or simplifications that align with existing principles.

| Item | Why Needed | Simpler Alternative Rejected Because |
|------|-----------|-------------------------------------|
| Decision record for Principle V amendment | Constitution Principle XII requires decision records for principle changes | N/A — amendment is mandatory, not optional |
| BudgetLedgerEntry creation removal from 6 commands | Table is being dropped; creation code becomes dead | Cannot keep creation code for non-existent table |
