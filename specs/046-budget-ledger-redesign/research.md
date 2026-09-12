# Research: Budget Preparation — BudgetItemAllocations Model

**Date**: 2026-09-10 | **Feature**: 046-budget-ledger-redesign

## R1: BudgetItemAllocations Entity Design

**Decision**: New entity `BudgetItemAllocation` with fields: Id, BudgetId, BudgetItemId, ProposedAmount, ApprovedAmount, Remarks, audit fields, RowVersion. UNIQUE(BudgetId, BudgetItemId).

**Rationale**: The allocation is the central entity for budget preparation. It captures the proposed and approved amounts per budget item, with freezing at approval time. The uniqueness constraint prevents duplicate items within a budget.

**Alternatives considered**:
- Storing allocations on BudgetItem directly: rejected because BudgetItem is shared across budgets (same item code in different budgets).
- Using BudgetTransactionLines for amounts: rejected because the user explicitly wants per-allocation tracking separate from transactions.

## R2: ActualExpenditure Computation Source

**Decision**: Compute from posted JournalEntryLines via AccountId linked to BudgetItem. Filter by FiscalYearId, exclude cancelled and reversed entries.

**Rationale**: Constitution Principle IV requires posted entries to be the financial system of record. BudgetTransaction amounts and PaymentOrder amounts are not reliable sources for actual expenditure. The GL account link provides traceability.

**Alternatives considered**:
- Using PaymentOrder amounts: rejected per user requirement (FR-023).
- Using BudgetTransaction net amounts: rejected per user requirement (FR-023).
- Adding BudgetItemId to JournalEntryLine: noted as possible future enhancement but not required for v1 (assumption in spec).

## R3: Shared GL Account Prevention

**Decision**: Prevent linking the same BudgetItem.AccountId to multiple budget items with independent allocations within the same budget and fiscal year. Validate at allocation creation/update time.

**Rationale**: When multiple allocations share an account, expenditure attribution becomes ambiguous without a BudgetItemId on JournalEntryLine. Prevention is simpler and more reliable than runtime attribution logic.

**Alternatives considered**:
- Auto-split expenditure proportionally: rejected as error-prone and not auditable.
- Require BudgetItemId on JournalEntryLine: requires changes to Accounting module, deferred.
- Flag for manual attribution: adds operational burden without clear resolution path.

## R4: BudgetTransaction Per-Allocation Model

**Decision**: Each BudgetTransaction carries BudgetItemAllocationId (required FK). BudgetTransactionLines table is removed. Existing BudgetTransactionLines data migrated to BudgetTransaction records.

**Rationale**: Simplifies the transaction model — each transaction affects exactly one allocation. Eliminates multi-line complexity and ensures atomic impact on ApprovedAmount.

**Alternatives considered**:
- Keep BudgetTransactionLines: rejected per user requirement.
- Use BudgetTransactionLine for single-line transactions: adds unnecessary complexity.

## R5: Transfer Type Removal

**Decision**: Remove Transfer=3 from BudgetTransactionType as a createable option. Preserve the stored int value to avoid renumbering. Fund movement uses Reduction + Supplement against separate allocations.

**Rationale**: User explicitly requested Transfer removal. Preserving the enum value avoids breaking existing database rows.

**Alternatives considered**:
- Remove enum value entirely: risks renumbering and breaking existing data.
- Keep Transfer but deprecate: same as chosen approach.

## R6: ApprovedAmount Floor Protection

**Decision**: Block posting if decrease would drive ApprovedAmount below zero. ApprovedAmount must always be >= 0.

**Rationale**: Negative ApprovedAmount would indicate an invalid state. Budget control should prevent overspend at the allocation level during transaction posting.

**Alternatives considered**:
- Allow negative ApprovedAmount: simpler but weakens control.
- Check only at AvailableAmount level: allows invalid ApprovedAmount state.

## R7: New Allocations After Active Status

**Decision**: New allocations can be added via Supplement BudgetTransaction. The Supplement follows standard lifecycle and is subject to budget control checks. On posting, the new allocation's ApprovedAmount is set to the Supplement amount.

**Rationale**: Allows budget expansion during the fiscal year while maintaining control and audit trail.

**Alternatives considered**:
- Freeze allocations at approval: simplest but inflexible.
- Direct add in Active: bypasses approval workflow.

## R8: Budget Close vs Cancel

**Decision**: Close preserves allocations and expenditure records, blocks new operations, no automatic reversal. Cancel allowed only if no posted transactions or outstanding commitments exist.

**Rationale**: Close is informational (budget period ended). Cancel is destructive (budget never existed). Different semantics require different guards.

**Alternatives considered**:
- Auto-reverse on close: generates many reversals, may not be desired.
- No enforcement on close: weakens budget control.

## R9: Existing Codebase Patterns

**Decision**: Follow established patterns in src/Domain/Budgeting/, src/Application/Budgeting/, src/Web/Endpoints/Budgeting/. Use BaseAuditableEntity for new entities. Use IEndpointGroup for endpoints. Use FluentValidation for commands. Use IDocumentStatusLogger for status transitions.

**Rationale**: Consistency with existing codebase reduces cognitive load and maintenance burden.

**Alternatives considered**: None — patterns are established and verified.
