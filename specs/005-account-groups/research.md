# Research: إدارة مجموعات الحسابات (Account Groups) — Phase 0

**Branch**: `005-account-groups` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Summary

All `NEEDS CLARIFICATION` resolved via 5/5 clarification session (2026-09-02). No unknowns remain in Technical Context. Research validates that existing repo patterns satisfy every FR without new infrastructure, and documents the precise choices for hierarchical integrity, dual-mode listing, and immutability guarantees.

## Decisions

### D-01: Hierarchical model — self-referential ParentId Restrict + Level tinyint

- **Decision**: Keep existing `AccountGroupConfiguration`: `ParentId` nullable FK self `DeleteBehavior.Restrict`, `Level` tinyInt, `HasIndex(ParentId)`, `RowVersion IsRowVersion`, `Code HasMaxLength(20).IsUnique()`. `Level` computed server-side (`null → 1`, else `Parent.Level+1`), never client-supplied. Descendant Level recalc done in handler inside same transaction when re-parenting.
- **Rationale**: Matches Constitution VI (Restrict, no cascade) and existing `Account` pattern (`Account.ParentId Restrict`). tinyint sufficient for 1-5. Index on ParentId already supports tree queries.
- **Alternatives considered**: `HierarchyId` SQL Server type — rejected: over-engineered for depth 5, breaks cross-DB portability. `Materialized Path` string — rejected: extra column, eventual consistency risk. Adjacency list + CTE suffices.

### D-02: Depth limit 5 — server-side guard + subtree validation

- **Decision**: Enforce `MAX_LEVEL=5` constant in domain service/validator. On create: `if parent.Level+1>5 → fail 400`. On re-parent: compute newLevel, then query max depth of moving subtree (`maxDescendantLevel - movingNode.Level + newLevel`); if any >5 → fail. All inside handler before `SaveChanges`.
- **Rationale**: Spec clarification fixes 5 as hard bound. Single constant keeps rule testable. Subtree check prevents silently pushing grandchildren beyond 5.
- **Alternatives**: Config-driven limit — deferred to future ADR; hard-coding now keeps scope tight, easy to extract later.

### D-03: Cycle detection — ancestor chain walk

- **Decision**: Before accepting `ParentId`, walk ancestors of proposed parent via loop querying `ParentId` chain (or single CTE `WITH RECURSIVE` for depth ≤5). If chain contains `movingNode.Id` or `ParentId == Id` → fail 400 "حلقة هرمية". Complexity O(depth) ≤5, no full tree scan.
- **Rationale**: Depth-bounded chain walk is cheapest and already sufficient per clarified FR-011. No need for closure table.
- **Alternatives**: Closure table / nested sets — rejected: migration cost, trigger complexity for tiny depth.

### D-04: Type inheritance — child Type == parent Type

- **Decision**: On create/update/reparent, if `ParentId != null` → `require request.Type == parent.Type`, else fail 400 "نوع الابن يجب أن يساوي نوع الأب". Guard runs after loading parent.
- **Rationale**: Clarified Q4 (Option A). Keeps each major-category tree pure, simplifies reporting. Single equality check.
- **Alternatives**: Compatible groups (Assets+Liabilities group) — rejected per clarification; independent types — rejected as breaks accounting classification.

### D-05: NormalBalance compatibility + posted-entries guard

- **Decision**: Matrix enforced in FluentValidation: `Asset/Expense→Debit`, `Liability/Equity/Revenue→Credit` (FR-008). Separate guard in Update handler: if `Type`/`NormalBalance` changed, query `context.Moves`/`MoveLines` where `Account.AccountGroupId` in `{movingNode subtree IDs}` and move is posted (`EntryStatus == Posted` or `PostedAt != null`) — if any exists → fail 400 "تغيير النوع/الرصيد ممنوع مع قيود مرحلة". Requires loading subtree IDs (CTE).
- **Rationale**: Clarified Q5 Option B — protects ledger integrity without blocking edits on unused groups. Uses existing `Move`/`MoveLine` model (already in Domain).
- **Alternatives**: Block on any active Account — rejected as too strict (Q5 A). Allow always — rejected as risks ledger corruption.

### D-06: Code uniqueness — permanent, case-insensitive, all records

- **Decision**: Keep `HasIndex(Code).IsUnique()` unique index (SQL Server collation default case-insensitive for Arabic_CI — verify collation; if not, add normalized column `NormalizedCode = Code.ToUpperInvariant().Trim()` with unique index, or enforce via handler `Where(NormalizedCode==normalized) AnyAsync` before insert + rely on DB constraint for race). Handler does `Where(NormalizedCode==normalized).AnyAsync` + `try SaveChanges catch DbUpdateException on unique violation → 400`. Code stored trimmed as entered, comparison normalized. Applies to active+inactive permanently (FR-006 clarified Q2 A).
- **Rationale**: Prevents reuse of deactivated codes, preserves audit history. App-layer check gives friendly 400; DB constraint guarantees race safety.
- **Alternatives**: Unique filtered on IsActive — rejected per Q2. Auto-rename on deactivate — rejected.

### D-07: Dual-mode listing — tree (roots paginated) vs flat search

- **Decision**: `GetAccountGroupsList` takes `Search?`, `Type?`, `IsActive?`, `Page`, `PageSize`. If `Search` null/empty AND no Type/IsActive filter → **tree mode**: query `Where(ParentId==null) OrderBy(Code) Skip/Take` roots, then for each root load children recursively (or single query `Where Level<=5` then build tree in memory). Returns `List<AccountGroupTreeDto>` with `Children` collection. If any filter/search present → **flat mode**: `Where(Code.Contains(search) OR Name.Contains(search))` + Type/IsActive predicates, `OrderBy(Code) Skip/Take`, include `AncestorPath` (breadcrumb) computed via parent chain up to root (5 steps max). Frontend switches: no-search → `<GroupTree>` expandable, with-search → `<DataGrid>` flat + breadcrumb column.
- **Rationale**: Clarified Q1 Option C — avoids expensive hierarchical pagination with filters; both queries stay O(n) with indexes on `Code`, `Name` (add index on Name), `Type`, `IsActive`, `ParentId`. Flat mode covers SC-001 search 30s.
- **Alternatives**: Always hierarchical with ancestor inclusion — rejected: pagination ambiguous, query complexity. Always flat — rejected: loses tree UX.

### D-08: Concurrency — RowVersion IsRowVersion → 409

- **Decision**: Keep `builder.Property(e=>RowVersion).IsRowVersion()` (rowversion/timestamp). Commands carry `byte[] RowVersion`. Handler attaches entity, sets `OriginalValues["RowVersion"]=request.RowVersion` before SaveChanges (or use `context.Entry(entity).Property(x=>RowVersion).OriginalValue=`). Catch `DbUpdateConcurrencyException → Result.Conflict` mapped in endpoint to 409 problem-details with Arabic message + current values (re-read). Frontend `ConflictDialog` (existing pattern in fiscal-years) prompts refresh.
- **Rationale**: Matches Constitution VI/VIII and existing `UpdateAccountGroupCommand` pattern (already has RowVersion). Verified via `AccountGroupConfiguration`.
- **Alternatives**: ETag/If-Match header — rejected: repo uses body RowVersion uniformly.

### D-09: Deep deactivate guard — entire subtree + accounts

- **Decision**: `ToggleAccountGroupActive` (deactivate path) loads `subtreeIds` via recursive CTE (`WITH AccountGroupTree AS ...`) starting at target Id. Query 1: `context.AccountGroups.AnyAsync(x=> subtreeIds.Contains(x.ParentId) && x.IsActive)` or `x.Id in subtreeIds && IsActive && x.Id != target` — if any → fail 400. Query 2: `context.Accounts.AnyAsync(x=> subtreeIds.Contains(x.AccountGroupId) && x.IsActive)` — if any → fail 400. Both run before mutating. Activate path skips guard, but create-child-under-inactive guard separately rejects `Parent.IsActive==false` on create/reparent.
- **Rationale**: Clarified Q3 Option C — full subtree prevents orphan active nodes under inactive ancestor, protects chart integrity.
- **Alternatives**: Direct children only — rejected as leaves orphan. Application-level recursive queries without CTE — acceptable for depth 5 (loop queries) simpler than raw SQL, but CTE preferred for single round-trip.

### D-10: Audit immutability — interceptor + trigger

- **Decision**: Reuse existing `AuditableEntityInterceptor` (sets `Created/LastModified`) + `AuditTrailChangeTracker` (field-level diffs, 16KB cap) that writes `AuditTrails` via `SaveChanges` interceptor. DB triggers `AuditTrails_Immutable.sql` + `SecurityAuditLogs_Immutable.sql` enforce INSERT-ONLY (UPDATE/DELETE throw). `AccountGroup` changes auto-captured (IsActive toggle included). Detail query reads `context.AuditTrails.Where(DocumentType=="AccountGroup" && DocumentId==id) OrderBy(Timestamp desc)`.
- **Rationale**: Verified files: `src/Infrastructure/Data/Interceptors/*`, `src/Infrastructure/Migrations/Triggers/*`. No new table needed. Satisfies Constitution VIII.
- **Alternatives**: Separate AccountGroupAudit table — rejected: duplicates existing generic audit, breaks traceability.

### D-11: Authorization — fail-closed + audit

- **Decision**: Use existing `PermissionCodes.ChartOfAccountsRead/Create/Edit` + `[Authorize(Policy=...)]` on both endpoint (`RequireAuthorization`) and use-case (`[Authorize]` attribute). Policy wiring via `AddAuthorization` in `Web` (already registers). Failure path logs to `SecurityAuditLogs` via existing `AuthorizationAuditEvent` (or interceptor). Frontend `usePermission(PERMISSIONS.ChartOfAccounts.*)` + `PERMISSIONS` constants for UX gating only; backend is source of truth.
- **Rationale**: Direct reuse of verified permission constants (`PermissionCodes.cs:43-45`). Aligns with Constitution VII fail-closed; logged Grant/Deny.
- **Alternatives**: New permissions — unnecessary; spec maps exactly.

### D-12: Validation + problem-details

- **Decision**: FluentValidation validators per command (Code 1-20 trimmed, Name 1-200 trimmed required, Type enum, NormalBalance enum, Description ≤500, ParentId exists & active & type match, depth & cycle checks in handler for DB-dependent rules). Endpoint returns `Results.ValidationProblem` → 400 problem-details with Arabic messages; concurrency → 409; not found → 404; forbidden → 403 via auth middleware. `Web` already has `UseExceptionHandler` + problem-details setup.
- **Rationale**: Mirrors existing `CreateAccountGroupCommandValidator` pattern (already validates Code/Name/Type/NormalBalance). Extend with remaining rules.
- **Alternatives**: DataAnnotations — rejected: team standard is FluentValidation.

### D-13: Frontend — RTL/tokens/dark + shared components

- **Decision**: Pages use `PageHeader`, `DataGrid` (TanStack Table), `FilterBar`/`FilterSelect`, `StatusBadge`, `Dialog`/`ConfirmDialog`/`ConflictDialog`, `Input`, `Button` from `src/Web/ClientApp/src/components/ui/*` (verified FiscalYearsListPage does). Styling strictly via `src/Web/ClientApp/src/design-system/tokens.css.scss` + `tokens.ts` (no hard-coded colors/spacing). Layout uses logical CSS (`inline-start`/`inline-end`, `margin-inline`) tested RTL (`rtl.scss`) + `next-themes` dark. Arabic first. Hooks via `usePermission`, `useQuery`/`useMutation` pattern.
- **Rationale**: Constitution X + existing feature pattern reuse. Shared components guarantee visual consistency.
- **Alternatives**: New custom tree lib — rejected: build on `DataGrid` + expandable rows (already supports tree indentation) suffices.

## Open Items

None — all decisions ready for Phase 1. No NEEDS CLARIFICATION remains.

## References

- Existing entity/config: `src/Domain/Accounting/Entities/AccountGroup.cs`, `src/Infrastructure/Data/Configurations/Accounting/AccountGroupConfiguration.cs`
- Existing commands: `src/Application/Accounting/Commands/AccountGroups/CreateAccountGroup/*`, `UpdateAccountGroup/*`
- Existing queries: `src/Application/Accounting/Queries/AccountGroups/GetAccountGroupsList/*`, `GetAccountGroupById/*`
- Endpoints: `src/Web/Endpoints/Accounting/AccountGroups.cs`
- Permissions: `src/Application/Common/Security/PermissionCodes.cs` (ChartOfAccounts* )
- Audit: `src/Domain/Security/Entities/AuditTrail.cs`, `src/Infrastructure/Data/Interceptors/*`, triggers `AuditTrails_Immutable.sql`
- Frontend example: `src/Web/ClientApp/src/features/financial/fiscal-years/*`
- Design system: `src/Web/ClientApp/src/design-system/*`, `src/Web/ClientApp/src/components/ui/*`
