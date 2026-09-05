# Contract: /api/Documents

**Route Prefix**: `/api/Documents`
**Endpoint Group**: `Web/Endpoints/Documents/Documents.cs`

All routes are polymorphic — `{entityName}` is the plural lowercase entity name (e.g., "budgets", "paymentorders", "appropriations").

---

## GET /api/Documents/{entityName}/{id}/approvals

Get all approval history records for a document.

**Path Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| entityName | string | Plural lowercase entity name |
| id | int | Document ID |

**Response 200**:
```json
[
  {
    "id": 1,
    "documentType": "Budget",
    "documentId": 42,
    "approvalStep": 1,
    "action": 1,
    "decision": "Submitted -> Approved",
    "approverUserId": 5,
    "approverUserName": "Ahmed Ali",
    "requiredRole": "BudgetApprover",
    "reason": null,
    "evaluationSnapshot": "{ ... }",
    "decisionAt": "2026-09-05T10:30:00Z"
  }
]
```

**Response 404**: Problem details if entityName is not a recognized document type

---

## GET /api/Documents/{entityName}/{id}/status-log

Get all status log entries for a document, ordered chronologically.

**Path Parameters**: Same as approvals

**Response 200**:
```json
[
  {
    "id": 1,
    "entityName": "budgets",
    "documentId": 42,
    "fromStatus": "Draft",
    "toStatus": "Submitted",
    "changedById": 5,
    "changedByName": "Ahmed Ali",
    "changedAt": "2026-09-05T09:00:00Z",
    "reason": null
  },
  {
    "id": 2,
    "entityName": "budgets",
    "documentId": 42,
    "fromStatus": "Submitted",
    "toStatus": "Approved",
    "changedById": 3,
    "changedByName": "Sara Mohamed",
    "changedAt": "2026-09-05T10:30:00Z",
    "reason": "Budget meets all criteria"
  }
]
```

---

## GET /api/Documents/{entityName}/{id}/attachments

Get all attachments for a document.

**Response 200**:
```json
[
  {
    "id": 7,
    "entityName": "budgets",
    "documentId": 42,
    "documentType": "Budget",
    "attachmentTypeCode": "BOQ",
    "isRequired": true,
    "fileName": "boq-2026.xlsx",
    "mimeType": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    "storagePath": "attachments/budgets/42/boq-2026.xlsx",
    "sizeBytes": 102400,
    "uploadedById": 5,
    "uploadedByName": "Ahmed Ali",
    "createdAt": "2026-09-05T09:00:00Z"
  }
]
```

---

## POST /api/Documents/{entityName}/{id}/attachments

Upload a file as an attachment.

**Request**: `multipart/form-data`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| file | file | Yes | The file to upload (max 50MB) |
| attachmentTypeCode | string | Yes | Code linking to DocumentAttachmentRequirement |
| documentType | string | Yes | Document type (e.g., "Budget") |

**Response 201**: Created attachment object

**Response 400**: Validation errors (file too large, invalid type code)
**Response 404**: Document not found

---

## DELETE /api/Documents/{entityName}/{id}/attachments/{attachmentId}

Delete an attachment and its stored file.

**Response 204**: No content

**Response 404**: Attachment not found

---

## GET /api/Documents/{entityName}/{id}/attachments/requirements

Get attachment requirements for a document type, showing which are fulfilled.

**Response 200**:
```json
{
  "documentType": "Budget",
  "requirements": [
    {
      "attachmentTypeCode": "BOQ",
      "titleAr": "جدول الكميات",
      "isMandatory": true,
      "isFulfilled": true,
      "attachmentCount": 1
    },
    {
      "attachmentTypeCode": "CONTRACT",
      "titleAr": "العقد",
      "isMandatory": true,
      "isFulfilled": false,
      "attachmentCount": 0
    }
  ],
  "allMandatoryFulfilled": false
}
```
