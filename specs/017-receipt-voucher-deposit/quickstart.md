# Quickstart Validation Guide: Receipt Voucher & Deposit Slip Workflow

**Date**: 2026-09-05
**Feature**: 017-receipt-voucher-deposit

## Prerequisites

- .NET 9 SDK installed
- SQL Server running (local or container)
- Test database created
- Application configured and running

## Validation Scenarios

### Scenario 1: Create Receipt Voucher (US1)

**Goal**: Verify voucher creation with voucher number assignment.

**Steps**:
1. Authenticate as a cashier user
2. Create a receipt voucher with line items
3. Verify voucher number is assigned (RCV-{D6} format)
4. Verify status is Draft

**Expected Outcome**:
```json
{
  "voucherNumber": "RCV-000001",
  "status": "Draft"
}
```

**Command**:
```bash
curl -X POST http://localhost:5000/api/ReceiptVouchers \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "voucherDate": "2026-09-05",
    "partyId": 1,
    "paymentMethod": 1,
    "receivedFrom": "Test Party",
    "lines": [
      {"revenueAccountId": 1, "amount": 1000.00}
    ]
  }'
```

---

### Scenario 2: Submit and Approve Voucher (US2)

**Goal**: Verify reviewer gate enforcement.

**Steps**:
1. Submit voucher for review (cashier)
2. Attempt approval by same user (should fail)
3. Approve by different user with reviewer role (should succeed)

**Expected Outcome**:
- Same-user approval returns 403 Forbidden
- Different-user approval succeeds with status "Approved"

**Commands**:
```bash
# Submit
curl -X POST http://localhost:5000/api/ReceiptVouchers/1/submit \
  -H "Authorization: Bearer {cashier-token}"

# Same-user approve (should fail)
curl -X POST http://localhost:5000/api/ReceiptVouchers/1/approve \
  -H "Authorization: Bearer {cashier-token}" \
  -d '{"reason": "Self-approval test"}'

# Different-user approve (should succeed)
curl -X POST http://localhost:5000/api/ReceiptVouchers/1/approve \
  -H "Authorization: Bearer {reviewer-token}" \
  -d '{"reason": "Verified"}'
```

---

### Scenario 3: Create Form 47 Deposit Slip (US3)

**Goal**: Verify cash-only homogeneity.

**Steps**:
1. Create multiple cash-only vouchers
2. Create Form 47 slip with those vouchers
3. Attempt to add a check voucher (should fail)

**Expected Outcome**:
- Form 47 slip created successfully
- Adding check voucher returns 400 Bad Request

**Commands**:
```bash
# Create Form 47 slip
curl -X POST http://localhost:5000/api/DepositSlips \
  -H "Authorization: Bearer {token}" \
  -d '{
    "slipDate": "2026-09-05",
    "formType": 47,
    "voucherIds": [1, 2]
  }'

# Add check voucher (should fail)
curl -X POST http://localhost:5000/api/DepositSlips/1/add-voucher \
  -H "Authorization: Bearer {token}" \
  -d '{"voucherId": 3}'
```

---

### Scenario 4: Approve Form 47 Slip — Revenue Recognition (US3)

**Goal**: Verify revenue recognized on Form 47 approval.

**Steps**:
1. Create and approve vouchers
2. Create Form 47 slip
3. Approve slip as treasury manager
4. Verify journal entry created

**Expected Outcome**:
- Slip status changes to "Approved"
- JournalEntry created with Dr cash/bank GL, Cr revenue

**Command**:
```bash
curl -X POST http://localhost:5000/api/DepositSlips/1/approve \
  -H "Authorization: Bearer {treasury-manager-token}" \
  -d '{"reason": "Verified all vouchers"}'
```

---

### Scenario 5: Form 48 Slip — Two-Stage Clearing (US3, US4)

**Goal**: Verify deferred revenue recognition for checks.

**Steps**:
1. Create check-only vouchers
2. Create Form 48 slip
3. Approve slip (checks move to UnderCollection)
4. Confirm check clearing (revenue recognized)
5. Verify journal entry created on clearing

**Expected Outcome**:
- After slip approval: checks in UnderCollection status
- After clearing: checks in Cleared status, JournalEntry created

**Commands**:
```bash
# Approve Form 48 slip
curl -X POST http://localhost:5000/api/DepositSlips/2/approve \
  -H "Authorization: Bearer {treasury-manager-token}" \
  -d '{"reason": "Verified"}'

# Clear check
curl -X POST http://localhost:5000/api/Checks/1/clear \
  -H "Authorization: Bearer {token}" \
  -d '{"clearedAt": "2026-09-10T09:00:00Z"}'
```

---

### Scenario 6: Bounced Check Handling (US4)

**Goal**: Verify voucher reopening on bounced check.

**Steps**:
1. Process check through UnderCollection
2. Report bounce
3. Verify new voucher created for replacement

**Expected Outcome**:
- Check status changes to "Bounced"
- New ReceiptVoucher created in Draft status
- ReplacementVoucherId linked to original check

**Command**:
```bash
curl -X POST http://localhost:5000/api/Checks/1/bounce \
  -H "Authorization: Bearer {token}" \
  -d '{
    "bouncedAt": "2026-09-10T09:00:00Z",
    "reason": "Insufficient funds"
  }'
```

---

### Scenario 7: Monthly Statement Generation (US5)

**Goal**: Verify statement includes all collections.

**Steps**:
1. Create vouchers for the month
2. Create and approve deposit slips
3. Process check clearings
4. Generate monthly statement

**Expected Outcome**:
- Statement includes all vouchers, deposits, and clearings
- Totals calculated correctly

**Command**:
```bash
curl -X GET "http://localhost:5000/api/Statements/monthly?year=2026&month=9&fundId=1" \
  -H "Authorization: Bearer {token}"
```

---

### Scenario 8: Voucher Number Gap Audit

**Goal**: Verify gap auditing capability.

**Steps**:
1. Create vouchers (numbers assigned sequentially)
2. Cancel one voucher (creates gap)
3. Create another voucher (next number, gap preserved)
4. Query audit report

**Expected Outcome**:
- Voucher numbers: RCV-000001, RCV-000002, RCV-000004 (gap at 000003)
- Audit report shows gap

---

### Scenario 9: Date Sanity Validation

**Goal**: Verify slip date constraints.

**Steps**:
1. Create voucher dated 2026-09-10
2. Attempt to create slip dated 2026-09-05 (before voucher — should fail)
3. Attempt to create slip dated 2026-09-15 (future — should fail)
4. Create slip dated 2026-09-10 (valid — should succeed)

**Expected Outcome**:
- Slips with invalid dates rejected
- Slip with valid date accepted

---

## Test Commands

### Unit Tests
```bash
dotnet test tests/Domain.UnitTests --filter "FullyQualifiedName~Revenue"
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Revenue"
```

### Functional Tests
```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~ReceiptVoucher"
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~DepositSlip"
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~Check"
```

### Integration Tests
```bash
dotnet test tests/Infrastructure.IntegrationTests --filter "FullyQualifiedName~Revenue"
```

---

## Success Criteria Validation

| Criterion | How to Validate |
|-----------|-----------------|
| SC-001: Collection in <2 minutes | Time manual test of create + add lines + save |
| SC-002: Statement without manual tallying | Generate statement, verify no manual calculation needed |
| SC-003: Zero premature revenue recognition | Check no JournalEntry before trigger event |
| SC-004: 100% reviewer gate | Attempt same-user approval, verify rejection |
| SC-005: 100% homogeneity enforcement | Attempt mixed slip, verify rejection |
| SC-006: Gap auditing | Create gap, verify audit report |
| SC-007: Append-only status history | Query status history, verify no updates/deletes |
