# Contract: /api/Parties

**Route Prefix**: `/api/Parties`
**Endpoint Group**: `Web/Endpoints/Parties/Parties.cs`

## GET /api/Parties

List parties with optional filters and search.

**Query Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| PartyType | int | No | Filter by PartyType enum value (0-4) |
| IsActive | bool | No | Filter by active status |
| search | string | No | Search by NameAr, NameEn, or TaxNumber (contains match) |
| page | int | No | Page number (default: 1) |
| pageSize | int | No | Page size (default: 20, max: 100) |

**Response 200**:
```json
{
  "items": [
    {
      "id": 1,
      "partyCode": "PTY-000001",
      "partyType": 0,
      "nameAr": "شركة المثال",
      "nameEn": "Example Corp",
      "taxNumber": "123456789",
      "nationalId": null,
      "phone": "+966501234567",
      "email": "info@example.com",
      "address": "الرياض، المملكة العربية السعودية",
      "notes": null,
      "isActive": true,
      "rowVersion": "AAAAAA==",
      "createdAt": "2026-09-05T00:00:00Z",
      "createdBy": "admin",
      "lastModifiedAt": "2026-09-05T00:00:00Z",
      "lastModifiedBy": "admin"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20
}
```

## GET /api/Parties/{id}

Get a single party by ID.

**Response 200**: Party object (same shape as list item)

**Response 404**: Problem details with "Party not found."

## POST /api/Parties

Create a new party. PartyCode is auto-assigned.

**Request Body**:
```json
{
  "partyType": 1,
  "nameAr": "شركة جديدة",
  "nameEn": "New Company",
  "taxNumber": "987654321",
  "nationalId": null,
  "phone": "+966509876543",
  "email": "new@example.com",
  "address": null,
  "notes": null
}
```

**Validation**:
- `partyType` must be a valid PartyType enum value
- `nameAr` is required, non-empty
- `email` must be valid format if provided
- `taxNumber` uniqueness is recommended but not enforced at DB level (business dedup)

**Response 201**: Created party object with generated `partyCode`

**Response 400**: Validation errors

## PUT /api/Parties/{id}

Full update of a party. Requires `rowVersion` for concurrency.

**Request Body**: Same as POST plus `rowVersion`

**Response 200**: Updated party object

**Response 400**: Validation errors
**Response 404**: Party not found
**Response 409**: Concurrency conflict (row version mismatch)

## PATCH /api/Parties/{id}/toggle-active

Toggle the IsActive flag. Requires `rowVersion`.

**Request Body**:
```json
{
  "rowVersion": "AAAAAA=="
}
```

**Response 200**: Updated party object with toggled `isActive`

**Response 404**: Party not found
**Response 409**: Concurrency conflict
