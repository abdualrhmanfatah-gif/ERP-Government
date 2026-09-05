# DEP-021: Remove Payment Sub-Entity API Endpoints

**Date**: 2026-09-03
**Status**: Accepted
**Deciders**: Engineering Team

## Context

The Payments module contains 4 tables (AdvancePayments, PaymentExecutions, PaymentAllocations, PaymentMethods) being removed as part of feature 008-remove-payment-tables. Removing these tables requires removing their corresponding API endpoints:

- `PaymentMethods` CRUD endpoints
- `PaymentExecutions` CRUD endpoints
- `PaymentAllocations` CRUD endpoints
- `AdvancePayments` CRUD endpoints

Per Constitution Principle IX, removing endpoints is a breaking API change requiring a decision record.

## Decision

Remove the 4 endpoint files and their backing use cases. The PaymentMethod lookup is replaced by an enum. AdvancePayments, PaymentExecutions, and PaymentAllocations functionality is covered by existing PaymentOrder state machine and Moves pipeline.

## Consequences

- **Breaking change**: API consumers relying on these endpoints will break
- **Frontend**: No payment frontend pages exist yet, so no frontend impact
- **Mitigation**: Migration handles all data preservation (deletion reports)
- **Reversibility**: Migration Down() restores all endpoints and data

## Alternatives Considered

- Deprecation period: Rejected because no consumers exist yet
- Redirect to alternative endpoints: Rejected because functionality is being consolidated, not moved
