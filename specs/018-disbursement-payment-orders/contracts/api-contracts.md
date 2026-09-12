# API Contracts: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-09

All endpoints follow existing conventions: IEndpointGroup, `/api/{ClassName}`, `[Authorize(Policy)]`, `Result<T>` response, `PermissionCodes` authorization.

## DisbursementRequests

### POST /api/DisbursementRequests

**Permission**: `DisbursementRequests.Create`
**Purpose**: Create a disbursement request (standalone — no source document required).

**Request**:
```json
{
  "beneficiaryName": "Acme Corp",
  "requestedAmount": 35000.00,
  "currencyId": 1,
  "purpose": "Payment for services",
  "financialYearId": 5,
  "notes": "string?"
}
```

**Response 201**:
```json
{
  "id": 1,
  "requestNumber": "DSB-000001",
  "requestedById": 5,
  "requestedByName": "ahmed.ali",
  "beneficiaryName": "Acme Corp",
  "requestedAmount": 35000.00,
  "currencyId": 1,
  "purpose": "Payment for services",
  "financialYearId": 5,
  "requestDate": "2026-09-09",
  "status": "Draft",
  "notes": "string?"
}
```

**Response 400** (validation):
```json
{
  "errors": ["Requested amount must be greater than zero."]
}
```

---

### PUT /api/DisbursementRequests/{id}

**Permission**: `DisbursementRequests.Update`
**Purpose**: Edit a Draft disbursement request. After first approval, only notes and purpose may be updated.

**Request**:
```json
{
  "beneficiaryName": "Acme Corp (updated)",
  "requestedAmount": 40000.00,
  "currencyId": 1,
  "purpose": "Updated purpose",
  "financialYearId": 5,
  "notes": "Updated notes",
  "rowVersion": "AAAAAAAAB="
}
```

**Response 200**: Updated DisbursementRequestDto.

**Response 400** (not Draft):
```json
{
  "errors": ["Only draft requests can be edited."]
}
```

**Response 400** (amount frozen):
```json
{
  "errors": ["Requested amount is frozen after the first approval. Only notes and purpose may be updated."]
}
```

**Response 409** (concurrency):
```json
{
  "errors": ["The record has been modified by another user. Please refresh and try again."]
}
```

---

### PATCH /api/DisbursementRequests/{id}/submit

**Permission**: `DisbursementRequests.Submit`
**Purpose**: Submit a Draft request for dual-signature approval.

**Request**: Empty body.

**Response 200**: `Result` with status transition to PendingApproval.

---

### PATCH /api/DisbursementRequests/{id}/approve

**Permission**: `DisbursementRequests.Approve`
**Purpose**: Record an approval decision. Both approvers must hold AccountsManager or AuthorizingOfficer role. Both must authorize the same amount.

**Request**:
```json
{
  "approvedAmount": 35000.00,
  "issuingAuthorityName": "Mohammed Al-Said",
  "issuingAuthorityCapacity": "General Manager",
  "reason": "string?",
  "rowVersion": "AAAAAAAAB="
}
```

**Response 200** (first approval):
```json
{
  "status": "PendingApproval",
  "approvalStep": 1,
  "message": "First approval recorded. Awaiting second approver."
}
```

**Response 200** (second approval — order generated):
```json
{
  "status": "Approved",
  "approvalStep": 2,
  "message": "Disbursement request approved. Payment order PO-000001 generated."
}
```

**Response 400** (same user):
```json
{
  "errors": ["A different approver is required."]
}
```

**Response 400** (wrong role):
```json
{
  "errors": ["Approver must hold AccountsManager or AuthorizingOfficer role."]
}
```

**Response 400** (amount exceeds requested):
```json
{
  "errors": ["Approved amount cannot exceed the requested amount."]
}
```

**Response 400** (amount mismatch on step 2):
```json
{
  "errors": ["Signatures on different amounts cannot finalize. Reauthorization required."]
}
```

---

### PATCH /api/DisbursementRequests/{id}/reject

**Permission**: `DisbursementRequests.Reject`
**Purpose**: Reject a PendingApproval request.

**Request**:
```json
{
  "reason": "Insufficient documentation",
  "rowVersion": "AAAAAAAAB="
}
```

**Response 200**: `Result` with status transition to Rejected.

---

### PATCH /api/DisbursementRequests/{id}/cancel

**Permission**: `DisbursementRequests.Cancel`
**Purpose**: Cancel a Draft, PendingApproval, or Approved (unpaid) request. Invalidates linked PaymentOrder if generated.

**Request**:
```json
{
  "reason": "Changed mind",
  "rowVersion": "AAAAAAAAB="
}
```

**Response 200**: `Result` with status transition to Cancelled.

**Response 400** (order already paid):
```json
{
  "errors": ["Cannot cancel — the linked payment order has already been paid."]
}
```

---

### GET /api/DisbursementRequests

**Permission**: `DisbursementRequests.View`
**Purpose**: List disbursement requests with filters.

**Query Parameters**:
- `status` (optional): Filter by DisbursementRequestStatus
- `requestedById` (optional): Filter by requester

**Response 200**:
```json
[
  {
    "id": 1,
    "requestNumber": "DSB-000001",
    "requestedById": 5,
    "requestedByName": "ahmed.ali",
    "beneficiaryName": "Acme Corp",
    "requestedAmount": 35000.00,
    "currencyId": 1,
    "purpose": "Payment for services",
    "financialYearId": 5,
    "requestDate": "2026-09-09",
    "status": "Approved",
    "notes": "..."
  }
]
```

---

### GET /api/DisbursementRequests/{id}

**Permission**: `DisbursementRequests.View`
**Purpose**: Get disbursement request details with approvals and linked order.

**Response 200**:
```json
{
  "id": 1,
  "requestNumber": "DSB-000001",
  "requestedById": 5,
  "requestedByName": "ahmed.ali",
  "beneficiaryName": "Acme Corp",
  "requestedAmount": 35000.00,
  "currencyId": 1,
  "purpose": "Payment for services",
  "financialYearId": 5,
  "requestDate": "2026-09-09",
  "status": "Approved",
  "notes": "...",
  "paymentDate": "2026-09-09T15:00:00Z",
  "paymentOrderId": 42,
  "paymentOrderNumber": "PO-000042",
  "approvals": [
    {
      "step": 1,
      "approverUserId": 10,
      "approverName": "sara.hassan",
      "role": "AccountsManager",
      "decision": "Approved",
      "decisionAt": "2026-09-09T10:30:00Z",
      "approvedAmount": 35000.00,
      "issuingAuthorityName": "Mohammed Al-Said",
      "issuingAuthorityCapacity": "General Manager"
    },
    {
      "step": 2,
      "approverUserId": 15,
      "approverName": "omar.khan",
      "role": "AuthorizingOfficer",
      "decision": "Approved",
      "decisionAt": "2026-09-09T14:00:00Z",
      "approvedAmount": 35000.00,
      "issuingAuthorityName": "Mohammed Al-Said",
      "issuingAuthorityCapacity": "General Manager"
    }
  ],
  "rowVersion": "AAAAAAAAB="
}
```

---

## PaymentOrders

### GET /api/PaymentOrders

**Permission**: `PaymentOrders.View`
**Purpose**: List payment orders with filters.

**Query Parameters**:
- `status` (optional): Filter by PaymentOrderStatus
- `fundId` (optional): Filter by fund
- `fiscalYearId` (optional): Filter by fiscal year

---

### GET /api/PaymentOrders/{id}

**Permission**: `PaymentOrders.View`
**Purpose**: Get payment order details with deductions and linked request.

---

### GET /api/PaymentOrders/{id}/totals

**Permission**: `PaymentOrders.View`
**Purpose**: Get computed totals (gross, deductions, net, paid, remaining, isFullyPaid).

---

### PUT /api/PaymentOrders/{id}

**Permission**: `PaymentOrders.Update`
**Purpose**: Update a Draft payment order (header + deductions). Used to prepare auto-generated orders before submission.

---

### PATCH /api/PaymentOrders/{id}/submit

**Permission**: `PaymentOrders.Submit`
**Purpose**: Submit for approval. Requires FundId and AppropriationId. Runs budget check.

---

### PATCH /api/PaymentOrders/{id}/approve

**Permission**: `PaymentOrders.Approve`
**Purpose**: Approve a Submitted order. Supports OverrideFailedBudgetCheck.

---

### PATCH /api/PaymentOrders/{id}/reject

**Permission**: `PaymentOrders.Reject`
**Purpose**: Reject a Submitted order.

---

### PATCH /api/PaymentOrders/{id}/cancel

**Permission**: `PaymentOrders.Cancel`
**Purpose**: Cancel a Draft or Submitted order. Invalidates linked DisbursementRequest.

---

### PATCH /api/PaymentOrders/{id}/send-to-treasury

**Permission**: `PaymentOrders.SendToTreasury`
**Purpose**: Send an Approved order to treasury.

---

### PATCH /api/PaymentOrders/{id}/void

**Permission**: `PaymentOrders.Void`
**Purpose**: Void an Approved/SentToTreasury order (no completed payment). Invalidates linked DisbursementRequest.

---

## Payments

### POST /api/Payments

**Permission**: `Payments.Create`
**Purpose**: Record payment execution against an Approved or SentToTreasury order.

**Request**:
```json
{
  "paymentOrderId": 42,
  "paymentMethod": "Check",
  "referenceNumber": "CHK-2026-09-001",
  "notes": "string?"
}
```

**Response 201**:
```json
{
  "id": 1,
  "paymentNumber": "PAY-000001",
  "disbursementRequestId": 1,
  "disbursementRequestNumber": "DSB-000001",
  "paymentOrderId": 42,
  "paymentOrderNumber": "PO-000042",
  "paymentMethod": "Check",
  "amount": 35000.00,
  "paidById": 5,
  "paidByName": "ahmed.ali",
  "paidAt": "2026-09-09T15:00:00Z",
  "referenceNumber": "CHK-2026-09-001",
  "status": "Completed",
  "beneficiaryName": "Acme Corp"
}
```

**Response 400** (already paid):
```json
{
  "errors": ["A payment has already been recorded for this payment order."]
}
```

**Response 400** (order not approved):
```json
{
  "errors": ["Payment order must be approved before payment execution."]
}
```

---

### GET /api/Payments

**Permission**: `Payments.View`
**Purpose**: List payments with filters.

---

### GET /api/Payments/{id}

**Permission**: `Payments.View`
**Purpose**: Get payment details.
