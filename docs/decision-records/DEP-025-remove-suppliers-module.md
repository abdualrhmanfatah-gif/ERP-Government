# DEP-025: Remove Suppliers Module — Unified Party Entity

**Date**: 2026-09-05
**Status**: Accepted
**Deciders**: Engineering Team

## Context

The current Suppliers module contains a single `Supplier` entity with no application layer, no endpoints, and no use cases. It serves as a vendor master referenced by PaymentOrder (VendorId), Encumbrance (VendorId), PurchaseOrder (SupplierId), Quotation (SupplierId), and RFQSupplier (SupplierId).

A unified `Party` entity is being introduced to absorb all Supplier functionality and serve as the canonical party record for the ERP system (Supplier, Customer, GovEntity, TaxAuthority, Other). Maintaining two parallel entities creates data inconsistency and migration complexity.

**Constitution Principles Affected**: XII (Controlled Architectural Change), VI (Data Integrity — module registry constraint)

## Decision

1. Create a new `Parties` bounded context under `src/Domain/Parties/` with a `Party` entity and `PartyType` enum.
2. Migrate all Supplier records to the Parties table with PartyType=Supplier, generating PTY-prefixed codes.
3. Replace all FK references (VendorId, SupplierId) on PaymentOrder, Encumbrance, PurchaseOrder, Quotation, RFQSupplier with Party foreign keys.
4. Delete the Suppliers entity, its configuration, DbSet declaration, and the Suppliers database table.
5. Remove all "Supplier" references from `src/`.

## Scope

**IN**:
- Party entity creation
- Supplier→Party data migration with deduplication
- FK replacement on 5 referencing entities
- Suppliers entity/config/DbSet/table removal

**OUT**:
- Customer, GovEntity, TaxAuthority, Other party type management (future work)
- Supplier application layer endpoints (never existed)
- Frontend changes (API-only feature)

## Consequences

- **Data Integrity**: Single source of truth for party data; eliminates duplicate supplier records
- **Module Registry**: "Suppliers" removed from module registry; "Parties" added
- **Migration Risk**: Row-for-row migration with deduplication; reversible by re-inserting Suppliers table from Parties data
- **Reversibility**: Migration script can be inverted; Party records保留 all Supplier fields

## Alternatives Considered

- **Merge in-place**: Preserve Supplier schema while adding Party columns. Rejected — doubles query complexity with no benefit.
- **Keep both entities**: Maintain Supplier as a view/projection of Party. Rejected — unnecessary indirection for a simple master entity.
