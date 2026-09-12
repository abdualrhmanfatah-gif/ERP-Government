# API Contracts: Inventory Management

**Date**: 2026-09-11

## Items

### GET /api/Items

**Query**: page?, pageSize?, search?, categoryId?, unitId?, itemType?, isActive?, underReorderLevel?

**Response** (200): PaginatedList of ItemResponse

### GET /api/Items/{id}

**Response** (200): ItemDetailResponse (includes ItemUnits)

### POST /api/Items

**Request**: CreateItemRequest (Code auto-generated)

**Response** (201): int (id)

### PUT /api/Items/{id}

**Request**: UpdateItemRequest

**Response** (204): No content

### PATCH /api/Items/{id}/toggle-active

**Response** (204): No content

## ItemCategories

### GET /api/ItemCategories

**Query**: search?, isActive?

**Response** (200): IReadOnlyList of ItemCategoryResponse

### GET /api/ItemCategories/{id}

**Response** (200): ItemCategoryResponse

### POST /api/ItemCategories

**Request**: CreateItemCategoryRequest

**Response** (201): int (id)

### PUT /api/ItemCategories/{id}

**Request**: UpdateItemCategoryRequest

**Response** (204): No content

### PATCH /api/ItemCategories/{id}/toggle-active

**Response** (204): No content

## Units

### GET /api/Units

**Query**: search?, isActive?

**Response** (200): IReadOnlyList of UnitResponse

### GET /api/Units/{id}

**Response** (200): UnitResponse

### POST /api/Units

**Request**: CreateUnitRequest

**Response** (201): int (id)

### PUT /api/Units/{id}

**Request**: UpdateUnitRequest

**Response** (204): No content

### PATCH /api/Units/{id}/toggle-active

**Response** (204): No content

## ItemUnits

### GET /api/Items/{itemId}/units

**Response** (200): IReadOnlyList of ItemUnitResponse

### POST /api/Items/{itemId}/units

**Request**: AddItemUnitRequest

**Response** (201): int (id)

### PUT /api/Items/{itemId}/units/{id}

**Request**: UpdateItemUnitRequest

**Response** (204): No content

### DELETE /api/Items/{itemId}/units/{id}

**Response** (204): No content

## Warehouses

### GET /api/Warehouses

**Query**: search?, isActive?

**Response** (200): IReadOnlyList of WarehouseResponse

### GET /api/Warehouses/{id}

**Response** (200): WarehouseResponse

### POST /api/Warehouses

**Request**: CreateWarehouseRequest

**Response** (201): int (id)

### PUT /api/Warehouses/{id}

**Request**: UpdateWarehouseRequest

**Response** (204): No content

### PATCH /api/Warehouses/{id}/toggle-active

**Response** (204): No content

## Error Responses

### 400 Validation
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Code": ["Code is required."] }
}
```

### 500 Server
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "An error occurred.",
  "status": 500
}
```
