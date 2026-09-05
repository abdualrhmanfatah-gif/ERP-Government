# API Contracts: Receipt Voucher & Deposit Slip Workflow

**Date**: 2026-09-05
**Feature**: 017-receipt-voucher-deposit

## Base URL

```
/api/ReceiptVouchers
/api/DepositSlips
/api/Checks
```

## ReceiptVouchers

### POST /api/ReceiptVouchers

Create a new receipt voucher with line items.

**Request**:
```json
{
  "voucherDate": "2026-09-05",
  "partyId": 123,
  "paymentMethod": 1,
  "receivedFrom": "Mohammed Al-Rashid",
  "notes": "Monthly tax payment",
  "lines": [
    {
      "revenueAccountId": 456,
      "amount": 5000.00,
      "description": "September 2026 tax"
    },
    {
      "revenueAccountId": 789,
      "amount": 2500.00,
      "description": "License fee"
    }
  ],
  "checks": [
    {
      "bankName": "Al Rajhi Bank",
      "checkNumber": "123456",
      "checkDate": "2026-09-05",
      "amount": 7500.00
    }
  ]
}
```

**Response** (201 Created):
```json
{
  "id": 1,
  "voucherNumber": "RCV-000001",
  "status": "Draft",
  "totalAmount": 7500.00,
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `ReceiptVouchers.Create`

---

### POST /api/ReceiptVouchers/{id}/submit

Submit voucher for review.

**Request**: Empty body

**Response** (200 OK):
```json
{
  "id": 1,
  "voucherNumber": "RCV-000001",
  "status": "PendingReview",
  "submittedAt": "2026-09-05T10:30:00Z",
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `ReceiptVouchers.Submit`

---

### POST /api/ReceiptVouchers/{id}/approve

Approve voucher (reviewer gate enforced).

**Request**:
```json
{
  "reason": "Verified against party records"
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "voucherNumber": "RCV-000001",
  "status": "Approved",
  "reviewedAt": "2026-09-05T14:00:00Z",
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `ReceiptVouchers.Approve`

**Errors**:
- `400 Bad Request`: "Voucher must be in PendingReview status"
- `403 Forbidden`: "Reviewer cannot be the same as submitter"

---

### POST /api/ReceiptVouchers/{id}/cancel

Cancel voucher.

**Request**:
```json
{
  "reason": "Party requested cancellation"
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "voucherNumber": "RCV-000001",
  "status": "Cancelled",
  "cancellationReason": "Party requested cancellation",
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `ReceiptVouchers.Cancel`

---

### GET /api/ReceiptVouchers

List vouchers with optional filters.

**Query Parameters**:
- `partyId` (int, optional)
- `status` (int, optional)
- `fromDate` (date, optional)
- `toDate` (date, optional)
- `page` (int, default 1)
- `pageSize` (int, default 20)

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": 1,
      "voucherNumber": "RCV-000001",
      "voucherDate": "2026-09-05",
      "partyId": 123,
      "partyName": "Mohammed Al-Rashid",
      "paymentMethod": "Cash",
      "totalAmount": 7500.00,
      "status": "Approved"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

**Authorization**: `ReceiptVouchers.View`

---

### GET /api/ReceiptVouchers/{id}

Get voucher details with lines and checks.

**Response** (200 OK):
```json
{
  "id": 1,
  "voucherNumber": "RCV-000001",
  "voucherDate": "2026-09-05",
  "partyId": 123,
  "partyName": "Mohammed Al-Rashid",
  "paymentMethod": "Cash",
  "receivedFrom": "Mohammed Al-Rashid",
  "notes": "Monthly tax payment",
  "status": "Approved",
  "totalAmount": 7500.00,
  "depositSlipId": null,
  "lines": [
    {
      "id": 1,
      "revenueAccountId": 456,
      "revenueAccountName": "Tax Revenue",
      "amount": 5000.00,
      "description": "September 2026 tax"
    },
    {
      "id": 2,
      "revenueAccountId": 789,
      "revenueAccountName": "License Fees",
      "amount": 2500.00,
      "description": "License fee"
    }
  ],
  "checks": [
    {
      "id": 1,
      "bankName": "Al Rajhi Bank",
      "checkNumber": "123456",
      "checkDate": "2026-09-05",
      "amount": 7500.00,
      "status": "UnderCollection"
    }
  ],
  "statusHistory": [
    {
      "fromStatus": null,
      "toStatus": "Draft",
      "changedAt": "2026-09-05T10:00:00Z",
      "changedBy": "cashier1"
    },
    {
      "fromStatus": "Draft",
      "toStatus": "PendingReview",
      "changedAt": "2026-09-05T10:30:00Z",
      "changedBy": "cashier1"
    },
    {
      "fromStatus": "PendingReview",
      "toStatus": "Approved",
      "changedAt": "2026-09-05T14:00:00Z",
      "changedBy": "reviewer1",
      "reason": "Verified against party records"
    }
  ],
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `ReceiptVouchers.View`

---

### GET /api/ReceiptVouchers/by-party/{partyId}

List vouchers for a specific party.

**Response**: Same as GET /api/ReceiptVouchers

**Authorization**: `ReceiptVouchers.View`

---

### GET /api/ReceiptVouchers/by-period?fromDate={from}&toDate={to}

List vouchers for a date range.

**Response**: Same as GET /api/ReceiptVouchers

**Authorization**: `ReceiptVouchers.View`

---

## DepositSlips

### POST /api/DepositSlips

Create a new deposit slip.

**Request**:
```json
{
  "slipDate": "2026-09-05",
  "formType": 47,
  "voucherIds": [1, 2, 3]
}
```

**Response** (201 Created):
```json
{
  "id": 1,
  "slipNumber": "DSL-000001",
  "slipDate": "2026-09-05",
  "formType": 47,
  "status": "Draft",
  "totalAmount": 15000.00,
  "memberCount": 3,
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `DepositSlips.Create`

**Errors**:
- `400 Bad Request`: "All vouchers must be Approved"
- `400 Bad Request`: "Form 47 requires cash-only vouchers"
- `400 Bad Request`: "Slip date must be on or after latest voucher date"
- `400 Bad Request`: "Slip date cannot be in the future"

---

### POST /api/DepositSlips/{id}/add-voucher

Add a voucher to an existing slip.

**Request**:
```json
{
  "voucherId": 4
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "slipNumber": "DSL-000001",
  "totalAmount": 20000.00,
  "memberCount": 4,
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `DepositSlips.Manage`

**Errors**:
- `400 Bad Request`: "Voucher must be Approved"
- `400 Bad Request`: "Voucher payment method does not match slip form type"

---

### POST /api/DepositSlips/{id}/remove-voucher

Remove a voucher from a slip.

**Request**:
```json
{
  "voucherId": 4,
  "reason": "Voucher amount incorrect"
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "slipNumber": "DSL-000001",
  "totalAmount": 15000.00,
  "memberCount": 3,
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `DepositSlips.Manage`

---

### POST /api/DepositSlips/{id}/approve

Approve deposit slip (Treasury manager only).

**Request**:
```json
{
  "reason": "Verified all vouchers"
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "slipNumber": "DSL-000001",
  "status": "Approved",
  "approvedAt": "2026-09-05T16:00:00Z",
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `DepositSlips.Approve`

**Side Effects**:
- Form 47: Triggers `ReceiptVoucherCollected` event → JournalEntry created
- Form 48: Updates member voucher checks to `UnderCollection` status

---

### GET /api/DepositSlips

List deposit slips with optional filters.

**Query Parameters**:
- `status` (int, optional)
- `formType` (int, optional)
- `fromDate` (date, optional)
- `toDate` (date, optional)
- `page` (int, default 1)
- `pageSize` (int, default 20)

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": 1,
      "slipNumber": "DSL-000001",
      "slipDate": "2026-09-05",
      "formType": 47,
      "status": "Approved",
      "totalAmount": 15000.00,
      "memberCount": 3
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

**Authorization**: `DepositSlips.View`

---

### GET /api/DepositSlips/{id}

Get slip details with member vouchers.

**Response** (200 OK):
```json
{
  "id": 1,
  "slipNumber": "DSL-000001",
  "slipDate": "2026-09-05",
  "formType": 47,
  "status": "Approved",
  "totalAmount": 15000.00,
  "approvedById": 5,
  "approvedByName": "Treasury Manager",
  "approvedAt": "2026-09-05T16:00:00Z",
  "members": [
    {
      "voucherId": 1,
      "voucherNumber": "RCV-000001",
      "voucherDate": "2026-09-05",
      "partyName": "Mohammed Al-Rashid",
      "amount": 5000.00
    },
    {
      "voucherId": 2,
      "voucherNumber": "RCV-000002",
      "voucherDate": "2026-09-05",
      "partyName": "Sara Company",
      "amount": 7000.00
    }
  ],
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `DepositSlips.View`

---

## Checks

### POST /api/Checks/{id}/clear

Confirm check clearing (bank confirmation).

**Request**:
```json
{
  "clearedAt": "2026-09-10T09:00:00Z"
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "status": "Cleared",
  "clearedAt": "2026-09-10T09:00:00Z",
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `Checks.Clear`

**Side Effects**: Triggers `CheckCleared` event → JournalEntry created (Dr bank GL, Cr revenue)

---

### POST /api/Checks/{id}/bounce

Report check bounce (bank notification).

**Request**:
```json
{
  "bouncedAt": "2026-09-10T09:00:00Z",
  "reason": "Insufficient funds"
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "status": "Bounced",
  "bouncedAt": "2026-09-10T09:00:00Z",
  "replacementVoucherId": 5,
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `Checks.Bounce`

**Side Effects**: Creates new ReceiptVoucher in Draft status for replacement payment

---

### POST /api/Checks/{id}/replace

Record replacement payment for bounced check.

**Request**:
```json
{
  "paymentMethod": 2,
  "checkDetails": {
    "bankName": "Al Rajhi Bank",
    "checkNumber": "789012",
    "checkDate": "2026-09-12",
    "amount": 7500.00
  }
}
```

**Response** (200 OK):
```json
{
  "id": 1,
  "status": "Replaced",
  "replacementVoucherId": 5,
  "rowVersion": "AAAAAAAAB9k="
}
```

**Authorization**: `Checks.Clear`

---

### GET /api/Checks

List checks with optional filters.

**Query Parameters**:
- `status` (int, optional)
- `fromDate` (date, optional)
- `toDate` (date, optional)
- `page` (int, default 1)
- `pageSize` (int, default 20)

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": 1,
      "voucherNumber": "RCV-000001",
      "bankName": "Al Rajhi Bank",
      "checkNumber": "123456",
      "checkDate": "2026-09-05",
      "amount": 7500.00,
      "status": "UnderCollection"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

**Authorization**: `Checks.View`

---

## Statements

### GET /api/Statements/monthly?year=2026&month=9&fundId=1

Generate monthly collections statement.

**Response** (200 OK):
```json
{
  "year": 2026,
  "month": 9,
  "fundId": 1,
  "fundName": "General Fund",
  "generatedAt": "2026-09-30T23:59:59Z",
  "summary": {
    "totalCashCollections": 50000.00,
    "totalCheckCollections": 75000.00,
    "totalDeposited": 100000.00,
    "totalUnderCollection": 25000.00,
    "totalCleared": 20000.00,
    "totalBounced": 5000.00
  },
  "vouchers": [
    {
      "voucherNumber": "RCV-000001",
      "voucherDate": "2026-09-05",
      "partyName": "Mohammed Al-Rashid",
      "paymentMethod": "Cash",
      "amount": 5000.00,
      "depositSlipNumber": "DSL-000001"
    }
  ],
  "clearings": [
    {
      "checkNumber": "123456",
      "bankName": "Al Rajhi Bank",
      "clearedAt": "2026-09-10T09:00:00Z",
      "amount": 7500.00
    }
  ]
}
```

**Authorization**: `ReceiptVouchers.View`
