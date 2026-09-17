# Quickstart: Asset Management Module Rebuild

**Feature**: 058-asset-module-spec | **Date**: 2026-09-15

Validation guide proving the feature end-to-end. Implementation details live in [tasks.md](./tasks.md) (Phase 2); model and contract details in [data-model.md](./data-model.md) and [contracts/api.md](./contracts/api.md).

## Prerequisites

- .NET 10 SDK (10.0.201+), SQL Server instance, Node.js for the ClientApp
- Read [DEP-030 decision record](../../../docs/decision-records/) — the drop-and-recreate reset must be accepted before migrating any environment

## Setup

```bash
dotnet build                      # warnings-as-errors; must pass clean
dotnet test tests/Application.UnitTests
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.FunctionalTests     # real DB via TestAppHost, per-test reset
dotnet ef database update --project src/Infrastructure --startup-project src/Web   # applies DEP-030 migration
npm --prefix src/Web/ClientApp run generate-api   # regenerates clients for the reshaped contract
npm --prefix src/Web/ClientApp run lint && npm --prefix src/Web/ClientApp run build
```

## Scenario 1 — Clean-state verification (SC-001)

1. After `database update`, run the deletion-report acceptance test (`Web.AcceptanceTests`).
2. **Expected**: the report lists every legacy assets table as dropped (9 tables, data and history) and the 13 new tables as created and EMPTY; shared tables (users, employees, accounts, journal entries, locations, funds, cost centers, currencies, fiscal periods) reconcile unchanged — zero rows added/removed there.

## Scenario 2 — Full lifecycle on the empty register (SC-002)

1. Create an asset group with depreciation defaults and the single `depreciationAccountId`; bind two attribute definitions (one required).
2. Register an asset with an employee custodian and attribute values; activate it.
3. Execute a transfer (location + custodian) — card reflects destination; history preserves the source.
4. Run depreciation for the period; post it — verify: entry balanced, per-line `depreciationScheduleId` tagging, card `accumulatedDepreciation`/`lastDepreciationDate` updated, document `Posted`, `journalEntryId` linked.
5. Reverse one posted schedule — new linked record built from the ORIGINAL entry's accounts; original untouched.
6. Dispose of another asset — gain/loss derived; status transitions per current rule; card/history never erased.
7. Record a revaluation (increase) and an impairment + its reversal (linked transaction).
8. Create an entity-wide count («جميع المواقع — جميع الإدارات»), start it (lines generated, System* frozen), record observations including one not-found, attempt completion (blocked while a line is "not examined"), set the definite state, complete, review.
9. **Expected**: every step above succeeds on the clean register; no step requires legacy data; the audit trail shows re-examination old/new diffs.

## Scenario 3 — Posting gates (FR-094a, Constitution IV)

1. Post depreciation with the group's `depreciationAccountId` unset → **expected**: immediate rejection `ASSET-POST-GATE-FAILED` (409), nothing persisted.
2. Remove the template's substitution role designation (config), retry → **expected**: same blocking rejection; the document never enters `Posting`.
3. Restore configuration, retry → **expected**: success path of Scenario 2 step 4.

## Scenario 4 — Integrity invariants

1. Re-run a normal depreciation run for the same period → **expected**: `ASSET-DUPLICATE-RUN` rejection (reversals excluded from the rule).
2. Edit a posted schedule directly → **expected**: impossible (no endpoint; posted records immutable, FR-093).
3. Two users edit the same count line → **expected**: second save returns 409 `CONCURRENCY-CONFLICT`; no partial write.
4. Attempt to insert a duplicate count line (count, asset) → **expected**: rejected by the UNIQUE constraint.
5. Kill the outbox processor mid-posting, restart → **expected**: lease recovery re-drives the message exactly once; no repeated financial effect (idempotent consumer).

## Frontend smoke (Arabic-first RTL)

1. `npm --prefix src/Web/ClientApp run start` — navigate: groups (re-pointed), register (employee custodian + derived department), transfers, depreciation, counts.
2. **Expected**: RTL layout correct, shared components and tokens only, money via shared formatting, scope label «جميع المواقع — جميع الإدارات» visible on the count screen/document, nav entries gated by the new permission codes.
