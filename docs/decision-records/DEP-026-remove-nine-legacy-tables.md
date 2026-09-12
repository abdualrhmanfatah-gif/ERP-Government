# DEP-026: Remove Nine Legacy/Unused Tables (Security Model Simplification + Ledger Recompute-on-Read)

**Date**: 2026-09-08
**Status**: Accepted
**Deciders**: Engineering Team
**Implements**: Constitution Principle XII (Controlled Architectural Change)
**Related**: DEP-022 (payment tables), DEP-023 (liquidations)

## Context

Nine tables are no longer justified in the schema. They fall into three groups:

1. **Unenforced security model scaffolding** — `RecordRules`, `FieldSecurityPolicies`, `SoDMatrix`, `ApprovalDelegations`. These tables model row-level security, field-level security, segregation-of-duties checks, and approval delegation, but no enforcement path reads them. Endpoint authorization is currently open placeholder (see Registered Exceptions #1); use-case authorization is the effective control. Maintaining tables, seeds, and configurations for mechanisms that are never evaluated is dead weight and widens the security-model surface the RBAC enforcement spec must eventually implement.
2. **Duplicate/redundant accounting state** — `AccountBalances` (materialized balances duplicating `JournalEntryLines`, drift-prone), `AccountingEvents` (intermediate staging entity in the posting pipeline whose outbox shape can write `JournalEntries` directly), `RecurringEntryExecutionLogs` (audit log with no reader), `CashFlowMappingRules` (configuration table with a single hardcoded consumer).
3. **Unused join entity** — `CostCenterAccounts` (many-to-many CostCenter↔Account with no active consumer).

Per Constitution Principle XII, changes to the security model and module boundaries REQUIRE a numbered decision record.

## Decision

1. Drop all nine tables with their FKs, indexes, and PKs: `RecordRules`, `FieldSecurityPolicies`, `SoDMatrix`, `CostCenterAccounts`, `CashFlowMappingRules`, `ApprovalDelegations`, `RecurringEntryExecutionLogs`, `AccountBalances`, `AccountingEvents`.
2. Drop `JournalEntries.SourceEventId` column + FK + index (AccountingEvents removal).
3. Delete all Domain entities/configs, Application commands/queries/DTOs/event-handlers, Web endpoints/policies/permission codes, Infrastructure configurations/seeds, tests, and frontend pages/clients referencing them.
4. Replace `AccountBalances` reads with live aggregation from `JournalEntryLines` (group by AccountId/CurrencyId, sum Debit − Credit) at query time — consistent with "no stored computed values" (AGENTS.md).
5. Collapse the posting pipeline: outbox shape retained, `JournalEntries` written directly on processing; `AccountingEvents` staging entity removed.
6. Remove associated `PermissionCodes` (`ApprovalDelegationsView/Manage`, `AccountingEventsRead`) and `RolePermissionSeedData` entries.
7. Generate a **deletion report** table per dropped table before the drop for audit ([RecordRuleDeletionReport], [FieldSecurityPolicyDeletionReport], [SoDMatrixDeletionReport], [CostCenterAccountDeletionReport], [CashFlowMappingRuleDeletionReport], [ApprovalDelegationDeletionReport], [RecurringEntryExecutionLogDeletionReport], [AccountBalanceDeletionReport], [AccountingEventDeletionReport]).
8. Retire `specs/002-posting-pipeline/` to `specs/_retired/002-posting-pipeline/` at spec 042 completion (pipeline shape superseded, not deleted — outbox remains).
9. One EF Core migration per spec, full reversibility (Down recreates schema), historical migrations untouched.

## Scope

- **IN**: 9 tables, `JournalEntries.SourceEventId`, ~60 files across Domain/Application/Web/Infrastructure/tests/frontend, related permission codes, seed data, initialiser triggers.
- **OUT**: All other security entities (`SecurityRole`, `RolePermission`, `SecurityPermission`, `UserPermission`, `SecurityAuditLog`, `ApprovalRule`, `ApprovalHistory`), all other accounting entities, `MoveGenerator` posting logic (retained, redirected), outbox processing shape, all remaining modules.

## Rationale

- Unenforced security scaffolding misleads implementers into believing row/field-level security and delegation exist. Removing them makes the actual security model honest pending the RBAC enforcement spec.
- `AccountBalances` duplicates `JournalEntryLines` state and can drift; live aggregation is the constitutionally consistent computation point.
- `AccountingEvents` adds a staging hop with no transform benefit between domain events and `JournalEntries`.
- Each removed table reduces schema complexity, permission surface, and seed surface with no consumer loss (verified per-spec).

## Migration / Remediation Plan

1. Per-spec execution order (least → most dependent): RecordRules, FieldSecurityPolicies, SoDMatrix, CostCenterAccounts, CashFlowMappingRules, ApprovalDelegations, RecurringEntryExecutionLogs, AccountBalances, AccountingEvents.
2. Domain layer deleted before Application (compile break is the discovery test), Application before Web.
3. Deletion report tables populated via INSERT...SELECT before each drop; SecurityAuditLog entries per table (entity=System, action=SchemaDeletion).
4. Migration last, after build green without the table. Snapshot regen via `dotnet ef migrations add Remove<X>` then trim to the intended block (InitialCreate is 343KB — manual full-snapshot edits error-prone).
5. Full 5-project test suite gates each spec merge.
6. NSwag client regeneration (`npm run generate-api`) after Web layer changes; git-diff and revert unrelated regen noise.
7. Docs sweep per spec: database-schema.md, database-tables-complete.md, feature-architecture-map-v1.0.md, AGENTS.md RBAC line at spec 036 close.

## Consequences

- **Schema**: 9 fewer tables + 1 column. Audit preserved via deletion reports.
- **Security model**: single source of truth (roles/permissions/user-permissions + audit). RBAC enforcement spec scope shrinks — no row/field/delegation/SoD mechanisms to implement.
- **Ledger**: single source of truth (`JournalEntryLines`); balances and reports computed at read time.
- **Posting pipeline**: slimmer but shape-preserving (outbox idempotency retained).
- **Feature registry**: "Account Balances monitoring" feature removed (12 → 11 business features) at spec 041.
- **Reversibility**: full migration Down paths recreate schema from deletion reports.
