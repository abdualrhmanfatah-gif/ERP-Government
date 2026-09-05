# Research: Unified Party + Document Infrastructure

**Branch**: `016-unified-party-document` | **Date**: 2026-09-05

## R1: ApprovalHistory Writer Site Count Discrepancy

**Decision**: The spec states "14 writer sites" but actual codebase audit found **22 direct Add sites** plus 1 `IApprovalService` pipeline call.

**Rationale**: The spec likely counts unique lifecycle actions (Submit, Approve, Activate, Suspend, Close, Cancel, Reverse = 7) across document types (Budget, Appropriation, Encumbrance = 3) giving 21, plus the 1 centralized `ApprovalService` = 22. The "14" may have been an earlier count before full audit.

**Impact**: The `IDocumentStatusLogger` injection must cover **all 22 writer sites**, not 14. The task breakdown must enumerate every handler individually.

**Alternatives considered**: None — the codebase is the source of truth.

## R2: PaymentOrder Inline Approval Columns

**Decision**: PaymentOrder entity currently has **no inline approval columns** (ApprovedById, ApprovedAt, RejectedById, RejectedAt, RejectionReason, CancelledById, CancelledAt, CancellationReason, VoidedById, VoidedAt, VoidReason). These columns do not exist in the current entity.

**Rationale**: The entity only carries `Status` (PaymentOrderStatus enum) and `BudgetCheckStatus`. Approval is tracked exclusively via `ApprovalHistory` with `DocumentType = "PaymentOrder"`. The spec's "column drop" may refer to columns planned but never implemented, or to a different branch.

**Impact**: FR-012 (drop columns) becomes a **no-op** in the migration. The migration script should verify these columns don't exist and fail-fast if they do (defensive check). The `ApprovePaymentOrderCommand` handler refactor (FR-013) still applies — it currently uses `IApprovalService` and needs `IDocumentStatusLogger` injection.

**Alternatives considered**: Skip the column-drop migration entirely. Rejected — defensive verification is cheap and protects against schema drift.

## R3: Supplier Entity Field Mapping to Party

**Decision**: Direct field mapping from Supplier to Party with the following transformation:

| Supplier Field | Party Field | Transform |
|----------------|-------------|-----------|
| SupplierCode | PartyCode | Re-generate via PTY sequence |
| SupplierName | NameAr | Direct copy |
| SupplierNameEn | NameEn | Direct copy |
| SupplierType | PartyType | Map to enum (Supplier) |
| CommercialRegistrationNumber | TaxNumber | Direct copy |
| ContactPerson | Notes | Append to Notes |
| ContactEmail | Email | Direct copy |
| ContactPhone | Phone | Direct copy |
| Address + City | Address | Concatenate |
| Notes | Notes | Append (with ContactPerson if present) |
| IsActive | IsActive | Direct copy |
| IndustryClassification | Notes | Append to Notes |
| CurrencyCode | (dropped) | Not Party-relevant |

**Rationale**: Supplier has fields not on Party (IndustryClassification, CurrencyCode, City separate from Address). These are captured in Notes during migration. Party is a superset of the Party-relevant Supplier fields.

**Alternatives considered**: Create additional Party fields for IndustryClassification. Rejected — out of scope for v1; can be added later if needed.

## R4: DocumentSequenceService PrefixMap Extension

**Decision**: Add 5 new prefixes to the existing `PrefixMap` dictionary:

| DocumentType | Prefix | Notes |
|--------------|--------|-------|
| `Party` | `PTY` | New entity |
| `ReceiptVoucher` | `RCV` | New entity (future module) |
| `DepositSlip` | `DSL` | New entity (future module) |
| `DisbursementRequest` | `DSB` | New entity (future module) |
| `Payment` | `PAY` | New — distinct from existing `PaymentOrder`→`PO` |

**Rationale**: The PrefixMap uses `StringComparer.OrdinalIgnoreCase`. Existing entries show `"PaymentOrder" → "PO"` and `"PurchaseOrder" → "PO"` share a prefix (known collision). `"Payment" → "PAY"` is intentionally different from `"PaymentOrder" → "PO"`.

**Alternatives considered**: Use `"PY"` instead of `"PAY"`. Rejected — 3-letter prefixes are the established convention.

## R5: IDocumentStatusLogger Design

**Decision**: Create `IDocumentStatusLogger` as a thin service injected into all 22 writer sites. Interface:

```csharp
public interface IDocumentStatusLogger
{
    Task LogAsync(string entityName, int documentId, string fromStatus, string toStatus, int changedById, string? reason, CancellationToken ct);
}
```

**Rationale**: 
- The 21 budgeting handlers currently insert `ApprovalHistory` directly. They don't have a `DocumentStatusLog` concept yet.
- The 21 budgeting handlers currently use `RequiredRole = string.Empty`. The spec requires real RequiredRole — but this is a separate concern from status logging.
- The `IDocumentStatusLogger` handles only the `DocumentStatusLog` insert, not the `ApprovalHistory` insert. Each writer site does both.

**Alternatives considered**: Extend `IApprovalService` to also write status logs. Rejected — would couple approval validation (role checking) with status logging (audit trail); they are separate concerns.

## R6: Attachment Gate Scope

**Decision**: The attachment gate applies to `Submitted→Approved` transitions only. Currently, only `ApproveBudgetCommand`, `ApproveAppropriationCommand`, `ApproveEncumbranceCommand`, and `ApprovePaymentOrderCommand` perform approval transitions. The gate must be wired into these 4 handlers.

**Rationale**: The spec explicitly states "Submitted→Approved transitions" and "Wire into Budgets + Appropriations now; other modules adopt in their specs." Encumbrance approval is implicitly included since it follows the same pattern.

**Alternatives considered**: Wire into all state transitions. Rejected — spec is explicit about Submitted→Approved only.

## R7: Module Registry Change (Decision Record Required)

**Decision**: Deleting the Suppliers module requires a decision record per Constitution Principle XII and the Module Registry binding constraint.

**Rationale**: The Module Registry lists "Suppliers" as a registered module. Removing it is a structural change. The Party module replaces it entirely.

**Impact**: A decision record (DR-001) must be created in `docs/decision-records/` before implementation begins. The record should document:
- Rationale: Unified Party absorbs all Supplier functionality
- Scope: Delete `src/Domain/Suppliers/`, remove `Suppliers` DbSet from `IApplicationDbContext`, remove `Suppliers` configuration
- Migration: Row-for-row migration to Parties table
- Remediation: None needed — Party is the replacement

## R8: Supplier FK References to Migrate

**Decision**: Migrate 5 FK reference sites:

| Entity | Field | Type | Navigation | Action |
|--------|-------|------|------------|--------|
| PaymentOrder | VendorId | int (required) | None | → VendorPartyId (FK → Parties) |
| Encumbrance | VendorId | int? (nullable) | None | → VendorPartyId (FK → Parties) |
| PurchaseOrder | SupplierId | int (required) | Supplier? | → SupplierPartyId (FK → Parties) |
| Quotation | SupplierId | int (required) | None | → PartyId (FK → Parties) |
| RFQSupplier | SupplierId | int (required) | None | → PartyId (FK → Parties) |

**Rationale**: Each entity uses a different field name convention. The migration preserves semantic meaning while standardizing on Party FKs.

**Alternatives considered**: Standardize all to `PartyId`. Rejected — `VendorPartyId` and `SupplierPartyId` preserve semantic intent (vendor vs supplier context).
