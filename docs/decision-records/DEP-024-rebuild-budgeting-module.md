# DEP-024: Rebuild Budgeting Module from Scratch

**Date**: 2026-09-04
**Status**: Accepted
**Deciders**: Engineering Team

## Context

The Budgeting module stores string statuses, snapshot amount columns, stale dimension columns, and denormalized FK chains (Appropriation/Encumbrance carry FundId/FiscalYearId/BudgetId/BudgetItemId). Actual expenditure is derivable from posted Moves, yet budget tables store derived balances that can drift. Per Constitution Principles III/V, availability must be computed at transaction time; per Principle VI, no denormalized derived data.

## Decision

Drop and recreate the 7 Budgeting tables (BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance) with enum statuses, zero stored derived/duplicate columns, a transaction-time availability engine, and derived-value query projections. Rebuild the full Application layer, Web endpoints, frontend, tests, seeds, and docs. Wipe all existing Budgeting rows (fresh start, data loss accepted).

Key model decisions (from spec clarifications):
- No Transfer type; post-submission appropriation changes use signed Adjustment rows (positive adds, negative subtracts availability).
- No Appropriation reversal operation or Reversed status; encumbrance reversal keeps ReversalOfId/ReversalReason rows.
- Encumbrance availability uses the whole-BudgetItem net (appropriationId is only the anchor).
- Appropriation Update/Delete allowed only while Draft.
- Document numbers: Budget→BGT (new), Appropriation APP→APR, Encumbrance→ENC (safe under wipe).

## Consequences

- **Breaking contract change** (Principle IX): endpoints and payloads removed/reshaped; OpenAPI + NSwag client regenerated.
- **Permissions**: existing six families reused; missing lifecycle members added (least-privilege, SC-008).
- **Migration**: single EF Core wipe-and-recreate migration; historical migrations untouched; DocumentSequence BGT/APR/ENC rows ensured; idempotent reseed.
- **Scope**: FiscalYears, Currencies, Accounts, CostCenters, Suppliers, PurchaseOrders, Moves, PaymentOrders untouched. PostingPipeline integration out of scope (future work).
- **Remediation**: Down migration restores prior schema (data loss accepted per wipe decision).
