# Feature Specification: Remove CashFlowMappingRules Table

**Feature Branch**: `038-remove-cashflowmappingrules`
**Created**: 2026-09-08
**Decision Record**: DEP-026

## Context

`CashFlowMappingRules` is a configuration table (AccountGroupId → CashFlowSectionType) with a single consumer: GetCashFlowStatementQueryHandler. Classification is derivable from AccountGroup.Type directly — the table adds configuration surface with no configurability benefit.

## Requirements

- **FR-1**: Migration drops CashFlowMappingRules (PK, FK to AccountGroups, unique filtered index). Down recreates.
- **FR-2**: Deletion report [CashFlowMappingRuleDeletionReport] + SecurityAuditLog entry before drop.
- **FR-3**: Delete entity + config + seed + initialiser seed block + both DbSets.
- **FR-4**: GetCashFlowStatementQueryHandler refactor: classification by AccountGroup.Type (Revenue/Expense → Operating, Asset → Investing, Equity/Liability → Financing), cash accounts (IsReconcilable) excluded; empty-rules warning branch removed.
- **FR-5**: Docs sweep: database-tables-complete.md (105 tables, Accounting 13).

## Behavior change (accepted)

- Liability lines now classify under Financing (previously unmapped — seed covered root group IDs 1-4 only). Economically standard; improves reconciliation.
- Empty-rules warning no longer emitted (classification no longer configurable).
- Suite gate + planned handler unit test skipped per user instruction.
