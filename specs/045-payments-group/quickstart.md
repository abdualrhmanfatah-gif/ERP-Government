# Quickstart: Payments Group (PAY-01..04)

**Branch**: `045-payments-group`

Validation guide — proves the clarified behaviors end-to-end. Prerequisites, commands, and expected outcomes only; implementation details live in `tasks.md` and the design artifacts.

## Prerequisites

- SQL Server reachable (Aspire AppHost or local connection string in `src/Web/appsettings.json`)
- .NET 10 SDK; Node 20+ for the frontend
- Database initialized (migrations applied + seeder run: permissions merged idempotently)

## Build & test commands

```bash
# backend build (no .sln — target projects directly)
dotnet build src/Web/Web.csproj

# backend test suites (full suite gates the merge)
dotnet test tests/Domain.UnitTests
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
dotnet test tests/Infrastructure.IntegrationTests
dotnet test tests/Web.AcceptanceTests

# frontend (lint only — no frontend tests by governance decision)
cd src/Web/ClientApp && npm run lint && npm run build
```

## Validation scenarios

### S1 — Full order lifecycle with budget check (PAY-01)

1. Create an order (vendor, fund, fiscal year, appropriation, currency, lines, deductions, beneficiary) → expect Draft, `PO-{D6}` number, server-computed gross/deduction.
2. Create a second order against an appropriation with insufficient availability → submit → expect refusal, `BudgetCheckStatus=Failed` persisted, badge shows Failed.
3. Approve the Failed order without the override flag → expect 400 verbatim message.
4. Approve again with `overrideFailedBudgetCheck=true` as a user WITHOUT `PaymentOrders.OverrideBudgetCheck` → expect 403.
5. Same call as a user WITH the permission → expect Approved, `BudgetCheckStatus=Overridden`, ApprovalHistory + status-log rows present.
6. Submit an order with zero lines → expect refusal.
7. Send the approved order to treasury without a reference → expect refusal; with reference → `SentToTreasury` + treasury trace fields populated.

### S2 — Void rules (PAY-01)

1. Void the unpaid Approved order → Voided, final.
2. Void an order with a partial payment → refused, verbatim message.
3. Void an order that has a Draft/PendingApproval request → allowed AND the request flips to Invalidated (S4 evidence).

### S3 — Dual signature (PAY-02)

1. Create a request on an Approved order with no existing request → Draft, `DSB-{D6}`, snapshot amount.
2. Attempt a second request on the same order → verbatim 1:1 refusal.
3. Submit → PendingApproval (Blocking availability ⇒ refusal with detail; Warning ⇒ HasWarning badge).
4. Step-1 approve by a user without AccountsManager/AuthorizingOfficer → refused.
5. Step-1 approve by a qualified user → step recorded (history row with role), still PendingApproval.
6. Step-2 approve by the SAME user → refused ("A different approver is required.").
7. Step-2 approve by a different user WITHOUT a qualified role → refused (D5).
8. Step-2 approve by a different qualified user → Approved + approvalDate; Stepper shows both steps.

### S4 — Triad closure + invalidation (PAY-02/03)

1. Record a payment on the Approved request (Cash) → Completed, `PAY-{D6}`, request Disbursed, order Paid, totals show paidAmount/remainingAmount=0/isFullyPaid=true.
2. Cancel an Approved unpaid request → Cancelled; the order reappears in the create-request picker.
3. Cancel/void an order holding a Draft request → request Invalidated (badge visible).
4. Record a payment on a non-approved request → verbatim refusal.
5. Double-record on the same request → verbatim refusal (uniqueness guard).

### S5 — Bank accounts (PAY-04)

1. Create account A with IsDefault=true; create account B with IsDefault=true → B default, A demoted (single transaction).
2. Duplicate accountNumber → refused.
3. Deactivate the default account (confirmation) → excluded from the PAY-01 bank picker.
4. Activate/deactivate round-trips `{id, rowVersion}`; stale RowVersion → concurrency conflict surfaced.

### S6 — Totals integrity (PAY-01)

- For any order: `netAmount = amountGross − totalDeductions`, `remainingAmount = netAmount − paidAmount`, `isFullyPaid = remainingAmount == 0` — all six values from `GET /{id}/totals` only; frontend audit confirms zero client-side financial recomputation.

## Acceptance evidence mapping

| Scenario | Spec tests | Constitution |
|---|---|---|
| S1 | T1, T2, T3 | V (budget before expenditure), VIII (recorded decisions) |
| S2 | T5 | IV (no phantom reversal), III (server guard) |
| S3 | T2 (PAY-02) | VII (SoD), VIII (history) |
| S4 | T1–T4 (PAY-03) | V (availability gate), II (event → posting) |
| S5 | T1, T2 (PAY-04) | VI (concurrency, atomic default) |
| S6 | T4, T6 | III/CC-1 (server numbers) |
