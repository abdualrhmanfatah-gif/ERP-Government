# HTTP Contract Deltas: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-08

The backend-generated OpenAPI document is the SSOT (Constitution IX). This file documents **behavior deltas** against the as-built endpoints — no route additions, renames, or payload reshapes except where noted. Frontend consumes via the NSwag client regenerated after implementation.

## As-built endpoints (unchanged routes)

### /api/PaymentOrders

| Method | Path | Delta |
|---|---|---|
| GET | / | unchanged (filters: status/budgetCheckStatus/period/vendor) |
| GET | /{id} | unchanged |
| GET | /{id}/totals | unchanged route; `paidAmount`/`remainingAmount`/`isFullyPaid` computed from completed payments (D7) |
| POST | / | unchanged |
| POST | /{id}/submit | **behavior**: budget check via `BudgetAvailabilityService` (appropriation-level); Failed persists `BudgetCheckStatus=Failed` and fails the submit with verbatim message |
| POST | /{id}/approve | **behavior + request field**: new optional `overrideFailedBudgetCheck: boolean`; see contract detail below |
| POST | /{id}/reject | unchanged (reason required) |
| POST | /{id}/cancel | **behavior**: cancels + auto-invalidates linked non-terminal requests (D4) |
| POST | /{id}/send-to-treasury | unchanged (`TreasuryReference` required) |
| POST | /{id}/void | **behavior**: guard = Approved/SentToTreasury + unpaid; partial refused; auto-invalidates linked requests (D3/D4) |
| PUT | /{id} | unchanged (Draft-only) |

### /api/DisbursementRequests

| Method | Path | Delta |
|---|---|---|
| GET | / | unchanged |
| GET | /{id} | unchanged (detail incl. approvals[] from ApprovalHistory) |
| POST | / | unchanged (1:1 unique; availability warning → HasWarning) |
| PATCH | /{id}/submit | unchanged (availability Blocking ⇒ verbatim refusal) |
| PATCH | /{id}/approve | **behavior**: step 2 adds qualified-role check (D5) |
| PATCH | /{id}/reject | unchanged (reason required) |
| PATCH | /{id}/cancel | unchanged (reason required; releases order) |

### /api/Payments

| Method | Path | Delta |
|---|---|---|
| GET | / | unchanged |
| GET | /{id} | unchanged |
| POST | / | unchanged (Approved-only; amount = server snapshot; referenceNumber optional; double-payment guard) |

### /api/BankAccounts

| Method | Path | Delta |
|---|---|---|
| GET | / | unchanged |
| GET | /{id} | unchanged |
| POST | / | **behavior**: IsDefault=true auto-demotes previous default in same transaction (D6) |
| PUT | /{id} | **behavior**: same auto-switch |
| POST | /{id}/activate | unchanged |
| POST | /{id}/deactivate | unchanged |

## Contract detail — approve-with-override

`POST /api/PaymentOrders/{id}/approve`

```jsonc
// Request (extended — additive, non-breaking)
{
  "id": 1,
  "reason": "string?",
  "rowVersion": "base64",
  "overrideFailedBudgetCheck": false   // NEW, optional, default false
}
```

- `BudgetCheckStatus == Failed` && `overrideFailedBudgetCheck == false` → 400 problem-details, verbatim message "Cannot approve payment order with failed budget check."
- `Failed` + flag + caller lacks `PaymentOrders.OverrideBudgetCheck` → 403 problem-details.
- `Failed` + flag + permission held → approved; `BudgetCheckStatus = Overridden`; ApprovalHistory + DocumentStatusLog rows written with snapshot (actor, decision, reason, check detail).

## Contract detail — error semantics (Constitution IX)

400 validation / 401 unauthenticated / 403 forbidden / 404 not found / concurrency conflict surfaced distinctly (RowVersion). All business-rule refusals are `Result.Failure` problem-details with the verbatim server message (CC-2).

## New permission code

`PaymentOrders.OverrideBudgetCheck` — added to `PermissionCodes`, policy registration, idempotent permission seed. Frontend navigation entry for the override affordance carries the same identifier (Constitution X).

## Regeneration

After implementation: `cd src/Web/ClientApp && npm run generate-api` (NSwag client) — required because the approve request body gains a field.
