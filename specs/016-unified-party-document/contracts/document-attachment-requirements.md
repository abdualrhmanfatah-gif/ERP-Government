# Contract: /api/DocumentAttachmentRequirements

**Route Prefix**: `/api/DocumentAttachmentRequirements`
**Endpoint Group**: `Web/Endpoints/DocumentAttachmentRequirements/DocumentAttachmentRequirements.cs`

Admin CRUD for managing which attachment types are mandatory per document type.

---

## GET /api/DocumentAttachmentRequirements

List all requirements, optionally filtered by document type.

**Query Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| documentType | string | No | Filter by document type |
| isActive | bool | No | Filter by active status |

**Response 200**:
```json
[
  {
    "id": 1,
    "documentType": "Budget",
    "attachmentTypeCode": "BOQ",
    "titleAr": "جدول الكميات",
    "isMandatory": true,
    "isActive": true,
    "rowVersion": "AAAAAA==",
    "createdAt": "2026-09-05T00:00:00Z",
    "createdBy": "admin",
    "lastModifiedAt": "2026-09-05T00:00:00Z",
    "lastModifiedBy": "admin"
  }
]
```

---

## GET /api/DocumentAttachmentRequirements/{id}

Get a single requirement by ID.

**Response 200**: Requirement object

**Response 404**: Not found

---

## POST /api/DocumentAttachmentRequirements

Create a new requirement.

**Request Body**:
```json
{
  "documentType": "Budget",
  "attachmentTypeCode": "BOQ",
  "titleAr": "جدول الكميات",
  "isMandatory": true
}
```

**Validation**:
- `documentType` is required, non-empty
- `attachmentTypeCode` is required, non-empty
- `titleAr` is required, non-empty
- Unique constraint on (documentType, attachmentTypeCode) — returns 409 if duplicate

**Response 201**: Created requirement object

**Response 400**: Validation errors
**Response 409**: Duplicate (documentType + attachmentTypeCode already exists)

---

## PUT /api/DocumentAttachmentRequirements/{id}

Update an existing requirement.

**Request Body**: Same as POST plus `rowVersion`

**Response 200**: Updated requirement object

**Response 400**: Validation errors
**Response 404**: Not found
**Response 409**: Concurrency conflict or duplicate

---

## DELETE /api/DocumentAttachmentRequirements/{id}

Soft-delete (set `isActive = false`). Hard delete is not permitted for audit records.

**Response 204**: No content

**Response 404**: Not found
