---
document_type: security-review
review_type: plan
assessment_date: 2026-09-02
codebase_analyzed: ERP-Government
total_files_analyzed: 8
total_findings: 6
overall_risk: MITIGATED
critical_count: 0
high_count: 2
medium_count: 2
low_count: 2
informational_count: 0
owasp_categories: ["A01:2025-Broken Access Control", "A04:2025-Insecure Design"]
cwe_ids: ["CWE-862", "CWE-284", "CWE-330"]
asvs_requirements: ["V4.1.1", "V4.2.1", "V8.1.1"]
mitre_techniques: ["T1078", "T1565"]
field_summaries:
  document_type: "Always 'security-review'. Allows indexers to skip non-review documents."
  review_type: "Which command generated this document: audit, branch, staged, plan, tasks, followup, or export."
  assessment_date: "ISO 8601 date the review was performed (YYYY-MM-DD)."
  overall_risk: "Highest severity tier with active findings (CRITICAL, HIGH, MEDIUM, LOW, INFORMATIONAL), or NONE when no active findings exist."
  critical_count: "Number of Critical findings (CVSS 9.0-10.0)."
  high_count: "Number of High findings (CVSS 7.0-8.9)."
  medium_count: "Number of Medium findings (CVSS 4.0-6.9)."
  low_count: "Number of Low findings (CVSS 0.1-3.9)."
  informational_count: "Number of Informational findings."
  owasp_categories: "OWASP Top 10 2025 categories (A01-A10) that have at least one finding."
  cwe_ids: "Common Weakness Enumeration identifiers referenced in this document."
  asvs_requirements: "ASVS v4.0 requirements mapped to findings."
  mitre_techniques: "MITRE ATT&CK techniques applicable to findings."
  finding_id: "Unique finding identifier (SEC-NNN) for cross-referencing and task linkage."
  location: "Artifact or code path and line number supporting the finding (path/to/artifact:line)."
  owasp_category: "OWASP Top 10 2025 category for this finding (AXX:2025-Name)."
  cwe: "Common Weakness Enumeration identifier with short name (CWE-NNN: Name)."
  cvss_score: "CVSS v3.1 base score (0.0-10.0). 9.0+=Critical, 7.0-8.9=High, 4.0-6.9=Medium, 0.1-3.9=Low."
  security_task: "Security task ID for backlog tracking and remediation follow-up (TASK-SEC-NNN). Supports legacy spec_kit_task as alias."
---

# Security Review — Plan: User-Roles Refactor

**Branch**: `001-user-roles-refactor`
**Review date**: 2026-09-02
**Risk rating**: HIGH

## Executive Summary

The plan is architecturally sound and aligns with the Constitution's security principles. However, two high-severity gaps and two medium-severity gaps exist that could weaken the security model if not addressed before implementation. The most critical issue is that the new `PUT /{id}/role` endpoint inherits the project's open authorization policy pattern (`RequireAssertion(_ => true)`), meaning it would be unprotected at the ASP.NET Core level without the MediatR `[Authorize]` attribute on the command — but the plan does not explicitly address this defense-in-depth gap. A second high-severity finding concerns the audit trail for role changes: the plan claims the `AuditableEntityInterceptor` captures old/new values automatically, but does not verify that `SetUserRoleCommand`'s handler path triggers the interceptor's change-tracking for the `RoleId` field specifically.

## Plan Artifacts Reviewed

| Artifact | Path | Status |
|----------|------|--------|
| Implementation Plan | `specs/001-user-roles-refactor/plan.md` | Reviewed |
| Research | `specs/001-user-roles-refactor/research.md` | Reviewed |
| Data Model | `specs/001-user-roles-refactor/data-model.md` | Reviewed |
| API Contracts | `specs/001-user-roles-refactor/contracts/api-changes.md` | Reviewed |
| Feature Spec | `specs/001-user-roles-refactor/spec.md` | Reviewed |
| Constitution | `.specify/memory/constitution.md` | Reviewed |

## Findings

### SEC-001: Open Authorization Policy on New Role Assignment Endpoint

| Field | Value |
|-------|-------|
| **Severity** | HIGH |
| **CVSS** | 7.5 |
| **OWASP** | A01:2025-Broken Access Control |
| **CWE** | CWE-862: Missing Authorization |
| **ASVS** | V4.1.1 |
| **Location** | `plan.md:111`, `Web.DependencyInjection.cs:42-46`, `Endpoints/Security/Users.cs:76-79` |
| **Historical status** | ACCEPTED RISK — tracked as DEP-020 |

**Description**: The plan adds `PUT /{id}/role` with `.RequireAuthorization(PermissionCodes.UsersManageRoles)`. However, every authorization policy in `DependencyInjection.cs` is currently `RequireAssertion(_ => true)` (line 42-46: "Policies use RequireAssertion (open) until real RBAC is wired per DEP-020"). This means the endpoint-level policy check passes for everyone. Protection relies solely on the MediatR `AuthorizationBehaviour` pipeline checking `[Authorize(Policy = PermissionCodes.UsersManageRoles)]` on `SetUserRoleCommand`.

**Risk**: If the `[Authorize]` attribute is accidentally omitted from the command, or if the MediatR pipeline is bypassed (e.g., direct handler invocation in tests, background jobs), the endpoint is fully open. The defense-in-depth gap means a single misconfiguration exposes role assignment to any authenticated user.

**Recommendation**: The plan should explicitly note this dependency and either:
1. Wire the `UsersManageRoles` policy to check `IPermissionService` before implementation (preferred), OR
2. Document that this is an accepted risk under DEP-020 with a specific remediation deadline

---

### SEC-002: Audit Trail Completeness for Role Change Not Verified

| Field | Value |
|-------|-------|
| **Severity** | HIGH |
| **CVSS** | 7.1 |
| **OWASP** | A04:2025-Insecure Design |
| **CWE** | CWE-778: Insufficient Logging |
| **ASVS** | V8.1.1 |
| **Location** | `research.md:36-38`, `plan.md:44`, `Infrastructure/Data/Interceptors/AuditableEntityInterceptor.cs` |
| **Historical status** | MITIGATED — explicit SecurityAuditLog added to SetUserRoleCommand and AssignUserPermissionCommand |

**Description**: The plan states: "The existing `AuditableEntityInterceptor` automatically captures INSERT/UPDATE/DELETE on auditable entities." The `SetUserRoleCommand` handler updates `Users.RoleId` on the `User` entity (which extends `BaseAuditableEntity`). The interceptor will capture the entity change. However, the plan does not specify:
1. Whether the interceptor captures the `RoleId` field name in `FieldChanges` (it excludes `RowVersion` but not `RoleId`)
2. Whether the `SecurityAuditLog` entry (written explicitly from the handler) includes the oldRoleId/newRoleId values
3. The exact `EventCategory` and `Action` format for the security audit log entry

**Risk**: Without explicit verification, the audit trail may record "User updated" without the specific role-change details (oldRoleId, newRoleId), making it impossible to reconstruct who changed a user's role and to what — a violation of Constitution Principle VIII.

**Recommendation**: The plan should add a sub-task to verify that:
1. The `SetUserRoleCommand` handler writes a `SecurityAuditLog` entry with `EventCategory = "RoleChange"`, `Action = "SetUserRole"`, and JSON `OldValues`/`NewValues` containing `{ oldRoleId, newRoleId }`
2. The `AuditableEntityInterceptor` includes `RoleId` in its `FieldChanges` output
3. Tests assert on both audit records

---

### SEC-003: EffectiveFrom/EffectiveTo on UserPermission Not Addressed in Resolution Formula

| Field | Value |
|-------|-------|
| **Severity** | MEDIUM |
| **CVSS** | 5.3 |
| **OWASP** | A04:2025-Insecure Design |
| **CWE** | CWE-284: Improper Access Control |
| **Location** | `data-model.md:75-86`, `data-model.md:41-43`, `spec.md:120` |
| **Historical status** | ADDRESSED — EffectiveFrom/EffectiveTo active on UserPermission, respected in PermissionService union-minus formula |

**Description**: The `UserPermission` entity has `EffectiveFrom` and `EffectiveTo` fields (nullable `DateTimeOffset`). The data model's resolution formula includes the clause `AND EffectiveFrom/To window active`. However, the clarified spec (FR-003) says Reason is required for every override, and the spec removed time-windowing from role assignment. The plan does not clarify whether `EffectiveFrom/EffectiveTo` on `UserPermission` is still active, deprecated, or always-nullable.

**Risk**: If `EffectiveFrom/EffectiveTo` is active on `UserPermission`, an override could silently become inactive at a future date without administrator awareness, creating a security gap where a revoked permission re-activates. If it's deprecated, the fields should be removed or marked unused to prevent confusion.

**Recommendation**: The plan should explicitly state whether `UserPermission.EffectiveFrom/EffectiveTo` is:
- Active (time-bounded overrides) — then the formula must include it and the UI must expose these fields
- Deprecated (always null) — then the fields should be removed from the entity and the formula simplified

---

### SEC-004: No Transaction Boundaries Specified for Migration

| Field | Value |
|-------|-------|
| **Severity** | MEDIUM |
| **CVSS** | 4.2 |
| **OWASP** | A04:2025-Insecure Design |
| **CWE** | CWE-330: Use of Insufficiently Random Values |
| **Location** | `data-model.md:88-107` |
| **Historical status** | ADDRESSED — EF Core wraps migrations in implicit transactions; SQL Server DDL is atomic |

**Description**: The migration script outline in `data-model.md` shows 4 sequential operations (add column, set NOT NULL, drop table, update index) without specifying transaction boundaries. The clarified spec says "no legacy data exists" so phases 1+2 combine, but the DROP TABLE and index changes are separate operations.

**Risk**: If the migration fails after DROP TABLE but before the index update, the `UserPermissions` table has a stale unique index that doesn't match the new constraint semantics. While SQL Server wraps DDL in implicit transactions, the plan should explicitly address atomicity.

**Recommendation**: The plan should specify that the EF Core migration runs in a single transaction, and that rollback is safe (the old schema is still valid if migration reverts).

---

### SEC-005: SystemAdmin Bypass Not Scoped in Plan

| Field | Value |
|-------|-------|
| **Severity** | LOW |
| **CVSS** | 2.1 |
| **OWASP** | A01:2025-Broken Access Control |
| **CWE** | CWE-284: Improper Access Control |
| **Location** | `AuthorizationBehaviour.cs:48-53`, `plan.md:39` |
| **Historical status** | Open — existing pattern |

**Description**: The `AuthorizationBehaviour` grants `SystemAdmin` role holders a full bypass of all permission checks (line 49-53). This means a SystemAdmin can assign any role to any user via `PUT /{id}/role` without needing the `UsersManageRoles` permission. This is an existing pattern, not introduced by this feature, but the plan should acknowledge it.

**Risk**: Low — SystemAdmin is a controlled role. However, if a SystemAdmin account is compromised, the attacker can assign arbitrary roles including their own escalation.

**Recommendation**: No action required for this feature. Document as an existing accepted risk. The bypass is intentional and appropriate for system administration.

---

### SEC-006: Breaking API Change Not Escalated to Decision Record

| Field | Value |
|-------|-------|
| **Severity** | LOW |
| **CVSS** | 2.0 |
| **OWASP** | A04:2025-Insecure Design |
| **CWE** | CWE-284: Improper Access Control |
| **Location** | `contracts/api-changes.md:6-11`, `constitution.md:143` |
| **Historical status** | ADDRESSED — breaking change documented in spec.md and api-changes.md with rationale |

**Description**: The plan removes 3 API endpoints (`GET/POST/DELETE /{id}/roles`) and modifies the `GET /{id}` response shape (from `roles[]` to `role`). Constitution Principle IX states: "Removing, renaming, or reshaping an endpoint or payload field is a breaking change and requires a decision record before implementation."

**Risk**: Low — the breaking change is intentional and documented in the plan. However, the Constitution requires a formal decision record.

**Recommendation**: Add a brief decision record (or note in the plan's Complexity Tracking section) documenting the breaking change, its rationale, and any consumer impact.

## Confirmed Secure Patterns

1. **FK Restrict on RoleId** — Prevents orphaned users if a role is deleted. Correctly specified in data-model.md.
2. **RowVersion concurrency** — SetUserRole uses optimistic concurrency. Prevents lost-update race conditions.
3. **INSERT-only audit trails** — AuditTrail and SecurityAuditLog are `IImmutableEntity`. Enforced by `ImmutableEntityConstraint` interceptor.
4. **Fail-closed authorization** — `AuthorizationBehaviour` catches exceptions and denies access (line 96-100). Correctly preserved.
5. **Mandatory Reason on overrides** — Clarified and enforced in FR-003. Prevents silent permission changes without justification.
6. **Unique constraint on (UserId, PermissionId)** — Prevents duplicate overrides. Correctly specified.
7. **No hard deletes** — Constitution Principle VI prohibits hard deletes on audit records. Correctly enforced.

## Action Plan & Next Steps

1. **Address SEC-001 and SEC-002 before implementation** — These are high-severity gaps that could weaken the security model.
2. **Clarify SEC-003** — The `EffectiveFrom/EffectiveTo` question on `UserPermission` should be resolved before coding begins.
3. **Execute `/speckit.tasks`** — The plan is otherwise ready for task decomposition after addressing the above findings.

## flash-mem INDEX.md Row

```text
| specs/001-user-roles-refactor/plan.md | plan | 2026-09-02 | HIGH | C:0 H:2 M:2 L:2 | A01,A04 |
```
