# Data Model: إدارة مجموعات الحسابات (Account Groups) — Phase 1

**Branch**: `005-account-groups` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Research**: [research.md](./research.md)

## Entities

### 1. AccountGroup (مجموعة حسابات) — Domain: Accounting

Existing entity `src/Domain/Accounting/Entities/AccountGroup.cs` (extends `BaseAuditableEntity`). Table `AccountGroups` (configuration `AccountGroupConfiguration.cs` — verified).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `int` PK | identity, `HasKey` | `BaseEntity.Id` |
| `Code` | `string` | Required, 1-20, `HasMaxLength(20)`, `HasIndex(Code).IsUnique()` permanent (active+inactive), trimmed, stored as entered but compared via `NormalizedCode = UpperInvariant(Trim())` | FR-005/006. DB unique guarantees race. Handler checks normalized before insert + catches `DbUpdateException`. |
| `Name` | `string` | Required, 1-200, `HasMaxLength(200)`, trimmed | FR-005 |
| `Type` | `AccountGroupType` enum | Required, `IsInEnum` — `Asset=0, Liability=1, Equity=2, Revenue=3, Expense=4` | FR-005. Child must equal parent Type (FR-010). |
| `NormalBalance` | `NormalBalanceType` enum | Required, `Debit=0, Credit=1`, matrix: Asset/Expense→Debit, Liability/Equity/Revenue→Credit | FR-008. Guard on create/update/reparent + posted-entries guard. |
| `Description` | `string?` | Optional, 0-500, `HasMaxLength(500)`, trimmed | FR-005 |
| `ParentId` | `int?` FK self | Nullable, `HasIndex(ParentId)`, `HasOne(Parent).WithMany().HasForeignKey(ParentId).OnDelete(Restrict)` | FR-019. Null= root. Must ref existing & active group (except activate path). Same-table Restrict per Constitution VI. |
| `Level` | `byte` (`tinyint`) | `HasColumnType("tinyint")`, computed: `Parent==null ? 1 : Parent.Level+1`, read-only, 1-5 | FR-007/009. Recalculated for entire subtree on reparent inside transaction. |
| `IsActive` | `bool` | Default `true` | FR-013/015. Toggle via `ToggleAccountGroupActive`. Deactivate blocked by deep guard (FR-014). Reactivate always allowed. Create under inactive parent blocked. |
| `RowVersion` | `byte[]` | `IsRowVersion()` concurrency token | FR-012/022. Sent on every read, required on every mutate. `DbUpdateConcurrencyException → 409`. |
| `Created` | `DateTimeOffset` | auto via `AuditableEntityInterceptor` | `BaseAuditableEntity` |
| `CreatedBy` | `string?` | auto | |
| `LastModified` | `DateTimeOffset` | auto | |
| `LastModifiedBy` | `string?` | auto | |
| `Parent` | `AccountGroup?` nav | | |
| `ParentCode` | `string?` | `[NotMapped]` transient for seeding only | |

**Indexes**: `IX_AccountGroups_Code UNIQUE`, `IX_AccountGroups_ParentId`.

**Validation rules (FluentValidation + handler)**:
- Code: NotEmpty, 1-20 after Trim, unique normalized permanent → 400 "الكود موجود مسبقاً".
- Name: NotEmpty, 1-200 after Trim.
- Type: IsInEnum.
- NormalBalance: IsInEnum + matrix vs Type → 400 "الرصيد الطبيعي غير متوافق مع النوع".
- Description: Max 500.
- ParentId: if set → exists 404, IsActive==true (except when toggling), Type == child Type → 400, Level+subtree depth ≤5 → 400, not self nor descendant → 400.

**State transitions**:

```
[Create IsActive=true Level computed] → Active
Active --(Toggle deactivate, deep guard pass, RowVersion ok)--> Inactive
Inactive --(Toggle activate, RowVersion ok)--> Active
Active --(Update: Name/Type/NormalBalance/Description/ParentId with guards)--> Active  (Level may change + subtree recalc)
Inactive --(Update allowed)--> Inactive  (but cannot move under active? allowed only if parent inactive check passes)
No delete transition (no DELETE endpoint, no hard delete).
```

### 2. Account (حساب دليل) — referenced, not modified

Existing `src/Domain/Accounting/Entities/Account.cs`. Key fields for this feature:

| Field | Type | Relevance |
|-------|------|-----------|
| `Id` | `int` PK | |
| `Code` | `string` 50 unique | |
| `Name` | `string` 200 | |
| `AccountGroupId` | `int` FK Restrict → `AccountGroup.Id` | FR-014: deep guard checks `Accounts.Where(AccountGroupId in subtreeIds && IsActive)` |
| `ParentId` | `int?` FK self Restrict | |
| `Level` | `byte` | |
| `NormalBalance` | `NormalBalanceType` | |
| `IsPostable` | `bool` | |
| `IsActive` | `bool` | |
| `RowVersion` | `byte[]` | |
| `CurrencyId` | `int?` | deferred |

**Relationship**: `AccountGroup 1—* Account`. No cascade.

### 3. Move / MoveLine (قيود محاسبية مرحلة) — for posted-entries guard only

Existing `Domain/Accounting/Entities/Move.cs`, `MoveLine.cs`. Relevant for FR-008 posted guard:

- `Move.EntryStatus` / `PostedAt` / `PostedById` indicates posted.
- `MoveLine.AccountId → Account.Id`
- Guard query: `exists MoveLine where Account.AccountGroupId in subtreeIds AND Move.EntryStatus == Posted (or PostedAt != null)`. If exists → block Type/NormalBalance change (400).

No schema change.

### 4. AuditTrail (سجل التدقيق) — generic, INSERT-ONLY

Existing `Domain/Security/Entities/AuditTrail.cs` (`BaseLongEntity`, `IImmutableEntity`):

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `long` PK | |
| `EventCategory` | `string` | e.g., "Accounting" |
| `DocumentType` | `string?` | "AccountGroup" |
| `DocumentId` | `int?` | AccountGroup.Id |
| `Action` | `AuditAction` enum | Create/Update/Deactivate/Activate |
| `UserId` | `int?` | Actor |
| `Success` | `bool` | |
| `Timestamp` | `DateTimeOffset` | |
| `IpAddress/SessionId/DeviceInfo` | `string?` | RequestContext |
| `FieldChanges` | `string?` JSON `{field:{Old,New}}` | from `AuditTrailChangeTracker.CaptureChanges` |
| `ChangeSummary` | `string?` | "Code: X→Y, Name: ..." |
| `OldValues/NewValues` | `string?` JSON | full capture on insert |

**Constraints**: Triggers `AuditTrails_Immutable.sql` / `SecurityAuditLogs_Immutable.sql` reject UPDATE/DELETE. `FieldChanges` capped 16KB with `[truncated]`. `IImmutableEntity` marker.

Query: `AuditTrails.Where(DocumentType=="AccountGroup" && DocumentId==id) OrderBy(Timestamp desc)`.

### 5. SecurityAuditLog (سجل التفويض)

`Domain/Security/Entities/SecurityAuditLog.cs` — records Grant/Deny with Reason per VII. Written on every authorization check (via `AuthorizationAuditEvent`). INSERT-ONLY.

## Relationships Diagram

```
AccountGroup (self-referential)
  ParentId ──Restrict──> AccountGroup.Id
  1 ───────── * Account (AccountGroupId Restrict)
  1 ───────── * AccountGroup (children via ParentId)
  AccountGroup.Id ───────── 0..* MoveLine.AccountId ──> Move (posted)
  AccountGroup.Id ───────── 0..* AuditTrail.DocumentId (DocumentType="AccountGroup")
```

## Derived / Computed

- `Level`: `Parent.Level+1`, subtree recalc recursive. Not stored as formula.
- `AncestorPath` (API only, not persisted): list of ancestor `Code`/`Name` up the chain for flat search breadcrumb — computed on query via parent chain up to 5 steps.
- `HasActiveDescendants` / `HasActiveAccountsInSubtree` — computed guards, not stored.

## Migration Impact

No new table. Possible migration only if:
- Add filtered index nuance (e.g., `IX_AccountGroups_Name` for search, or `NormalizedCode` column with unique index for case-insensitive guarantee). If SQL collation already `Arabic_CI_AS` (default), existing unique index already case-insensitive — verify via `SELECT COLLATIONPROPERTY`. If not, add migration `AddAccountGroupNormalizedCode` with computed column + unique constraint.
- Add `IX_Accounts_AccountGroupId_IsActive` composite for deep guard performance (optional, not mandatory for scope).

Otherwise, configuration already satisfies FR-019. Seed data remains idempotent.

## Open Questions

None.
