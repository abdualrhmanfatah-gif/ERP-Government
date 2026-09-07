# Quickstart: Deposit Slips (TRE-02)

Validation guide proving the feature end-to-end. Prereqs: SQL Server via Aspire AppHost, backend
`src/Web`, frontend `src/Web/ClientApp`. See [contracts/api.md](contracts/api.md) for payload shapes
and [data-model.md](data-model.md) for rules — not duplicated here.

## Setup

```powershell
dotnet build src/Web/Web.csproj                       # backend compiles clean (warnings = errors)
cd src/Web/ClientApp; npm install; npm run generate-api   # regenerates client against /api/Revenue/DepositSlips
dotnet run --project src/AppHost                      # or: dotnet run --project src/Web
```

Seed: TRE-01 vouchers exist (cash + check, Approved), at least one Fund row.

## Test commands (evidence per constitution XI)

```powershell
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Revenue"
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~DepositSlip|MonthlyStatement"
dotnet test tests/Web.AcceptanceTests
cd src/Web/ClientApp; npm run lint; npm run test; npm run build
```

## Journey 1 — Create a Form47 slip (US1)

1. `GET /api/Revenue/DepositSlips` with a cash-check list filter (picker behavior) — only Approved, unassigned, Cash vouchers appear.
2. `POST /api/Revenue/DepositSlips` `{slipDate: today, formType: "Form47", voucherIds: [two cash vouchers]}` → 201; `GET /{id}` shows `Draft`, `slipNumber` matching `DSL-\d{6}`, `totalAmount` = sum of member lines.
3. Repeat with a check voucher in `voucherIds` → 400 homogeneity failure.
4. Empty create `{voucherIds: []}` → 201 Draft, empty members (FR-015).

## Journey 2 — Correct the batch while Draft (US2)

1. `POST /{id}/remove-voucher` with a reason → 204; `GET /{id}`: member gone, total recomputed.
2. Remove again with empty reason → 400 (FR-003).
3. Add the removed voucher to a *different* Form47 Draft slip → 204 (FR-013).
4. Approve the first slip; then attempt any add/remove → 400 Draft-only failure (server guard; UI hides controls).

## Journey 3 — Approval effects (US3)

1. Form47 slip with members: `POST /{id}/approve` with rowVersion → 204. Slip `Approved`, `approvedById/At` set; `ApprovalHistory` row carries the rule-evaluation snapshot; `DocumentStatusLog` Draft→Approved; outbox shows `ReceiptVoucherCollected` per member → `JournalEntry` produced by posting rules (visible ledger effect, SC-003).
2. Form48 slip with a check voucher: approve → member check stays/becomes `UnderCollection`.
3. Approve an empty slip → 400 (FR-005). Approve with stale rowVersion → concurrency failure (FR-008). Approve a slip whose member was cancelled → 400 naming the voucher (edge case).

## Journey 4 — Monthly statement (US4)

1. `GET /api/Revenue/DepositSlips/monthly-statement?year=2026&month=9&fundId=3` after Journey 3 → summary figures consistent: `totalDeposited` = approved slip totals; `vouchers[]` lists ALL Approved vouchers of the month (including un-deposited); `clearings[]` lists cleared checks.
2. Query a month with no activity → explicit zeros + empty arrays (no error).
3. Unknown `fundId` → 400 validation failure.

## Frontend acceptance

- Deposit slips list + detail under `features/treasury/deposit-slips`: Arabic-first RTL, dark mode, money-display conventions.
- Approved slip: add/remove/approve controls hidden; rowVersion round-trips on every mutation.
- Statement page: month + fund selectors; zero-month renders explicit zeros, not an empty screen.
