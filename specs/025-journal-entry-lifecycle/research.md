# Research: Journal Entry Lifecycle Screens

**Date**: 2026-09-06
**Feature**: 025-journal-entry-lifecycle

## Decision 1: Lifecycle Action Pattern (POST vs PATCH)

**Decision**: Use POST-style lifecycle actions via NSwag client methods (`submitPOST3`, `approvePOST7`, `post`, `reversePOST2`, `cancelPOST6`).

**Rationale**: The backend exposes POST endpoints for all lifecycle transitions (`/{id}/submit`, `/{id}/approve`, `/{id}/post`, `/{id}/reverse`, `/{id}/cancel`). The budgeting `LifecycleActions` component uses PATCH-style transitions which are NOT applicable here.

**Alternatives considered**:
- PATCH-style transitions (budgeting pattern): Rejected — backend uses POST for journal entries.
- Raw fetch calls: Rejected — NSwag client provides type safety.

## Decision 2: State Machine Enforcement

**Decision**: Mirror the six-state machine in the UI for action rendering only. Server is the sole authority for state transitions.

**Rationale**: Constitution Principle III requires server-side enforcement. The UI renders actions based on the current state to prevent invalid transitions from being clickable, but every action is validated server-side.

**State machine**:
```
Draft →(submit)→ Submitted →(approve)→ Approved →(post)→ Posted →(reverse)→ Reversed
Draft|Submitted →(cancel)→ Cancelled
```

**Guard rules**:
- Submit: ≥1 line + balanced (total debit = total credit)
- Approve: status = Submitted
- Post: status = Approved + period unlocked + documentDate within period
- Reverse: status = Posted + period unlocked + year Open + period not finalized
- Cancel: status = Draft or Submitted

## Decision 3: Balance Validation

**Decision**: Client-side balance validation as UI convenience; server is authoritative.

**Rationale**: Live balance totals improve UX by showing unbalanced state immediately. Server validates on submit/post/reject.

**XOR enforcement**:
- Both debit and credit zero → invalid line (blocked client-side)
- Both debit and credit non-zero → violates XOR rule (blocked client-side)
- Exactly one non-zero → valid

## Decision 4: RowVersion Conflict Handling

**Decision**: Surface conflict with toast message and refetch offer.

**Rationale**: Constitution Principle VI requires optimistic concurrency. On 409 Conflict, show toast with "Entry was modified by another user" and offer refetch button to reload the entry.

## Decision 5: System-Generated Entries

**Decision**: Display system badge, render read-only, suppress all lifecycle actions.

**Rationale**: FR-007 requires system-generated entries to be read-only. System entries originate from accounting events (disbursement/treasury posting) and should not be manually modified.

## Decision 6: Reversal Linkage Display

**Decision**: Bidirectional display — original shows reversal (with reason), reversal shows origin.

**Rationale**: FR-013 requires bidirectional visibility. The `reversalOfId` and `reversalReason` fields on the DTO support this.

## Decision 7: Existing Component Reuse

**Decision**: Reuse `ApprovalsPanel` and `StatusLogPanel` from documents feature. Create new `EntryLifecycleActions` component (not reuse budgeting `LifecycleActions`).

**Rationale**: `ApprovalsPanel` and `StatusLogPanel` are generic and accept `documentType` + `documentId` props. `LifecycleActions` uses PATCH-style which doesn't match journal entry POST endpoints.

## Decision 8: Feature Folder Organization

**Decision**: Rebuild within existing `features/accounting/` folder, replacing stale pages/components.

**Rationale**: The accounting feature already has the correct folder structure. Journal entry pages are stale and broken against the renamed API. Replacing them is the cleanest approach.

## Decision 9: Dimension Pickers

**Decision**: Keep existing `DimensionPickers` component. It already loads from FundsClient, BudgetItemsClient, and Projects hook.

**Rationale**: Dimension pickers are already functional and load the correct data from NSwag clients. No changes needed.
