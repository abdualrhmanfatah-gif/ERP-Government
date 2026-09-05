# Quickstart — Budgeting Backend Completion (015)

End-to-end validation. Prereqs: Docker (Aspire SQL Server), .NET 10 SDK, Node (ClientApp). TDD (Constitution XI): every scenario below must exist as a failing-then-passing test before its implementation task is "done".

## Setup

```powershell
dotnet build ERP-Government.sln                 # warnings-as-errors gate
dotnet test tests/Application.UnitTests         # unit suites
# functional tests need the Aspire test host + SQL:
dotnet test tests/Application.FunctionalTests   # per-test DB reset (Respawn)
```

Migration check (schema drift fix):

```powershell
dotnet ef migrations list  --project src/Infrastructure --startup-project src/Web
dotnet ef database update  --project src/Infrastructure --startup-project src/Web
# expect exactly ONE new migration after InitialCreate; apply → zero model/DB drift
```

## Scenario A — unique document numbers (US1)

1. POST two Appropriations in parallel (`POST /api/Appropriations` ×2, same BudgetItem).
2. Expect: both 201; numbers `APR-000001`-style, DISTINCT; format `{prefix}-{6 digits}`.
3. Repeat for `/api/Budgets` (BGT, request contains NO budgetNumber field) and `/api/Encumbrances` (ENC).
4. PUT any approved document with a changed number attempt → number unchanged.
5. Test: functional test firing N parallel creates asserts 0 duplicates / 0 collisions.

## Scenario B — encumbrance lifecycle + gate (US2)

1. Seed Budget (Active) → BudgetItem → Original Appropriation (Active, amount 10 000).
2. `POST /api/Encumbrances` amount 12 000:
   - ControlMethod Blocking → 400 failure.
   - Warning → 201 + ApprovalHistory row with override reason.
   - None → 201, no warning row.
3. Walk: submit → approve → activate (each PATCH with `rowVersion`; stale token → concurrency conflict response).
4. `PUT`/`DELETE` after activation → 400 (Draft-only).
5. `PATCH /{id}/reverse` (Active) → original status Reversed; NEW row returned with `reversalOfId = original`; `GET /{original}` shows `isReversed: true`; original amount fields untouched.
6. Reverse the reversal row (now Active) → 400 (reverse-from-Reversed/… not allowed; reverse only from Active|Suspended).
7. After reversal, `GET /api/BudgetItems/{itemId}/availability` shows `encumbered` reduced by the reversed amount.

## Scenario C — transfer pair (US3)

1. `POST /api/Appropriations/transfers` `{source, target, amount}` → 200 `{sourceId, targetId}`; exactly 2 new rows; source row negative effect / target positive / `targetBudgetItemId` set.
2. Submit/approve/activate the source row → BOTH rows transition (query both).
3. Force a failure mid-pair (validator rejection: different budgets / source < amount) → NEITHER row created (atomic).
4. Availability of source item unchanged by an active transfer pair (net zero).

## Scenario D — availability endpoints (US4)

1. Seed item with appropriations 10 000 (Active) − 2 000 (Reduction Active) and encumbrance 3 000 (Active).
2. `GET /api/BudgetItems/{id}/availability` → `netAppropriated: 8000, encumbered: 3000, available: 5000`, `effectiveAllowOverrun` matches item > budget > type resolution (flip each layer to verify precedence).
3. `GET /api/Appropriations/{anyRowId}/availability` → identical figures (owning item).
4. Unknown ids → 404.

## Scenario E — budget head + monthly plan (US5)

1. `POST /api/Budgets` with `totalAmount: 500000, allowOverrun: true, effectiveFrom/To, description` → persisted; `GET /api/Budgets/{id}` echoes all (totalAmount ≠ 0).
2. Activate → status Active, no active-flag column exists (schema check).
3. `PUT /api/BudgetItems/{id}/monthly-plan` with 12 entries → `GET` returns them; repeat PUT (replace, idempotent); negative amount / month 0 / month 13 / duplicate month → 400.

## Scenario F — authorization (US6)

1. Unauthenticated call to any new route → 401.
2. Authenticated without permission (e.g. no EncumbrancesCreate) → 403; use case enforces independently (fail-closed).
3. PUT/DELETE use EncumbrancesUpdate/Delete codes (separate from View/Create).

## Frontend + docs (US6)

```powershell
cd src/Web/ClientApp
npm run generate-api     # regenerates web-api-client.ts incl. new EncumbrancesClient/BudgetItems availability + monthly plan
npm run build            # prebuild regen gate must pass
```

- `useEncumbrances.ts` has no liquidatePartial/liquidateFull; wrappers target the regenerated client's real operations.
- DocumentSequenceForm has no 'Liquidation' (التصفية) option.
- `docs/database-schema.md` gains the Budgeting section; FM-004 REMOVED; registry BF-002/BF-003 updated; `specs/013-.../spec.md` status = superseded by 015.

## Done signals

All suites green (no skipped/weakened tests); parallel-create test shows 0 duplicate numbers over 50+ trials; migration applies cleanly with zero drift; contract additions visible in regenerated client.
