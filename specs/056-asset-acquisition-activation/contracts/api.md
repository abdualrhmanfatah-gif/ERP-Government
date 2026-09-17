# API Contracts: Asset Acquisition & Activation

**Feature**: 056-asset-acquisition-activation | **Date**: 2026-09-15

## Endpoint Group: `/api/Assets`

| Verb | Route | Handler | Permission | Produces |
|------|-------|---------|------------|----------|
| POST | `/api/Assets/{id}/activate` | HandleActivate | Assets.Update | Result |

---

## POST /api/Assets/{id}/activate

**Purpose**: Activate a Draft asset by creating an acquisition journal entry

**Request Body**:

```json
{
  "depreciationStartDate": "2025-02-01",
  "rowVersion": "AAAAAA=="
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| depreciationStartDate | string (date) | Yes | When depreciation begins (defaults to activation date if not provided) |
| rowVersion | string (base64) | Yes | Optimistic concurrency token |

**Notes**:
- Asset must be in Draft status
- OriginalValue must be > 0
- PurchaseDate must be set
- AssetGroup must have AccountAssetId configured
- Accounting period for activation date must be open

**Response 200**: `Result.Success()`

```json
{
  "succeeded": true,
  "value": null
}
```

**Response 400**: Validation errors

```json
{
  "type": "about:blank",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "Status": ["لا يمكن تفعيل الأصل من حالة Draft"]
  }
}
```

**Response 404**: Asset not found

```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "errors": {
    "code": "Assets.AssetNotFound",
    "message": "الأصل بالمعرف 1 غير موجود"
  }
}
```

**Response 409**: Concurrency conflict

```json
{
  "type": "about:blank",
  "title": "Conflict",
  "status": 409,
  "errors": {
    "code": "ConcurrencyConflict",
    "message": "تم تعديل الأصل من مستخدم آخر"
  }
}
```

---

## Updated GET /api/Assets/{id} Response

The asset detail response now includes the `journalEntryId` field:

```json
{
  "id": 1,
  "code": "AST-000001",
  "name": "طابعة HP",
  "status": "Active",
  "activationDate": "2025-02-01",
  "acquisitionCost": 1500.00,
  "journalEntryId": 42,
  "journalEntryNumber": "JRN-000042",
  "...": "..."
}
```

| Field | Type | Description |
|-------|------|-------------|
| journalEntryId | int? | ID of the acquisition journal entry (null if Draft) |
| journalEntryNumber | string? | Entry number of the acquisition journal entry (null if Draft) |

---

## Updated AssetDetailResponse Record

```csharp
public record AssetDetailResponse(
    // ... existing fields ...
    int? JournalEntryId,        // NEW
    string? JournalEntryNumber  // NEW
);
```

---

## Error Response Contract

All error responses follow the problem-details format:

```json
{
  "type": "about:blank",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "FieldName": ["Error message in Arabic"]
  }
}
```

Error codes are machine-readable; messages are user-friendly Arabic text.
