# Research: Accounting Core Refactor

**Feature**: 014-accounting-core-refactor
**Date**: 2026-09-05

## R1: Move → JournalEntry Rename Strategy

**Decision**: Solution-wide rename using find-and-replace with word-boundary matching, followed by EF migration to rename database tables and columns.

**Rationale**: The existing entities `Move.cs` and `MoveLine.cs` have clear boundaries — they live in `Domain/Accounting/Entities/` and are referenced by name in configurations, services, handlers, controllers, DTOs, and the client app. A systematic rename is safer than creating new entities and migrating data.

**Alternatives considered**:
- Create new JournalEntry entities alongside Move, migrate data, then delete Move — rejected because it doubles migration complexity and risks data loss during the transition
- Use database views to alias old table names — rejected because it violates the "zero stale references" constraint (FR-019)

**Migration approach**: EF migration with `RenameTable`/`RenameColumn` operations. The `Move.cs` entity already has `RowVersion` for optimistic concurrency (FR-021).

## R2: EntryStatus String → Enum Conversion

**Decision**: Create `EntryStatus` enum (Draft=0, Posted=1, Reversed=2) with EF value conversion. Migration converts existing string values via a data migration step.

**Rationale**: The current `Move.EntryStatus` is `string` with default `"Draft"`. EF Core value conversion handles the string↔enum mapping transparently. Existing values ("Draft", "Posted", "Reversed") map directly to enum ordinals.

**Alternatives considered**:
- Keep as string with a CHECK constraint — rejected because the spec explicitly requires enum conversion (FR-004)
- Use int-backed enum with a lookup table — rejected as over-engineered for three fixed values

**Migration data step**: UPDATE statements to normalize any variant casing (e.g., "draft" → "Draft") before applying the column type change.

## R3: AccountingEvent Source Document Composite Key

**Decision**: Three-column unique constraint: (EventType, SourceDocumentType, SourceDocumentId).

**Rationale**: Clarified in spec session 2026-09-05. The existing `AccountingEvent` already has `SourceTable` (string) and `SourceId` (int) — these become `SourceDocumentType` and `SourceDocumentId`. `EventType` changes from string to enum.

**Alternatives considered**:
- Two-column (EventType, SourceDocumentId) — rejected because SourceDocumentId alone is not globally unique across entity types
- Single-column on SourceDocumentId — rejected for same reason

**Current entity fields**: `SourceTable` → rename to `SourceDocumentType`; `SourceId` → rename to `SourceDocumentId`; add `JournalEntryId` (nullable FK); add `EventCategory`, `EventType` (enum), `Status` (renamed from `AccountingEventStatus`).

## R4: PaymentOrder Aggregate Stripping

**Decision**: Remove 7 stored aggregate fields and 8 inline approval columns from `PaymentOrder`. Remove 4 computed fields from `PaymentOrderLine`. Remove `BaseAmount` from `PaymentOrderDeduction`. Expose computed totals via a new GET `/{id}/totals` endpoint.

**Rationale**: Constitution rule 23 mandates zero stored computed fields in the Payments domain. ApprovalHistory (Command 1) is the sole approval source.

**Fields to drop from PaymentOrder**:
- Aggregates: AmountNet, BaseAmountNet, TotalDeductionAmount, TotalNetAmount, TotalPaidAmount, TotalRemainingAmount, IsFullyPaid
- Approvals: ApprovedById, ApprovedAt, RejectedById, RejectedAt, CancelledById, CancelledAt, VoidedById, VoidedAt

**Fields to keep**: AmountGross, DeductionAmount (entered planning inputs), all non-computed fields (PaymentOrderNumber, dates, vendor, fund, etc.)

**Computed totals formula**:
- Net = AmountGross − SUM(lines deductions)
- Paid = SUM(completed Payments linked to this order)
- Remaining = Net − Paid

## R5: PaymentMethod InKind Addition

**Decision**: Add `InKind = 5` to existing enum, shift `Other` from 5 to 6.

**Rationale**: Current enum has Cash(0), BankTransfer(1), Check(2), CreditCard(3), WireTransfer(4), Other(5). Adding InKind(5) and renumbering Other(6) preserves backward compatibility for stored integer values if no InKind payments exist yet.

**Alternatives considered**:
- Add InKind as 6 without renumbering — rejected because it creates a gap (5 is unused until Other is renumbered)
- Use string-backed enum — rejected because existing data is integer-backed

**Migration concern**: If any rows already have PaymentMethod=5 (Other), the renumber would corrupt them. Check data first; if rows exist, add InKind as 6 instead.

## R6: Analytic Dimensions on JournalEntryLine

**Decision**: Add five nullable FK columns to JournalEntryLine: FundId, ProjectId, BudgetItemId, EncumbranceId, PaymentOrderId. Add CHECK constraint `NOT (Debit > 0 AND Credit > 0)`.

**Rationale**: These dimensions enable cross-dimensional reporting and feed the availability engine (Command 0). All nullable because not every line needs all dimensions.

**FK targets**: Fund, Project, BudgetItem, Encumbrance, PaymentOrder — all exist in the domain model (per spec, Command 3 adds ReceiptVoucherId later).

**Validation**: FluentValidation rule enforces XOR constraint at application level; database CHECK constraint enforces at storage level.

## R7: Optimistic Concurrency on JournalEntry Posting

**Decision**: Use existing `RowVersion` byte[] property on `Move.cs` (becomes `JournalEntry.cs`) for optimistic concurrency. EF Core handles concurrency token comparison automatically.

**Rationale**: The entity already has `RowVersion`. EF Core throws `DbUpdateConcurrencyException` when the token mismatches. The handler catches this and returns a conflict error to the user.

**Alternatives considered**:
- Pessimistic locking — rejected because it degrades throughput for a read-heavy accounting system
- Last-write-wins — rejected because it risks silent data loss in financial systems

## R8: Reversal Pattern

**Decision**: Add `ReversalOfId` (nullable FK to self) on JournalEntry if not already present. The existing `Move.cs` already has `ReversalOfMoveId` — rename to `ReversalOfId`. Add `ReversalReason` (already exists).

**Rationale**: The entity already supports the reversal pattern via `ReversalOfMoveId`. The rename to `ReversalOfId` aligns with the new naming convention. No new columns needed — just rename.
