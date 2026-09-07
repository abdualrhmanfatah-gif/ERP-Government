# Feature Specification: Remove RecordRules Table

**Feature Branch**: `034-remove-recordrules`
**Created**: 2026-09-08
**Status**: Draft
**Decision Record**: DEP-026 (docs/decision-records/DEP-026-remove-nine-legacy-tables.md)

## Context

`RecordRules` models row-level security rules (entity-scoped, role-scoped, filter expressions) but no enforcement path evaluates them. The RBAC enforcement spec will not implement row-level rules (DEP-026). Table, entity, configuration, and DbSets are dead surface area.

References (verified): Domain entity, `IApplicationDbContext.cs:38`, `ApplicationDbContext.cs:40`, `RecordRuleConfiguration.cs`, migrations/snapshot, `docs/database-tables-complete.md` §2.7. Zero Application/Web/frontend references. No seed data, no initialiser trigger.

## Requirements

### FR-1: Schema Removal

The migration MUST drop the `RecordRules` table with its PK, unique index (`IX_RecordRules_EntityName_RoleId_RuleType`), index (`IX_RecordRules_RoleId`), and FK (`FK_RecordRules_SecurityRoles_RoleId`) — dropping indexes/FK with the table. Down migration MUST recreate the full schema.

### FR-2: Deletion Report

Before the drop, the migration MUST create `[RecordRuleDeletionReport]` capturing every deleted record's data (all columns + DeletedAt) via INSERT...SELECT, and MUST log a SecurityAuditLog entry (entity=System, action=SchemaDeletion, target=RecordRules).

### FR-3: Code Removal

- Delete `src/Domain/Security/Entities/RecordRule.cs`.
- Remove `DbSet<RecordRule> RecordRules` from `IApplicationDbContext.cs` and `ApplicationDbContext.cs`.
- Delete `src/Infrastructure/Data/Configurations/Security/RecordRuleConfiguration.cs`.
- Domain layer deletion precedes context edits (compile break is the discovery test); context edits precede Infrastructure config deletion.

### FR-4: No Application Impact

No commands, queries, endpoints, permissions, seeds, or frontend code reference RecordRules (verified by grep). Test suite MUST pass unchanged (minus nothing — no RecordRules tests exist).

### FR-5: Docs Sweep

Update `docs/database-tables-complete.md` §2.7 removal. `docs/database-schema.md` and AGENTS.md unaffected (no RecordRules mention — AGENTS.md RBAC line updated at spec 036 close).

## Acceptance Scenarios

1. **Given** the schema contains RecordRules with data, **When** the migration runs, **Then** the table is dropped, `[RecordRuleDeletionReport]` contains every prior row, and a SecurityAuditLog entry exists.
2. **Given** the application builds, **When** Domain/Application/Infrastructure/Web compile, **Then** no symbol referencing RecordRule exists outside migrations history.
3. **Given** the full 5-project test suite runs, **Then** all tests pass.

**Check: FR-3 covers Principle XII ordering; FR-1/FR-2 cover Principle VI (data integrity + reversibility); no principle conflicts.**
