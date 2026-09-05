# Data Model: Remove Liquidations and BudgetLedgerEntries

**Feature**: 009-remove-liquidations-budgetledger
**Date**: 2026-09-04

## Summary

This feature removes 2 tables, drops 7 columns from 5 existing tables, and creates 2 deletion report tables. No new business entities are introduced.

## Tables Dropped

### Liquidations (Table 49)

**Before**: 35 columns including PK, 11 FKs, self-FK (ReversalOfId), 3 unique/indexed columns, RowVersion, audit fields.

**After**: Table deleted entirely.

**FK dependencies to resolve before drop**:
- PaymentOrders.LiquidationId → drop column first (FR-003)
- All other FKs are from Liquidations pointing outward (Restrict) — no inbound references after PaymentOrders column drop

### BudgetLedgerEntries (Table 50)

**Before**: 18 columns including bigint PK, 6 FKs, audit fields.

**After**: Table deleted entirely.

**FK dependencies**: Standalone — no other code references this table.

## Columns Dropped from Existing Tables

### PaymentOrders (FR-003)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| LiquidationId | int | NULLABLE | FK → Liquidations |

**Index dropped**: IX_PaymentOrders_LiquidationId

### Encumbrances (FR-004)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| LiquidatedAmount | decimal(23,2) | NOT NULL | Counter field, always 0 without Liquidation |
| LiquidationStatus | nvarchar(20) | NOT NULL | ReleaseStatus enum, always None without Liquidation |

### Budgets (FR-004)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| LiquidatedAmount | decimal(18,4) | NULLABLE | Counter field |

### BudgetItems (FR-004)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| LiquidatedAmount | decimal(18,4) | NULLABLE | Counter field |

### Appropriations (FR-004)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| LiquidatedAmount | decimal(18,4) | NOT NULL | Counter field |

## Tables Created (Migration Artifacts — Permanent)

### LiquidationDeletionReport

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int | NOT NULL | PK, identity |
| LiquidationNumber | nvarchar(50) | NOT NULL | |
| EncumbranceId | int | NOT NULL | |
| Amount | decimal(23,2) | NOT NULL | |
| Status | nvarchar(30) | NOT NULL | |
| CreatedAt | datetimeoffset | NOT NULL | |
| DeletedAt | datetimeoffset | NOT NULL | Default GETUTCDATE() |

**Population**: INSERT...SELECT from Liquidations before drop. Also captures PaymentOrders with non-null LiquidationId.

### BudgetLedgerEntryDeletionReport

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | long | NOT NULL | PK, identity |
| BudgetId | int | NOT NULL | |
| EntryType | nvarchar(30) | NOT NULL | |
| Amount | decimal(23,2) | NOT NULL | |
| Direction | nvarchar(10) | NOT NULL | |
| Status | nvarchar(30) | NOT NULL | |
| OccurredAt | datetimeoffset | NOT NULL | |
| DeletedAt | datetimeoffset | NOT NULL | Default GETUTCDATE() |

**Population**: INSERT...SELECT from BudgetLedgerEntries before drop.

## Entity Changes

### Encumbrance.cs
- Remove: `public decimal LiquidatedAmount { get; set; }` (line 26)
- Remove: `public Enums.ReleaseStatus LiquidationStatus { get; set; }` (line 30)
- Keep: `ReleaseStatus` field (used for encumbrance release tracking)

### Budget.cs
- Remove: `public decimal? LiquidatedAmount { get; set; }` (line 21)

### BudgetItem.cs
- Remove: `public decimal? LiquidatedAmount { get; set; }` (line 26)

### Appropriation.cs
- Remove: `public decimal LiquidatedAmount { get; set; }` (line 22)

### PaymentOrder.cs
- Remove: `public int? LiquidationId { get; set; }` (line 20)
- Remove: Liquidation navigation property (if present)

### ReleaseStatus.cs
- Update comment: remove "and Encumbrances.LiquidationStatus" from line 5

## Enums Deleted

| Enum | File | Values |
|------|------|--------|
| LiquidationStatus | Domain/Budgeting/Enums/LiquidationStatus.cs | Draft..Reversed (9 values) |
| LiquidationType | Domain/Budgeting/Enums/LiquidationType.cs | GoodsReceipt..Correction (5 values) |
| BudgetLedgerEntryType | Domain/Budgeting/Enums/BudgetLedgerEntryType.cs | Appropriation..Adjustment (5 values) |
| BudgetLedgerDirection | Domain/Budgeting/Enums/BudgetLedgerDirection.cs | Increase..Reverse (5 values) |
| BudgetLedgerStatus | Domain/Budgeting/Enums/BudgetLedgerStatus.cs | Pending..Failed (4 values) |

## Enums Retained

| Enum | Reason |
|------|--------|
| ReleaseStatus | Used by Encumbrance.ReleaseStatus |
| PaymentStatus | Used by PaymentOrder (not Liquidation-specific) |
| EncumbranceStatus | Independent of Liquidation |
| BudgetStatus | Independent of Liquidation |

## EF Configuration Changes

| Configuration | Change |
|---------------|--------|
| LiquidationConfiguration.cs | Delete entire file |
| BudgetLedgerEntryConfiguration.cs | Delete entire file |
| EncumbranceConfiguration.cs | Remove LiquidatedAmount (L38-40) and LiquidationStatus (L54-56) property mappings |
| BudgetConfiguration.cs | Remove LiquidatedAmount (L43-45) property mapping |
| BudgetItemConfiguration.cs | Remove LiquidatedAmount (L39-41) property mapping |
| AppropriationConfiguration.cs | Remove LiquidatedAmount (L39-41) property mapping |
| PaymentOrderConfiguration.cs | Remove LiquidationId index (L119) |

## DbContext Changes

| File | Change |
|------|--------|
| IApplicationDbContext.cs | Remove `DbSet<Liquidation> Liquidations` (L84) and `DbSet<BudgetLedgerEntry> BudgetLedgerEntries` (L85) |
| ApplicationDbContext.cs | Remove `DbSet<Liquidation> Liquidations` (L89) and `DbSet<BudgetLedgerEntry> BudgetLedgerEntries` (L90) |
