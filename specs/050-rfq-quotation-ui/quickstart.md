# Quickstart Validation: RFQ & Quotation Management

**Feature**: 050-rfq-quotation-ui
**Date**: 2026-09-11

## Prerequisites

- Backend running (`dotnet run --project src/Web`)
- Frontend running (`npm run dev` in `src/Web/ClientApp`)
- Database migrated and seeded
- Test user with RFQView, RFQCreate, RFQPublish, RFQComplete, RFQCancel, QuotationsView, QuotationsCreate, QuotationsEvaluate, QuotationsSelect, QuotationsAward, QuotationsReject permissions
- At least one approved PurchaseRequest with detail lines in the database
- At least 2 suppliers (PartyType.Supplier) in the Parties module

## Validation Scenarios

### V1: RFQ Lifecycle (end-to-end)

1. Navigate to `/procurement/rfqs`
2. Click "إنشاء طلب عروض أسعار" → navigate to `/procurement/rfqs/create`
3. Select an approved PurchaseRequest from dropdown
4. Set DeadlineDate to a future date
5. Select 2+ suppliers from the Parties list
6. Submit → verify Draft status, RFQ-{D6} number generated
7. Navigate to detail page → verify all data displayed correctly
8. Click "نشر" (Publish) → verify status transitions to Published
9. Click "تسجيل رد" on a supplier → record response date → verify status changes to Responded
10. Click "إغلاق جمع الردود" (Close) → verify status transitions to Closed
11. Navigate back to list → verify RFQ appears with correct data

### V2: RFQ Edit (Draft only)

1. Create a new RFQ (Draft status)
2. Navigate to detail page → click "تعديل" (Edit)
3. Verify edit page loads at `/procurement/rfqs/:id/edit` with pre-populated values
4. Modify DeadlineDate and Notes
5. Save → verify changes persisted on detail page
6. Publish the RFQ → verify Edit button no longer shown

### V3: RFQ Cancel

1. Create an RFQ in Draft status
2. Click "إلغاء" (Cancel)
3. Verify confirmation dialog appears
4. Confirm cancel → verify status transitions to Cancelled
5. Verify no action buttons shown on detail page

### V4: Quotation Lifecycle (end-to-end)

1. Navigate to `/procurement/quotations`
2. Click "إنشاء عرض سعر" → navigate to create page
3. Select RFQ, RFQSupplier, and SupplierParty
4. Add 2+ line items with Quantity, UnitPrice, DiscountPercent, TaxPercent
5. Verify computed totals (SubTotal, DiscountAmount, TaxAmount, GrandTotal) display correctly
6. Submit → verify Draft status, QT-{D6} number generated
7. Click "تقديم" (Submit) → verify status transitions to Submitted
8. Click "بدء التقييم" (Start Evaluation) → verify status transitions to UnderEvaluation
9. Click "إكمال التقييم" (Complete Evaluation) → enter TechnicalScore and FinancialScore → verify status transitions to Evaluated
10. Click "اختيار" (Select) → enter SelectionReason in dialog → verify status transitions to Selected
11. Click "ترسية" (Award) → verify status transitions to Awarded
12. Verify no action buttons shown (read-only)

### V5: Quotation Edit (Draft only)

1. Create a new quotation (Draft status)
2. Navigate to detail page → click "تعديل" (Edit)
3. Verify edit page loads at `/procurement/quotations/:id/edit` with pre-populated values
4. Modify line items and commercial terms
5. Save → verify changes persisted
6. Submit the quotation → verify Edit button no longer shown

### V6: Quotation Reject

1. Create a quotation and advance to UnderEvaluation status
2. Click "رفض" (Reject) → enter RejectionReason in dialog
3. Verify status transitions to Rejected
4. Verify RejectionReason displayed on detail page

### V7: Backend Endpoint Verification

Run these API calls to verify backend wiring:

```bash
# List quotations
GET /api/Quotations?page=1&pageSize=10

# Get quotation by id (verify full response, not stub)
GET /api/Quotations/{id}

# Submit quotation
PATCH /api/Quotations/{id}/submit

# Reject quotation
PATCH /api/Quotations/{id}/reject
Body: { "rejectionReason": "Test rejection" }

# Record supplier response
PATCH /api/RequestForQuotations/suppliers/{rfqSupplierId}/response
Body: { "responseDate": "2026-09-15" }

# Update RFQ (Draft)
PUT /api/RequestForQuotations/{id}
Body: { "purchaseRequestId": 5, "deadlineDate": "2026-09-25", "supplierPartyIds": [10, 11] }

# Update Quotation (Draft)
PUT /api/Quotations/{id}
Body: same as POST create body
```

### V8: Form Validation

1. Try to create RFQ without selecting suppliers → verify validation error
2. Try to set DeadlineDate before RFQDate → verify validation error
3. Try to submit quotation with no line items → verify validation error
4. Try to enter Quantity = 0 on quotation line → verify validation error
5. Try to enter DiscountPercent = 150 → verify validation error
6. Try to Complete Evaluation without scores → verify validation error
7. Try to Select without SelectionReason → verify validation error
8. Try to Reject without RejectionReason → verify validation error

### V9: UI Quality

1. Verify all pages render in Arabic RTL
2. Verify RFQNumber and QuotationNumber display in LTR (dir="ltr")
3. Verify monetary amounts use tabular-nums
4. Verify skeleton loading states on all list and detail pages
5. Verify empty state when no results match filters
6. Verify error state with retry button on API failure
7. Verify action buttons disabled during mutations
8. Verify no CSS physical properties (ml/mr/pl/pr) in RFQ/quotation code
9. Verify all interactive elements have aria-label

## Expected Outcomes

- V1-V6: All status transitions enforced, data persisted correctly
- V7: All endpoints return Result<T> with correct data (not stubs)
- V8: All validations prevent invalid submissions 100% of the time
- V9: All UI meets RTL, accessibility, and design system standards
