# Quickstart Validation: Procurement Lifecycle

**Branch**: `047-procurement-lifecycle` | **Date**: 2026-09-12

## Prerequisites

- SQL Server running with the latest migration applied
- Backend API running (`dotnet run --project src/Web`)
- Frontend dev server running (`npm run dev` in `src/Web/ClientApp`)
- At least 3 suppliers in Parties (PartyType=Supplier)
- At least 5 items in Inventory with units
- At least 1 Budget with active BudgetItemAllocations
- At least 1 Warehouse and 1 Location
- User with all procurement permissions

---

## Scenario 1: Full Competitive Procurement Cycle (PR → RFQ → Quotation → PO → Receipt → Invoice → Payment)

### Step 1: Create Purchase Request
1. Navigate to Purchase Requests list page
2. Click "إنشاء طلب شراء" (Create PR)
3. Add 3 line items with quantities, units, estimated costs
4. Save → verify Draft status, PRQ-{D6} number generated
5. Click "تقديم" (Submit) → verify status = Submitted

### Step 2: Approve Purchase Request
1. Log in as approver
2. Open the submitted PR
3. Click "اعتماد" (Approve) → verify status = Approved
4. Verify ApprovalHistory entry created
5. Verify DocumentStatusLog entry created

### Step 3: Create RFQ
1. From the approved PR, click "إنشاء طلب عروض" (Create RFQ)
2. Set deadline, currency, terms
3. Invite 3 suppliers
4. Click "نشر" (Publish) → verify status = Published

### Step 4: Record Supplier Responses
1. Open the RFQ
2. For each supplier, record response (Responded) or mark as Expired
3. Click "إغلاق الاستلام" (Close Collection) → verify status = Closed

### Step 5: Record Quotations
1. From the RFQ, click "تسجيل عرض" (Record Quotation) for each supplier
2. Enter line items with unit prices, discounts, taxes
3. Submit each quotation

### Step 6: Assign Evaluation Committee
1. Open the RFQ
2. Click "تعيين لجنة التقييم" (Assign Evaluation Committee)
3. Select a committee → verify CommitteeAssignment created with type Tender

### Step 7: Evaluate and Select
1. Open each quotation
2. Click "بدء التقييم" (Start Evaluation)
3. Record technical score, financial score
4. Click "إكمال التقييم" (Complete Evaluation)
5. Select the winning quotation with selection reason
6. Click "اعتماد العرض المختار" (Award Selected Offer)
7. Verify status = Awarded, ApprovalHistory recorded

### Step 8: Create Purchase Order
1. From the awarded quotation, click "إنشاء أمر شراء" (Create PO)
2. Verify PO linked to supplier, PR, quotation
3. Verify line items match quotation
4. Submit for approval

### Step 9: Approve Purchase Order (Budget Check)
1. Approver opens the PO
2. Click "اعتماد" (Approve)
3. **Verify**: Encumbrance created (type Commitment)
4. **Verify**: Budget availability was checked
5. **Verify**: Encumbrance amount matches PO total

### Step 10: Issue Purchase Order
1. Click "إصدار" (Issue) → verify status = Issued

### Step 11: Receive Goods
1. Navigate to Goods Receipt Notes
2. Click "إنشاء سند استلام" (Create GRN)
3. Select the PO
4. Enter received quantities (partial: 60 of 100)
5. Enter accepted (55) and rejected (5) quantities
6. Click "تأكيد الاستلام" (Confirm Receipt)
7. **Verify**: Stock transactions created for 55 units
8. **Verify**: PO line ReceivedQuantity = 55, RemainingQuantity = 45
9. **Verify**: Encumbrance line liquidated proportionally
10. **Verify**: PO status = PartiallyReceived

### Step 12: Record Supplier Invoice
1. Navigate to Supplier Invoices
2. Click "تسجيل فاتورة" (Record Invoice)
3. Select PO, enter supplier invoice number, date
4. Add invoice lines matching PO lines
5. Submit → system performs three-way match
6. **Verify**: Match passes (quantity ≤ accepted, price = PO price)
7. Click "تأكيد المطابقة" (Confirm Match) → status = Matched

### Step 13: Process Payment
1. Navigate to Payment Orders (existing module)
2. Create PaymentOrder linked to the matched invoice
3. Submit, approve, send to treasury, record payment
4. **Verify**: Journal entries posted
5. **Verify**: Invoice status = Paid

### Step 14: Verify PO Closure
1. Receive remaining 40 units (second GRN)
2. Invoice remaining quantity
3. Pay remaining invoice
4. **Verify**: PO status transitions to Received → Closed
5. **Verify**: Remaining encumbrance released

---

## Scenario 2: Direct Purchase (Below Threshold)

1. Create and approve a PR
2. From the approved PR, click "شراء مباشر" (Direct Purchase)
3. **Verify**: System checks total PO value ≤ 5,000,000 YER
4. Create PO directly (no RFQ)
5. Proceed with approval, issue, receipt, invoice, payment

---

## Scenario 3: Rejection and Re-evaluation

1. Create RFQ, record quotations, start evaluation
2. Reject a quotation with reason → verify status = Rejected
3. Select different quotation → award → create PO
4. **Verify**: Rejected quotation visible in history with reason

---

## Scenario 4: PR Cancellation

1. Create and approve a PR
2. Cancel the PR with reason
3. **Verify**: Status = Cancelled
4. Attempt to create RFQ from cancelled PR → **verify rejected**
5. Attempt to create PO from cancelled PR → **verify rejected**

---

## Scenario 5: PO Cancellation (Encumbrance Reversal)

1. Create and approve a PO (encumbrance created)
2. Cancel the PO with reason
3. **Verify**: Encumbrance reversed (status = Cancelled)
4. **Verify**: Budget released
5. **Verify**: Cannot create GRN against cancelled PO

---

## Scenario 6: Over-Invoice Prevention

1. Create PO for 100 units
2. Receive 60 units (55 accepted)
3. Create invoice for 60 units → match passes
4. Create second invoice for 46 units → **verify rejected** ("تم فوترة هذه الكمية بالكامل")
5. Create second invoice for 45 units → match passes (55 + 45 = 100)

---

## Scenario 7: Duplicate Invoice Prevention

1. Record invoice #INV-001 from Supplier A
2. Attempt to record another invoice #INV-001 from Supplier A → **verify rejected** ("فاتورة مكررة لهذا المورد")
3. Record invoice #INV-001 from Supplier B → **verify allowed** (different supplier)

---

## Scenario 8: Manual PO Close

1. Create PO for 100 units
2. Receive 60 units
3. Supplier cannot fulfill remaining 40
4. Click "إغلاق أمر الشراء" (Close PO) with reason
5. **Verify**: PO status = Closed
6. **Verify**: Encumbrance for 40 units released
7. **Verify**: Cannot receive more against this PO

---

## Scenario 9: GRN Frontend — Create from PO Detail

1. Create and approve a PO, issue it
2. Open PO detail page → verify "Create Goods Receipt Note" button visible (status=Issued, user has GoodsReceipts.Create)
3. Click button → navigate to `/procurement/goods-receipt-notes/create?purchaseOrderId=<poId>`
4. **Verify**: PO is pre-selected, user cannot change it
5. **Verify**: Only PO lines with RemainingQuantity > 0 are displayed
6. **Verify**: Read-only fields (Item, Unit, Ordered Qty, Remaining Qty, Unit Cost) match PO data
7. Enter received/accepted/rejected quantities for one line
8. **Verify**: Zod validation: accepted + rejected = received, received ≤ remaining
9. Select warehouse → verify location derived from warehouse
10. Submit → navigate to GRN detail page
11. **Verify**: GRN detail shows correct summary, line items, status = Draft
12. Click Confirm → confirmation dialog appears → confirm → status = Confirmed

---

## Scenario 10: GRN Frontend — Reject with Notes

1. Create a GRN (from PO detail, per Scenario 9)
2. Open GRN detail page → verify status = Draft
3. Click Reject → dialog with optional Notes textarea appears
4. Enter notes → submit
5. **Verify**: status = Rejected, notes visible in detail

---

## Scenario 11: GRN Frontend — Permission Guard

1. Open GRN create page without `?purchaseOrderId=` → **verify error shown, form not rendered**
2. Attempt to access `/procurement/goods-receipt-notes/create` without GoodsReceipts.Create permission → **verify access denied**
3. Attempt to confirm a GRN without GoodsReceipts.Confirm permission → **verify button not shown**

---

## Verification Checklist

After completing all scenarios, verify:

- [ ] All 8 document types have working list/detail screens
- [ ] All screens render in Arabic RTL
- [ ] All status transitions recorded in ApprovalHistory
- [ ] All status transitions recorded in DocumentStatusLog
- [ ] Navigation between linked documents works (PR→RFQ→Quotation→PO→GRN→Invoice→Payment)
- [ ] GRN create requires `?purchaseOrderId=` — shows error if absent
- [ ] GRN list has server-side pagination, status/search/PO filters
- [ ] GRN detail confirm/reject only for Draft status, with dialogs
- [ ] GRN permission codes match backend (GoodsReceipts.*)
- [ ] PO detail shows "Create GRN" button only for Issued/PartiallyReceived + correct permission
- [ ] Supplier Invoice list has server-side pagination, status/search/PO filters
- [ ] Supplier Invoice list has no inline lifecycle actions (Submit/Match/Cancel are detail-page-only)
- [ ] Supplier Invoice create requires `?purchaseOrderId=` — shows error if absent
- [ ] Supplier Invoice create pre-fills PO lines, Unit Price editable
- [ ] Supplier Invoice detail shows status-gated actions: Submit (Draft), Match (Submitted), Cancel (Submitted/Matched/Disputed)
- [ ] Supplier Invoice detail Cancel opens dialog with optional notes textarea
- [ ] Supplier Invoice permission codes match backend (SupplierInvoices.*)
- [ ] PO detail shows "Create Supplier Invoice" button only for Issued/PartiallyReceived + correct permission
- [ ] No orphaned foreign keys in database
- [ ] No duplicate invoices accepted
- [ ] No over-receipt accepted
- [ ] No over-invoicing accepted
- [ ] Encumbrance created atomically on PO approval
- [ ] Encumbrance reversed atomically on PO cancellation
- [ ] Existing tests still pass
