# Quickstart: Unified Party Registry & Shared Document Panels

**Feature**: 022-unified-party-registry
**Date**: 2026-09-06

## Prerequisites

- Backend running (`dotnet run --project src/Web`)
- Frontend dev server running (`cd src/Web/ClientApp && npm run dev`)
- NSwag client regenerated (`npm run generate-api`)

## Validation Scenarios

### V1: Party List — Search & Filter

1. Navigate to `/parties`
2. Verify the party list loads with existing parties
3. Type "أحمد" in the search box → results filter to matching NameAr entries
4. Type "12345" in the search box → results filter to matching TaxNumber entries
5. Select PartyType = "مورد" from filter dropdown → only Supplier parties shown
6. Toggle the "Active" filter → list shows only active or inactive parties
7. **Expected**: Search returns results in under 1 second

### V2: Party Create & Duplicate Tax Warning

1. Click "New Party" button
2. Fill in: PartyType = Supplier, NameAr = "شركة اختبار", TaxNumber = "12345"
3. Click "Save" → party created, appears in list
4. Click "New Party" again
5. Fill in: PartyType = Customer, NameAr = "عميل جديد", TaxNumber = "12345"
6. Click "Save" → warning dialog appears: "Duplicate tax number found"
7. Click "Confirm" → party saved despite warning
8. **Expected**: Warning appears within 2 seconds of clicking Save

### V3: Party Toggle Active

1. Click on a party row to open detail page
2. Click "Toggle Active" button
3. Confirm the dialog
4. Navigate back to list → party shows as inactive
5. **Expected**: Toggle completes without error

### V4: Party Detail — Related Documents

1. Open a party that has vouchers/encumbrances linked to it
2. Scroll to "Related Documents" section
3. Verify documents list shows: DocumentType, DocumentNumber, Status, Date, Amount
4. Click a document row → navigates to that document's detail page
5. **Expected**: Related documents load within 2 seconds

### V5: Shared Approvals Panel

1. Navigate to any document with approval history (e.g., an approved budget)
2. Scroll to "Approvals" section
3. Verify timeline shows: ApproverName, DecisionAt, Decision, Reason
4. Verify pending steps show "Pending" badge
5. Collapse and expand the panel
6. **Expected**: Panel renders correctly with all approval records

### V6: Shared Status Log Panel

1. Navigate to any document with status transitions
2. Scroll to "Status History" section
3. Verify table shows: FromStatus, ToStatus, ChangedBy, ChangedAt, Reason
4. **Expected**: All status transitions listed, newest first

### V7: Attachments Panel — Upload & Gate Badge

1. Navigate to a document with mandatory attachment requirements (e.g., PurchaseOrder requiring INVOICE)
2. Scroll to "Attachments" section
3. Verify gate badge shows: "Missing required attachment: INVOICE"
4. Verify approval button is disabled
5. Click "Upload Attachment" → type dropdown appears with known types + "Other"
6. Select "INVOICE", choose a file under 10 MB → attachment appears in list
7. Verify gate badge changes to satisfied state
8. Verify approval button is now enabled
9. **Expected**: Gate state updates immediately after upload

### V8: Attachments Panel — Delete

1. On the same document, click "Delete" on an attachment
2. Confirmation dialog appears
3. Confirm → attachment removed from list
4. **Expected**: If the deleted attachment was mandatory, gate badge reverts to warning

### V9: Attachments Panel — Oversized Upload

1. Click "Upload Attachment"
2. Select a file exceeding 10 MB
3. **Expected**: Error message states maximum file size, upload rejected

### V10: Permission-Based Button Visibility

1. Log in as a user WITHOUT PartiesCreate permission
2. Navigate to `/parties`
3. **Expected**: "New Party" button is NOT visible
4. Log in as a user WITHOUT PartiesEdit permission
5. Navigate to a party detail page
6. **Expected**: "Edit" button is NOT visible

### V11: Shared Panels Reuse

1. Import `ApprovalsPanel`, `StatusLogPanel`, `AttachmentsPanel` from `@/features/documents`
2. Embed in a new document screen with props: `{ documentType: "Budget", documentId: 1 }`
3. **Expected**: All three panels render correctly with zero custom code

## Test Commands

```bash
# Backend build
dotnet build src/Web/Web.csproj

# Frontend lint + type check
cd src/Web/ClientApp && npm run lint && npm run typecheck

# Frontend tests
cd src/Web/ClientApp && npm run test

# Full test suite
cd src/Web/ClientApp && npm run test -- --run
```
