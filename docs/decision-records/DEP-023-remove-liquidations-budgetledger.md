# DEP-023: Remove Liquidations and BudgetLedgerEntries Tables

**Date**: 2026-09-04
**Status**: Accepted
**Deciders**: Engineering Team
**Supersedes**: Spec 008 (which kept Liquidation)
**Constitution Amendment**: Principle V (Budget Control Chain) — version 1.0.0 → 1.1.0

## Context

The budget module contains two tables being removed from the budget cycle:

- **Liquidations** (BF-004) — lifecycle table for liquidating encumbrances. The Constitution Principle V chain currently reads Budget → Appropriation → Encumbrance → Liquidation → Payment.
- **BudgetLedgerEntries** — standalone write-only ledger with no consumers. No code references this table beyond its own entity and configuration.

Per Constitution Principle XII, changes to principles, module boundaries, or security model REQUIRE a numbered decision record.

## Decision

1. Drop **Liquidations** table with all FKs (11 FKs + self-FK ReversalOfId), indexes, PK. Drop **BudgetLedgerEntries** table with all FKs, indexes, PK.
2. Remove **PaymentOrders.LiquidationId** column + index.
3. Remove counter fields: **Encumbrances.LiquidatedAmount**, **Encumbrances.LiquidationStatus**, **Budgets.LiquidatedAmount**, **BudgetItems.LiquidatedAmount**, **Appropriations.LiquidatedAmount**.
4. Delete all Domain entities/enums/events, Application commands/queries/DTOs, Web endpoints/policies, Infrastructure configs/DbSets/seeds, tests, and docs referencing these tables.
5. Amend Constitution **Principle V** chain to: **Budget → Appropriation → Encumbrance → Payment** (Liquidation removed).
6. Remove **BF-004** from feature registry (12 → 11 business features).
7. Generate **deletion report** tables [LiquidationDeletionReport] + [BudgetLedgerEntryDeletionReport] before drops for audit. Log **SecurityAuditLog** entries per table.
8. Single EF Core **migration** with full reversibility (Down recreates schema). Historical migrations remain untouched.

## Scope

- **IN**: ~48 files (18 deleted, 30 modified), 2 tables dropped, 7 columns dropped, 2 audit tables created, 5 enums deleted, 6 permissions deleted, 2 endpoint files deleted.
- **OUT**: PaymentOrders/PaymentOrderLines/PaymentOrderDeductions, Encumbrances, Budgets, BudgetItems, Appropriations, Funds, Moves, all other modules. No replacement tables or columns. Frontend has zero references (verified).

## Rationale

- Liquidation duplicates encumbrance release logic. BudgetLedgerEntry was write-only with no readers.
- Reduces schema complexity, permission surface, and code surface area.
- Budget → Appropriation → Encumbrance → Payment is the simplified control chain.

## Migration / Remediation Plan

1. Create deletion report tables; populate via INSERT...SELECT from source tables.
2. Log SecurityAuditLog entries (entity=System, action=SchemaDeletion, target=TableName).
3. Drop FK columns, counter columns, and tables in dependency order.
4. Update EF Core model snapshot.
5. Down migration recreates full schema from deletion reports.
6. Docs: database-schema.md (remove tables 49-50, LiquidatedAmount columns), feature registry (remove BF-004), OpenAPI regeneration, Constitution amendment.

## Consequences

- **Schema simplification**: 2 fewer tables, 7 fewer columns, 6 fewer permissions, 8 fewer endpoints.
- **Data preservation**: Deletion reports + SecurityAuditLog entries.
- **Reversibility**: Full migration reversibility.
- **Constitution**: Principle V amended (MINOR version bump 1.0.0 → 1.1.0 per versioning rules: material expansion of existing principle).
- **Feature registry**: 12 → 11 business features. Budget Cycle: 3 → 2 features (BF-002, BF-003). Version 1.1 → 1.2.
- **No business logic change**: Encumbrance release/reversal, Appropriation amount updates, Budget/BudgetItem calculations remain (only ledger entry creation removed).
