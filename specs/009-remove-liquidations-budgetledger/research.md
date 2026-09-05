# Research: Remove Liquidations and BudgetLedgerEntries

**Feature**: 009-remove-liquidations-budgetledger
**Date**: 2026-09-04

## R1: BudgetLedgerEntry removal scope — cascade impact on other commands

**Decision**: Remove all BudgetLedgerEntry creation code from 6 non-Liquidation commands while preserving their core business logic.

**Rationale**: BudgetLedgerEntry is a write-only audit ledger with zero consumers. Its creation code exists in 6 commands (ApproveEncumbrance, ReleaseEncumbrance, ReverseEncumbrance, CancelEncumbrance, ApproveAppropriation, ReverseAppropriation). Removing the ledger entry creation does not affect financial calculations — those commands independently update Appropriation/Budget/BudgetItem amount fields. The ledger was a redundant audit trail alongside the existing SecurityAuditLog and entity-change audit.

**Alternatives considered**:
- Keep BudgetLedgerEntry but only remove Liquidation references: Rejected — the entire table is being dropped per spec. No point keeping creation code for a deleted table.
- Replace BudgetLedgerEntry with a different audit mechanism: Rejected — SecurityAuditLog already provides audit trail per Constitution Principle VIII.

**Impact on commands** (extracted BudgetLedgerEntry creation blocks to remove):
| Command | Lines to remove | Business logic preserved |
|---------|----------------|------------------------|
| ApproveEncumbranceCommand | L38-57 (ledger add) | Status change, Appropriation/Budget/BudgetItem amount updates |
| ReleaseEncumbranceCommand | L56-75 (ledger add) | ReleasedAmount/ReleaseStatus updates, Appropriation/Budget/BudgetItem amount updates |
| ReverseEncumbranceCommand | L54-73 (ledger add) | IsReversed/Status change, release amounts, Appropriation/Budget/BudgetItem amount updates |
| CancelEncumbranceCommand | L50-69 (ledger add) | LiquidatedAmount guard removal, status change, release amounts, Appropriation/Budget/BudgetItem amount updates |
| ApproveAppropriationCommand | L48-66 (ledger add) | Status change, budget check, Budget/BudgetItem amount updates |
| ReverseAppropriationCommand | L46-64 (ledger add) | IsReversed/Status change, Budget/BudgetItem amount updates |

## R2: CancelEncumbrance LiquidatedAmount guard

**Decision**: Remove the `if (entity.LiquidatedAmount > 0)` guard entirely. Cancel now relies only on EncumbranceStatus (Draft/Active) checks.

**Rationale**: The LiquidatedAmount field is being dropped. After removal, the guard would reference a non-existent property. The existing EncumbranceStatus check (only Draft or Active can be cancelled) is sufficient — an Active encumbrance that was released would have Status changed, preventing cancellation.

**Alternatives considered**:
- Replace guard with ReleasedAmount > 0 check: Rejected — cancellation of released encumbrances is already handled by the release amount logic in the same command. No new guard needed.

## R3: BudgetLedgerEntryType enum — Liquidation value

**Decision**: Delete the entire BudgetLedgerEntryType.cs file (along with BudgetLedgerDirection.cs and BudgetLedgerStatus.cs).

**Rationale**: These enums are exclusively used by BudgetLedgerEntry. With the table and all creation code removed, the enums have zero consumers. The Liquidation value (2) in BudgetLedgerEntryType is dead code.

**Alternatives considered**:
- Keep enums for future use: Rejected — YAGNI. If a ledger is reintroduced, new enums can be created.

## R4: ReleaseStatus enum — comment update

**Decision**: Update the comment in ReleaseStatus.cs from "Used by Encumbrances.ReleaseStatus and Encumbrances.LiquidationStatus" to "Used by Encumbrances.ReleaseStatus".

**Rationale**: After removing the LiquidationStatus property from Encumbrance, ReleaseStatus is only used by the ReleaseStatus field on Encumbrance. The enum itself remains — it's a live field.

## R5: EF Core migration strategy

**Decision**: Generate a single new EF Core migration that drops tables, columns, and adds deletion report tables. Do NOT modify historical migration files.

**Rationale**: Constitution Principle VI requires versioned, reviewable migrations. The InitialCreate migration (5930 lines) is immutable. The new migration will:
1. Create [LiquidationDeletionReport] and [BudgetLedgerEntryDeletionReport] tables
2. Populate them via INSERT...SELECT from source tables
3. Log SecurityAuditLog entries
4. Drop Liquidations table (cascades FKs via Restrict — need manual column drops first)
5. Drop BudgetLedgerEntries table
6. Drop PaymentOrders.LiquidationId column + index
7. Drop counter columns from Encumbrances, Budgets, BudgetItems, Appropriations

**EF Core model snapshot** must be updated to reflect all deletions. The migration Down() must recreate everything.

## R6: Deletion report table schemas

**Decision**: LiquidationDeletionReport captures columns per spec FR-001. BudgetLedgerEntryDeletionReport follows the same pattern.

**LiquidationDeletionReport columns**:
- Id (int, PK, identity)
- LiquidationNumber (nvarchar(50))
- EncumbranceId (int)
- Amount (decimal(23,2))
- Status (nvarchar(30))
- CreatedAt (datetimeoffset)
- DeletedAt (datetimeoffset, default GETUTCDATE())

**BudgetLedgerEntryDeletionReport columns**:
- Id (long, PK, identity)
- BudgetId (int)
- EntryType (nvarchar(30))
- Amount (decimal(23,2))
- Direction (nvarchar(10))
- Status (nvarchar(30))
- OccurredAt (datetimeoffset)
- DeletedAt (datetimeoffset, default GETUTCDATE())

## R7: Constitution Principle V amendment

**Decision**: Amend Principle V chain from "Budget → Appropriation → Encumbrance → Liquidation → Payment" to "Budget → Appropriation → Encumbrance → Payment". Create decision record per Principle XII.

**Impact**: This is a MINOR version amendment (1.0.0 → 1.1.0) per Constitution versioning rules — material expansion of existing principle by removal. The chain reduction is a simplification, not a redefinition.

**Decision record scope**: Rationale (BF-004 removed), scope (2 tables, 6 columns, ~48 files), migration plan, BF-004 removal from feature registry (12 → 11 business features).

## R8: Permission cleanup — no SecurityPermission seed rows to delete

**Decision**: No dedicated SecurityPermission seed rows exist for Liquidations or BudgetLedgerEntries. Permissions are generated dynamically by the module-level code generation pattern in SecurityPermissionSeedData.cs.

**Impact**: Only PermissionCodes constants, AddPolicy registrations, and RolePermissionSeedData references need removal. No seed DELETE needed — the permissions were never persisted as standalone seed rows.

## R9: DocumentSequence cleanup

**Decision**: Remove "Liquidation" → "LIQ" mapping from DocumentSequenceService.PrefixMap. No DocumentSequence seed data exists for Liquidation (only 8 document types are seeded).

**Impact**: The prefix map goes from 10 entries to 9. DocumentSequenceServiceTests must be updated (remove Liquidation assertions, update count assertion from 10 to 9).
