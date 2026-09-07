# API Contract: Deposit Slips (TRE-02)

**Binding** — field names, routes, and payload shapes are literal (spec "Data Contract").
Route base: `/api/Revenue/DepositSlips` (endpoint group `Revenue/DepositSlips` — `RoutePrefix` override; client regenerated via `npm run generate-api`).

Permissions: `DepositSlips.View` · `DepositSlips.Create` · `DepositSlips.Update` (add/remove members) · `DepositSlips.Approve`.
Error semantics: 400 validation (Result failures as problem details), 401 unauthenticated, 403 forbidden, 404 not found; concurrency conflicts surfaced distinctly (FR-008).

## Endpoints

### GET /api/Revenue/DepositSlips
List slips (filters via `[AsParameters]`). → `DepositSlipDto[]` · policy `DepositSlips.View`

### GET /api/Revenue/DepositSlips/{id}
Single slip or 404. → `DepositSlipDto` · policy `DepositSlips.View`

### POST /api/Revenue/DepositSlips
Create. Body:

```json
{ "slipDate": "2026-09-07", "formType": "Form47", "voucherIds": [101, 102] }
```

`voucherIds` MAY be empty (FR-015). 201 → slip id. Rejections (400): future slip date; slip date < latest member voucher date; non-approved voucher; wrong-method voucher for the form (homogeneity message, FR-001); voucher already member of another slip.

### POST /api/Revenue/DepositSlips/{id}/add-voucher
Body `{ "voucherId": 103, "rowVersion": "<base64>" }` → 204.
Rejections: slip not Draft; voucher not Approved; voucher already member of a slip; homogeneity (FR-001); concurrency conflict (409-style distinct failure per FR-008).

### POST /api/Revenue/DepositSlips/{id}/remove-voucher
Body `{ "voucherId": 103, "reason": "خطأ في التجميع", "rowVersion": "<base64>" }` → 204.
`reason` is required by FR-003 (empty reason ⇒ 400) though the field stays optional in the wire type. Removal only while Draft (FR-003); removed voucher returns to the eligible pool (FR-013).

### POST /api/Revenue/DepositSlips/{id}/approve
Body `{ "reason": "<optional>", "rowVersion": "<base64>" }` → 204.
Rejections: slip not Draft; **no members** (FR-005); a member cancelled after joining (R9); approval-rule evaluation failed (R8); concurrency conflict. On success: status → Approved, `approvedById/approvedAt` set, ApprovalHistory (with evaluation snapshot) + DocumentStatusLog appended; Form47 emits `ReceiptVoucherCollected` per member (revenue posting via outbox, FR-004); Form48 sets member checks UnderCollection (FR-004).

### GET /api/Revenue/DepositSlips/monthly-statement
Query: `year`, `month`, `fundId`. → `MonthlyStatementDto` · policy `DepositSlips.View` (aligns with existing `ReceiptVouchers.View` — final policy is the contract `View` code).

## DTO shapes (literal)

```jsonc
// DepositSlipDto
{
  "id": 1, "slipNumber": "DSL-000047", "slipDate": "2026-09-07",
  "formType": "Form47", "formTypeName": "نقدية",
  "status": "Draft", "statusName": "مسودة",
  "totalAmount": 1250000.00,
  "approvedById": null, "approvedAt": null,
  "receiptVouchers": [ /* ReceiptVoucherDto[] */ ],
  "rowVersion": "<base64>",
  "created": "...", "createdBy": "...", "lastModified": "...", "lastModifiedBy": "..."
}

// MonthlyStatementDto
{
  "year": 2026, "month": 9, "fundId": 3, "fundName": "<from Funds>", "generatedAt": "...",
  "summary": {
    "totalCashCollections": 0, "totalCheckCollections": 0, "totalDeposited": 0,
    "totalUnderCollection": 0, "totalCleared": 0, "totalBounced": 0
  },
  "vouchers": [ /* ALL approved vouchers of the month — full activity (FR-016) */ ],
  "clearings": [ /* CheckClearingDto[] */ ]
}
```

Enums: `FormType: Form47 | Form48`; `DepositSlipStatus: Draft | Approved` (terminal).

## Frontend obligations (constitution IX/X)

- Typed calls through the regenerated NSwag client only.
- Round-trip `rowVersion` on add/remove/approve; surface concurrency failures distinctly.
- Voucher picker pre-filters by payment method + Approved + unassigned (US1/US2); add/remove controls hidden when Approved (server still guards).
- Money via shared money-display; RTL logical properties; nav entries carry permission identifiers.
