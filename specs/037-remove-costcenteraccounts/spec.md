# Feature Specification: Remove CostCenterAccounts Join Table

**Feature Branch**: `037-remove-costcenteraccounts`
**Created**: 2026-09-08
**Decision Record**: DEP-026

## Context

`CostCenterAccounts` is a join table (CostCenterId, AccountId) with no consumers beyond seed data and the DeleteCostCenter guard. Account entity has no CostCenter relationship (journal entry lines carry CostCenterId directly). Verified references: entity, config, seed (CostCenterAccountSeedData), initialiser trigger + call site, both contexts, DeleteCostCenterCommand guard. Zero other Application/Web/frontend references.

## Requirements

- **FR-1**: Migration drops CostCenterAccounts (composite PK, FK_CostCenterAccounts_CostCenters_CostCenterId). Down recreates.
- **FR-2**: Deletion report [CostCenterAccountDeletionReport] + SecurityAuditLog entry before drop.
- **FR-3**: Delete entity + config + seed + initialiser method/call + both DbSets.
- **FR-4**: DeleteCostCenterCommand: remove accounts guard (no reference mechanism post-removal). Projects guard retained.
- **FR-5**: Docs sweep: database-tables-complete.md (106 tables).

## Deviation from original plan (accepted)

- Planned "CostCenter.Accounts.Count == 0" nav guard: NOT possible — Account has no CostCenter relationship and adding one (Account.CostCenterId column) exceeds removal scope.
- Planned test "DeleteCostCenter rejects when accounts exist": superseded — no account-reference mechanism exists. Suite gate skipped per user instruction.
