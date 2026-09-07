# Quickstart: Checks & Monthly Statement (TRE-03)

**Branch**: `033-checks-statements`

Validation scenarios proving the feature end-to-end. Details live in [data-model.md](data-model.md) and [contracts/api.md](contracts/api.md).

## Prerequisites

- .NET 10 SDK, SQL Server (Aspire-hosted dev environment as per repo).
- Seeded posting rules including the new `CheckCleared` rule; seeded permissions include `Checks.*`.
- A fund + party + revenue account setup as TRE-01 requires.

## Build & test

```powershell
dotnet build src/Web/Web.csproj
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
dotnet test tests/Web.AcceptanceTests
cd src/Web/ClientApp; npm run lint; npm run build
```

## Scenario 1 — Checks list (US1)

1. Create + approve a Check-method receipt voucher dated this month (TRE-01 flow) with a check (TRE-02 Form48 approval also transitions checks UnderCollection).
2. `GET /api/Revenue/Checks` → the check appears with status `UnderCollection`, bank/number/date/amount/voucher.
3. `GET /api/Revenue/Checks?status=UnderCollection` after clearing another check → cleared check absent.

**Expected**: list reflects current state only; date-range default = current month.

## Scenario 2 — Clearing + posting (US2, SC-003)

1. `POST /api/Revenue/Checks/{id}/clear` with `clearedAt` ≥ check date, ≤ today, valid `rowVersion`.
2. Re-query check → `Cleared`, `clearedAt` set; DocumentStatusLog row appended.
3. `GET /api/Accounting/Events/pending` (or event query) → `AccountingEvent(EventType=CheckCleared)` for SourceId = check id; after outbox processing → balanced `JournalEntry` linked to the event.
4. Rejects: clear a bounced check (400); `clearedAt` before check date or in the future (400); stale `rowVersion` (distinct concurrency failure).

## Scenario 3 — Bounce + replace (US3)

1. `POST /api/Revenue/Checks/{id}/bounce` with date + reason → 200, check `Bounced`, DocumentStatusLog carries the reason. **No** replacement voucher is created by bounce.
2. `POST /api/Revenue/Checks/{id}/replace` with `paymentMethod=Cash` + voucher date → 200 + replacement voucher id (same amount as the check, sequence-allocated number, Draft).
3. Repeat step 1's replace call → 400 (double replace, FR-005).
4. Replace a different bounced check with `paymentMethod=Check` + `checkDetails` → new `Check` row (UnderCollection) on the replacement voucher.
5. Replace with method Check but no `checkDetails` → 400.

## Scenario 4 — Monthly statement (US4)

1. With clearings recorded this month: `GET /api/Revenue/DepositSlips/monthly-statement?year=..&month=..&fundId=..` → `clearings[]` lists number/bank/clearedAt/amount.
2. For a month with no clearings → `clearings: []` (explicitly empty, no error).

## Evidence gates (constitution XI)

- Every behavior above has a functional test that was observed red before implementation.
- Full 5-project suite green before merge.
