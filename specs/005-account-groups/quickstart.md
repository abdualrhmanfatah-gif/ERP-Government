# Quickstart: إدارة مجموعات الحسابات (Account Groups) — Phase 1

**Branch**: `005-account-groups` | **Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Data Model**: [data-model.md](./data-model.md) | **Contracts**: [account-groups-api.yaml](./contracts/account-groups-api.yaml)

## Prerequisites

- .NET SDK `10.0.201` (`global.json` rollForward latestFeature) + EF Core tools.
- Node `>=20` + npm.
- SQL Server (Aspire `AppHost` provisions; or local `localhost,1433`).
- User with `Accounting.ChartOfAccounts.Read/Create/Edit` (seed via Security module; fallback: `dotnet run --project src/Web -- seed` or assign via DB `UserRoles`).
- Repo cloned at `F:\Projects\ERP-Government`, branch `005-account-groups`.

## Setup

```powershell
# Backend — restore + DB
dotnet restore ERP-Government.slnx
dotnet ef database update --project src/Infrastructure --startup-project src/Web

# Optional idempotent seed (AccountGroups sample tree if any)
dotnet run --project src/Web -- --seed-account-groups

# Frontend — install + generate OpenAPI client + tokens
cd src/Web/ClientApp
npm ci
npm run generate-api   # nswag run /runtime:Net100 → src/web-api-client.ts (must reflect contracts/account-groups-api.yaml after backend build)
npm run generate-tokens
```

## Run

```powershell
# Aspire orchestration (Web + DB + observability)
dotnet run --project src/AppHost

# Or backend only + frontend dev separately
dotnet run --project src/Web --urls https://localhost:5001
cd src/Web/ClientApp; npm run start  # Vite https://localhost:5173, proxy to /api
```

Health: `GET https://localhost:5001/health` + scalar docs `https://localhost:5001/scalar/v1`.

## Validation Scenarios (end-to-end)

All scenarios assume authenticated user with required permission; 403 paths tested with insufficient permission (expect problem-details + SecurityAuditLog).

### 1) Browse tree (no search) — roots paginated
- **Action**: `GET /api/account-groups?page=1&pageSize=20` (Authorization: `Accounting.ChartOfAccounts.Read`)
- **Expect**: `200` with `mode:"tree"`, `items` are Level 1 only with `children` nested, `totalCount` roots count, `level` 1-5 correct, `isActive` badge variant from tokens.
- **UI**: `/accounting/account-groups` → tree with indentation (`margin-inline-start`), expand/collapse, Level column, Status badge. RTL + dark correct.

### 2) Search → flat mode with breadcrumb
- **Action**: `GET /api/account-groups?search=101&type=Asset&isActive=true&page=1&pageSize=20`
- **Expect**: `200` `mode:"flat"`, each `item` contains `ancestorPath` up to root (max 5), filtered by `Code.Contains OR Name.Contains` case-insensitive, Type/IsActive predicates.
- **UI**: type into search (debounced 300ms) + FilterBar → DataGrid flat + breadcrumb column.

### 3) Create root + child
- **Action**:
  ```http
  POST /api/account-groups
  {"code":"AST-100","name":"أصول متداولة","type":"Asset","normalBalance":"Debit","description":"اختبار"}
  ```
  → `200 {id}`; verify `Level==1, IsActive==true`, audit entry exists.
  ```http
  POST /api/account-groups
  {"code":"AST-110","name":"نقدية","type":"Asset","normalBalance":"Debit","parentId":<id>}
  ```
  → `200`, `Level==2`, child's `type` must equal parent else `400` "نوع الابن يجب أن يساوي نوع الأب".
- **Negative**: repeat same `code` → `400` "الكود موجود مسبقاً" (even if first is later deactivated — permanent unique). `code` >20 / `name`>200 / `description`>500 → `400`. `Asset`+`Credit` → `400` "غير متوافق". `parentLevel==5` → `400` depth. Parent inactive → `400`.

### 4) Update + reparent + cycle + concurrency
- **Setup**: Create `A (root)`, `B (child of A)`, `C (child of B)` (levels 1,2,3).
- **Valid reparent**: `PUT /api/account-groups/{B.id}` `{name:"B2", type:Asset, normalBalance:Debit, parentId:<otherRootId>, rowVersion:...}` → `204`, verify `B.level` recalculated + `C.level` recalculated.
- **Cycle**: `PUT ... {parentId:B.id}` where B is descendant of A → `400` حلقة.
- **Depth overflow**: move subtree where `newLevel + maxDescendantDepth >5` → `400` حد المستويات.
- **Posted guard**: create Account under `A` with `Move` posted → `PUT ... {type:Expense}` (or opposite normalBalance) on `A` → `400` ممنوع مع قيود مرحلة.
- **Concurrency**: open same group in two sessions, first `PUT` succeeds, second with stale `rowVersion` → `409` + `current` payload, UI shows `ConflictDialog` "تم تعديل البيانات من قبل مستخدم آخر".

### 5) Deactivate/activate — deep guard
- **Create**: root `R` with child `C` active and Account active under `C`.
- **Deactivate R** → `POST /api/account-groups/{R.id}/toggle-active {"isActive":false,"rowVersion":...}` → `400` "عطّل الفروع/الحسابات أولاً" (deep check finds active descendant/account).
- **Deactivate leaf account** + **deactivate C** → then deactivate `R` → `204`, `IsActive==false`, not selectable when creating Account. Audit shows `Deactivate` diff.
- **Create under inactive parent** → `400` يجب تفعيل الأب أولاً.
- **Activate R** → `204` succeeds even though children remain inactive.

### 6) Detail page
- **Action**: `GET /api/account-groups/{id}` with Read.
- **Expect**: `group` core + `children` direct only + `accounts` (`code/name/isActive/isPostable`) + `audit` ordered desc with `fieldChanges` JSON. Empty states: "لا توجد مجموعات فرعية" / "لا توجد حسابات مرتبطة". Audit is INSERT-ONLY verified: attempt `UPDATE AuditTrails` → trigger error.
- **UI**: `/accounting/account-groups/{id}` → header card (Code/Name/Type/NormalBalance/Level/Status/Description/Parent), tabs/sections for Children/Accounts/AuditTimeline (using `AuditTimeline` component), navigation to child detail on click.

### 7) Permissions fail-closed
- **Action**: call any endpoint without token → `401`; with token lacking permission → `403` problem-details, `SecurityAuditLogs` row `Decision=Deny`.
- **Induce subsystem error** (stop auth DB) → all requests `403` (fail-closed).

## Test Commands

```powershell
# Backend unit + functional (real DB, Respawn)
dotnet test tests/Application.Tests --filter AccountGroup --logger "console;verbosity=detailed"
dotnet test tests/FunctionalTests --filter AccountGroup --logger "console;verbosity=detailed"

# Frontend
cd src/Web/ClientApp
npm run lint
npx vitest run src/features/accounting/account-groups --coverage
npx playwright test --project=account-groups  # if e2e spec added
```

## Performance Checklist

- Tree roots paginated <2s for 1000 roots (Indexed `Code`, `ParentId`).
- Flat search <2s with `LIKE` on indexed `Code`/`Name`.
- Deactivate guard <500ms for subtree 5 depth (CTE).
- Audit insert <1s, 16KB truncation active.

## References

- Contracts: [account-groups-api.yaml](./contracts/account-groups-api.yaml), [types.ts](./contracts/types.ts)
- Data model: [data-model.md](./data-model.md) (Level calc, type inheritance, deep guard queries)
- Research: [research.md](./research.md) (D-01..D-13 decisions)
