# API Contracts: Unified Party Registry & Shared Document Panels

**Feature**: 022-unified-party-registry
**Date**: 2026-09-06

> Contracts for frontend consumption. Backend endpoints already exist unless marked `[NEW]`.

## Parties Endpoints (existing)

### GET /api/Parties

**Query params**: `partyType?` (int), `isActive?` (bool), `search?` (string)
**Response**: `200 OK` — `PartyResponse[]`

### GET /api/Parties/{id}

**Response**: `200 OK` — `PartyResponse`

### POST /api/Parties

**Body**: `CreatePartyCommand`
**Response**: `201 Created` — `int` (new party ID)

### PUT /api/Parties/{id}

**Body**: `UpdatePartyCommand`
**Response**: `200 OK` — `Result`

### PATCH /api/Parties/{id}/toggle-active

**Response**: `200 OK` — `Result`

---

## Parties Endpoints [NEW]

### GET /api/Parties/{id}/documents

**Purpose**: Related documents for party detail page (US4)
**Response**: `200 OK` — `PartyDocumentResponse[]`

```json
[
  {
    "documentType": "ReceiptVoucher",
    "documentId": 42,
    "documentNumber": "RV-000042",
    "status": "Active",
    "date": "2026-09-01T00:00:00Z",
    "amount": 15000.00
  }
]
```

---

## Documents Endpoints (existing)

### GET /api/Documents/{documentType}/{documentId}/approvals

**Response**: `200 OK` — `ApprovalHistory[]`

### GET /api/Documents/{documentType}/{documentId}/status-log

**Response**: `200 OK` — `DocumentStatusLog[]`

### GET /api/Documents/{documentType}/{documentId}/attachments

**Response**: `200 OK` — `Attachment[]`

### GET /api/Documents/{documentType}/attachment-requirements

**Response**: `200 OK` — `DocumentAttachmentRequirement[]`

### POST /api/Documents/{documentType}/{documentId}/attachments

**Query param**: `attachmentTypeCode?` (string, default "GENERAL")
**Body**: `multipart/form-data` with `file` field
**Response**: `201 Created` — `Result<int>`

### DELETE /api/Documents/attachments/{id}

**Response**: `200 OK` — empty body

---

## Documents Endpoints [NEW]

### GET /api/Documents/{documentType}/{documentId}/attachment-gate-check

**Purpose**: Check which mandatory attachments are missing (US3 gate badge)
**Response**: `200 OK` — `string[]` (list of missing type codes; empty = all satisfied)

```json
["INVOICE"]
```

### GET /api/Documents/attachments/{id}/download

**Purpose**: Download/view an attached file
**Response**: `200 OK` — file stream with appropriate `Content-Type` and `Content-Disposition` headers

---

## Frontend Client Methods

### features/parties/shared/client.ts — partiesClient

| Method | HTTP | Route | Returns |
|--------|------|-------|---------|
| `list(filters)` | GET | `/api/Parties` | `PartyResponse[]` |
| `getById(id)` | GET | `/api/Parties/{id}` | `PartyResponse` |
| `create(data)` | POST | `/api/Parties` | `number` (ID) |
| `update(id, data)` | PUT | `/api/Parties/{id}` | `void` |
| `toggleActive(id)` | PATCH | `/api/Parties/{id}/toggle-active` | `void` |
| `getDocuments(id)` | GET | `/api/Parties/{id}/documents` | `PartyDocumentResponse[]` |

### features/documents/shared/client.ts — documentsClient

| Method | HTTP | Route | Returns |
|--------|------|-------|---------|
| `getApprovals(type, id)` | GET | `/api/Documents/{type}/{id}/approvals` | `ApprovalRecord[]` |
| `getStatusLog(type, id)` | GET | `/api/Documents/{type}/{id}/status-log` | `StatusLogRecord[]` |
| `getAttachments(type, id)` | GET | `/api/Documents/{type}/{id}/attachments` | `AttachmentRecord[]` |
| `getRequirements(type)` | GET | `/api/Documents/{type}/attachment-requirements` | `AttachmentRequirement[]` |
| `checkGate(type, id)` | GET | `/api/Documents/{type}/{id}/attachment-gate-check` | `string[]` |
| `upload(type, id, file, code)` | POST | `/api/Documents/{type}/{id}/attachments` | `number` (ID) |
| `deleteAttachment(id)` | DELETE | `/api/Documents/attachments/{id}` | `void` |
| `downloadAttachment(id)` | GET | `/api/Documents/attachments/{id}/download` | `Blob` |
