# Feature Specification: Remove SoDMatrix Table

**Feature Branch**: `036-remove-sodmatrix`
**Created**: 2026-09-08
**Decision Record**: DEP-026

## Context

`SoDMatrix` models segregation-of-duties conflict pairs (PermissionA/PermissionB + RiskLevel + ActionOnViolation) but no enforcement path evaluates it. RBAC enforcement spec will not implement SoD checks (DEP-026). References verified: entity, config, seed (SoDMatrixSeedData), initialiser trigger (ApplicationDbContextInitialiser.cs), both contexts, migrations. Zero Application/Web/frontend references.

## Requirements

- **FR-1**: Migration drops SoDMatrix (PK, IX_SoDMatrix_PermissionAId_PermissionBId unique, IX_SoDMatrix_PermissionBId, 2 FKs to SecurityPermissions). Down recreates.
- **FR-2**: Deletion report [SoDMatrixDeletionReport] + SecurityAuditLog entry before drop.
- **FR-3**: Delete entity + configuration + SoDMatrixSeedData.cs + initialiser seed block + both DbSet lines.
- **FR-4**: Docs sweep: database-tables-complete.md (107 tables); AGENTS.md RBAC line updated (SoDMatrix/FieldSecurityPolicy/RecordRule removed, DEP-026).

Plan/tasks mirror specs/034-remove-recordrules. Suite gate skipped per user instruction.
