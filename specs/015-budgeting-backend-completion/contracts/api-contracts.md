# HTTP API Contracts — Budgeting Backend Completion (015)

Additive contract extensions only. Conventions (as-built): minimal-API `IEndpointGroup` → `/api/{Group}`; every route `.RequireAuthorization(permission)`; errors = problem-details (400 validation, 401, 403, 404; concurrency surfaced distinctly); every mutation round-trips `rowVersion`. Enums as int in payloads. OpenAPI v1 regenerates the TS client (`nswag run`).

## 1. Encumbrances — NEW group `/api/Encumbrances`

| Method | Route | Permission | Body / Query | Returns |
|--------|-------|-----------|--------------|---------|
| GET | `/api/Encumbrances` | EncumbrancesView | query: `appropriationId?`, `type?`, `status?`, `from?`, `to?`, `reversalState?` (all/non-reversal/reversals/reversed-only), paging | `{ items: EncumbranceListItem[], totalCount }` |
| GET | `/api/Encumbrances/{id}` | EncumbrancesView | — | `EncumbranceDetail` |
| POST | `/api/Encumbrances` | EncumbrancesCreate | `{ appropriationId, type, vendorId?, purchaseOrderId?, documentType?, documentId?, description, encumbranceDate, amount, rowVersion? }` | `201` + `{ id, encumbranceNumber }` — number ENC-###### system-assigned; availability gate per budget-type ControlMethod (Blocking → 400 result failure; Warning → 200/201 with warning recorded in ApprovalHistory Reason) |
| PUT | `/api/Encumbrances/{id}` | EncumbrancesUpdate | `{ description?, encumbranceDate?, amount?, vendorId?, purchaseOrderId?, rowVersion }` | `200` — Draft only (else 400); concurrency token verified |
| DELETE | `/api/Encumbrances/{id}` | EncumbrancesDelete | `rowVersion` | `200` — Draft only |
| PATCH | `/api/Encumbrances/{id}/submit` | EncumbrancesSubmit | `rowVersion` | Draft → PendingApproval |
| PATCH | `/api/Encumbrances/{id}/approve` | EncumbrancesApprove | `rowVersion, reason?` | PendingApproval → Approved |
| PATCH | `/api/Encumbrances/{id}/activate` | EncumbrancesActivate | `rowVersion` | Approved → Active |
| PATCH | `/api/Encumbrances/{id}/suspend` | EncumbrancesClose† | `rowVersion, reason?` | Active → Suspended |
| PATCH | `/api/Encumbrances/{id}/close` | EncumbrancesClose | `rowVersion, reason?` | Active\|Suspended → Closed |
| PATCH | `/api/Encumbrances/{id}/cancel` | EncumbrancesCancel | `rowVersion, reason?` | any non-terminal → Cancelled |
| PATCH | `/api/Encumbrances/{id}/reverse` | EncumbrancesReverse | `rowVersion, reason (required)` | Active\|Suspended → original Reversed + NEW negative-effect row (ReversalOfId = original); returns `{ id: newReversalId }` |

† suspend reuses the Close permission code (no dedicated Suspend code exists; documented choice — or a new code may be added in tasks if review prefers; contract route unaffected).

**EncumbranceListItem / Detail** include: `id, encumbranceNumber, type, status, appropriationId, vendorId?, purchaseOrderId?, documentType?, documentId?, description, encumbranceDate, amount, reversalOfId?, reversalReason?, isReversed (COMPUTED: exists row WHERE ReversalOfId = id — never stored), rowVersion, created, lastModified`.

Every lifecycle PATCH records ApprovalHistory (`DocumentType="Encumbrance"`, `Decision="{from} -> {to}"`).

## 2. BudgetItems — NEW group `/api/BudgetItems`

| Method | Route | Permission | Returns |
|--------|-------|-----------|---------|
| GET | `/api/BudgetItems/{id}/availability` | BudgetItemsView | `BudgetItemAvailability` |
| GET | `/api/BudgetItems/{id}/monthly-plan` | BudgetItemsView | `MonthlyPlan { budgetItemId, items: [{ month 1-12, plannedAmount }] }` (≤12 rows) |
| PUT | `/api/BudgetItems/{id}/monthly-plan` | BudgetItemsUpdate? ‡ | body: `{ items: [{ month, plannedAmount }] }` batch of ≤12; replaces the plan idempotently; rejects negative amounts / month ∉ 1–12 / duplicate months; returns `MonthlyPlan` |

‡ monthly-plan permissions reuse the BudgetItems codes used by nested item endpoints today (final code names fixed in tasks; contract routes fixed as above).

**BudgetItemAvailability**: `{ budgetItemId, netAppropriated, encumbered, available, effectiveAllowOverrun, budgetControlMethod }` — whole-item semantics, computed at request time; 404 unknown id.

## 3. Appropriations — additions to existing group

| Method | Route | Permission | Notes |
|--------|-------|-----------|-------|
| POST | `/api/Appropriations/transfers` | AppropriationsCreate | body `{ budgetId, sourceBudgetItemId, targetBudgetItemId, amount (>0), rowVersion? }` → creates the PAIR atomically: source row (negative effect, BudgetItemId=source) + target row (positive, TargetBudgetItemId=source). Validations: same Budget, source ≠ target, source net active appropriation ≥ amount. Returns `{ sourceId, targetId }`. Transfer remains Draft-only (post-submission movement uses Adjustment). |
| GET | `/api/Appropriations/{id}/availability` | AppropriationsView | `BudgetItemAvailability` for the OWNING BudgetItem (control point = item). 404 unknown id. |

Existing `POST /api/Appropriations` keeps rejecting `effect = Transfer` (pair command only). All lifecycle PATCHes on a transfer row transition BOTH rows.

## 4. Budgets — head fields in existing payloads

`POST /api/Budgets` request ADDS: `totalAmount` (entered plan; no longer forced 0), `allowOverrun?` (bool), `effectiveFrom?`, `effectiveTo?` (nullable), `description?`. `BudgetNumber` REMOVED from the request (system-assigned BGT-######; response already carries it). `PUT /api/Budgets/{id}` accepts the same added fields + `rowVersion`. No new routes; lifecycle PATCH routes unchanged; approve/activate never write an active flag.

## 5. Non-functional contract notes

- All numbers server-assigned, immutable; `{prefix}-{D6}` format; concurrent creates NEVER return the same number (atomic allocation); sequence exhaustion/missing = 400 problem-details (typed failure), never silent.
- Transfer rows share every status transition; a transfer can never be half-transitioned.
- Encumbrance amounts never mutate after leaving Draft; corrections only via `/reverse`.
- `isReversed` and availability figures are computed per request — no stored aggregates.
