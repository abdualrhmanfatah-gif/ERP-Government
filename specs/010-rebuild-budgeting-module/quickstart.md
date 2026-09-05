# Quickstart: Validate the Budgeting Rebuild

**Feature**: `010-rebuild-budgeting-module`. See [data-model.md](./data-model.md) for schema and [contracts/](./contracts/budgeting-api.md) for endpoints/DTOs.

## Prerequisites

- .NET 10.0.201 SDK (`global.json`), SQL Server reachable via Aspire; `dotnet build ERP-Government.slnx -warnaserror` green.
- Node 20+ for the ClientApp (`npm run build`, `npm test`).

## 1. Apply migration on a fresh database

```powershell
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

Expected: the 7 Budgeting tables exist per [data-model.md](./data-model.md) (no `Level`, `IsReversed`, denormalized FK, snapshot-amount, or approval-identity columns); unique indexes on Code/Number columns; FK indexes on `AppropriationId`/`BudgetId`/`BudgetItemId`; `DocumentSequences` rows for `Budget`, `Appropriation`, `Encumbrance`.

## 2. Seed twice, expect no duplicates

Run the app (Development triggers `InitialiseDatabaseAsync`) or the seed path twice. Expected: `BudgetTypes`, `Funds`, `BudgetClassifications`, `DocumentSequences`, and `SecurityPermissions` gain no duplicate rows on the second run.

## 3. Exercise lifecycles + availability (functional tests)

```powershell
dotnet test tests/Application.FunctionalTests --filter "Budgeting"
```

Expected: Budget/Appropriation/Encumbrance transitions follow FR-010; `AvailableForAppropriation` matches `Original + Supplement − Reduction + signed Adjustment` to 2 decimals; Blocking rejects over-availability with the computed amount in the failure; Warning allows with a logged warning; the 3-level AllowOverrun chain resolves; depth-5+ `Level` and `IsReversed` projections match; RowVersion conflicts fail distinctly; ApprovalHistory snapshots exist.

## 4. Backend unit tests

```powershell
dotnet test tests/Application.UnitTests --filter "Budgeting"
dotnet test tests/Domain.UnitTests
```

Expected: every command's success + failure paths pass; zero stub tests.

## 5. Regenerate and verify the API contract

```powershell
dotnet build src/Web
cd src/Web/ClientApp
npm run generate-api
```

Expected: `wwwroot/openapi/v1.json` and `src/web-api-client.ts` contain the routes/DTOs in [contracts/](./contracts/budgeting-api.md) and no removed tables/fields.

## 6. Frontend checks

```powershell
cd src/Web/ClientApp
npm test
npm run build
```

Expected: budgeting pages render RTL in light/dark modes; lifecycle buttons enable per status; availability indicator shows green/amber/red correctly; approval panel renders latest-first history; routes/sidebar carry permission identifiers.

## 7. Auth smoke test (SC-008)

Unauthenticated `GET /api/Budgets` → 401; authenticated without `Budgets.View` → 403 ProblemDetails; authorized admin → 200.
