# Research: User Management Frontend + Backend

**Date**: 2026-09-02
**Feature**: 004-user-management-frontend-backend

## Research Items

### 1. Existing Backend API Coverage

**Decision**: Backend API already covers all required operations.

**Rationale**: The existing `Users.cs` endpoint group exposes:
- `GET /` (list users with filters)
- `GET /{id}` (user detail)
- `POST /` (create user)
- `PUT /{id}` (update user)
- `POST /{id}/deactivate`
- `POST /{id}/reactivate`
- `GET /{id}/sessions`
- `POST /{id}/sessions/{sessionId}/revoke`
- `POST /{id}/sessions/revoke-all`
- `POST /{id}/reset-failed-login-attempts`
- `GET /{id}/roles`, `POST /{id}/roles`, `DELETE /{id}/roles/{roleId}`
- `GET /{id}/permissions`, `POST /{id}/permissions`, `DELETE /{id}/permissions/{permissionId}`

All permission codes declared: UsersView, UsersCreate, UsersUpdate, UsersDeactivate, UsersManageRoles, UsersManageSessions, UsersResetLogin.

**Alternatives considered**: Building new endpoints — rejected because existing endpoints fully satisfy FR-001 through FR-021.

### 2. Password Handling on Create

**Decision**: Administrator sets initial password in the create form (clarified in spec).

**Rationale**: Simplest path for government ERP environments where accounts are provisioned in person. Password is sent to backend as part of CreateUserCommand; backend hashes and stores it.

**Alternatives considered**: Temporary password generation (more secure but requires email infrastructure); invitation link (requires email service). Both rejected as out-of-scope for this feature.

### 3. SoD Conflict Blocking

**Decision**: Block role assignment on SoD conflict with error message (clarified in spec).

**Rationale**: Government ERP requires strict SoD enforcement. Blocking is the safest default; overrides should go through formal approval workflows, not a UI toggle.

**Alternatives considered**: Warning with override (too permissive for compliance); secondary approval (deferred to approval workflow module).

### 4. Frontend API Client Generation

**Decision**: Use NSwag-generated TypeScript client (existing pattern).

**Rationale**: The project already uses `nswag run /runtime:Net100` to generate `web-api-client.ts` from the auto-generated OpenAPI spec. Constitution Principle IX requires frontend to conform to the published contract exactly.

**Alternatives considered**: Hand-written fetch calls — rejected per Principle IX ("hand-written frontend clients permitted only when they mirror the published OpenAPI contract exactly").

### 5. Optimistic Concurrency Round-Trip

**Decision**: Frontend must read RowVersion on load, include it in update requests, and handle 409 Conflict responses.

**Rationale**: Constitution Principle VI (Data Integrity) requires optimistic-concurrency token on every mutable record. Principle IX requires round-trip from frontend to backend. The existing `UpdateUserCommand` already accepts and validates `RowVersion`.

**Alternatives considered**: No concurrency control — violates Principles VI and IX.

### 6. Self-Protection Rules

**Decision**: Server-side enforcement of FR-017 (cannot deactivate self) and FR-018 (cannot remove own last admin role) in use cases.

**Rationale**: Constitution Principle III requires all business rules enforced server-side. Frontend checks are UX-only per Principle VII.

**Alternatives considered**: Frontend-only checks — rejected as insufficient per Constitution.

### 7. Audit Trail Data Source

**Decision**: Backend already captures entity-change audit records. Frontend queries existing audit endpoint or entity-change history.

**Rationale**: Constitution Principle VIII requires INSERT-ONLY audit trails with actor, timestamp, action, per-field diffs. The existing `AuthorizationAuditEvent` and entity-change tracking infrastructure supports this.

**Alternatives considered**: Building new audit infrastructure — rejected as unnecessary; data already captured.

### 8. Department Data Source

**Decision**: Department dropdown populated from Organization module via cross-module read.

**Rationale**: Constitution Principle II permits cross-module reads via shared persistence abstraction. Department data is reference data owned by Organization module.

**Alternatives considered**: Duplicating department data in Security module — violates bounded context ownership.
