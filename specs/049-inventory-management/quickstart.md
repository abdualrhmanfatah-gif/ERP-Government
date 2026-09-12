# Quickstart: Inventory Management Validation

**Date**: 2026-09-11

## Prerequisites

- Backend running on https://localhost:5001
- Frontend dev server: `npm run dev` from `src/Web/ClientApp`
- User logged in with inventory permissions
- Database seeded with initial units, categories, warehouses

## Validation Scenarios

### SC1: Items List (FR-001 to FR-003)

1. Navigate to `/inventory/items`
2. **Expected**: Table with all columns, skeleton while loading
3. Type in search → **Expected**: Filtered by code/name/barcode
4. Select category filter → **Expected**: Filtered by category
5. Select "under reorder level" → **Expected**: Only low-stock items shown

### SC2: Create Item (FR-004 to FR-005)

1. Click "صنف جديد" → navigate to `/inventory/items/create`
2. Fill Code, Name, select Unit, select ItemType
3. **Expected**: Validation errors for missing required fields
4. Submit with valid data → **Expected**: Navigate to detail page

### SC3: Item Detail (FR-007)

1. Navigate to `/inventory/items/:id`
2. **Expected**: Basic info, stock levels, category, item units section
3. **Expected**: Edit button, toggle active button

### SC4: Edit Item (FR-006)

1. Navigate to `/inventory/items/:id/edit`
2. **Expected**: Form pre-populated with current values
3. Modify and save → **Expected**: Changes persist

### SC5: Item Categories (FR-009 to FR-011)

1. Navigate to `/inventory/item-categories`
2. **Expected**: Tree/list with parent info
3. Create child category → **Expected**: Cannot set self as parent

### SC6: Units (FR-012 to FR-014)

1. Navigate to `/inventory/units`
2. **Expected**: List with base unit and conversion
3. Create unit with BaseUnit → **Expected**: ConversionToBase > 0 required

### SC7: Warehouses (FR-017 to FR-019)

1. Navigate to `/inventory/warehouses`
2. **Expected**: List with capacity info
3. Create warehouse → **Expected**: CurrentLoad <= TotalCapacity enforced

### SC8: Toggle Active (FR-008, FR-011, FR-014, FR-019)

1. On any list or detail page, click toggle active
2. **Expected**: Confirmation dialog
3. Confirm → **Expected**: Status toggles

## Commands

```bash
# Backend build
dotnet build src/Web/Web.csproj

# Backend tests
dotnet test tests/Application.UnitTests

# Frontend lint
cd src/Web/ClientApp && npm run lint

# Frontend build
cd src/Web/ClientApp && npm run build

# Regenerate API client
cd src/Web/ClientApp && npm run generate-api
```
