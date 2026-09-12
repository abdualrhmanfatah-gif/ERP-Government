# Quickstart: Payments Group (PAY-01..04)

**Branch**: `045-payments-group`

Validation guide — proves the clarified behaviors end-to-end per ADR-001. Prerequisites, commands, and expected outcomes only; implementation details live in `tasks.md` and the design artifacts.

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

1. Create an order (beneficiaryName, fund, fiscal year, appropriation, currency, accountId, deductions) → expect Draft, `PO-{D6}` number, server-computed gross/deduction. **Note**: BeneficiaryName only (no Party ID, ADR-001 D-1); AccountId optional in Draft (ADR-001 D-4).
2. Create a second order against an appropriation with insufficient availability → submit (fundId + appropriationId + accountId required) → expect refusal, `BudgetCheckStatus=Failed` persisted, badge shows Failed.
3. Approve the Failed order without the override flag → expect 400 verbatim message.
4. Approve again with `overrideFailedBudgetCheck=true` as a user WITHOUT `PaymentOrders.OverrideBudgetCheck` → expect 403.
5. Same call as a user WITH the permission → expect Approved, `BudgetCheckStatus=Overridden`, ApprovalHistory + status-log rows present.
6. Submit an order with zero deductions where required → expect valid submission.
7. Send the approved order to treasury without a reference → expect refusal; with reference → `SentToTreasury` + treasury trace fields populated.

### S2 — Void rules (PAY-01)

1. Void the unpaid Approved order → Voided, final.
2. Void an order with a payment → refused, verbatim message.
3. Void an order that has a Draft/PendingApproval request → allowed AND the request flips to Invalidated (S4 evidence).

### S3 — Request-first workflow (PAY-02 → PAY-01)

1. Create an independent request (beneficiaryName, requestedAmount, currency, purpose, financialYear) → Draft, `DSB-{D6}`. **Note**: BeneficiaryName string only (no BeneficiaryId, ADR-001 D-1).
2. Submit → PendingApproval (pure status transition — no availability check, ADR-001 D-3).
3. Step-1 approve by a user without AccountsManager/AuthorizingOfficer → refused.
4. Step-1 approve by a qualified user with `approvedAmount` and `issuingAuthorityName`/`issuingAuthorityCapacity` → step recorded in ApprovalHistory (ADR-001 D-5), still PendingApproval.
5. Step-2 approve by the SAME user → refused ("A different approver is required.").
6. Step-2 approve by a different qualified user with the SAME amount → Approved + `approvalDate` in ApprovalHistory; **exactly one Draft order generated** with `amountGross` = approved amount, `BeneficiaryName` copied, `DisbursementRequestId` set (UNIQUE, ADR-001 D-2). FundId/AppropriationId/AccountId null (deferred).
7. Open the generated Draft order → header + deductions (no lines, ADR-001 D-4), fundId/appropriationId/accountId empty.
8. Fill in fundId + appropriationId + accountId, submit → budget check runs → Submitted.
9. Approve the order → Approved (budget check must pass).
10. Send to treasury → Paid path open.

### S4 — Triad closure + invalidation (PAY-02/03)

1. Record a payment on the Approved order (Cash) → Completed, `PAY-{D6}`, request Disbursed, order Paid. **Note**: `paymentOrderId` in request (ADR-001 D-6); `disbursementRequestId` derived from order.
2. Cancel an Approved unpaid request → Cancelled; the order is released.
3. Cancel/void an order holding a Draft request → request Invalidated (badge visible).
4. Record a payment on a non-approved order → verbatim refusal.
5. Double-record on the same order → verbatim refusal (PaymentOrderId UNIQUE, ADR-001 D-6).

### S5 — Bank accounts (PAY-04)

1. Create account A with IsDefault=true; create account B with IsDefault=true → B default, A demoted (single transaction).
2. Duplicate accountNumber → refused.
3. Deactivate the default account (confirmation) → excluded from the PAY-01 bank picker.
4. Activate/deactivate round-trips `{id, rowVersion}`; stale RowVersion → concurrency conflict surfaced.

### S6 — Totals integrity (PAY-01)

- For any order: `netAmount = amountGross − totalDeductions`, `remainingAmount = netAmount − paidAmount`, `isFullyPaid = remainingAmount == 0` — values from `GET /{id}/totals` only; frontend audit confirms zero client-side financial recomputation.

## Acceptance evidence mapping

| Scenario | Spec tests | Constitution |
|---|---|---|
| S1 | T1, T2, T3 | V (budget before expenditure), VIII (recorded decisions) |
| S2 | T5 | IV (no phantom reversal), III (server guard) |
| S3 | T3–T10 (PAY-02) + T7 (PAY-01) | VII (SoD), VIII (history), V (budget at submit) |
| S4 | T1–T6 (PAY-03) | V (availability gate), II (event → posting) |
| S5 | T1, T2 (PAY-04) | VI (concurrency, atomic default) |
| S6 | T4, T6 | III/CC-1 (server numbers) |
