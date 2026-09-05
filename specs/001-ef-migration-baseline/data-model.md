# Data Model: EF Migration Baseline

**Date**: 2026-09-02
**Feature**: EF Migration Baseline

## Entities

### Migration

Represents a versioned change to the database schema. Each migration contains up and down operations that can be applied or reverted.

**Attributes**:
- `MigrationId`: Unique identifier (timestamp + name)
- `Name`: Human‑readable name (e.g., "InitialBaseline")
- `ProductVersion`: EF Core version that created the migration
- `TargetModel`: Snapshot of the model at migration creation
- `Operations`: List of schema change operations (AddTable, AlterColumn, etc.)

**Relationships**:
- Belongs to a `MigrationContext`
- Has ordered relationship with other migrations (by `MigrationId`)

### Database Schema

The current structure of the database (tables, columns, relationships, constraints, indexes).

**Attributes**:
- `Tables`: Collection of table definitions
- `Views`: Collection of view definitions
- `StoredProcedures`: Collection of stored procedure definitions
- `Constraints`: Primary keys, foreign keys, unique constraints
- `Indexes`: Collection of index definitions

**Relationships**:
- Source of truth for baseline validation
- Compared against `Migration.TargetModel` during reconciliation

### Migration Context

The EF Core `DbContext` configuration that defines the model and mapping.

**Attributes**:
- `ContextType`: Full type name of the DbContext
- `Model`: The compiled model
- `ConnectionStrings`: Database connection information
- `Options`: DbContext options (provider, command timeout, etc.)

**Relationships**:
- Generates `Migration` objects
- Defines the `TargetModel` that is compared with `Database Schema`

## Validation Rules

- Baseline migration must be generated from the `Migration Context` model.
- The `Migration.Operations` must exactly reconcile the `Database Schema` to match the `TargetModel`.
- All migrations must be idempotent when applied (safe to run multiple times).
- Each migration must have corresponding up and down operations.

## State Transitions

A migration can be in one of three states:

1. **Pending**: Generated but not yet applied to the database.
2. **Applied**: Successfully applied to the database.
3. **Reverted**: Applied and then reverted using the down operations.

The baseline migration starts as **Pending**, moves to **Applied** when the developer runs `dotnet ef database update`, and can be **Reverted** if needed.

## Diagram

```text
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ Migration       │      │ Database Schema  │      │ Migration Context│
├─────────────────┤      ├─────────────────┤      ├─────────────────┤
│ MigrationId     │      │ Tables          │      │ ContextType     │
│ Name            │      │ Views           │      │ Model           │
│ ProductVersion  │      │ StoredProcedures│      │ ConnectionStrings│
│ TargetModel     │◄────►│ Constraints     │      │ Options         │
│ Operations      │      │ Indexes         │      │                 │
└────────┬────────┘      └────────┬────────┘      └────────┬────────┘
         │                        │                        │
         │                        │                        │
         └────────────────────────┼────────────────────────┘
                                  │
                          Compare & Reconcile
```