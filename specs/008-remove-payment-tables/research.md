# Research: Remove Payment Sub-Entity Tables and Convert PaymentMethod to Enum

**Date**: 2026-09-03
**Feature**: 008-remove-payment-tables

## R1: FK Constraint Analysis for Table Drops

**Decision**: Follow explicit drop order: PaymentAllocations → PaymentExecutions → (PaymentMethods, AdvancePayments) → PaymentOrders. PaymentMethods and AdvancePayments have no inbound FKs and can be dropped in any order.

**Rationale**: EF Core snapshot reveals 4 Restrict FK constraints:
1. `FK_PaymentAllocations_PaymentExecutions_PaymentExecutionId` — PaymentAllocations depends on PaymentExecutions
2. `FK_PaymentExecutions_PaymentOrders_PaymentOrderId` — PaymentExecutions depends on PaymentOrders
3. `FK_PaymentOrderDeductions_PaymentOrders_PaymentOrderId` — (kept entity, but blocks PaymentOrders drop if not handled)
4. `FK_PaymentOrderLines_PaymentOrders_PaymentOrderId` — (kept entity, same)

Since PaymentOrderDeductions and PaymentOrderLines are **kept**, we must NOT drop PaymentOrders. The FK constraints from PaymentAllocations and PaymentExecutions to PaymentOrders will be resolved by dropping the child tables first, not the parent.

**Alternatives considered**:
- Dropping all FK constraints first, then tables in any order — rejected because it's more migration steps with no benefit
- Using cascade delete — rejected per Constitution Principle VI (Restrict only)

## R2: PaymentMethodId Has No Database FK Constraint

**Decision**: PaymentMethodId columns in PaymentOrders, PaymentExecutions, and RevenueReceipts are **bare integer columns with no FK constraint**. Only index-only. This simplifies the migration — no FK to drop, just the index and column.

**Rationale**: The entity classes have `int PaymentMethodId` but no navigation property to PaymentMethod. Configurations only create `HasIndex()`, not `HasOne()`. The application validates via `FindAsync` in command handlers, not database constraints.

**Alternatives considered**: N/A — this is a discovery, not a decision.

## R3: PaymentMethod Entity Code → Enum Mapping

**Decision**: The PaymentMethod entity has a `string Code` property. Map each unique Code value to the corresponding enum int:
- Code="Cash" → 0
- Code="BankTransfer" → 1
- Code="Check" → 2
- Code="CreditCard" → 3
- Code="WireTransfer" → 4
- Code="Other" → 5

Unknown/missing Code → default to 5 (Other) with warning in PaymentMethodMigrationReport.

**Rationale**: The entity stores `Code` as a string. The migration must query `PaymentMethods` table to get the Code for each `PaymentMethodId`, then map to enum int. Since there are no FK constraints, the migration uses raw SQL or EF to look up the mapping.

**Alternatives considered**:
- Using the entity's `Name` field for mapping — rejected because Code is the canonical identifier
- Hardcoding the mapping in migration — acceptable since the enum values are fixed and specified in the requirement

## R4: Migration Pattern

**Decision**: Use EF Core scaffolded migration with raw SQL for data manipulation steps (backfill, report generation, audit logging). The migration follows the standard pattern: `Up(MigrationBuilder migrationBuilder)` / `Down(MigrationBuilder migrationBuilder)`.

**Rationale**: The most recent migration `20260902230111_AddFinancialReports` establishes the pattern. The migration name will be `20260903HHMMSS_RemovePaymentSubEntities`. Raw SQL is needed for:
- Creating deletion report tables with `SELECT INTO`
- Backfilling enum columns with `UPDATE ... SET`
- Inserting SecurityAuditLog entries
- Deleting permission rows

**Alternatives considered**:
- Separate migration for schema vs data — rejected because the operation must be atomic
- Custom migration runner — rejected; EF Core migration with raw SQL is sufficient

## R5: DTO PaymentMethod Exposure

**Decision**: Replace `int PaymentMethodId` with `string PaymentMethod` (enum name) and `int PaymentMethodValue` (enum int) in all DTOs. The frontend receives the enum name for display.

**Rationale**: The spec requires "PaymentMethod as enum name + int value." The current DTOs only expose `int PaymentMethodId`. After migration, there's no lookup table to join, so the DTO must carry both the display name and the underlying int value.

**Alternatives considered**:
- Only exposing the enum name — rejected because the int value is needed for round-trip (create/update)
- Only exposing the int value — rejected because the spec requires name display
- Using a dedicated DTO property with `[JsonConverter]` — acceptable but more complex; keeping both properties is simpler

## R6: PermissionCleanup Strategy

**Decision**: Delete SecurityPermission rows where Code starts with the 4 prefixes, then delete the corresponding PermissionCodes constants from code, then remove AddPolicy lines from DependencyInjection.cs.

**Rationale**: The 18 permissions are:
- PaymentMethods: View, Create, Update, Activate, Deactivate (5)
- PaymentExecutions: View, Create, Approve, Send, Complete (5)
- PaymentAllocations: View, Create, Confirm, Reverse (4)
- AdvancePayments: View, Create, Approve, Consume, Cancel (5)
Total: 19 (not 18 as stated in spec — spec has a minor count error, but the list is correct)

Wait — re-counting from the exploration: PaymentAllocations has View, Create, Confirm, Reverse = 4. But the user's original input listed only 3 for PaymentAllocations: Create, Confirm, Reverse. The PermissionCodes file shows 4: View, Create, Confirm, Reverse. Using the actual PermissionCodes file as source of truth: 5+5+4+5 = 19 permissions.

**Alternatives considered**:
- Cascade delete via RolePermission FK — acceptable if FK exists, but manual delete is safer
- Soft-delete permissions — rejected; these are being removed entirely

## R7: Reversibility Strategy

**Decision**: The Down() migration must:
1. Recreate all 4 tables with original schema
2. Restore seed data for PaymentMethods
3. Convert enum columns back to FK int via lookup
4. Restore DocumentSequence rows
5. Restore SecurityPermission rows
6. Restore PermissionCodes constants and AddPolicy lines

**Rationale**: Constitution Principle VI requires reviewable migrations. Full reversibility means the Down() must be a complete mirror of Up(). Since deletion report tables are temporary artifacts, they should be dropped in Down() after data is restored to the original tables.

**Alternatives considered**:
- Irreversible migration — rejected per Constitution
- Partial reversibility (schema only, no data) — rejected; the spec requires full reversibility

## R8: Frontend Impact

**Decision**: No frontend changes required. No payment-related pages or components exist in the React SPA. The `features/` directory has no `payments/` folder. The only change is in the API contract (endpoints removed), which the frontend will reflect when the OpenAPI client is regenerated.

**Rationale**: The frontend codebase has no payment feature implementation. Removing backend endpoints will cause the generated client types to update automatically on next generation.

**Alternatives considered**: N/A
