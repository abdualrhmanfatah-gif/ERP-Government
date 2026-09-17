# API Contracts: Asset Groups CRUD

**Feature**: 054-asset-groups-crud
**Base URL**: `/api/AssetGroups`

## Endpoints

### GET /api/AssetGroups

List all asset groups (flat list for tree construction).

**Permission**: `AssetGroups.View`

**Query Parameters**:
| Parameter | Type | Default | Notes |
|-----------|------|---------|-------|
| search | string | null | Filter by Code or Name (contains) |
| isActive | bool | null | Filter by active status |
| parentId | int | null | Filter by parent (null = root groups) |

**Response 200**:
```json
[
  {
    "id": 1,
    "code": "AG-001",
    "name": "مباني وإنشاءات",
    "parentAssetGroupId": null,
    "isActive": true,
    "assetCategory": "Tangible",
    "isDepreciable": true,
    "defaultUsefulLifeYears": 50,
    "hasChildren": true
  }
]
```

---

### GET /api/AssetGroups/{id}

Get single asset group basic info.

**Permission**: `AssetGroups.View`

**Response 200**:
```json
{
  "id": 1,
  "code": "AG-001",
  "name": "مباني وإنشاءات",
  "description": null,
  "parentAssetGroupId": null,
  "isActive": true,
  "assetCategory": "Tangible",
  "isDepreciable": true,
  "depreciationMethod": "StraightLine",
  "defaultUsefulLifeYears": 50,
  "residualValuePercentage": 10.00,
  "rowVersion": "AAAAAAAAB9k="
}
```

---

### GET /api/AssetGroups/{id}/detail

Get full asset group detail including GL accounts.

**Permission**: `AssetGroups.View`

**Response 200**:
```json
{
  "id": 1,
  "code": "AG-001",
  "name": "مباني وإنشاءات",
  "description": null,
  "parentAssetGroupId": null,
  "isActive": true,
  "assetCategory": "Tangible",
  "isDepreciable": true,
  "depreciationMethod": "StraightLine",
  "depreciationRate": null,
  "defaultUsefulLifeYears": 50,
  "residualValuePercentage": 10.00,
  "accountAssetId": null,
  "accountAccumulatedDepreciationId": null,
  "accountExpenseId": null,
  "accountDisposalId": null,
  "accountRevaluationId": null,
  "accountImpairmentId": null,
  "rowVersion": "AAAAAAAAB9k=",
  "created": "2026-01-15T10:30:00Z",
  "createdBy": "admin",
  "lastModified": "2026-09-14T14:00:00Z",
  "lastModifiedBy": "admin"
}
```

---

### POST /api/AssetGroups

Create a new asset group.

**Permission**: `AssetGroups.Create`

**Request Body**:
```json
{
  "code": "AG-004",
  "name": "أجهزة حاسوب",
  "description": "أجهزة حاسوب ومعدات تقنية",
  "parentAssetGroupId": null,
  "assetCategory": "Tangible",
  "isDepreciable": true,
  "depreciationMethod": "StraightLine",
  "defaultUsefulLifeYears": 4,
  "residualValuePercentage": 10.00,
  "accountAssetId": null,
  "accountAccumulatedDepreciationId": null,
  "accountExpenseId": null,
  "accountDisposalId": null,
  "accountRevaluationId": null,
  "accountImpairmentId": null
}
```

**Response 201**: `{"id": 4}`

**Error Responses**:
| Status | Code | Message |
|--------|------|---------|
| 400 | DUPLICATE_CODE | كود المجموعة مستخدم بالفعل |
| 400 | CYCLE_DETECTED | لا يمكن إنشاء حلقة في التسلسل الهرمي |
| 400 | MAX_DEPTH_EXCEEDED | تم تجاوز الحد الأقصى لمستويات التصنيف |
| 400 | SELF_PARENT | لا يمكن أن تكون المجموعة والدتها |
| 400 | VALIDATION_ERROR | [field-level errors] |

---

### PUT /api/AssetGroups/{id}

Update an existing asset group. Code is immutable.

**Permission**: `AssetGroups.Update`

**Request Body**:
```json
{
  "name": "مباني وإنشاءات (محدث)",
  "description": "وصف جديد",
  "parentAssetGroupId": null,
  "assetCategory": "Tangible",
  "isDepreciable": true,
  "depreciationMethod": "StraightLine",
  "defaultUsefulLifeYears": 40,
  "residualValuePercentage": 15.00,
  "accountAssetId": 101,
  "accountAccumulatedDepreciationId": 102,
  "accountExpenseId": 103,
  "accountDisposalId": 104,
  "accountRevaluationId": 105,
  "accountImpairmentId": 106,
  "rowVersion": "AAAAAAAAB9k="
}
```

**Response 200**: `{"id": 1}`

**Error Responses**:
| Status | Code | Message |
|--------|------|---------|
| 400 | INACTIVE_GROUP | يجب تفعيل المجموعة قبل التعديل |
| 400 | CYCLE_DETECTED | لا يمكن إنشاء حلقة في التسلسل الهرمي |
| 400 | MAX_DEPTH_EXCEEDED | تم تجاوز الحد الأقصى لمستويات التصنيف |
| 400 | SELF_PARENT | لا يمكن أن تكون المجموعة والدتها |
| 409 | CONCURRENCY_CONFLICT | تم تعديل المجموعة من مستخدم آخر |

---

### POST /api/AssetGroups/{id}/deactivate

Deactivate an asset group.

**Permission**: `AssetGroups.Deactivate`

**Request Body**:
```json
{
  "rowVersion": "AAAAAAAAB9k="
}
```

**Response 200**: `{"id": 1}`

**Error Responses**:
| Status | Code | Message |
|--------|------|---------|
| 400 | HAS_ASSETS | لا يمكن تعطيل مجموعة مرتبطة بأصول |
| 400 | ALREADY_INACTIVE | المجموعة معطلة بالفعل |

---

### POST /api/AssetGroups/{id}/activate

Reactivate a deactivated asset group.

**Permission**: `AssetGroups.Activate`

**Request Body**:
```json
{
  "rowVersion": "AAAAAAAAB9k="
}
```

**Response 200**: `{"id": 1}`

**Error Responses**:
| Status | Code | Message |
|--------|------|---------|
| 400 | ALREADY_ACTIVE | المجموعة مفعلة بالفعل |

---

## Error Response Contract

All errors follow the problem-details contract (Constitution XIII):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "code": ["كود المجموعة مستخدم بالفعل"]
  },
  "traceId": "00-abc123-def456-00"
}
```

## OpenAPI

The backend generates OpenAPI automatically from endpoint metadata. Frontend clients should be generated from this contract or hand-written to mirror it exactly (Constitution IX).
