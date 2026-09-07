# Quickstart Validation: Receipt Vouchers (TRE-01)

**Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

## Prerequisites

- .NET 10 SDK; SQL Server running; database migrated to latest
- Backend build: `dotnet build src/Web/Web.csproj`
- Valid JWT token with `ReceiptVouchers.*` permissions (policies are open-assertion placeholders in dev)
- Seeded: an active Party, active revenue Account, `DocumentSequences` row for `ReceiptVoucher`

## Automated gates

```bash
dotnet test tests/Application.FunctionalTests      # Revenue/ReceiptVoucherTests.cs — all gap rules
dotnet test tests/Domain.UnitTests                 # entity invariants
dotnet test tests/Application.UnitTests
dotnet test tests/Infrastructure.IntegrationTests
dotnet test tests/Web.AcceptanceTests
# Frontend (cd src/Web/ClientApp):
npm run lint && npm run test && npm run build      # prebuild regenerates NSwag client
```

## Validation Scenarios

### V1: Create cash voucher — number displayed immediately (US1, FR-001, SC-001)

```bash
curl -X POST http://localhost:5000/api/Revenue/ReceiptVouchers \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"voucherDate":"2026-09-07","partyId":1,"paymentMethod":0,"receivedFrom":"","notes":"","lines":[{"revenueAccountId":1,"amount":5000.00}],"checks":[]}'
# Expected: 201 with full DTO in body
# Verify: voucherNumber matches ^DSL-\d{6}$; status "Draft"; totalAmount = 5000.00 (server-computed)
```

### V2: Create check voucher — checks section required (US1, FR-002/003)

Same call with `"paymentMethod":1` and `checks:[{"bankName":"بنك اليمن","checkNumber":"100234","checkDate":"2026-09-10","amount":5000.00}]`.
- Verify: 201, checks echoed in DTO.
- Repeats with `checks:[]` → 400 ("At least one check is required for check payments.").

### V3: Σ(checks) > total rejected (FR-008)

Two lines totaling 3000, one check of 3500 → 400 with the overage error. Σ(checks) = 2500 (less than total) → 201 allowed.

### V4: Lifecycle (US2)

1. `POST /{id}/submit` `{id, rowVersion}` → 204; status PendingReview; submittedById/At set.
2. Approve as the SAME user who submitted → 204 (self-approval allowed per clarification); status Approved; reviewedById/At set; ApprovalHistory + DocumentStatusLog rows written.
3. Cancel the Approved voucher → 400 (terminal). Cancel a Draft without reason → 400. Cancel a PendingReview with reason → 204; status Cancelled.
4. Submit the same voucher again → 400 (non-Draft).
5. Re-run any mutation with a stale rowVersion → conflict failure (distinct message).

### V5: Cancel status-log integrity (audit accuracy)

After any cancel, inspect `DocumentStatusLogs`: `FromStatus` reflects the true prior status (Draft or PendingReview), never "Cancelled → Cancelled".

### V6: Browse filters (US3)

`GET /api/Revenue/ReceiptVouchers?paymentMethod=...` style filters via list/by-party/by-period: filter by status Check-method vouchers returns only check vouchers; period filter bounds respected.

### V7: UI journey (US1–US3)

- Create form: party picker excludes disabled parties; choosing Check reveals the checks section (bank/number/date/amount rows) and marks them required; on save the Draft number + server total appear immediately.
- List: filter by party/period/status/method; numbers render `DSL-######`; gaps remain visible in order.
- Detail: lifecycle buttons per status; approve shows reviewer name + time (resolved from users lookup); deposited vouchers show deposit slip link; cancel requires reason modal; RowVersion conflict shows a toast.

## Edge regression checks

- Disabled partyId at create → 400.
- Cash with checks → 400.
- Zero-line create → 400 (validator); zero-line submit → 400 (handler guard).
