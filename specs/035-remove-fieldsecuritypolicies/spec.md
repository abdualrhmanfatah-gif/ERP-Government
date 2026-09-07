# Feature Specification: Remove FieldSecurityPolicies Table

**Feature Branch**: `035-remove-fieldsecuritypolicies`
**Created**: 2026-09-08
**Decision Record**: DEP-026

## Context

`FieldSecurityPolicies` models field-level masking (entity/field/role + AccessLevel + MaskingFormat) but no enforcement path reads it. RBAC enforcement spec will not implement field-level security (DEP-026). References verified: entity, config, both contexts, migrations. Zero Application/Web/frontend references, no seeds.

## Requirements

- **FR-1**: Migration drops FieldSecurityPolicies (PK, unique index IX_FieldSecurityPolicies_EntityName_FieldName_RoleId, IX_FieldSecurityPolicies_RoleId, FK_FieldSecurityPolicies_SecurityRoles_RoleId). Down recreates.
- **FR-2**: Deletion report [FieldSecurityPolicyDeletionReport] + SecurityAuditLog entry before drop.
- **FR-3**: Delete entity + configuration + both DbSet lines.
- **FR-4**: Docs sweep: database-tables-complete.md (108 tables).

Full plan/tasks mirror specs/034-remove-recordrules (same shape). Suite gate skipped per user instruction (recorded: AGENTS.md deviation).
