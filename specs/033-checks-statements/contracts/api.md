# API Contract: Checks (TRE-03)

**Binding** — field names, routes, and payload shapes are literal (spec "Data Contract").
Route base: `/api/Revenue/Checks` (endpoint group `Revenue/Checks` exists; client regenerated via `npm run generate-api`).

Permissions: `Checks.View` · `Checks.Clear` · `Checks.Bounce` · `Checks.Replace` (`Checks.Replace` code is NEW — added to PermissionCodes.cs + policy registration).
Error semantics: 400 validation (Result failures as problem details), 401 unauthenticated, 403 forbidden, 404 not found; concurrency conflicts surfaced as a distinct failure (FR-008).

## Endpoints

### GET /api/Revenue/Checks
Under-collection checks list built from Check-method receipt vouchers (FR-001 — no standalone registry). Filters via `[AsParameters]`: `from`, `to` (date range on source voucher date; default = current month when omitted), `status?` (CheckStatus filter). → `CheckDto[]` · policy `Checks.View`

### GET /api/Revenue/Checks/{id}
Single check or 404. → `CheckDetailDto` · policy `Checks.View`

### POST /api/Revenue/Checks/{id}/clear
Body:

```json
{ "clearedAt": "2026-09-07T10:00:00Z", "rowVersion": "<base64>" }
```

→ 204. Rejections (400): check not UnderCollection (FR-006/FR-011); `clearedAt` < check date or in the future (FR-002); source voucher cancelled; concurrency conflict (distinct failure, FR-008). On success: status → Cleared, `clearedAt` set, DocumentStatusLog appended (FR-007), `CheckCleared` domain event → AccountingEvent(EventType.CheckCleared) → PostingRule (seed: "ترحيل تحصيل الشيكات") → balanced JournalEntry (FR-009/SC-003).

### POST /api/Revenue/Checks/{id}/bounce
Body `{ "bouncedAt": "2026-09-07T10:00:00Z", "reason": "عدم كفاية الرصيد", "rowVersion": "<base64>" }` → 200 with the bounced check id (no voucher is created — US3.1 only locks the source voucher for replacement; replacement is a separate call, FR-003). Rejections: check not UnderCollection; `bouncedAt` < check date or in the future (FR-003); source voucher cancelled; concurrency conflict. On success: status → Bounced, `bouncedAt` set, DocumentStatusLog appended with reason.

### POST /api/Revenue/Checks/{id}/replace
Body (Cash):

```json
{ "paymentMethod": "Cash", "voucherDate": "2026-09-07", "rowVersion": "<base64>" }
```

Body (Check):

```json
{
  "paymentMethod": "Check",
  "voucherDate": "2026-09-07",
  "checkDetails": { "bankName": "بنك التنمية", "checkNumber": "778899", "checkDate": "2026-09-10", "amount": 450000.00 },
  "rowVersion": "<base64>"
}
```

→ 200 with the replacement voucher id. Creates the replacement ReceiptVoucher (number via IDocumentSequenceService, voucher date from request, amount fixed = original check amount, Status Draft), links it (`ReplacementVoucherId`), and — when Check — creates the new `Check` row (UnderCollection) from `checkDetails`. Rejections: check not Bounced (FR-006); **`ReplacementVoucherId` already set — double replace (FR-005)**; `checkDetails` missing when method = Check (FR-004); source voucher cancelled; concurrency conflict. Policy: `Checks.Replace` (NOT Checks.Clear — divergence D5 fixed).

### GET /api/Revenue/DepositSlips/monthly-statement
Existing TRE-02 endpoint — `clearings[]` lists the month's clearings (`CheckClearingDto: checkNumber, bankName, clearedAt, amount`); explicitly empty when none (FR-012). No change to this endpoint's shape; policy per TRE-02 contract.

## DTO shapes (literal)

```jsonc
// CheckDto (list)
{
  "id": 12, "receiptVoucherId": 405, "bankName": "بنك التنمية",
  "checkNumber": "778899", "checkDate": "2026-09-10", "amount": 450000.00,
  "status": "UnderCollection", "statusName": "تحت التحصيل",
  "clearedAt": null, "bouncedAt": null, "replacementVoucherId": null,
  "created": "..."
}

// CheckDetailDto (single)
{
  "checkId": 12, "bankName": "بنك التنمية", "checkNumber": "778899",
  "checkDate": "2026-09-10", "amount": 450000.00,
  "status": "UnderCollection", "clearedAt": null
}

// CheckClearingDto (statement)
{ "checkNumber": "778899", "bankName": "بنك التنمية", "clearedAt": "2026-09-07T10:00:00Z", "amount": 450000.00 }
```

## Status / Lifecycle (literal)

`UnderCollection → Cleared` (terminal) · `UnderCollection → Bounced → (replace → replacement voucher Draft + optional new check UnderCollection)`

Pending-replacement lock (derived): `Bounced && ReplacementVoucherId == null` ⇒ source voucher locked for replacement only.
