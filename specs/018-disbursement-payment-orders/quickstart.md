# Quickstart Validation Guide: Disbursement of Approved Payment Orders

**Feature**: 018-disbursement-payment-orders
**Date**: 2026-09-05

## Prerequisites

- SQL Server running with the latest migration applied
- Backend project builds: `dotnet build src/Web/Web.csproj`
- Test projects available: `tests/Application.UnitTests`, `tests/Application.FunctionalTests`
- An existing Approved PaymentOrder with a linked Appropriation and BudgetItem
- Two test users: one with AccountsManager role, one with AuthorizingOfficer role
- BudgetAvailabilityService configured with a test budget item (control method = Blocking)

## Validation Scenarios

### V1: Create Disbursement Request (Sufficient Funds)

**Setup**: Approved PaymentOrder #42, net total 35,000. Budget item has 50,000 available, control = Blocking.

1. Call `POST /api/DisbursementRequests` with `paymentOrderId: 42`
2. **Expect**: 201, status = Draft, requestNumber = DSB-000001, hasWarning = false
3. Call `GET /api/DisbursementRequests/1` to verify linked payment order

### V2: Create Disbursement Request (Insufficient Funds, Blocking)

**Setup**: Same payment order. Budget item has 20,000 available, control = Blocking.

1. Call `POST /api/DisbursementRequests` with `paymentOrderId: 42` (on a different PO)
2. **Expect**: 400 with availabilityBreakdown showing shortfall of 15,000

### V3: Create Disbursement Request (Warning Control)

**Setup**: Budget item has 20,000 available, control = Warning.

1. Call `POST /api/DisbursementRequests` with `paymentOrderId: 42`
2. **Expect**: 201, status = Draft, hasWarning = true

### V4: Duplicate Disbursement Request Blocked

**Setup**: PaymentOrder #42 already has a DisbursementRequest (any status).

1. Call `POST /api/DisbursementRequests` with `paymentOrderId: 42`
2. **Expect**: 400, "A disbursement request already exists for this payment order."

### V5: Dual-Signature Approval (Happy Path)

**Setup**: DisbursementRequest #1 in Draft status.

1. Call `PATCH /api/DisbursementRequests/1/submit` → status = PendingApproval
2. User A (AccountsManager) calls `PATCH /api/DisbursementRequests/1/approve`
3. **Expect**: 200, status = PendingApproval, approvalStep = 1
4. User B (AuthorizingOfficer, distinct from A) calls `PATCH /api/DisbursementRequests/1/approve`
5. **Expect**: 200, status = Approved, approvalStep = 2

### V6: Dual-Signature Approval (Same User Rejected)

**Setup**: DisbursementRequest #1 in PendingApproval with 1 approval from User A.

1. User A calls `PATCH /api/DisbursementRequests/1/approve` again
2. **Expect**: 400, "A different approver is required."

### V7: First Approver Role Check

**Setup**: DisbursementRequest #1 in PendingApproval.

1. User C (no AccountsManager/AuthorizingOfficer role) calls `PATCH /api/DisbursementRequests/1/approve`
2. **Expect**: 400, "First approver must hold AccountsManager or AuthorizingOfficer role."

### V8: Payment Execution

**Setup**: DisbursementRequest #1 in Approved status, PaymentOrder #42 net total = 35,000.

1. Call `POST /api/Payments` with `disbursementRequestId: 1, paymentMethod: "BankTransfer", referenceNumber: "TRF-001"`
2. **Expect**: 201, paymentNumber = PAY-000001, amount = 35,000, status = Completed
3. Call `GET /api/DisbursementRequests/1` → status = Disbursed
4. Call `GET /api/PaymentOrders/42` → status = Paid

### V9: Payment Execution Without Approval

**Setup**: DisbursementRequest #2 in Draft status.

1. Call `POST /api/Payments` with `disbursementRequestId: 2`
2. **Expect**: 400, "Disbursement request must be approved before payment execution."

### V10: Cancellation

**Setup**: DisbursementRequest #1 in Approved status, no payment yet.

1. Call `PATCH /api/DisbursementRequests/1/cancel` with reason "Changed mind"
2. **Expect**: 200, status = Cancelled
3. Verify PaymentOrder #42 status remains Approved (eligible for new request)

### V11: Register Report

**Setup**: Multiple disbursement requests and payments exist.

1. Call `GET /api/DisbursementRequests?fundId=1&status=Approved`
2. **Expect**: Filtered list with correct fund and status
3. Verify totals in response

### V12: Ledger Posting Verification

**Setup**: Payment executed per V8.

1. Query AccountingEvents for SourceTable = "Payment", SourceId = 1
2. **Expect**: One AccountingEvent with matching JournalEntry
3. Verify JournalEntry has balanced debits and credits (Dr party/liability 35,000, Cr cash/bank 35,000)

## Test Commands

```bash
# Unit tests
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Disbursement"
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Payment"

# Functional tests
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~DisbursementLifecycle"

# Full suite before merge
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
```
