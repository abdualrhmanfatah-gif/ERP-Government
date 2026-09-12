# Research: Procurement Lifecycle Rebuild

**Branch**: `047-procurement-lifecycle` | **Date**: 2026-09-10

## R1: Document Sequence Prefix Registration

**Decision**: Register `SupplierInvoice` with prefix `SINV` in the DocumentSequenceService PrefixMap.

**Rationale**: The existing PrefixMap in `DocumentSequenceService.cs` already contains PRQ, RFQ, QT, PO, GRN. Only `SupplierInvoice` (SINV) is missing and must be added to the seed data.

**Alternatives considered**: Using a different prefix (e.g., `INV`) — rejected because `SINV` avoids collision with any future internal invoice types and is consistent with the "Supplier" qualifier.

**Impact**: One new entry in `DocumentSequences` seed data table. No code change to the service itself — just a seed data addition.

## R2: CommitteeAssignment RFQ Linkage

**Decision**: Add `RfqId` (int FK → RequestForQuotation, nullable) to `CommitteeAssignment`. The existing `PurchaseOrderId` remains for post-PO assignments (e.g., Receiving committee).

**Rationale**: Evaluation committees must be linked to an RFQ (before PO creation). The existing `PurchaseOrderId` is nullable and used for Tender-type assignments currently, but the semantic is wrong. Adding a dedicated `RfqId` is cleaner and preserves the existing `PurchaseOrderId` for Receiving/Inspection committee types.

**Alternatives considered**:
- Repurpose `PurchaseOrderId` for RFQ ID — rejected because it breaks the semantic contract and would confuse queries that filter by PurchaseOrderId.
- Create a new `CommitteeAssignmentRfq` junction table — rejected as over-engineered; a nullable FK is sufficient.

**Impact**: One new nullable FK column on `CommitteeAssignment`. Migration required.

## R3: SupplierId → SupplierPartyId Migration Strategy

**Decision**: Two-step migration: (1) Add `SupplierPartyId` column as nullable, (2) Backfill from `SupplierId` → `Parties.Id` WHERE PartyType=Supplier, (3) Set NOT NULL after backfill, (4) Drop old `SupplierId` column.

**Rationale**: The existing `PurchaseOrder.SupplierId` (int, required) and `GoodsReceiptNote.SupplierId` (int?, nullable) reference a legacy supplier ID. The `Parties` table already has a `PartyType.Supplier` filter. The migration maps old IDs to the corresponding Party record.

**Alternatives considered**:
- Drop and recreate — rejected because spec FR-069 requires preserving all existing business documents.
- Single-step migration — rejected because the FK constraint requires the target to exist before the column can be set NOT NULL.

**Impact**: A multi-step EF migration. Migration report for unmatched records. No data loss.

## R4: Encumbrance Liquidation at Receipt

**Decision**: At GRN confirmation, compute liquidation amount as: `(AcceptedQuantity / OrderedQuantity) × EncumbranceLine.Amount`. Update `EncumbranceLine.LiquidatedAmount` atomically with the GRN confirmation transaction.

**Rationale**: The existing `EncumbranceLine` already has `LiquidatedAmount` and `CancelledAmount` fields. The proportional liquidation approach is standard for procurement. The `BudgetAvailabilityService` already computes outstanding encumbrance as `Amount - LiquidatedAmount - CancelledAmount`.

**Alternatives considered**:
- Liquidate at invoice match instead of receipt — rejected because spec FR-037 explicitly states liquidation at receipt.
- Full-line liquidation on first receipt — rejected because partial receipts are supported and each receipt should liquidate proportionally.

**Impact**: Handler logic in `ConfirmGoodsReceiptNoteCommand` must query the linked encumbrance line and update it atomically.

## R5: Three-Way Matching Logic

**Decision**: Matching is per-line: each `SupplierInvoiceDetail` references one `PurchaseOrderDetail` and optionally one `GoodsReceiptNoteDetail`. The match verifies:
1. Invoice Quantity ≤ GRN AcceptedQuantity (or PO OrderedQuantity if no GRN yet)
2. Invoice UnitPrice = PO UnitPrice (exact match, 0% tolerance per clarification)
3. Cumulative invoiced quantity per PO line ≤ PO OrderedQuantity

**Rationale**: Per-line matching is the standard approach for three-way matching. The 0% tolerance was explicitly chosen by the user. Cumulative tracking prevents over-invoicing across multiple invoices against the same PO.

**Alternatives considered**:
- Header-level matching — rejected because it cannot detect line-level discrepancies.
- Configurable tolerance — rejected because user chose 0% (exact match).

**Impact**: A `CumulativeInvoicedQuantity` computation must be available per PO line. This can be computed at query time from `SupplierInvoiceDetail` records or maintained as a denormalized field on `PurchaseOrderDetail`.

## R6: PR Budget Availability Check (Soft Gate)

**Decision**: At PR approval, call `BudgetAvailabilityService.GetAvailableAmount()` per budget item to verify sufficiency. Do NOT create an encumbrance. The check is informational — if insufficient, log a warning but do not block approval.

**Rationale**: Spec D1 states "Reservation at PO approval" and FR-011 states PR approval performs a "budget availability check (soft gate — no reservation yet)". The existing `BudgetAvailabilityService` already supports this query.

**Alternatives considered**:
- Hard block on insufficient budget — rejected because D1 explicitly chose soft gate.
- No check at PR — rejected because it would allow PRs to be approved for items with zero budget, causing failures later at PO approval.

**Impact**: Handler for `ApprovePurchaseRequestCommand` must call `BudgetAvailabilityService` and log the result. No encumbrance created.

## R7: Frontend Feature Folder Structure

**Decision**: Follow the entity-based feature folder pattern from `src/Web/ClientApp/src/features/budgeting/`. Each document type gets a subfolder: `purchase-requests/`, `request-for-quotations/`, `quotations/`, `purchase-orders/`, `goods-receipt-notes/`, `supplier-invoices/`. A shared `hooks/` and `shared/` folder at the procurement feature root handles cross-entity concerns.

**Rationale**: This matches the existing pattern in `features/budgeting/` which has `appropriations/`, `classifications/`, `funds/`, etc. with cross-entity `hooks/`/`shared/`/`utils/` at root.

**Alternatives considered**:
- Flat structure — rejected because it would create too many files at one level.
- Group by lifecycle stage — rejected because it violates the entity-based convention.

**Impact**: 6 subfolders under `features/procurement/`, each with `pages/`, `hooks/`, `shared/`.

## R8: Decimal Precision for Money Fields

**Decision**: All monetary fields use `decimal(23,2)` per repository convention. Quantities use `decimal(18,4)`. Exchange rates use `decimal(18,6)`.

**Rationale**: The existing codebase uses `decimal(23,2)` for amounts (verified in `PurchaseOrderConfiguration.cs` and budgeting entities). This is the repository standard.

**Alternatives considered**: None — this is an established convention.

**Impact**: EF configurations must specify precision for all new decimal columns.
