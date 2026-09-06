# Feature Specification: Unified Party Registry & Shared Document Panels

**Feature Branch**: `022-unified-party-registry`

**Created**: 2026-09-06

**Status**: Draft

**Input**: "Unified party registry and shared document panels. The backend now has a single Party entity (absorbing suppliers) and generic document services: approval history, append-only status log, and attachments with a mandatory-attachment gate."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Party Management (Priority: P1)

As a registrar, I manage parties (suppliers, customers, government entities, tax authorities, other). I create a party with Arabic name and optional details, edit it, toggle its active status, filter by type and active flag, and search by Arabic name or tax number. When I enter a duplicate tax number, the system warns me before saving.

**Why this priority**: Party management is the foundation for all document references to parties. Without a working registry, documents cannot link to parties.

**Independent Test**: Can be fully tested by creating parties of each type, editing details, toggling active/inactive, filtering the list, searching by name and tax number, and observing duplicate tax number warnings. No documents or approvals needed.

**Acceptance Scenarios**:

1. **Given** the registrar navigates to the party registry, **When** they click "New Party", **Then** a form collects: PartyType (dropdown), NameAr (required), NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes.
2. **Given** the form is filled with valid data, **When** the registrar clicks "Save", **Then** a PartyCode is generated automatically and the party appears in the list.
3. **Given** an existing party with TaxNumber "12345", **When** the registrar creates another party with TaxNumber "12345", **Then** a warning dialog asks to confirm before saving.
4. **Given** a party list with 50 entries across 3 types, **When** the registrar filters by PartyType = Supplier, **Then** only Supplier parties are shown.
5. **Given** the registrar searches for "أحمد" in the search box, **When** results load, **Then** all parties whose NameAr contains "أحمد" are displayed.
6. **Given** the registrar searches for "12345" in the search box, **When** results load, **Then** parties whose TaxNumber matches "12345" are shown.
7. **Given** an active party, **When** the registrar clicks "Toggle Active", **Then** the party's status changes to inactive and the list reflects the change.
8. **Given** a party with open (Draft/Submitted) documents, **When** the registrar attempts to deactivate it, **Then** a blocking warning lists the open documents and prevents deactivation unless confirmed.
9. **Given** a party detail page, **When** the registrar clicks "Edit", **Then** all editable fields are presented in an editable form with current values pre-filled.
10. **Given** a user without PartiesCreate permission navigates to the party list, **When** the page loads, **Then** the "New Party" button is not visible.
11. **Given** a user without PartiesEdit permission views a party detail page, **When** the page loads, **Then** the "Edit" button is not visible.

---

### User Story 2 — Shared Approvals Timeline & Status Log (Priority: P1)

As any user on a document screen, I see a shared approvals timeline showing who approved, when, the decision, and the reason, plus the complete status history log. This panel works identically on every document type (budgets, vouchers, encumbrances, parties).

**Why this priority**: Approvals and status history are core to every document lifecycle. Reusing one panel across all screens eliminates duplication and ensures consistent UX.

**Independent Test**: Can be fully tested by viewing any document's approval timeline and status log entries, verifying they display the correct data from ApprovalHistory and DocumentStatusLog entities.

**Acceptance Scenarios**:

1. **Given** a document with 3 approval records, **When** the user views the document detail page, **Then** an "Approvals" section shows a timeline with: ApproverName, DecisionAt, Decision (Approved/Rejected), Reason — ordered newest first.
2. **Given** a document with 5 status transitions, **When** the user scrolls to the "Status History" section, **Then** a table shows: FromStatus, ToStatus, ChangedBy, ChangedAt, Reason — ordered newest first.
3. **Given** a document with no approval records, **When** the user views the approvals section, **Then** it shows "No approvals yet" (empty state).
4. **Given** a document with no status history, **When** the user views the status history section, **Then** it shows "No status changes recorded" (empty state).
5. **Given** the approvals timeline and status log, **When** the user views either panel, **Then** both panels are collapsible/expandable and start expanded.

---

### User Story 3 — Attachments with Mandatory Gate (Priority: P2)

As a user, I upload and delete attachments on any document. When a document type has mandatory attachment requirements, the approval button shows whether the gate is met or not — blocking approval if required attachments are missing.

**Why this priority**: Attachments ensure document completeness. The gate mechanism prevents approval of incomplete documents without custom code per document type.

**Independent Test**: Can be fully tested by uploading/deleting attachments on a document, checking the gate indicator against known requirements, and observing approval blocking when mandatory attachments are missing.

**Acceptance Scenarios**:

1. **Given** a document detail page, **When** the user clicks "Upload Attachment", **Then** a dropdown shows known attachment types (from the document type's requirements) plus an "Other" option; after selecting a type and choosing a file, the attachment appears in the list with FileName, AttachmentType, UploadedBy, CreatedAt, Size.
2. **Given** an attachment on a document, **When** the user clicks "Delete" on it, **Then** a confirmation dialog appears; on confirm the attachment is removed from the list.
3. **Given** a document type with a mandatory attachment requirement (e.g., "INVOICE" required), **When** no attachment of that type exists, **Then** the approval section shows a warning badge: "Missing required attachment: INVOICE" and the approval button is disabled.
4. **Given** the same document with the required attachment uploaded, **When** the user views the approval section, **Then** the badge turns to a satisfied state and the approval button is enabled.
5. **Given** a user attempts to upload a file exceeding the size limit, **When** the upload is attempted, **Then** a clear error message states the maximum allowed size and the upload is rejected.
6. **Given** a document type with no mandatory attachment requirements, **When** the user views the approval section, **Then** no gate indicator is shown and the approval button is enabled.

---

### User Story 4 — Party Detail with Related Documents (Priority: P2)

As a user viewing a party's detail page, I see a list of all documents related to that party — vouchers, payment orders, encumbrances — so I can understand the party's full financial history.

**Why this priority**: Viewing related documents on the party detail page eliminates the need to cross-reference separate screens, improving efficiency for auditors and accountants.

**Independent Test**: Can be fully tested by creating documents (vouchers, encumbrances) referencing a party, then viewing the party detail page and verifying the documents appear in the related documents list.

**Acceptance Scenarios**:

1. **Given** a party with 2 vouchers and 1 encumbrance, **When** the user views the party detail page, **Then** a "Related Documents" section lists all 3 documents with: DocumentType, DocumentNumber, Status, Date, Amount.
2. **Given** the related documents list, **When** the user clicks a document row, **Then** the user navigates to that document's detail page.
3. **Given** a party with no related documents, **When** the user views the related documents section, **Then** it shows "No related documents" (empty state).
4. **Given** the related documents list has 20 entries, **When** the list renders, **Then** pagination or virtual scrolling ensures the page loads within acceptable time.
5. **Given** the related documents list, **When** the user filters by document type (e.g., only vouchers), **Then** only documents of that type are displayed.

---

## Clarifications

### Session 2026-09-06

- **Q: Duplicate tax number warning — should it be a hard block or a soft warning with confirmation?** → A: Soft warning with confirmation. The registrar can proceed if they have a valid reason (e.g., subsidiary companies sharing a tax ID). This balances data quality with flexibility.
- **Q: For the attachment size limit, what is the maximum file size?** → A: 10 MB per file. This is a reasonable default for document scans and invoices.
- **Q: Should the party detail page related documents section query the backend in real time or use a cached/preloaded list?** → A: Real-time query on page load. The party ID is passed as a filter parameter. Performance is acceptable because document queries are indexed by document type and entity references.
- **Q: For the approvals timeline, should it show approval steps that are pending (not yet decided)?** → A: Yes. Pending steps show with a "Pending" badge and the assigned approver, so the user sees the full approval chain including what's outstanding.
- **Q: Should the shared panels (approvals, status log, attachments) be collapsible by default or always visible?** → A: Collapsible, starting expanded. This allows users to hide panels they don't need while keeping the default view informative.
- **Q: When uploading an attachment, how does the user specify the attachment type code?** → A: Dropdown of known types (from DocumentAttachmentRequirement for the document type) plus an "Other" option with free-text input. This guides users toward expected types while allowing custom types.
- **Q: Should action buttons be hidden when the user lacks permission?** → A: Yes — hide buttons entirely (New Party, Edit, Toggle Active) when the user's role lacks the corresponding permission. Standard ERP behavior; prevents users from attempting actions that will fail.

## Assumptions

1. **Backend services exist**: ApprovalHistory, DocumentStatusLog, Attachment, DocumentAttachmentRequirement, IAttachmentGateService — all exist and are functional. This spec covers only the frontend UI.
2. **Document type discriminator**: Each document type is identified by a string (e.g., "Budget", "ReceiptVoucher", "PaymentOrder", "Encumbrance", "Party"). The backend uses this pattern already.
3. **Party as a document type**: Parties can have attachments using `DocumentType="Party"` and `DocumentId=party.Id`. This is consistent with the generic attachment pattern.
4. **Related documents query**: A query endpoint exists or will be built that accepts a PartyId and returns documents referencing that party. The exact mechanism (direct FK or document metadata) is determined during implementation.
5. **File storage**: `IFileStorageService` handles file persistence. The frontend interacts through the `UploadAttachmentCommand` endpoint.
6. **Search performance**: The existing `GetPartiesQuery` with `NameAr`/`TaxNumber` search is indexed and performs adequately for the expected data volume (under 1 second per success criteria).
7. **Arabic-first design**: NameAr is the primary display field. NameEn is optional and supplementary.
8. **No new entities**: This spec builds UI over existing backend services. No database schema changes are needed.

## Key Entities

| Entity | Purpose | Key Fields |
|--------|---------|------------|
| Party | Registry of all external parties | PartyCode, PartyType, NameAr, TaxNumber, IsActive |
| ApprovalHistory | Append-only approval decisions | DocumentType, DocumentId, Decision, ApproverUserId, DecisionAt, Reason |
| DocumentStatusLog | Append-only status transitions | EntityName, DocumentId, FromStatus, ToStatus, ChangedById, ChangedAt |
| Attachment | File attachments on documents | EntityName, DocumentId, DocumentType, FileName, StoragePath, IsRequired |
| DocumentAttachmentRequirement | Mandatory attachment rules | DocumentType, AttachmentTypeCode, IsMandatory |

## Success Criteria

1. **Party search response time**: Party list search (by Arabic name or tax number) returns results in under 1 second for up to 10,000 parties.
2. **Reusability**: The approvals timeline panel, status history panel, and attachments panel are used on at least 3 different document screens without custom code per screen.
3. **Duplicate tax number detection**: When a user enters a tax number that already exists, a warning is shown within 2 seconds of the field losing focus.
4. **Attachment gate accuracy**: The mandatory attachment gate correctly reflects the backend's `IAttachmentGateService` state — if the backend says attachments are missing, the UI disables approval.
5. **Task completion**: A registrar can create a party, edit it, and toggle its active status in under 2 minutes total.
6. **Related documents load time**: The party detail page's related documents section loads within 2 seconds for up to 100 related documents.
7. **Zero custom code per document screen**: Future document screens that embed the shared panels require only component import and configuration — no inline approval/status/attachment logic.

## Scope

**In Scope**:
- Party list page with search, filter, create, edit, toggle active
- Party detail page with related documents section
- Shared approvals timeline panel (reusable component)
- Shared status history panel (reusable component)
- Shared attachments panel with mandatory gate indicator (reusable component)
- Duplicate tax number warning on create/edit
- Deactivation guard for parties with open documents
- Oversized file upload rejection

**Out of Scope**:
- Backend entity or schema changes (none needed)
- Document creation or editing flows (existing)
- Approval workflow logic (existing via IApprovalService)
- File storage implementation (existing via IFileStorageService)
- Party-to-document linking logic (existing via DocumentType + DocumentId pattern)
- Mobile-responsive design (assumed desktop-first per project convention)
