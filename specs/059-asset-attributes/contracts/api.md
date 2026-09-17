# API Contracts: Asset Attributes

## Endpoint Group: `/api/AssetAttributes`

### GET `/api/AssetAttributes/definitions`

**Permission**: `AssetGroups.View`

**Query Parameters**:
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| search | string | "" | Filter by Code or Name (case-insensitive contains) |
| dataType | int? | null | Filter by data type (1-5) |
| isActive | bool? | null | Filter by active status |
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page |

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": 1,
      "code": "COLOR",
      "name": "اللون",
      "description": null,
      "attributeDataType": 1,
      "attributeDataTypeName": "Text",
      "unit": null,
      "isActive": true,
      "sortOrder": 1,
      "linkedValueCount": 45
    }
  ],
  "totalCount": 12,
  "page": 1,
  "pageSize": 20
}
```

---

### GET `/api/AssetAttributes/definitions/{id}`

**Permission**: `AssetGroups.View`

**Response**: `200 OK`
```json
{
  "id": 1,
  "code": "COLOR",
  "name": "اللون",
  "description": null,
  "attributeDataType": 1,
  "attributeDataTypeName": "Text",
  "unit": null,
  "isActive": true,
  "sortOrder": 1,
  "linkedValueCount": 45
}
```

---

### POST `/api/AssetAttributes/definitions`

**Permission**: `AssetGroups.Create`

**Request**:
```json
{
  "code": "COLOR",
  "name": "اللون",
  "description": "لون الأصل الرئيسي",
  "attributeDataType": 1,
  "unit": null,
  "sortOrder": 1
}
```

**Response**: `201 Created` → `{ "id": 1 }`

**Errors**:
- `400` — Code already exists
- `400` — Invalid data type
- `400` — Code or Name missing

---

### PUT `/api/AssetAttributes/definitions/{id}`

**Permission**: `AssetGroups.Update`

**Request**:
```json
{
  "id": 1,
  "code": "COLOR",
  "name": "اللون",
  "description": "لون الأصل",
  "attributeDataType": 1,
  "unit": null,
  "isActive": true,
  "sortOrder": 1
}
```

**Response**: `200 OK` → `{ "id": 1 }`

**Errors**:
- `400` — Code mismatch (URL id != body id)
- `400` — Code already exists (another definition)
- `409` — Cannot change DataType when values exist
- `404` — Definition not found

---

### PUT `/api/AssetGroups/{groupId}/attributes`

**Permission**: `AssetGroups.Update`

**Request**:
```json
{
  "assetGroupId": 1,
  "bindings": [
    { "assetAttributeDefinitionId": 1, "isRequired": true, "sortOrder": 1 },
    { "assetAttributeDefinitionId": 2, "isRequired": false, "sortOrder": 2 }
  ]
}
```

**Response**: `200 OK` → `{ "id": 1 }`

**Errors**:
- `400` — Bindings list is empty
- `400` — Definition not found
- `400` — Group not found
- `400` — Group ID mismatch

---

### GET `/api/AssetGroups/{id}` (modified)

**Permission**: `AssetGroups.View`

**Response addition** (new field):
```json
{
  "id": 1,
  "code": "GRP-001",
  "name": "أجهزة مكتبية",
  "attributeBindings": [
    {
      "definitionId": 1,
      "code": "COLOR",
      "name": "اللون",
      "attributeDataType": 1,
      "isRequired": true,
      "sortOrder": 1
    }
  ]
}
```

---

### GET `/api/Assets/{id}` (modified)

**Permission**: `Assets.View`

**Response addition** (new fields):
```json
{
  "id": 1,
  "code": "AST-001",
  "attributeValues": [
    {
      "definitionId": 1,
      "code": "COLOR",
      "name": "اللون",
      "attributeDataType": 1,
      "textValue": "أبيض",
      "integerValue": null,
      "decimalValue": null,
      "dateValue": null,
      "booleanValue": null
    }
  ],
  "currentDepartment": "ال accounting"
}
```

---

### POST/PUT `/api/Assets` (modified)

**Permission**: `Assets.Create` / `Assets.Update`

**Request addition**:
```json
{
  "attributeValues": [
    { "assetAttributeDefinitionId": 1, "textValue": "أبيض", "integerValue": null, "decimalValue": null, "dateValue": null, "booleanValue": null },
    { "assetAttributeDefinitionId": 3, "textValue": null, "integerValue": null, "decimalValue": 2.5, "dateValue": null, "booleanValue": null }
  ]
}
```

**Validation**:
- Each value row must have exactly one non-null typed column matching the definition's DataType
- Required attributes (from group bindings) must be present
- Duplicates (same definitionId) are rejected
- Unknown definitionId is rejected
