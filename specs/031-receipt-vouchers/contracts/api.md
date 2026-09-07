# API Contracts: Receipt Vouchers (TRE-01)

**Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md) — binding contract, literal.

Route base: `/api/Revenue/ReceiptVouchers` (endpoint group `Revenue/ReceiptVouchers`).

## DTO (literal — field order and names are the contract)

`ReceiptVoucherDto`: `id, voucherNumber, voucherDate*, partyId*, partyName, paymentMethod* (enum: Cash/Check), paymentMethodName, receivedFrom?, notes?, depositSlipId?, depositSlipNumber?, status (enum ReceiptVoucherStatus: Draft/PendingReview/Approved/Cancelled), statusName, totalAmount, submittedById?, submittedAt?, reviewedById?, reviewedAt?, cancellationReason?, lines[], checks[], rowVersion, created, createdBy, lastModified, lastModifiedBy`

- Line: `revenueAccountId*, amount*, description?`
- CheckDto: `id, bankName, checkNumber, checkDate, amount, status…` (TRE-03 extras)
- CreateCheckDto: `bankName*, checkNumber*, checkDate*, amount*`

## Endpoints

| Method | Route | Auth (policy) | Request | Response | Notes |
|--------|-------|---------------|---------|----------|-------|
| GET | `/` | ReceiptVouchers.View | `?partyId&status&fromDate&toDate&page&pageSize` (as-params) | `200 List<ReceiptVoucherDto>` | Filters: party / period / status / method (US3) |
| GET | `/{id}` | ReceiptVouchers.View | — | `200 ReceiptVoucherDto` / `404` | |
| GET | `/by-party/{partyId}` | ReceiptVouchers.View | `?page&pageSize` | `200 List<ReceiptVoucherDto>` | |
| GET | `/by-period` | ReceiptVouchers.View | `?fromDate&toDate&page&pageSize` | `200 List<ReceiptVoucherDto>` | |
| POST | `/` | ReceiptVouchers.Create | Create body below | `201 ReceiptVoucherDto` / `400 problem-details` | **Changed**: response carries DTO (number + total immediately, FR-001) |
| POST | `/{id}/submit` | ReceiptVouchers.Submit | `{id, rowVersion}` | `204` / `400` | Rejects: non-Draft, zero lines, Σ(checks) > total, stale rowVersion |
| POST | `/{id}/approve` | ReceiptVouchers.Approve | `{id, reason?, rowVersion}` | `204` / `400` | Rejects: non-PendingReview, stale rowVersion. Submitter MAY approve (clarified) |
| POST | `/{id}/cancel` | ReceiptVouchers.Cancel | `{id, reason*, rowVersion}` | `204` / `400` | Rejects: Approved/Cancelled source, missing reason, stale rowVersion |

**Change vs current build**: create route path stays; response shape changes from bare `int` to `ReceiptVoucherDto`. Per constitution IX this is a contract clarification recorded in this feature's binding spec — the spec §Data Contract declares it.

## Create request

```json
{
  "voucherDate": "2026-09-07",
  "partyId": 12,
  "paymentMethod": 0,
  "receivedFrom": "",
  "notes": "دفعة ضريبة سبتمبر",
  "lines": [{ "revenueAccountId": 456, "amount": 5000.00, "description": "ضريبة" }],
  "checks": []
}
```

`paymentMethod: 1 (Check)` ⇒ `checks[]` required (each: bankName, checkNumber, checkDate, amount), Σ(checks) ≤ Σ(lines). `paymentMethod: 0 (Cash)` ⇒ `checks[]` must be empty. `receivedFrom` optional.

## Lifecycle

`Draft → PendingReview → Approved`; `Draft | PendingReview → Cancelled` (reason required). Cancelled/Approved terminal. Errors use the uniform problem-details contract; concurrency conflicts surfaced distinctly (400 with rowVersion conflict message per current Result error mapping).

## Permissions

`ReceiptVouchers.View / Create / Submit / Approve / Cancel` — already defined in `PermissionCodes` and registered; policies currently open-assertion placeholders (known dev state, tracked separately).
