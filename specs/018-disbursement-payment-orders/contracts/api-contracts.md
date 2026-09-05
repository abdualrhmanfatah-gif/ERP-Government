# API Contracts: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-05

All endpoints follow existing conventions: IEndpointGroup, `/api/{ClassName}`, `[Authorize(Policy)]`, `Result<T>` response, `PermissionCodes` authorization.

## DisbursementRequests

### POST /api/DisbursementRequests

**Permission**: `DisbursementRequests.Create`
**Purpose**: Create a disbursement request against an approved payment order. Runs budget availability gate.

**Request**:
```json
{
  "paymentOrderId": 42,
  "notes": "string?"
}
```

**Response 201**:
```json
{
  "id": 1,
  "requestNumber": "DSB-000001",
  "paymentOrderId": 42,
  "requestedById": 5,
  "requestDate": "2026-09-05",
  "status": "Draft",
  "hasWarning": false
}
```

**Response 400** (Blocking + insufficient funds):
```json
{
  "errors": ["Budget availability insufficient."],
  "availabilityBreakdown": {
    "netAppropriated": 100000.00,
    "encumbered": 80000.00,
    "available": 20000.00,
    "requested": 35000.00,
    "shortfall": 15000.00
  }
}
```

**Response 400** (other validation):
```json
{
  "errors": ["Payment order must be approved first."]
}
```

---

### PATCH /api/DisbursementRequests/{id}/submit

**Permission**: `DisbursementRequests.Submit`
**Purpose**: Submit a Draft request for dual-signature approval.

**Request**: Empty body.

**Response 200**: `Result` with `status: "PendingApproval"`

---

### PATCH /api/DisbursementRequests/{id}/approve

**Permission**: `DisbursementRequests.Approve`
**Purpose**: Record an approval decision. First approval requires AccountsManager or AuthorizingOfficer role. Second approval must be from a distinct user.

**Request**:
```json
{
  "reason": "string?"
}
```

**Response 200**:
```json
{
  "status": "PendingApproval",
  "approvalStep": 1,
  "message": "First approval recorded. Awaiting second approver."
}
```

**Response 200** (second approval):
```json
{
  "status": "Approved",
  "approvalStep": 2,
  "message": "Disbursement request approved."
}
```

**Response 400** (same user):
```json
{
  "errors": ["A different approver is required."]
}
```

**Response 400** (wrong role on first approval):
```json
{
  "errors": ["First approver must hold AccountsManager or AuthorizingOfficer role."]
}
```

---

### PATCH /api/DisbursementRequests/{id}/reject

**Permission**: `DisbursementRequests.Reject`
**Purpose**: Reject a Pending Approval request.

**Request**:
```json
{
  "reason": "string"
}
```

**Response 200**: `Result` with `status: "Rejected"`

---

### PATCH /api/DisbursementRequests/{id}/cancel

**Permission**: `DisbursementRequests.Cancel`
**Purpose**: Cancel an Approved or Draft request. Clears the payment order link.

**Request**:
```json
{
  "reason": "string"
}
```

**Response 200**: `Result` with `status: "Cancelled"`

---

### GET /api/DisbursementRequests

**Permission**: `DisbursementRequests.View`
**Purpose**: List disbursement requests with filters (register query).

**Query Parameters**:
- `status` (optional): Filter by DisbursementRequestStatus
- `fundId` (optional): Filter by linked payment order's fund
- `fromDate` (optional): Filter by request date range start
- `toDate` (optional): Filter by request date range end
- `paymentOrderId` (optional): Filter by specific payment order

**Response 200**:
```json
[
  {
    "id": 1,
    "requestNumber": "DSB-000001",
    "paymentOrderNumber": "PO-000042",
    "payeeName": "Acme Corp",
    "requestedAmount": 35000.00,
    "status": "Approved",
    "requestDate": "2026-09-05",
    "approvalDate": "2026-09-05",
    "paymentDate": null,
    "fundName": "General Fund"
  }
]
```

---

### GET /api/DisbursementRequests/{id}

**Permission**: `DisbursementRequests.View`
**Purpose**: Get disbursement request details.

**Response 200**:
```json
{
  "id": 1,
  "requestNumber": "DSB-000001",
  "paymentOrderId": 42,
  "paymentOrderNumber": "PO-000042",
  "requestedById": 5,
  "requestedByName": "Ahmed Ali",
  "requestDate": "2026-09-05",
  "status": "Approved",
  "hasWarning": false,
  "notes": "...",
  "approvals": [
    {
      "step": 1,
      "approverUserId": 10,
      "approverName": "Sara Hassan",
      "role": "AccountsManager",
      "decision": "Approved",
      "decisionAt": "2026-09-05T10:30:00Z"
    },
    {
      "step": 2,
      "approverUserId": 15,
      "approverName": "Omar Khan",
      "role": "AuthorizingOfficer",
      "decision": "Approved",
      "decisionAt": "2026-09-05T14:00:00Z"
    }
  ]
}
```

---

## Payments

### POST /api/Payments

**Permission**: `Payments.Create`
**Purpose**: Record payment execution for an approved disbursement request.

**Request**:
```json
{
  "disbursementRequestId": 1,
  "paymentMethod": "BankTransfer",
  "referenceNumber": "TRF-2026-09-001",
  "notes": "string?"
}
```

**Response 201**:
```json
{
  "id": 1,
  "paymentNumber": "PAY-000001",
  "disbursementRequestId": 1,
  "paymentOrderId": 42,
  "paymentMethod": "BankTransfer",
  "amount": 35000.00,
  "paidAt": "2026-09-05T15:00:00Z",
  "referenceNumber": "TRF-2026-09-001",
  "status": "Completed"
}
```

**Response 400**:
```json
{
  "errors": ["Disbursement request must be approved before payment execution."]
}
```

---

### GET /api/Payments

**Permission**: `Payments.View`
**Purpose**: List payments with filters.

**Query Parameters**:
- `status` (optional): Filter by PaymentStatus
- `fundId` (optional): Filter by linked payment order's fund
- `fromDate` (optional): Filter by payment date range start
- `toDate` (optional): Filter by payment date range end

**Response 200**:
```json
[
  {
    "id": 1,
    "paymentNumber": "PAY-000001",
    "disbursementRequestNumber": "DSB-000001",
    "paymentOrderNumber": "PO-000042",
    "payeeName": "Acme Corp",
    "paymentMethod": "BankTransfer",
    "amount": 35000.00,
    "paidAt": "2026-09-05T15:00:00Z",
    "referenceNumber": "TRF-2026-09-001",
    "status": "Completed"
  }
]
```

---

### GET /api/Payments/{id}

**Permission**: `Payments.View`
**Purpose**: Get payment details.

**Response 200**:
```json
{
  "id": 1,
  "paymentNumber": "PAY-000001",
  "disbursementRequestId": 1,
  "disbursementRequestNumber": "DSB-000001",
  "paymentOrderId": 42,
  "paymentOrderNumber": "PO-000042",
  "paymentMethod": "BankTransfer",
  "amount": 35000.00,
  "paidById": 5,
  "paidByName": "Ahmed Ali",
  "paidAt": "2026-09-05T15:00:00Z",
  "referenceNumber": "TRF-2026-09-001",
  "notes": "...",
  "status": "Completed"
}
```
