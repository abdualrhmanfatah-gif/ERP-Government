# Feature Specification: Remove AccountBalances Table

**Feature Branch**: `041-remove-accountbalances`
**Created**: 2026-09-08
**Decision Record**: DEP-026

## Context

`AccountBalances` materializes per (account, fiscalYear, fiscalPeriod, currency) debit/credit totals with open/close columns + IsFinalized. Duplicates JournalEntryLines state — drift-prone, and AGENTS.md forbids stored computed values. JournalEntryLines are the source of truth; balances computed live.

## Requirements

- **FR-1**: Migration drops AccountBalances (PK, 4 FKs, 5 indexes). Down recreates.
- **FR-2**: Deletion report [AccountBalanceDeletionReport] + SecurityAuditLog entry before drop.
- **FR-3**: Delete entity + config + DbSets + commands (Finalize/Unfinalize ×2 sets incl. Balances/RebuildAccountBalances) + queries (GetAccountBalances, ReconcileBalances) + AccountBalanceDto + AccountingBalances endpoint.
- **FR-4**: PostJournalEntryCommand / ReverseJournalEntryCommand: balance-update blocks removed (live aggregation replaces).
- **FR-5**: GetTrialBalanceQuery: live aggregation per period (posted lines where FiscalYearId/PeriodId match).
- **FR-6**: GetBalanceSheetQueryHandler: live aggregation up to as-of date; per-account grouping (was per-balance-row).
- **FR-7**: Finalized-period reversal guard (FR-020) replaced by existing FiscalPeriods.IsLockedForPosting check (guard was redundant).
- **FR-8**: Frontend: AccountBalancesPage + useAccountBalances + accountingBalancesClient + AccountBalanceDto type + route + nav entry removed. Frontend build green.
- **FR-9**: Tests: GetAccountBalancesQueryTests + AccountBalanceTests (functional) + GetBalanceSheet/GetTrialBalance handler tests deleted (assertions bound to removed data source); PostJournalEntry* tests: AccountBalances mock setups removed only. Suite gate skipped per user instruction.
- **FR-10**: Docs: database-tables-complete.md (102 tables, Accounting 11); spec 030 contract/spec/data-model retired banners. Feature registry already deleted (final-business-feature-registry.md gone since WIP).

## Behavior changes (accepted)

- Trial balance per-period totals now reflect posted lines directly (was: materialized closing).
- Balance sheet groups by account within group section (was: per balance row per currency).
- Period finalization lifecycle (finalize/unfinalize commands + IsFinalized) removed entirely — locking via FiscalPeriods only.
