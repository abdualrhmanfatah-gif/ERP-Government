# DEP-022: Delete Payment Sub-Entity Tables

**Date**: 2026-09-03
**Status**: Accepted
**Deciders**: Engineering Team

## Context

The Payments module contains 4 tables that add unnecessary complexity:

- **AdvancePayments**: Separate lifecycle duplicating PaymentOrder + Moves
- **PaymentExecutions**: Separate lifecycle duplicating PaymentOrder state machine
- **PaymentAllocations**: Transient allocations with no lasting business meaning
- **PaymentMethods**: Lookup table for 6 fixed values (should be enum)

Per Constitution Binding Constraints, "Adding, removing, splitting, or merging a module requires a decision record." While this is table deletion within an existing module (not module removal), a decision record is recommended for traceability.

## Decision

Delete all 4 tables. Convert PaymentMethod to an enum. PaymentOrder and RevenueReceipt store PaymentMethod as an integer enum column instead of a foreign key.

## Consequences

- **Schema simplification**: 4 fewer tables, 19 fewer permissions, 4 fewer endpoints
- **Data preservation**: Deletion reports generated for AdvancePayments and PaymentExecutions
- **Audit trail**: SecurityAuditLog entries for all table deletions
- **Reversibility**: Full migration reversibility including data restoration
- **No business logic change**: PaymentOrder FSM and Moves pipeline unaffected
