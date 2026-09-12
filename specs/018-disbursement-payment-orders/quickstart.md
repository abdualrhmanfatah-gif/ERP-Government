# Quickstart Validation Guide: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-09

## Prerequisites

- SQL Server running with the latest migration applied
- Backend project builds: `dotnet build src/Web/Web.csproj`
- Test projects available: `tests/Application.UnitTests`, `tests/Application.FunctionalTests`
- Two test users: one with AccountsManager role + DisbursementRequestsApprove, one with AuthorizingOfficer role + DisbursementRequestsApprove
- A test user with DisbursementRequestsCreate + DisbursementRequestsUpdate permissions
- BudgetAvailabilityService configured with a test budget item (control method = Blocking)
- DocumentSequenceService configured with DSB, PO, PAY prefixes

## Validation Scenarios

### V1: Create Disbursement Request (Happy Path)

1. Call `POST /api/DisbursementRequests` with `beneficiaryName: "Acme Corp"`, `requestedAmount: 35000`, `currencyId: 1`, `purpose: "Services"`, `financialYearId: 5`
2. **Expect**: 201, status = Draft, requestNumber = DSB-000001, requestedByName recorded

### V2: Edit Draft Request

1. Call `PUT /api/DisbursementRequests/1` with updated `requestedAmount: 40000` + `rowVersion`
2. **Expect**: 200, amount updated to 40000

### V3: Amount Frozen After First Approval

1. Submit request: `PATCH /api/DisbursementRequests/1/submit`
2. First approver approves with `approvedAmount: 35000`
3. Try to edit amount: `PUT /api/DisbursementRequests/1` with `requestedAmount: 50000`
4. **Expect**: 400, "Requested amount is frozen after the first approval."

### V4: Notes Editable After First Approval

1. After first approval, update notes: `PUT /api/DisbursementRequests/1` with `notes: "Updated"` + `rowVersion`
2. **Expect**: 200, notes updated, amount unchanged

### V5: Dual-Signature Approval (Happy Path)

1. User A (AccountsManager) calls `PATCH /api/DisbursementRequests/1/approve` with `approvedAmount: 35000`, `issuingAuthorityName: "Mohammed"`, `issuingAuthorityCapacity: "GM"`
2. **Expect**: 200, status = PendingApproval, approvalStep = 1
3. User B (AuthorizingOfficer, distinct) calls `PATCH /api/DisbursementRequests/1/approve` with same amount + authority
4. **Expect**: 200, status = Approved, PaymentOrder auto-generated (Draft, FundId=0)

### V6: Same User Rejected

1. After first approval by User A, User A tries to approve again
2. **Expect**: 400, "A different approver is required."

### V7: Role Check on Both Steps

1. User C (no AccountsManager/AuthorizingOfficer role) tries to approve as first approver
2. **Expect**: 400, "Approver must hold AccountsManager or AuthorizingOfficer role."

### V8: Amount Mismatch on Step 2

1. First approver approves with `approvedAmount: 35000`
2. Second approver tries with `approvedAmount: 40000`
3. **Expect**: 400, "Signatures on different amounts cannot finalize."

### V9: Prepare and Submit Order

1. Get auto-generated order: `GET /api/PaymentOrders/{orderId}`
2. Update with Fund/Appropriation: `PUT /api/PaymentOrders/{orderId}` with `fundId: 1`, `appropriationId: 10`, deductions, `rowVersion`
3. Submit: `PATCH /api/PaymentOrders/{orderId}/submit`
4. **Expect**: 200, status = Submitted, budgetCheckStatus = Passed

### V10: Budget Check Blocks Approval

1. Order with BudgetCheckStatus = Failed
2. Try to approve: `PATCH /api/PaymentOrders/{orderId}/approve`
3. **Expect**: 400, "Budget check failed."

### V11: Override Failed Budget Check

1. User with PaymentOrdersOverrideBudgetCheck approves with `overrideFailedBudgetCheck: true`
2. **Expect**: 200, approval recorded with override documented

### V12: Record Payment

1. Order in Approved status: `POST /api/Payments` with `paymentOrderId: {orderId}`, `paymentMethod: "Check"`, `referenceNumber: "CHK-001"`
2. **Expect**: 201, paymentNumber = PAY-000001, amount = netTotal
3. Verify order status: `GET /api/PaymentOrders/{orderId}` → status = Paid
4. Verify request status: `GET /api/DisbursementRequests/1` → status = Disbursed

### V13: Duplicate Payment Blocked

1. Try second payment on same order
2. **Expect**: 400, "A payment has already been recorded for this payment order."

### V14: Cancel Request (Approved, Unpaid)

1. `PATCH /api/DisbursementRequests/1/cancel` with reason + `rowVersion`
2. **Expect**: 200, status = Cancelled
3. Verify order invalidated: `GET /api/PaymentOrders/{orderId}` → status = Cancelled

### V15: Cancel Blocked (Order Paid)

1. Request with Paid order
2. Try to cancel
3. **Expect**: 400, "Cannot cancel — the linked payment order has already been paid."

### V16: Void Order

1. Order in Approved status, no payment: `PATCH /api/PaymentOrders/{orderId}/void`
2. **Expect**: 200, status = Voided
3. Verify request invalidated: `GET /api/DisbursementRequests/1` → status = Invalidated

### V17: Ledger Posting

1. After V12 (payment recorded), query AccountingEvents
2. **Expect**: One AccountingEvent with matching JournalEntry
3. Verify balanced debits/credits

### V18: List and Detail Views

1. `GET /api/DisbursementRequests?status=Approved` → filtered list
2. `GET /api/DisbursementRequests/1` → full detail with approvals, order link, payment info
3. `GET /api/PaymentOrders?fundId=1` → filtered list
4. `GET /api/PaymentOrders/{orderId}` → full detail with deductions, request link

## Test Commands

```bash
# Unit tests
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Disbursement"
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~PaymentOrder"
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Payment"

# Functional tests
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~DisbursementLifecycle"

# Full suite before merge
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
```
