# Implementation Plan: Inventory Management

**Branch**: `049-inventory-management` | **Date**: 2026-09-11 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/049-inventory-management/spec.md`

## Summary

Build the Application (CQRS commands/queries), Web (Minimal API endpoints), and Frontend (React 19 + TypeScript) layers for 5 inventory entities: Items, ItemCategories, Units, ItemUnits, and Warehouses. Domain layer and EF configurations already exist.

## Technical Context

**Language/Version**: C# 13 / .NET 10, TypeScript 5.9, React 19

**Primary Dependencies**: MediatR, FluentValidation, EF Core + SQL Server (backend); Vite, React Router v7, TanStack Query, React Hook Form + Zod, shadcn, Tailwind (frontend)

**Storage**: SQL Server via EF Core (existing DbContext, no migrations needed)

**Testing**: dotnet test (backend), npm run lint + npm run build (frontend — no test suite by governance)

**Target Platform**: Modern browsers, Arabic RTL SPA

**Project Type**: Web application (backend + frontend)

**Performance Goals**: SC-002: list page loads within 3s, SC-003: form validations within 200ms

**Constraints**: Arabic only, RTL only, no physical CSS properties, no cross-feature imports, Result<T> for all endpoints, PermissionCodes for authorization

**Scale/Scope**: 5 entities, ~25 Application files, ~5 Web endpoint files, ~15 frontend files, 7 routes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ | Domain already built; Application/Web layers follow dependency direction |
| II. Bounded Contexts | ✅ | Inventory is a registered module; no cross-module writes |
| III. Server-Side Business-Rule Integrity | ✅ | Validation in FluentValidation handlers, not frontend |
| VI. Data Integrity | ✅ | No schema changes; existing entities have RowVersion, Restrict FKs |
| VII. Authorization | ✅ | PermissionCodes exist for Items, ItemCategories, Units, Warehouses |
| IX. API Contract Integrity | ✅ | Result<T>, Minimal APIs, OpenAPI via NSwag |
| X. UI Consistency | ✅ | Arabic RTL, logical CSS, shadcn components, DESIGN.md tokens |
| XI. Testing | ✅ | TDD mandatory; backend unit tests + functional tests |

No violations.

## Project Structure

### Documentation (this feature)

```text
specs/049-inventory-management/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Inventory/Entities/          # EXISTS — Item, ItemCategory, Unit, ItemUnit, Warehouse
├── Application/Inventory/
│   ├── Items/
│   │   ├── Commands/
│   │   │   ├── CreateItem/
│   │   │   ├── UpdateItem/
│   │   │   └── ToggleItemActive/
│   │   └── Queries/
│   │       ├── GetItems/
│   │       └── GetItemById/
│   ├── ItemCategories/
│   │   ├── Commands/
│   │   │   ├── CreateItemCategory/
│   │   │   ├── UpdateItemCategory/
│   │   │   └── ToggleItemCategoryActive/
│   │   └── Queries/
│   │       ├── GetItemCategories/
│   │       └── GetItemCategoryById/
│   ├── Units/
│   │   ├── Commands/
│   │   │   ├── CreateUnit/
│   │   │   ├── UpdateUnit/
│   │   │   └── ToggleUnitActive/
│   │   └── Queries/
│   │       ├── GetUnits/
│   │       └── GetUnitById/
│   ├── ItemUnits/
│   │   ├── Commands/
│   │   │   ├── AddItemUnit/
│   │   │   ├── UpdateItemUnit/
│   │   │   └── RemoveItemUnit/
│   │   └── Queries/
│   │       └── GetItemUnitsByItemId/
│   └── Warehouses/
│       ├── Commands/
│       │   ├── CreateWarehouse/
│       │   ├── UpdateWarehouse/
│       │   └── ToggleWarehouseActive/
│       └── Queries/
│           ├── GetWarehouses/
│           └── GetWarehouseById/
├── Web/Endpoints/Inventory/
│   ├── Items.cs
│   ├── ItemCategories.cs
│   ├── Units.cs
│   ├── ItemUnits.cs
│   └── Warehouses.cs
└── Web/ClientApp/src/
    ├── features/inventory/
    │   ├── items/
    │   │   ├── pages/
    │   │   │   ├── ItemsListPage.tsx
    │   │   │   ├── ItemDetailPage.tsx
    │   │   │   ├── ItemCreatePage.tsx
    │   │   │   └── ItemEditPage.tsx
    │   │   ├── hooks/
    │   │   │   └── useItems.ts
    │   │   └── shared/
    │   │       ├── types.ts
    │   │       └── schemas.ts
    │   ├── item-categories/
    │   │   ├── pages/
    │   │   │   └── ItemCategoriesListPage.tsx
    │   │   ├── hooks/
    │   │   │   └── useItemCategories.ts
    │   │   └── shared/
    │   │       ├── types.ts
    │   │       └── schemas.ts
    │   ├── units/
    │   │   ├── pages/
    │   │   │   └── UnitsListPage.tsx
    │   │   ├── hooks/
    │   │   │   └── useUnits.ts
    │   │   └── shared/
    │   │       ├── types.ts
    │   │       └── schemas.ts
    │   └── warehouses/
    │       ├── pages/
    │       │   └── WarehousesListPage.tsx
    │       ├── hooks/
    │       │   └── useWarehouses.ts
    │       └── shared/
    │           ├── types.ts
    │           └── schemas.ts
    └── components/
        └── Inventory*.tsx              # Shared components (if needed)
```

## Complexity Tracking

No constitution violations — no complexity justification needed.
