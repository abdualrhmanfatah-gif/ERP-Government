# Tasks: Inventory Management

**Input**: Design documents from `/specs/049-inventory-management/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Application layer foundation — enums, common patterns, IApplicationDbContext registration

- [x] T001 [P] Create ItemType enum in `src/Application/Inventory/Items/Enums/ItemType.cs` with values: Goods, Service, RawMaterial, Consumable, FixedAsset
- [x] T002 [P] Create InventoryMappingExtensions in `src/Application/Inventory/Common/MappingExtensions.cs` for entity-to-response record mappings (following Parties pattern)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Queries for catalog data (Items, Categories, Units) used by all frontend dropdowns/comboboxes. These MUST complete before any frontend work begins.

- [x] T003 [P] Create GetItems query in `src/Application/Inventory/Items/Queries/GetItems/GetItemsQuery.cs` with filters: search (code/name/barcode), categoryId, unitId, itemType, isActive, underReorderLevel. PaginatedList return.
- [x] T004 [P] Create GetItemById query in `src/Application/Inventory/Items/Queries/GetItemById/GetItemByIdQuery.cs` returning Item with Category and Unit included.
- [x] T005 [P] Create GetItemCategories query in `src/Application/Inventory/ItemCategories/Queries/GetItemCategories/GetItemCategoriesQuery.cs` with filters: search, isActive. IReadOnlyList return.
- [x] T006 [P] Create GetItemCategoryById query in `src/Application/Inventory/ItemCategories/Queries/GetItemCategoryById/GetItemCategoryByIdQuery.cs`.
- [x] T007 [P] Create GetUnits query in `src/Application/Inventory/Units/Queries/GetUnits/GetUnitsQuery.cs` with filters: search, isActive. IReadOnlyList return.
- [x] T008 [P] Create GetUnitById query in `src/Application/Inventory/Units/Queries/GetUnitById/GetUnitByIdQuery.cs`.
- [x] T009 [P] Create GetItemUnitsByItemId query in `src/Application/Inventory/ItemUnits/Queries/GetItemUnitsByItemId/GetItemUnitsByItemIdQuery.cs`.
- [x] T010 [P] Create GetWarehouses query in `src/Application/Inventory/Warehouses/Queries/GetWarehouses/GetWarehousesQuery.cs` with filters: search, isActive. IReadOnlyList return.
- [x] T011 [P] Create GetWarehouseById query in `src/Application/Inventory/Warehouses/Queries/GetWarehouseById/GetWarehouseByIdQuery.cs`.

**Checkpoint**: All catalog queries complete. Backend queries can be tested with `dotnet test tests/Application.UnitTests`. Frontend dropdowns can now fetch data.

---

## Phase 3: User Story 1 — Manage Items (Priority: P1) — MVP

**Goal**: Full CRUD for inventory items with search, filtering, detail view, and toggle active.

**Independent Test**: Navigate to `/inventory/items`, verify list loads, search/filter, create item, view detail, edit, toggle active.

### Implementation for User Story 1

- [x] T012 [US1] Create CreateItem command + handler + validator in `src/Application/Inventory/Items/Commands/CreateItem/CreateItemCommand.cs`. Auto-generate Code via IDocumentSequenceService ("Item", "ITEM-{D6}"). Validate: Name required, UnitId required, ItemType required and valid, quantities >= 0, MinimumStock <= MaximumStock.
- [x] T013 [US1] Create UpdateItem command + handler + validator in `src/Application/Inventory/Items/Commands/UpdateItem/UpdateItemCommand.cs`. Same validation as create. Update RowVersion and LastModified.
- [x] T014 [US1] Create ToggleItemActive command + handler in `src/Application/Inventory/Items/Commands/ToggleItemActive/ToggleItemActiveCommand.cs`. Pattern: find entity, toggle IsActive, update LastModified.
- [x] T015 [US1] Create Items endpoint group in `src/Web/Endpoints/Inventory/Items.cs`. Routes: GET / (list), GET /{id}, POST /, PUT /{id}, PATCH /{id}/toggle-active. Authorization: ItemsView, ItemsCreate, ItemsUpdate.
- [x] T016 [US1] [P] Create items types in `src/Web/ClientApp/src/features/inventory/items/shared/types.ts`. Response/request interfaces, label maps, ComboboxOption arrays.
- [x] T017 [US1] [P] Create items Zod schemas in `src/Web/ClientApp/src/features/inventory/items/shared/schemas.ts`. createItemSchema, updateItemSchema with all validation rules from spec.
- [x] T018 [US1] [P] Create items hooks in `src/Web/ClientApp/src/features/inventory/items/hooks/useItems.ts`. useItemsList (paginated), useItemById, useCreateItem, useUpdateItem, useToggleItemActive mutations with TanStack Query invalidation.
- [x] T019 [US1] Create ItemsListPage in `src/Web/ClientApp/src/features/inventory/items/pages/ItemsListPage.tsx`. Table with all columns (FR-001), search box (FR-002), filters for category/unit/type/status/underReorderLevel (FR-003), skeleton loading, empty state, error retry.
- [x] T020 [US1] Create ItemCreatePage in `src/Web/ClientApp/src/features/inventory/items/pages/ItemCreatePage.tsx`. RHF+Zod form, Combobox for Category/Unit, ItemType select, validation errors inline, submit button disabled during mutation.
- [x] T021 [US1] Create ItemDetailPage in `src/Web/ClientApp/src/features/inventory/items/pages/ItemDetailPage.tsx`. Basic info, stock levels, category, item units section (placeholder for US4), Edit button, ToggleActive ConfirmDialog.
- [x] T022 [US1] Create ItemEditPage in `src/Web/ClientApp/src/features/inventory/items/pages/ItemEditPage.tsx`. Pre-populated form, redirects to detail if not Draft-equivalent (items have no status, so always editable).
- [x] T023 [US1] Add inventory routes in `src/Web/ClientApp/src/app/routes.tsx`. Routes: /inventory/items, /inventory/items/create, /inventory/items/:id, /inventory/items/:id/edit.
- [x] T024 [US1] Add inventory nav group in `src/Web/ClientApp/src/layouts/navigation.ts`. Items link under inventory section.
- [x] T025 [US1] Build and lint verification. Run `dotnet build src/Web/Web.csproj`, `npm run lint`, `npm run build` from `src/Web/ClientApp`.
- [x] T026 [US1] Run `dotnet test tests/Application.UnitTests` to verify no regressions.

**Checkpoint**: Items list, create, detail, edit, and toggle active all functional. Items MVP complete.

---

## Phase 4: User Story 2 — Manage Item Categories (Priority: P1)

**Goal**: Hierarchical category management with CRUD and toggle active.

**Independent Test**: Navigate to `/inventory/item-categories`, verify tree/list, create, edit, create child, toggle active.

### Implementation for User Story 2

- [x] T027 [US2] Create CreateItemCategory command + handler + validator in `src/Application/Inventory/ItemCategories/Commands/CreateItemCategory/CreateItemCategoryCommand.cs`. Validate: Code required and unique, Name required, ParentItemCategoryId cannot be self. Compute Level and Breadcrumb from parent.
- [x] T028 [US2] Create UpdateItemCategory command + handler + validator in `src/Application/Inventory/ItemCategories/Commands/UpdateItemCategory/UpdateItemCategoryCommand.cs`. Same validation as create. Recompute Level/Breadcrumb if parent changes.
- [x] T029 [US2] Create ToggleItemCategoryActive command + handler in `src/Application/Inventory/ItemCategories/Commands/ToggleItemCategoryActive/ToggleItemCategoryActiveCommand.cs`.
- [x] T030 [US2] Create ItemCategories endpoint group in `src/Web/Endpoints/Inventory/ItemCategories.cs`. Routes: GET / (list), GET /{id}, POST /, PUT /{id}, PATCH /{id}/toggle-active. Authorization: ItemCategoriesView, ItemCategoriesCreate, ItemCategoriesUpdate.
- [x] T031 [US2] [P] Create item-categories types in `src/Web/ClientApp/src/features/inventory/item-categories/shared/types.ts`.
- [x] T032 [US2] [P] Create item-categories Zod schemas in `src/Web/ClientApp/src/features/inventory/item-categories/shared/schemas.ts`. createItemCategorySchema, updateItemCategorySchema.
- [x] T033 [US2] [P] Create item-categories hooks in `src/Web/ClientApp/src/features/inventory/item-categories/hooks/useItemCategories.ts`. useItemCategoriesList, useItemCategoryById, useCreateItemCategory, useUpdateItemCategory, useToggleItemCategoryActive.
- [x] T034 [US2] Create ItemCategoriesListPage in `src/Web/ClientApp/src/features/inventory/item-categories/pages/ItemCategoriesListPage.tsx`. Hierarchical list (flat list with level indicator), search, skeleton, empty state. Inline create/edit dialog or separate page (choose simpler: inline dialog).
- [x] T035 [US2] Update navigation in `src/Web/ClientApp/src/layouts/navigation.ts`. Add Item Categories link.
- [x] T036 [US2] Build and lint verification. Run `dotnet build src/Web/Web.csproj`, `npm run lint`, `npm run build`.

**Checkpoint**: Item categories list, create, edit, and toggle active all functional.

---

## Phase 5: User Story 3 — Manage Units (Priority: P1)

**Goal**: Unit management with base unit conversion and toggle active.

**Independent Test**: Navigate to `/inventory/units`, verify list, create, edit, set base unit with conversion factor, toggle active.

### Implementation for User Story 3

- [x] T037 [US3] Create CreateUnit command + handler + validator in `src/Application/Inventory/Units/Commands/CreateUnit/CreateUnitCommand.cs`. Validate: Code required and unique, Name required, ConversionToBase > 0 if BaseUnitId provided.
- [x] T038 [US3] Create UpdateUnit command + handler + validator in `src/Application/Inventory/Units/Commands/UpdateUnit/UpdateUnitCommand.cs`. Same validation as create.
- [x] T039 [US3] Create ToggleUnitActive command + handler in `src/Application/Inventory/Units/Commands/ToggleUnitActive/ToggleUnitActiveCommand.cs`.
- [x] T040 [US3] Create Units endpoint group in `src/Web/Endpoints/Inventory/Units.cs`. Routes: GET / (list), GET /{id}, POST /, PUT /{id}, PATCH /{id}/toggle-active. Authorization: UnitsView, UnitsCreate, UnitsUpdate.
- [x] T041 [US3] [P] Create units types in `src/Web/ClientApp/src/features/inventory/units/shared/types.ts`.
- [x] T042 [US3] [P] Create units Zod schemas in `src/Web/ClientApp/src/features/inventory/units/shared/schemas.ts`. createUnitSchema, updateUnitSchema.
- [x] T043 [US3] [P] Create units hooks in `src/Web/ClientApp/src/features/inventory/units/hooks/useUnits.ts`. useUnitsList, useUnitById, useCreateUnit, useUpdateUnit, useToggleUnitActive.
- [x] T044 [US3] Create UnitsListPage in `src/Web/ClientApp/src/features/inventory/units/pages/UnitsListPage.tsx`. Table with Code, Name, UnitType, BaseUnit, ConversionToBase, IsActive, search, skeleton, empty state. Inline create/edit dialog.
- [x] T045 [US3] Update navigation in `src/Web/ClientApp/src/layouts/navigation.ts`. Add Units link.
- [x] T046 [US3] Build and lint verification. Run `dotnet build src/Web/Web.csproj`, `npm run lint`, `npm run build`.

**Checkpoint**: Units list, create, edit, and toggle active all functional.

---

## Phase 6: User Story 4 — Manage Item Units (Priority: P2)

**Goal**: Link items to multiple units with conversion factors and base unit designation. Accessed from item detail page.

**Independent Test**: On item detail page, verify item units section loads, add unit to item, set conversion factor, designate base unit, remove unit.

### Implementation for User Story 4

- [x] T047 [US4] Create AddItemUnit command + handler + validator in `src/Application/Inventory/ItemUnits/Commands/AddItemUnit/AddItemUnitCommand.cs`. Validate: ItemId required, UnitId required, ConversionFactor > 0. If IsBase=true, set existing base unit's IsBase=false in same transaction.
- [x] T048 [US4] Create UpdateItemUnit command + handler + validator in `src/Application/Inventory/ItemUnits/Commands/UpdateItemUnit/UpdateItemUnitCommand.cs`. Same validation. Handle base unit swap.
- [x] T049 [US4] Create RemoveItemUnit command + handler in `src/Application/Inventory/ItemUnits/Commands/RemoveItemUnit/RemoveItemUnitCommand.cs`. Prevent removal if it's the only base unit. Validate no stock references if needed.
- [x] T050 [US4] Create ItemUnits endpoint group in `src/Web/Endpoints/Inventory/ItemUnits.cs`. Routes: GET /items/{itemId}/units, POST /items/{itemId}/units, PUT /items/{itemId}/units/{id}, DELETE /items/{itemId}/units/{id}. Authorization: ItemsUpdate.
- [x] T051 [US4] Update ItemDetailPage in `src/Web/ClientApp/src/features/inventory/items/pages/ItemDetailPage.tsx`. Replace placeholder with real item units section: table of linked units, add/edit/remove functionality, base unit designation.
- [x] T052 [US4] [P] Create item-units hooks in `src/Web/ClientApp/src/features/inventory/items/hooks/useItems.ts` (add to existing). useItemUnits, useAddItemUnit, useUpdateItemUnit, useRemoveItemUnit.
- [x] T053 [US4] Build and lint verification. Run `dotnet build src/Web/Web.csproj`, `npm run lint`, `npm run build`.

**Checkpoint**: Item units section functional on item detail page. Add, edit, remove, and base unit designation all working.

---

## Phase 7: User Story 5 — Manage Warehouses (Priority: P2)

**Goal**: Warehouse management with capacity tracking and toggle active.

**Independent Test**: Navigate to `/inventory/warehouses`, verify list with capacity, create, edit, toggle active, validation for capacity/email.

### Implementation for User Story 5

- [x] T054 [US5] Create CreateWarehouse command + handler + validator in `src/Application/Inventory/Warehouses/Commands/CreateWarehouse/CreateWarehouseCommand.cs`. Validate: Code required and unique, Name required, Email valid if provided, TotalCapacity >= 0, CurrentLoad >= 0, CurrentLoad <= TotalCapacity.
- [x] T055 [US5] Create UpdateWarehouse command + handler + validator in `src/Application/Inventory/Warehouses/Commands/UpdateWarehouse/UpdateWarehouseCommand.cs`. Same validation as create.
- [x] T056 [US5] Create ToggleWarehouseActive command + handler in `src/Application/Inventory/Warehouses/Commands/ToggleWarehouseActive/ToggleWarehouseActiveCommand.cs`.
- [x] T057 [US5] Create Warehouses endpoint group in `src/Web/Endpoints/Inventory/Warehouses.cs`. Routes: GET / (list), GET /{id}, POST /, PUT /{id}, PATCH /{id}/toggle-active. Authorization: WarehousesView, WarehousesCreate, WarehousesUpdate.
- [x] T058 [US5] [P] Create warehouses types in `src/Web/ClientApp/src/features/inventory/warehouses/shared/types.ts`.
- [x] T059 [US5] [P] Create warehouses Zod schemas in `src/Web/ClientApp/src/features/inventory/warehouses/shared/schemas.ts`. createWarehouseSchema, updateWarehouseSchema.
- [x] T060 [US5] [P] Create warehouses hooks in `src/Web/ClientApp/src/features/inventory/warehouses/hooks/useWarehouses.ts`. useWarehousesList, useWarehouseById, useCreateWarehouse, useUpdateWarehouse, useToggleWarehouseActive.
- [x] T061 [US5] Create WarehousesListPage in `src/Web/ClientApp/src/features/inventory/warehouses/pages/WarehousesListPage.tsx`. Table with Code, Name, Location, Manager, TotalCapacity, CurrentLoad, IsActive, search, skeleton, empty state. Inline create/edit dialog.
- [x] T062 [US5] Update navigation in `src/Web/ClientApp/src/layouts/navigation.ts`. Add Warehouses link.
- [x] T063 [US5] Build and lint verification. Run `dotnet build src/Web/Web.csproj`, `npm run lint`, `npm run build`.

**Checkpoint**: Warehouses list, create, edit, and toggle active all functional. All 5 entities complete.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, API client regeneration, edge case handling

- [x] T064 Regenerate API client: `cd src/Web/ClientApp && npm run generate-api`
- [x] T065 Run full backend test suite: `dotnet test tests/Application.UnitTests`, `dotnet test tests/Application.FunctionalTests`
- [x] T066 Run frontend lint and build: `cd src/Web/ClientApp && npm run lint && npm run build`
- [x] T067 Verify all interactive elements have aria-labels (SC-007)
- [x] T068 Verify no CSS physical properties (SC-006): search for `margin-left`, `margin-right`, `padding-left`, `padding-right` in inventory feature code
- [x] T069 Verify all monetary amounts use tabular-nums (SC-005)
- [x] T070 Verify empty states for all list pages when no results (FR-025)
- [x] T071 Verify all action buttons disabled during mutations (FR-026)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (enum + mapping) — BLOCKS all user stories
- **US1 Items (Phase 3)**: Depends on Phase 2 completion
- **US2 Categories (Phase 4)**: Depends on Phase 2 completion — can parallel with US1
- **US3 Units (Phase 5)**: Depends on Phase 2 completion — can parallel with US1, US2
- **US4 Item Units (Phase 6)**: Depends on US1 completion (needs ItemDetailPage) and Phase 2 (queries)
- **US5 Warehouses (Phase 7)**: Depends on Phase 2 completion — can parallel with US1-US3
- **Polish (Phase 8)**: Depends on all user stories complete

### User Story Dependencies

- **US1 Items (P1)**: Can start after Phase 2 — No dependencies on other stories
- **US2 Categories (P1)**: Can start after Phase 2 — No dependencies on other stories
- **US3 Units (P1)**: Can start after Phase 2 — No dependencies on other stories
- **US4 Item Units (P2)**: Depends on US1 (needs ItemDetailPage, ItemUnits endpoint, ItemId from item detail)
- **US5 Warehouses (P2)**: Can start after Phase 2 — No dependencies on other stories

### Within Each User Story

- Commands/Validators → Web Endpoints → Frontend Types/Schemas → Hooks → Pages → Navigation → Build/Lint

### Parallel Opportunities

- All tasks marked [P] within a phase can run in parallel
- US2, US3, US5 can all start in parallel after Phase 2 completes
- Within US1: T016, T017, T018 (types, schemas, hooks) can run in parallel
- Within US2: T031, T032, T033 can run in parallel
- Within US3: T041, T042, T043 can run in parallel
- Within US5: T058, T059, T060 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Foundation queries (all parallel):
Task: T003 GetItems query
Task: T004 GetItemById query

# Frontend foundation (all parallel):
Task: T016 Items types
Task: T017 Items Zod schemas
Task: T018 Items hooks

# Pages (sequential - each depends on hooks):
Task: T019 ItemsListPage
Task: T020 ItemCreatePage
Task: T021 ItemDetailPage
Task: T022 ItemEditPage
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1: Setup (enum, mapping)
2. Complete Phase 2: Foundational (all queries)
3. Complete Phase 3: US1 Items (full CRUD + list/filter + detail + toggle active)
4. **STOP and VALIDATE**: Test items list, create, detail, edit, toggle active independently
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready
2. Phase 3 (US1 Items) → Test independently → Deploy/Demo (MVP!)
3. Phase 4 (US2 Categories) → Test independently → Deploy/Demo
4. Phase 5 (US3 Units) → Test independently → Deploy/Demo
5. Phase 6 (US4 Item Units) → Test on item detail → Deploy/Demo
6. Phase 7 (US5 Warehouses) → Test independently → Deploy/Demo
7. Phase 8 (Polish) → Final validation → Release

### Parallel Team Strategy

With multiple developers:
1. Team completes Phase 1 + Phase 2 together
2. Once Phase 2 is done:
   - Developer A: US1 Items (Phase 3)
   - Developer B: US2 Categories (Phase 4)
   - Developer C: US3 Units (Phase 5) + US5 Warehouses (Phase 7)
3. After US1 complete:
   - Developer A: US4 Item Units (Phase 6)
4. All converge for Polish (Phase 8)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Domain entities and EF configurations already exist — no schema changes needed
- All commands follow Parties pattern: record command, handler with IApplicationDbContext, AbstractValidator
- All endpoints follow Parties pattern: IEndpointGroup with static Map method, request/response records
- Frontend follows procurement pattern: entity-scoped subfolders, types.ts + schemas.ts + hooks.ts + pages
- ItemUnits (US4) is accessed from ItemDetailPage, not as a separate page

---

## Phase 9: Convergence

**Purpose**: Close gap between spec/plan and implemented code

- [x] T072 [US1] Add `dir="ltr"` to item code and barcode cell renderers in `src/Web/ClientApp/src/features/inventory/items/pages/ItemsListPage.tsx` per FR-031 (partial)

---

## Phase 10: Convergence

**Purpose**: Close remaining gaps between spec and implementation

- [x] T073 [US1] Import `handleApiError` from `@/shared/api/result-to-ui` and replace generic `notify` in catch blocks with `handleApiError(err, setError)` in `src/Web/ClientApp/src/features/inventory/items/pages/ItemCreatePage.tsx` and `src/Web/ClientApp/src/features/inventory/items/pages/ItemEditPage.tsx` per FR-027 (partial)
- [x] T074 [US2/US3/US5] Destructure `error` and `refetch` from query hooks and pass `error={error?.message}` and `onRetry={() => refetch()}` to Page component in `src/Web/ClientApp/src/features/inventory/item-categories/pages/ItemCategoriesListPage.tsx`, `src/Web/ClientApp/src/features/inventory/units/pages/UnitsListPage.tsx`, and `src/Web/ClientApp/src/features/inventory/warehouses/pages/WarehousesListPage.tsx` per FR-024 (partial)
