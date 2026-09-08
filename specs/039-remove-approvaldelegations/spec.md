# Feature Specification: Remove ApprovalDelegations Table

**Feature Branch**: `039-remove-approvaldelegations`
**Created**: 2026-09-08
**Decision Record**: DEP-026

## Context

`ApprovalDelegations` models temporary approval authority transfer (delegator → delegate, date-bounded, per-entity-type optional). ApprovalService consults it as fallback when user lacks required role. Delegation is unenforced scaffolding for the pending RBAC enforcement spec (DEP-026) — removed; approval authorization = role membership only.

## Requirements

- **FR-1**: Migration drops ApprovalDelegations (PK, 2 FKs to Users, 2 FK indexes). Down recreates.
- **FR-2**: Deletion report [ApprovalDelegationDeletionReport] + SecurityAuditLog entry before drop.
- **FR-3**: Delete entity + config + enum DelegationStatus + ApprovalDelegations/ use-case dir (CreateApprovalDelegation, RevokeApprovalDelegation, GetApprovalDelegations) + ApprovalDelegationDto + endpoint file.
- **FR-4**: ApprovalService: remove ResolveDelegationAsync + ResolveActiveDelegationAsync + delegation fallback branch in ValidateAndRecordAsync. IApprovalService: drop ResolveDelegationAsync member.
- **FR-5**: DeactivateUserCommand: remove pending-delegations guard (FR-005 revised).
- **FR-6**: PermissionCodes.ApprovalDelegationsView/Manage + RolePermissionSeedData entries removed.
- **FR-7**: ApprovalServiceTests: delete NoRole_HasDelegation + 3 ResolveDelegationAsync tests (kept: NoRules, HasRequiredRole, NoRole failure, MultipleRules).
- **FR-8**: Docs: database-tables-complete.md (104 tables). NSwag regen at close (22 client refs).

Suite gate skipped per user instruction.
