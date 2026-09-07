# API Contracts: ACC-03 — Journals & Templates

**Source**: Binding contract from `web-api-client.ts` (NSwag-generated)

## Journals

### GET /api/Journals

**Query Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| IsActive | bool | No | Filter by active status |
| Type | JournalType (int) | No | Filter by journal type |

**Response**: `JournalDto[]`

### POST /api/Journals

**Request Body**: `CreateJournalCommand`

| Field | Type | Required |
|-------|------|----------|
| code | string | Yes |
| name | string | Yes |
| type | JournalType (int) | Yes |
| accountId | int? | No |
| suspenseAccountId | int? | No |
| allowForeignCurrency | bool | Yes |
| sequenceId | int? | No |
| requireApprovalBeforePosting | bool | Yes |

**Response**: 200 OK | 400 Bad Request (errors: string[])

### GET /api/Journals/{id}

**Response**: `JournalDto?`

### PUT /api/Journals/{id}

**Request Body**: `UpdateJournalCommand`

| Field | Type | Required |
|-------|------|----------|
| id | int | Yes |
| name | string | Yes |
| type | JournalType (int) | Yes |
| accountId | int? | No |
| suspenseAccountId | int? | No |
| allowForeignCurrency | bool | Yes |
| requireApprovalBeforePosting | bool | Yes |
| rowVersion | byte[] | Yes |

**Response**: 204 No Content | 400 Bad Request

**Server-side guards**:
- Duplicate code → 400 "Journal code already exists."
- Used journal (has entries) → locked fields enforced server-side
- RowVersion mismatch → 409 Conflict

## Templates

### GET /api/Templates

**Query Parameters**:
| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| IsActive | bool | No | Filter by active status |
| JournalId | int | No | Filter by journal |
| TemplateType | JournalEntryTemplateType (int) | No | Filter by type |

**Response**: `JournalEntryTemplateDto[]`

### POST /api/Templates

**Request Body**: `CreateTemplateCommand`

| Field | Type | Required |
|-------|------|----------|
| templateName | string | Yes |
| description | string? | No |
| journalId | int | Yes |
| templateType | JournalEntryTemplateType (int) | Yes |
| isSystemTemplate | bool | Yes |

**Response**: 200 OK | 400 Bad Request

### GET /api/Templates/{id}

**Response**: `JournalEntryTemplateDto?`

### PUT /api/Templates/{id}

**Request Body**: `UpdateTemplateCommand`

| Field | Type | Required |
|-------|------|----------|
| id | int | Yes |
| templateName | string | Yes |
| description | string? | No |
| journalId | int | Yes |
| templateType | JournalEntryTemplateType (int) | Yes |
| rowVersion | byte[] | Yes |

**Response**: 204 No Content | 400 Bad Request

### GET /api/Templates/{id} (amended)

**Response**: `JournalEntryTemplateDto` with `lines: JournalEntryTemplateLineDto[]` ordered by Sequence + `totalDebit/totalCredit/balanced` computed.

### GET /api/Templates/{id}/lines

**Response**: `JournalEntryTemplateLineDto[]` ordered by Sequence.

### POST /api/Templates/{id}/lines

**Request Body**: `CreateTemplateLineCommand` (no sequence — server assigns max+1)

| Field | Type | Required |
|-------|------|----------|
| accountId | int | Yes |
| description | string? | No |
| currencyId | int | Yes |
| exchangeRate | decimal | Yes |
| debit | decimal | Yes |
| credit | decimal | Yes |
| costCenterId | int? | No |

**Response**: 200 OK (id) | 400 Bad Request (XOR / account / currency errors)

### PUT /api/Templates/lines/{lineId}

**Request Body**: `UpdateTemplateLineCommand` (same fields + id + rowVersion, sequence immutable)

**Response**: 204 No Content | 400 | 409 Conflict

### DELETE /api/Templates/lines/{lineId}

**Response**: 204 No Content

**Server-side guards (lines)**:
- debit > 0 && credit > 0 → 400 "A line cannot have both debit and credit amounts."
- debit == 0 && credit == 0 → 400 "A line must have either a debit or credit amount."
- negative → 400 "Amounts cannot be negative."
- account missing / !IsPostable / !IsActive → 400
- RowVersion mismatch → 409 Conflict

## Enums (shared)

### JournalType
```
0 = General, 1 = Purchase, 2 = Sale, 3 = Cash, 4 = Bank, 5 = Adjustment, 6 = Closing
```

### JournalEntryTemplateType
```
0 = Standard, 1 = Recurring, 2 = Adjustment
```
